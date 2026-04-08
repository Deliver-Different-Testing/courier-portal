using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantPasswordResetRequest : IdentifierRequest
    {
        public string NewPassword { get; set; }
    }
}
