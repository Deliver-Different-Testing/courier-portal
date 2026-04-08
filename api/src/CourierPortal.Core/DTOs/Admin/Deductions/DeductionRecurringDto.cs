

using System;
using CourierPortal.Core.DTOs.Admin.Invoices;

namespace CourierPortal.Core.DTOs.Admin.Deductions
{
    public class DeductionRecurringDto : TransactionDto
    {
        public int RecurringType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Day { get; set; }
        public bool Paused { get; set; }
        public DateTime? LastProcessed { get; set; }
    }
}
