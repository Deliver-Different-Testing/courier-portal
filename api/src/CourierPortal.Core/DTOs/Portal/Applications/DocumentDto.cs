using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Instructions { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public long? Length { get; set; }
        public bool Mandatory { get; set; }
        public bool Active { get; set; }

        public IEnumerable<UploadDto> Uploads { get; set; }
    }
}
