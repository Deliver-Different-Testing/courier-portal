namespace CourierPortal.Core.DTOs.Admin.Common
{
    public class CourierDetailsDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string GstNumber { get; set; }
        public string IrdNumber { get; set; }
        public string DriversLicenceNo { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public string VehicleType { get; set; }
        public decimal WithholdingTaxPercentage { get; set; }
        public bool Active { get; set; }
        public string Location { get; set; }
        public string ExternalId { get; set; }
    }
}
