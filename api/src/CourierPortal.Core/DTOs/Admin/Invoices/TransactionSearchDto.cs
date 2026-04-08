using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class TransactionSearchDto
    {
        public string Number { get; set; }
        public DateTime Created { get; set; }
        public CourierDto Courier { get; set; }
    }
}
