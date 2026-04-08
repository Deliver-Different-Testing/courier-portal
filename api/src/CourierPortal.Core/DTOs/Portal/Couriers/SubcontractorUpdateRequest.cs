using System.ComponentModel.DataAnnotations;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Couriers
{
    public class SubcontractorUpdateRequest : BaseRequest
    {
        public int Id { get; set; }
        public decimal Percentage { get; set; }
        public decimal FuelPercentage { get; set; }
        public decimal BonusPercentage { get; set; }
    }
}
