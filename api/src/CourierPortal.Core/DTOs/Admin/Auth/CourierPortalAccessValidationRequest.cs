using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Auth
{
    public class CourierPortalAccessValidationRequest : BaseRequest
    {
        public string AccessKey { get; set; }
    }
}
