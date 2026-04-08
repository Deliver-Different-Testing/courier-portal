using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class AddRemoveInvoiceBatchRequest : BaseRequest
    {
        public List<string> InvoiceNos { get; set; }
    }
}
