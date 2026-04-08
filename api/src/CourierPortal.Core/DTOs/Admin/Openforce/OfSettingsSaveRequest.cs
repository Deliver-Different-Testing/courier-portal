using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfSettingsSaveRequest : BaseRequest
    {
        public string ClientId { get; set; }
        public string ClientGuid { get; set; }
        public string AccessKey { get; set; }
        public string ApiKey { get; set; }
        public string ActivationCode1 { get; set; }
        public string ActivationCode2 { get; set; }
        public string ActivationCode3 { get; set; }
    }
}
