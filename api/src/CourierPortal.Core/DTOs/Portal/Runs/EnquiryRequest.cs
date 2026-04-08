using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Runs
{
    public class EnquiryRequest : BaseRequest
    {
        public string JobNumber { get; set; }
        public string Message { get; set; }
    }
}
