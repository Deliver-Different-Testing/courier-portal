using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoiceSearchResponse : BaseResponse
    {
        public InvoiceSearchResponse(Guid messageId) : base(messageId)
        {
        }
        public IEnumerable<TransactionSearchDto> Invoices { get; set; }
    }
}
