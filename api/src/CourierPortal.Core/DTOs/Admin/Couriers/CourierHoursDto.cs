namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class CourierHoursDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public int Hours { get; set; }
        public bool AvailableMorning { get; set; }
        public bool AvailableAfternoon { get; set; }
    }
}
