namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class LocationSummaryJobDto
    {
        public int JobId { get; set; }
        public bool? Void { get; set; }
        public string RunName { get; set; }
        public int? CourierId { get; set; }
        public int? LocationId { get; set; }
    }
}
