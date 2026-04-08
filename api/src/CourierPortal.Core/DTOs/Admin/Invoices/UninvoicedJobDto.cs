using System;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class UninvoicedJobDto
    {
        public int JobId { get; set; }
        public DateTime Date { get; set; }
        public string ParentJobNumber { get; set; }
        public string JobNumber { get; set; }
        public string RunName { get; set; }
        public string ClientCode { get; set; }
        public int CourierId { get; set; }
        public string CourierCode { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public bool Reprice { get; set; }
        public DateTime? CompletedTime { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public bool JobDone { get; set; }
        public decimal CourierPayment { get; set; }
        public decimal CourierFuel { get; set; }
    }
}
