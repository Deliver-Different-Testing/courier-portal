using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class InvoiceCreateRequest : BaseRequest
    {
        public IEnumerable<InvoiceCreateRun> Runs { get; set; }
    }
}
