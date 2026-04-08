using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoiceResponse : BaseResponse
    {
        public InvoiceResponse(Guid messageId) : base(messageId)
        {
        }

        public TransactionDto Invoice { get; set; }
    }
}
