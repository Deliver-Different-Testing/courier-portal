namespace CourierPortal.Core.DTOs.Portal.Common
{
    public class EmailAttachmentDto
    {
        public string ContentId { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public byte[] Data { get; set; }
    }
}
