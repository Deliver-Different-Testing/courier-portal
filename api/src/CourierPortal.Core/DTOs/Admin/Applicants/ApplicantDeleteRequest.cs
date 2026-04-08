using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantDeleteRequest : IdentifierRequest
    {
        public bool SendEmail { get; set; }
        public string Reason { get; set; }
        public bool Reapply { get; set; }
    }
}
