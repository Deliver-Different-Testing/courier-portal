using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class InvoicesResponse : BaseResponse
    {
        public InvoicesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<InvoiceDto> Invoices { get; set; }
    }
}
