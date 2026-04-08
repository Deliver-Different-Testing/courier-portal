using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class CourierByScheduleDto
    {
        public CourierDetailsDto Courier { get; set; }
        //public int JobsAssigned { get; set; }
        public ScheduleResponseDto ScheduleResponse { get; set; }
    }
}
