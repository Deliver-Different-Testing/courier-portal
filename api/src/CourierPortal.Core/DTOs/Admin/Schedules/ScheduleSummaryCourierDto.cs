using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleSummaryCourierDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Mobile { get; set; }
        public string VehicleType { get; set; }
        public string Region { get; set; }
        public int JobsAssigned { get; set; }
        public IEnumerable<ScheduleSummaryScheduleDto> Schedules { get; set; }
        public IEnumerable<ScheduleSummaryJobDto> Jobs { get; set; }
    }
}
