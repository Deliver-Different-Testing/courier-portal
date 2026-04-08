using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoiceBatchRequest : BaseRequest
    {
        public string BatchNo { get; set; }
    }
}
