namespace CourierPortal.Core.Domain.Entities;

public class TenantSettings
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string? Slug { get; set; }
    public string CompanyName { get; set; } = "Urgent Couriers";
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#FFD200";
    public string? SecondaryColor { get; set; }
    public string? WelcomeTitle { get; set; } = "Join Our Team";
    public string? WelcomeSubtitle { get; set; } = "We're looking for reliable courier drivers across New Zealand.";
    public bool ShowVehicleStep { get; set; } = true;
    public bool ShowQuizStep { get; set; } = true;
    public int? ActiveQuizId { get; set; }
}
