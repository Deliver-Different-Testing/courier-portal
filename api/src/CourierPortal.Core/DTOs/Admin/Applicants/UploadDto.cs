namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class UploadDto
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public long Length { get; set; }
    }
}
