using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Auth
{
    public class TokenAccessKeyRequest : BaseRequest
    {
        public string AccessKey { get; set; }
    }
}
