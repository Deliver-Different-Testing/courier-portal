namespace CourierPortal.Core.DTOs.Portal.Couriers
{
    public class SubcontractorDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public decimal Percentage { get; set; }
        public decimal FuelPercentage { get; set; }
        public decimal BonusPercentage { get; set; }
    }
}
