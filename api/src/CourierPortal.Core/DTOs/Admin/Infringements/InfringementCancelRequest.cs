using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCancelRequest : BaseRequest
    {
        public int Id { get; set; }
        public string Reason { get; set; }
    }
}
