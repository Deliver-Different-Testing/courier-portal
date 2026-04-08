using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoiceRequest : BaseRequest
    {
        public string InvoiceNo { get; set; }
    }
}
