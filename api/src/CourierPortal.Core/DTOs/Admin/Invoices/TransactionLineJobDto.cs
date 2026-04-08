namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class TransactionLineJobDto
    {
        public long LineId { get; set; }
        public int JobId { get; set; }
        public string JobNumber { get; set; }
        public string ClientCode { get; set; }
        public string Location { get; set; }
        public string DeliveryAddressLine1 { get; set; }
        public string DeliveryAddressLine2 { get; set; }
        public string DeliveryAddressLine3 { get; set; }
        public string DeliveryAddressLine4 { get; set; }
        public string DeliveryAddressLine5 { get; set; }
        public string DeliveryAddressLine6 { get; set; }
        public string DeliveryAddressLine7 { get; set; }
        public string DeliveryAddressLine8 { get; set; }
        public decimal CourierPayment { get; set; }
        public decimal CourierFuel { get; set; }
        public decimal CourierBonus { get; set; }
    }
}
