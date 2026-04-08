
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Deductions
{
    public class DeductionTypeSaveRequest : BaseRequest
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }
}
