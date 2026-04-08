using System;

namespace CourierPortal.Core.Domain
{
    public class RunItem
    {
        public int JobId { get; set; }
        public DateTime BookDate { get; set; }
        public string JobNumber { get; set; }
        public string ClientCode { get; set; }
        public int RunId { get; set; }
        public string RunName { get; set; }
        public double RunKms { get; set; }
        public int RunMins { get; set; }
        public string DeliveryAddressLine1 { get; set; }
        public string DeliveryAddressLine2 { get; set; }
        public string DeliveryAddressLine3 { get; set; }
        public string DeliveryAddressLine4 { get; set; }
        public string DeliveryAddressLine5 { get; set; }
        public string DeliveryAddressLine6 { get; set; }
        public string DeliveryAddressLine7 { get; set; }
        public string DeliveryAddressLine8 { get; set; }
        public string DeliveryLongitude { get; set; }
        public string DeliveryLatitude { get; set; }
        public double CourierPercentage { get; set; }
        public decimal? CourierPayment { get; set; }
        public decimal? CourierFuel { get; set; }
        public decimal? CourierBonus { get; set; }
        public int? MasterCourierId { get; set; }
        public decimal? SubContractorPercentage { get; set; }
        public decimal? SubContractorFuelPercentage { get; set; }
        public decimal? SubContractorBonusPercentage { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public bool JobDone { get; set; }
        public bool Void { get; set; }
        public bool Reprice { get; set; }
        public bool Archived { get; set; }
        public bool Invoiced { get; set; }

    }
}
