using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Contracts
{
    public class ContractCreateRequest : BaseRequest
    {
        public DataUrlDto File { get; set; }
    }
}
