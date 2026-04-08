using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCreateRequest : BaseRequest
    {
        public string CourierCode { get; set; }
        public int CategoryId { get; set; }
        public int Severity { get; set; }
        public DateTime? OccurredOn { get; set; }
        public string Details{ get; set; }
        public bool Notify { get; set; }
    }
}
