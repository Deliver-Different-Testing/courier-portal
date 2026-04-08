using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoiceBatchesResponse : BaseResponse
    {
        public InvoiceBatchesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<InvoiceBatchDto> InvoiceBatches { get; set; }
    }
}
