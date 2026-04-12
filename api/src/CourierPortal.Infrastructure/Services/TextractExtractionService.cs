using System.Globalization;
using System.Text.Json;
using Amazon.Textract;
using Amazon.Textract.Model;
using CourierPortal.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CourierPortal.Infrastructure.Services;

public class DocumentExtractionConfig
{
    public bool Enabled { get; set; } = true;
    public bool AutoClassify { get; set; } = true;
    public float AutoAcceptConfidence { get; set; } = 95f;
    public float ReviewConfidence { get; set; } = 70f;
    public string TextractRegion { get; set; } = "us-east-1";
    public int MaxPagesPerDocument { get; set; } = 10;
}

public class TextractExtractionService : IDocumentExtractionService
{
    private readonly IAmazonTextract _textract;
    private readonly DocumentExtractionConfig _config;
    private readonly ILogger<TextractExtractionService> _logger;

    // Document type detection keywords
    private static readonly Dictionary<string, string[]> DocTypeKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Driver's License"] = ["driver", "licence", "license", "motor vehicle", "class", "endorsement"],
        ["Vehicle Registration"] = ["registration", "rego", "vehicle registration", "plate", "motor registration"],
        ["Insurance Certificate"] = ["insurance", "policy", "certificate of insurance", "indemnity", "coverage", "premium"],
        ["WOF"] = ["warrant of fitness", "wof", "inspection", "vehicle inspection"],
        ["DG Certificate"] = ["dangerous goods", "hazmat", "hazardous", "dg certificate", "HSNO"],
        ["Contract"] = ["contract", "agreement", "terms and conditions", "contractor agreement"]
    };

    // Date field names to look for
    private static readonly string[] ExpiryFieldNames =
    [
        "expiry", "expiry date", "expires", "expiration", "expiration date", "valid until",
        "valid to", "date of expiry", "end date", "renewal date", "due date"
    ];

    private static readonly string[] LicenseNumberFields =
    [
        "licence number", "license number", "licence no", "license no", "dl number", "driver number",
        "document number", "card number"
    ];

    private static readonly string[] PolicyNumberFields =
    [
        "policy number", "policy no", "certificate number", "certificate no", "reference number"
    ];

    public TextractExtractionService(IAmazonTextract textract, IConfiguration configuration, ILogger<TextractExtractionService> logger)
    {
        _textract = textract;
        _logger = logger;
        _config = new DocumentExtractionConfig();
        configuration.GetSection("DocumentExtraction").Bind(_config);
    }

    public async Task<DocumentExtractionResult> ExtractAsync(Stream documentStream, string mimeType, string? expectedDocType = null)
    {
        if (!_config.Enabled)
        {
            return new DocumentExtractionResult { OverallConfidence = 0 };
        }

        _logger.LogInformation("Starting Textract extraction, expectedType={Type}", expectedDocType);

        // Read stream into bytes for Textract
        using var ms = new MemoryStream();
        await documentStream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var result = new DocumentExtractionResult();

        // Try AnalyzeID first for license-type documents
        var isLikelyId = expectedDocType != null &&
            (expectedDocType.Contains("License", StringComparison.OrdinalIgnoreCase) ||
             expectedDocType.Contains("Licence", StringComparison.OrdinalIgnoreCase));

        if (isLikelyId)
        {
            await ExtractIdDocument(bytes, result);
        }

        // Always run AnalyzeDocument for key-value pairs
        await ExtractDocumentKeyValues(bytes, result);

        // Auto-classify if enabled
        if (_config.AutoClassify && string.IsNullOrEmpty(result.DetectedDocumentType))
        {
            result.DetectedDocumentType = ClassifyDocument(result);
        }

        // Find expiry date from extracted fields
        result.DetectedExpiryDate = FindExpiryDate(result.Fields);

        // Calculate overall confidence
        if (result.Fields.Count > 0)
        {
            result.OverallConfidence = result.Fields.Average(f => f.Confidence);
        }

        // Determine if auto-accepted
        result.AutoAccepted = result.OverallConfidence >= _config.AutoAcceptConfidence;

        _logger.LogInformation("Extraction complete: type={Type}, confidence={Confidence:F1}%, fields={Count}, autoAccepted={Auto}",
            result.DetectedDocumentType, result.OverallConfidence, result.Fields.Count, result.AutoAccepted);

        return result;
    }

    private async Task ExtractIdDocument(byte[] bytes, DocumentExtractionResult result)
    {
        try
        {
            var request = new AnalyzeIDRequest
            {
                DocumentPages = [new Document { Bytes = new MemoryStream(bytes) }]
            };

            var response = await _textract.AnalyzeIDAsync(request);

            foreach (var doc in response.IdentityDocuments)
            {
                foreach (var field in doc.IdentityDocumentFields)
                {
                    var fieldName = field.Type?.Text ?? "Unknown";
                    var value = field.ValueDetection?.Text;
                    var confidence = field.ValueDetection?.Confidence ?? 0;

                    result.Fields.Add(new ExtractedField
                    {
                        FieldName = NormalizeFieldName(fieldName),
                        Value = value,
                        Confidence = confidence,
                        RawText = value
                    });
                }
            }

            if (result.Fields.Count > 0)
            {
                result.DetectedDocumentType = "Driver's License";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AnalyzeID failed, falling back to AnalyzeDocument");
        }
    }

    private async Task ExtractDocumentKeyValues(byte[] bytes, DocumentExtractionResult result)
    {
        try
        {
            var request = new AnalyzeDocumentRequest
            {
                Document = new Document { Bytes = new MemoryStream(bytes) },
                FeatureTypes = [FeatureType.FORMS]
            };

            var response = await _textract.AnalyzeDocumentAsync(request);

            // Build block map
            var blockMap = response.Blocks.ToDictionary(b => b.Id);

            // Extract key-value pairs
            var keyBlocks = response.Blocks.Where(b => b.BlockType == BlockType.KEY_VALUE_SET && b.EntityTypes.Contains("KEY"));

            foreach (var keyBlock in keyBlocks)
            {
                var keyText = GetTextFromBlock(keyBlock, blockMap);
                var valueBlock = FindValueBlock(keyBlock, blockMap);
                var valueText = valueBlock != null ? GetTextFromBlock(valueBlock, blockMap) : null;
                var confidence = keyBlock.Confidence;

                if (!string.IsNullOrWhiteSpace(keyText))
                {
                    // Avoid duplicates from AnalyzeID
                    var normalizedKey = NormalizeFieldName(keyText);
                    if (!result.Fields.Any(f => f.FieldName.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.Fields.Add(new ExtractedField
                        {
                            FieldName = normalizedKey,
                            Value = valueText?.Trim(),
                            Confidence = confidence,
                            RawText = valueText
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AnalyzeDocument failed");
        }
    }

    private static string GetTextFromBlock(Block block, Dictionary<string, Block> blockMap)
    {
        if (block.Relationships == null) return string.Empty;

        var childRelation = block.Relationships.FirstOrDefault(r => r.Type == RelationshipType.CHILD);
        if (childRelation == null) return string.Empty;

        var texts = new List<string>();
        foreach (var childId in childRelation.Ids)
        {
            if (blockMap.TryGetValue(childId, out var child) && child.BlockType == BlockType.WORD)
            {
                texts.Add(child.Text);
            }
        }
        return string.Join(" ", texts);
    }

    private static Block? FindValueBlock(Block keyBlock, Dictionary<string, Block> blockMap)
    {
        var valueRelation = keyBlock.Relationships?.FirstOrDefault(r => r.Type == RelationshipType.VALUE);
        if (valueRelation == null || valueRelation.Ids.Count == 0) return null;
        blockMap.TryGetValue(valueRelation.Ids[0], out var valueBlock);
        return valueBlock;
    }

    private static string ClassifyDocument(DocumentExtractionResult result)
    {
        // Score each doc type by keyword matches in extracted field names and values
        var allText = string.Join(" ",
            result.Fields.SelectMany(f => new[] { f.FieldName, f.Value ?? "" }));

        string? bestType = null;
        int bestScore = 0;

        foreach (var (docType, keywords) in DocTypeKeywords)
        {
            var score = keywords.Count(kw =>
                allText.Contains(kw, StringComparison.OrdinalIgnoreCase));

            if (score > bestScore)
            {
                bestScore = score;
                bestType = docType;
            }
        }

        return bestType ?? "Unknown";
    }

    private static DateTime? FindExpiryDate(List<ExtractedField> fields)
    {
        // Look for fields with expiry-related names
        foreach (var field in fields)
        {
            var name = field.FieldName.ToLowerInvariant();
            if (ExpiryFieldNames.Any(efn => name.Contains(efn, StringComparison.OrdinalIgnoreCase))
                && !string.IsNullOrWhiteSpace(field.Value))
            {
                var parsed = ParseDate(field.Value);
                if (parsed.HasValue) return parsed;
            }
        }

        // Also check for EXPIRATION_DATE from AnalyzeID
        var expiryField = fields.FirstOrDefault(f =>
            f.FieldName.Contains("expiration", StringComparison.OrdinalIgnoreCase) ||
            f.FieldName.Contains("expiry", StringComparison.OrdinalIgnoreCase));

        if (expiryField != null && !string.IsNullOrWhiteSpace(expiryField.Value))
        {
            return ParseDate(expiryField.Value);
        }

        return null;
    }

    /// <summary>
    /// Parse date handling NZ (dd/MM/yyyy) and US (MM/dd/yyyy) formats.
    /// </summary>
    private static DateTime? ParseDate(string value)
    {
        var formats = new[]
        {
            "dd/MM/yyyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy",
            "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy",
            "yyyy-MM-dd", "dd-MM-yyyy", "MM-dd-yyyy",
            "dd MMM yyyy", "d MMM yyyy", "MMM dd, yyyy", "MMMM dd, yyyy",
            "yyyy/MM/dd"
        };

        foreach (var fmt in formats)
        {
            if (DateTime.TryParseExact(value.Trim(), fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                // Sanity check: dates should be within reasonable range
                if (dt.Year >= 2000 && dt.Year <= 2050)
                    return dt;
            }
        }

        // Fallback: general parse
        if (DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
        {
            if (fallback.Year >= 2000 && fallback.Year <= 2050)
                return fallback;
        }

        return null;
    }

    private static string NormalizeFieldName(string fieldName)
    {
        // Convert Textract field names to human-readable
        return fieldName
            .Replace("_", " ")
            .Replace("  ", " ")
            .Trim();
    }
}
