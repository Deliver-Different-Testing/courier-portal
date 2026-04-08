namespace CourierPortal.Core.DTOs.Portal.Couriers
{
    public class CourierSummaryDto
    {
        public int CurrentRunCount { get; set; }
        public double Driven { get; set; }
        public decimal Uninvoiced { get; set; }
    }
}
