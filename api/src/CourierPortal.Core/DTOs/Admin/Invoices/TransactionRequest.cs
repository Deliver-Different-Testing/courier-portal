using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class TransactionRequest : BaseRequest
    {
        public string Number { get; set; }
    }
}
