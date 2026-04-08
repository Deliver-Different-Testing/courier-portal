using System;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantDocumentDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public string Instructions { get; set; }
        public bool Mandatory { get; set; }
        public bool Active { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public long? Length { get; set; }
    }
}
