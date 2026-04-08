using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoicesResponse : BaseResponse
    {
        public InvoicesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<TransactionDto> Invoices { get; set; }
    }
}
