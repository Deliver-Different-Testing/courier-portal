using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantDocumentUpdateRequest : BaseRequest
    {
        public int Id { get; set; }
        public string Instructions { get; set; }
        public bool Mandatory { get; set; }
        public bool Active { get; set; }
    }
}
