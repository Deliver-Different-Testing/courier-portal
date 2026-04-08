using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoiceBatchResponse : BaseResponse
    {
        public InvoiceBatchResponse(Guid messageId) : base(messageId)
        {
        }

        public InvoiceBatchDto InvoiceBatch { get; set; }
    }
}
