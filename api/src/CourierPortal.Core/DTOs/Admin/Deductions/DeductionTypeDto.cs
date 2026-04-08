using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Common;
using Newtonsoft.Json;

namespace CourierPortal.Core.DTOs.Admin.Deductions
{
    public class DeductionTypeDto : NameIdDto
    {
        public string ExternalId { get; set; }
    }
}
