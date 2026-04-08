using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class InvoiceResponse : BaseResponse
    {
        public InvoiceResponse(Guid messageId) : base(messageId)
        {
        }

        public InvoiceDto Invoice { get; set; }
    }
}
