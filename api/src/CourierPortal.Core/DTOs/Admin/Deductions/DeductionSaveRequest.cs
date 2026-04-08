using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Deductions
{
    public class DeductionSaveRequest : BaseRequest
    {
        public string Number { get; set; }
        public NameIdDto Courier { get; set; }
        public string Reference { get; set; }
        public IEnumerable<TransactionLineDto> Lines { get; set; }
    }
}
