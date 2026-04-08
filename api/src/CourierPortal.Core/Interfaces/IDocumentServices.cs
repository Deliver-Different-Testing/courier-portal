using CourierPortal.Core.DTOs;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>
/// S3 document storage: upload, download, presigned URLs, delete.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>Upload a stream to S3. Returns the S3 key.</summary>
    Task<string> UploadAsync(Stream stream, string s3Key, string contentType);

    /// <summary>Download a file from S3.</summary>
    Task<Stream> DownloadAsync(string s3Key);

    /// <summary>Generate a presigned download URL (default 15 min expiry).</summary>
    Task<string> GetPresignedUrlAsync(string s3Key, int expiryMinutes = 15);

    /// <summary>Delete a file from S3.</summary>
    Task DeleteAsync(string s3Key);

    /// <summary>Build the S3 key for a courier document.</summary>
    string BuildCourierDocumentKey(int tenantId, int courierId, int docId, string fileName);

    /// <summary>Build the S3 key for a document type template.</summary>
    string BuildTemplateKey(int tenantId, int docTypeId, string fileName);
}

/// <summary>
/// AI document extraction via AWS Textract.
/// </summary>
public interface IDocumentExtractionService
{
    /// <summary>Extract fields from a document using Textract.</summary>
    Task<DocumentExtractionResult> ExtractAsync(Stream documentStream, string mimeType, string? expectedDocType = null);
}

/// <summary>
/// Internal extraction result (not a DTO — used between services).
/// </summary>
public class DocumentExtractionResult
{
    public string? DetectedDocumentType { get; set; }
    public float OverallConfidence { get; set; }
    public List<ExtractedField> Fields { get; set; } = new();
    public DateTime? DetectedExpiryDate { get; set; }
    public bool AutoAccepted { get; set; }
}

public class ExtractedField
{
    public string FieldName { get; set; } = string.Empty;
    public string? Value { get; set; }
    public float Confidence { get; set; }
    public string? RawText { get; set; }
}

/// <summary>
/// CRUD for configurable document types per tenant.
/// </summary>
public interface IDocumentTypeService
{
    Task<IReadOnlyList<DocumentTypeListDto>> GetAllAsync(int tenantId);
    Task<DocumentTypeDetailDto?> GetByIdAsync(int id, int tenantId);
    Task<DocumentTypeDetailDto> CreateAsync(CreateDocumentTypeDto dto, int tenantId);
    Task<DocumentTypeDetailDto?> UpdateAsync(int id, UpdateDocumentTypeDto dto, int tenantId);
    Task<bool> DeactivateAsync(int id, int tenantId);
    Task<string> UploadTemplateAsync(int id, int tenantId, Stream stream, string fileName, string mimeType);
    Task<(Stream Stream, string FileName, string MimeType)?> DownloadTemplateAsync(int id, int tenantId);
    Task SeedDefaultTypesAsync(int tenantId);
}

/// <summary>
/// Courier document management: upload, download, list, delete, verify.
/// </summary>
public interface ICourierDocumentService
{
    Task<IReadOnlyList<CourierDocumentDto>> GetDocumentsAsync(int courierId, int npAgentId);
    Task<CourierDocumentUploadResultDto> UploadAsync(int courierId, int npAgentId, int documentTypeId, Stream stream, string fileName, string mimeType, long fileSize, string? uploadedBy);
    Task<string> GetDownloadUrlAsync(int docId, int courierId, int npAgentId);
    Task<bool> DeleteAsync(int docId, int courierId, int npAgentId);
    Task<CourierDocumentDto?> VerifyAsync(int docId, int courierId, int npAgentId, string verifiedBy);
    Task<DocumentExtractionResultDto> ExtractOnlyAsync(Stream stream, string mimeType);
    Task CheckAndEnforceComplianceAsync(int courierId, int npAgentId);
}
