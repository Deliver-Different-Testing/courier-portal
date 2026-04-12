using System.Text.Json;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CourierPortal.Infrastructure.Services;

public class CourierDocumentService : ICourierDocumentService
{
    private readonly AgentsDbContext _db;
    private readonly IDocumentStorageService _storage;
    private readonly IDocumentExtractionService _extraction;
    private readonly ILogger<CourierDocumentService> _logger;
    private readonly DocumentExtractionConfig _extractionConfig;

    public CourierDocumentService(
        AgentsDbContext db,
        IDocumentStorageService storage,
        IDocumentExtractionService extraction,
        IConfiguration configuration,
        ILogger<CourierDocumentService> logger)
    {
        _db = db;
        _storage = storage;
        _extraction = extraction;
        _logger = logger;
        _extractionConfig = new DocumentExtractionConfig();
        configuration.GetSection("DocumentExtraction").Bind(_extractionConfig);
    }

    public async Task<IReadOnlyList<CourierDocumentDto>> GetDocumentsAsync(int courierId, int npAgentId)
    {
        // Verify courier belongs to NP
        var courier = await GetVerifiedCourier(courierId, npAgentId);
        if (courier == null) return [];

        var docs = await _db.CourierDocuments
            .Include(d => d.DocumentType)
            .Where(d => d.CourierId == courierId && !d.IsDeleted)
            .OrderBy(d => d.DocumentType.SortOrder)
            .ThenByDescending(d => d.UploadedDate)
            .ToListAsync();

        // Recalculate statuses
        foreach (var doc in docs)
        {
            doc.Status = CalculateStatus(doc);
        }
        await _db.SaveChangesAsync();

        return docs.Select(MapToDto).ToList();
    }

    public async Task<CourierDocumentUploadResultDto> UploadAsync(
        int courierId, int npAgentId, int documentTypeId,
        Stream stream, string fileName, string mimeType, long fileSize, string? uploadedBy)
    {
        var courier = await GetVerifiedCourier(courierId, npAgentId)
            ?? throw new InvalidOperationException($"Courier {courierId} not found or not owned by NP {npAgentId}");

        var docType = await _db.CourierDocumentTypes.FindAsync(documentTypeId)
            ?? throw new InvalidOperationException($"Document type {documentTypeId} not found");

        // Supersede existing documents of this type for this courier
        var existing = await _db.CourierDocuments
            .Where(d => d.CourierId == courierId && d.DocumentTypeId == documentTypeId && !d.IsDeleted && d.Status != DocumentStatus.Superseded)
            .ToListAsync();

        foreach (var old in existing)
        {
            old.Status = DocumentStatus.Superseded;
            _db.CourierDocumentAudits.Add(new CourierDocumentAudit
            {
                DocumentId = old.Id,
                Action = DocumentAuditAction.Replaced,
                PerformedBy = uploadedBy,
                Details = $"Superseded by new upload of {fileName}"
            });
        }

        // Create document record first to get ID
        var doc = new CourierDocument
        {
            CourierId = courierId,
            DocumentTypeId = documentTypeId,
            FileName = fileName,
            MimeType = mimeType,
            FileSize = fileSize,
            S3Key = "", // Will be set after upload
            UploadedBy = uploadedBy,
            UploadedDate = DateTime.UtcNow,
            Status = DocumentStatus.Current
        };

        _db.CourierDocuments.Add(doc);
        await _db.SaveChangesAsync();

        // Upload to S3
        var s3Key = _storage.BuildCourierDocumentKey(courier.TenantId, courierId, doc.Id, fileName);

        // Need to copy stream for both S3 upload and Textract
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        await _storage.UploadAsync(ms, s3Key, mimeType);
        doc.S3Key = s3Key;

        // Run AI extraction
        DocumentExtractionResultDto? extractionDto = null;
        if (_extractionConfig.Enabled)
        {
            try
            {
                ms.Position = 0;
                var extractionResult = await _extraction.ExtractAsync(ms, mimeType, docType.Name);

                doc.AiConfidence = extractionResult.OverallConfidence;
                doc.AiExtractionJson = JsonSerializer.Serialize(extractionResult);
                doc.AiDetectedType = extractionResult.DetectedDocumentType;
                doc.AiVerified = extractionResult.AutoAccepted;

                // Set expiry from AI if confidence is high enough
                if (extractionResult.DetectedExpiryDate.HasValue && extractionResult.OverallConfidence >= _extractionConfig.AutoAcceptConfidence)
                {
                    doc.ExpiryDate = extractionResult.DetectedExpiryDate;
                    doc.Status = CalculateStatus(doc);

                    // Auto-populate NpCourier expiry fields
                    await AutoPopulateCourierExpiry(courier, docType.Name, extractionResult);
                }
                else if (extractionResult.DetectedExpiryDate.HasValue)
                {
                    // Set as suggestion — still populate doc expiry but don't auto-accept
                    doc.ExpiryDate = extractionResult.DetectedExpiryDate;
                    doc.Status = CalculateStatus(doc);
                }

                extractionDto = new DocumentExtractionResultDto(
                    extractionResult.DetectedDocumentType,
                    extractionResult.OverallConfidence,
                    extractionResult.Fields.Select(f => new ExtractedFieldDto(f.FieldName, f.Value, f.Confidence, f.RawText)).ToList(),
                    extractionResult.DetectedExpiryDate,
                    extractionResult.AutoAccepted
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI extraction failed for document {DocId}, continuing without extraction", doc.Id);
            }
        }

        // Audit
        _db.CourierDocumentAudits.Add(new CourierDocumentAudit
        {
            DocumentId = doc.Id,
            Action = DocumentAuditAction.Uploaded,
            PerformedBy = uploadedBy,
            Details = $"Uploaded {fileName} ({fileSize} bytes). AI confidence: {doc.AiConfidence:F1}%"
        });

        await _db.SaveChangesAsync();

        _logger.LogInformation("Uploaded document {DocId} for courier {CourierId}, type={TypeName}, AI={Confidence}%",
            doc.Id, courierId, docType.Name, doc.AiConfidence);

        // Check and enforce compliance after upload
        await CheckAndEnforceComplianceAsync(courierId, npAgentId);

        return new CourierDocumentUploadResultDto(doc.Id, doc.FileName, doc.Status, extractionDto);
    }

    public async Task<string> GetDownloadUrlAsync(int docId, int courierId, int npAgentId)
    {
        var doc = await GetVerifiedDocument(docId, courierId, npAgentId)
            ?? throw new InvalidOperationException("Document not found");

        return await _storage.GetPresignedUrlAsync(doc.S3Key);
    }

    public async Task<bool> DeleteAsync(int docId, int courierId, int npAgentId)
    {
        var doc = await GetVerifiedDocument(docId, courierId, npAgentId);
        if (doc == null) return false;

        doc.IsDeleted = true;
        _db.CourierDocumentAudits.Add(new CourierDocumentAudit
        {
            DocumentId = doc.Id,
            Action = DocumentAuditAction.Deleted,
            Details = "Soft deleted"
        });

        // Clean up S3
        try
        {
            await _storage.DeleteAsync(doc.S3Key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete S3 object {Key}", doc.S3Key);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<CourierDocumentDto?> VerifyAsync(int docId, int courierId, int npAgentId, string verifiedBy)
    {
        var doc = await GetVerifiedDocument(docId, courierId, npAgentId);
        if (doc == null) return null;

        doc.HumanVerified = true;
        doc.VerifiedDate = DateTime.UtcNow;
        doc.VerifiedBy = verifiedBy;

        _db.CourierDocumentAudits.Add(new CourierDocumentAudit
        {
            DocumentId = doc.Id,
            Action = DocumentAuditAction.Verified,
            PerformedBy = verifiedBy,
            Details = "Human verified"
        });

        await _db.SaveChangesAsync();

        // Reload with navigation
        await _db.Entry(doc).Reference(d => d.DocumentType).LoadAsync();
        return MapToDto(doc);
    }

    public async Task<DocumentExtractionResultDto> ExtractOnlyAsync(Stream stream, string mimeType)
    {
        var result = await _extraction.ExtractAsync(stream, mimeType);

        return new DocumentExtractionResultDto(
            result.DetectedDocumentType,
            result.OverallConfidence,
            result.Fields.Select(f => new ExtractedFieldDto(f.FieldName, f.Value, f.Confidence, f.RawText)).ToList(),
            result.DetectedExpiryDate,
            result.AutoAccepted
        );
    }

    public async Task CheckAndEnforceComplianceAsync(int courierId, int npAgentId)
    {
        var courier = await GetVerifiedCourier(courierId, npAgentId);
        if (courier == null) return;

        var blockingTypes = await _db.CourierDocumentTypes
            .Where(dt => dt.TenantId == courier.TenantId && dt.Active && dt.BlockOnExpiry &&
                         (dt.AppliesTo == DocumentAppliesTo.ActiveCourier || dt.AppliesTo == DocumentAppliesTo.Both))
            .ToListAsync();

        var docs = await _db.CourierDocuments
            .Where(d => d.CourierId == courierId && !d.IsDeleted && d.Status != DocumentStatus.Superseded)
            .ToListAsync();

        var now = DateTime.UtcNow;
        bool hasBlockingIssue = false;

        foreach (var dt in blockingTypes)
        {
            var doc = docs.FirstOrDefault(d => d.DocumentTypeId == dt.Id);
            if (doc == null || (doc.ExpiryDate.HasValue && doc.ExpiryDate.Value < now))
            {
                hasBlockingIssue = true;
                break;
            }
        }

        if (hasBlockingIssue && courier.Status == "Active")
        {
            courier.Status = "Suspended-Compliance";
            courier.ModifiedDate = now;
            _db.CourierDocumentAudits.Add(new CourierDocumentAudit
            {
                DocumentId = 0,
                Action = DocumentAuditAction.AutoSuspended,
                PerformedBy = "System",
                Details = $"Auto-suspended courier {courierId} due to expired/missing blocking document"
            });
            _logger.LogWarning("Auto-suspended courier {CourierId} due to compliance failure", courierId);
            await _db.SaveChangesAsync();
        }
        else if (!hasBlockingIssue && courier.Status == "Suspended-Compliance")
        {
            courier.Status = "Active";
            courier.ModifiedDate = now;
            _db.CourierDocumentAudits.Add(new CourierDocumentAudit
            {
                DocumentId = 0,
                Action = DocumentAuditAction.AutoReactivated,
                PerformedBy = "System",
                Details = $"Auto-reactivated courier {courierId} — all blocking documents are current"
            });
            _logger.LogInformation("Auto-reactivated courier {CourierId} — compliance restored", courierId);
            await _db.SaveChangesAsync();
        }
    }

    // --- Private helpers ---

    private async Task<NpCourier?> GetVerifiedCourier(int courierId, int npAgentId)
    {
        return await _db.NpCouriers.FirstOrDefaultAsync(c => c.CourierId == courierId && c.AgentId == npAgentId);
    }

    private async Task<CourierDocument?> GetVerifiedDocument(int docId, int courierId, int npAgentId)
    {
        var courier = await GetVerifiedCourier(courierId, npAgentId);
        if (courier == null) return null;

        return await _db.CourierDocuments
            .Include(d => d.DocumentType)
            .FirstOrDefaultAsync(d => d.Id == docId && d.CourierId == courierId && !d.IsDeleted);
    }

    private DocumentStatus CalculateStatus(CourierDocument doc)
    {
        if (doc.Status == DocumentStatus.Superseded) return DocumentStatus.Superseded;
        if (!doc.ExpiryDate.HasValue) return DocumentStatus.Current;

        var now = DateTime.UtcNow;
        if (doc.ExpiryDate.Value < now) return DocumentStatus.Expired;

        var warningDays = doc.DocumentType?.ExpiryWarningDays ?? 30;
        if (doc.ExpiryDate.Value < now.AddDays(warningDays)) return DocumentStatus.ExpiringSoon;

        return DocumentStatus.Current;
    }

    private async Task AutoPopulateCourierExpiry(NpCourier courier, string docTypeName, DocumentExtractionResult extraction)
    {
        var updated = false;
        var normalizedName = docTypeName.ToLowerInvariant();

        if (normalizedName.Contains("license") || normalizedName.Contains("licence"))
        {
            courier.DriversLicenseExpiry = extraction.DetectedExpiryDate;
            // Also try to extract license number
            var licNum = extraction.Fields.FirstOrDefault(f =>
                f.FieldName.Contains("licence number", StringComparison.OrdinalIgnoreCase) ||
                f.FieldName.Contains("license number", StringComparison.OrdinalIgnoreCase) ||
                f.FieldName.Contains("document number", StringComparison.OrdinalIgnoreCase));
            if (licNum?.Value != null && licNum.Confidence >= _extractionConfig.AutoAcceptConfidence)
            {
                courier.DriversLicenseNo = licNum.Value;
            }
            updated = true;
        }
        else if (normalizedName.Contains("registration") || normalizedName.Contains("rego"))
        {
            courier.RegoExpiry = extraction.DetectedExpiryDate;
            updated = true;
        }
        else if (normalizedName.Contains("insurance"))
        {
            courier.InsuranceExpiry = extraction.DetectedExpiryDate;
            var policyNum = extraction.Fields.FirstOrDefault(f =>
                f.FieldName.Contains("policy", StringComparison.OrdinalIgnoreCase) ||
                f.FieldName.Contains("certificate number", StringComparison.OrdinalIgnoreCase));
            if (policyNum?.Value != null && policyNum.Confidence >= _extractionConfig.AutoAcceptConfidence)
            {
                courier.InsurancePolicyNumber = policyNum.Value;
            }
            updated = true;
        }
        else if (normalizedName.Contains("wof") || normalizedName.Contains("warrant"))
        {
            courier.WofExpiry = extraction.DetectedExpiryDate;
            updated = true;
        }
        else if (normalizedName.Contains("dangerous") || normalizedName.Contains("dg"))
        {
            courier.DangerousGoodsExpiry = extraction.DetectedExpiryDate;
            courier.HasDangerousGoods = true;
            updated = true;
        }
        else if (normalizedName.Contains("tsl"))
        {
            courier.TslExpiry = extraction.DetectedExpiryDate;
            updated = true;
        }

        if (updated)
        {
            courier.ModifiedDate = DateTime.UtcNow;
            _logger.LogInformation("Auto-populated expiry fields for courier {CourierId} from {DocType}", courier.CourierId, docTypeName);
        }
    }

    private static CourierDocumentDto MapToDto(CourierDocument doc) => new(
        doc.Id,
        doc.CourierId,
        doc.DocumentTypeId,
        doc.DocumentType?.Name ?? "Unknown",
        doc.DocumentType?.Category ?? DocumentCategory.Other,
        doc.FileName,
        doc.MimeType,
        doc.FileSize,
        doc.ExpiryDate,
        doc.Status,
        doc.AiConfidence,
        doc.AiDetectedType,
        doc.AiVerified,
        doc.HumanVerified,
        doc.UploadedDate,
        doc.UploadedBy,
        doc.VerifiedDate,
        doc.VerifiedBy,
        doc.Notes
    );
}
