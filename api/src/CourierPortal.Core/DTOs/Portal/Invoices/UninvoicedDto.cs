using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Runs;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class UninvoicedDto
    {
        public IEnumerable<RunDto> Runs { get; set; }
        public int? MasterId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal GstAmount { get; set; }
        public decimal WithholdingTaxPercentage { get; set; }
        public decimal WithholdingTaxAmount { get; set; }
        public decimal Total { get; set; }
    }
}
