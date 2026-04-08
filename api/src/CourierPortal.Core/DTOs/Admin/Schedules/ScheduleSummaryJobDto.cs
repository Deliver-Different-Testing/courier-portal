namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleSummaryJobDto
    {
        public int JobId { get; set; }
        public string Number { get; set; }
        public string ClientCode { get; set; }
        public int? CourierId { get; set; }
    }
}
