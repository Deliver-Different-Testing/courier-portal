using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Invoices;
using System;

namespace CourierPortal.Core.DTOs.Admin.Deductions
{
    public class DeductionRecurringSaveRequest : BaseRequest
    {
        public string Number { get; set; }
        public int RecurringType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Day { get; set; }
        public bool Paused { get; set; }
        public NameIdDto Courier { get; set; }
        public string Reference { get; set; }
        public IEnumerable<TransactionLineDto> Lines { get; set; }
    }
}
