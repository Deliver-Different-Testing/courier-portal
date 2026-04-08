using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class EmailVerificationRequest : BaseRequest
    {
        public string Email { get; set; }
        public string VerificationCode { get; set; }
    }
}
