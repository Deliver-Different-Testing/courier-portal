namespace CourierPortal.Core.DTOs.Portal.Runs
{
    public class JobDto
    {
        public int JobId { get; set; }
        public string JobNumber { get; set; }
        public string ClientCode { get; set; }
        public string DeliveryAddressLine1 { get; set; }
        public string DeliveryAddressLine2 { get; set; }
        public string DeliveryAddressLine3 { get; set; }
        public string DeliveryAddressLine4 { get; set; }
        public string DeliveryAddressLine5 { get; set; }
        public string DeliveryAddressLine6 { get; set; }
        public string DeliveryAddressLine7 { get; set; }
        public string DeliveryAddressLine8 { get; set; }
        public double? DeliveryLatitude { get; set; }
        public double? DeliveryLongitude { get; set; }
        public string Status { get; set; }
        public bool Void { get; set; }
        public decimal CourierPayment { get; set; }
        public decimal CourierFuel { get; set; }
        public decimal CourierBonus { get; set; }
        public int? MasterId { get; set; }
        public decimal? MasterPayment { get; set; }
        public decimal? MasterFuel { get; set; }
        public decimal? MasterBonus { get; set; }
    }
}
