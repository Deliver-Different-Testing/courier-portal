using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class InvoiceRequest : BaseRequest
    {
        public string InvoiceNo { get; set; }
    }
}
