using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class InvoicesPastResponse : BaseResponse
    {
        public InvoicesPastResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<InvoicePastDto> Invoices { get; set; }
    }
}
