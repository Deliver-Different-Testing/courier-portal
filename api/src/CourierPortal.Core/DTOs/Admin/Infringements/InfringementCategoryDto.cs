namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Severity { get; set; }
        public bool DetailsRequired { get; set; }
        public bool Active { get; set; }
    }
}
