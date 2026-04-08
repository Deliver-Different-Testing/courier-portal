using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantDocumentCreateRequest : BaseRequest
    {
        public string Name { get; set; }
        public string Instructions { get; set; }
        public bool Mandatory { get; set; }
        public bool Active { get; set; }
        public DataUrlDto File { get; set; }
    }
}
