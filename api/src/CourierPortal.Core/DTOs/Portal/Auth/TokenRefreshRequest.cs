using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Auth
{
    public class TokenRefreshRequest : BaseRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
