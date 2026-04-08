using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class InvoiceDto
    {
        public string InvoiceNo { get; set; }
        public DateTime Created { get; set; }
        public string TaxNo { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal GstAmount { get; set; }
        public decimal WithholdingTaxPercentage { get; set; }
        public decimal WithholdingTaxAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string Reference { get; set; }
        public string ToAddress { get; set; }
        public IEnumerable<InvoiceLineDto> Lines { get; set; }
    }
}
