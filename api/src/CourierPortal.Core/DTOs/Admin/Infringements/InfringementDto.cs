using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public CourierDto Courier { get; set; }
        public InfringementCategoryDto Category { get; set; }
        public int Severity { get; set; }
        public DateTime OccurredOn { get; set; }
        public string Details { get; set; }
        public bool Cancelled { get; set; }
        public string CancelledReason { get; set; }
    }
}
