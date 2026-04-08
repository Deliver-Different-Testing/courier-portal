using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class InvoiceBatchDto
    {
        public string BatchNo { get; set; }
        public DateTime Created { get; set; }
        public DateTime PayPeriodStartDate { get; set; }
        public DateTime PayPeriodEndDate { get; set; }
        public string Status { get; set; }
        public decimal GrossTaxable { get; set; }
        public decimal NetPayment { get; set; }
        public decimal WithholdingTax { get; set; }
        public decimal Gst { get; set; }
        public decimal GstExclusive { get; set; }
        public decimal GstInclusive { get; set; }
        public IEnumerable<TransactionDto> Invoices { get; set; }
    }
}
