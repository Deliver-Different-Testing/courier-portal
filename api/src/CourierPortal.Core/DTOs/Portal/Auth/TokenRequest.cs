using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Auth
{
    public class TokenRequest : BaseRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
