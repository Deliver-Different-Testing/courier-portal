
using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Deductions;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class TransactionDto
    {
        public string Number { get; set; }
        public DateTime Created { get; set; }
        public CourierDetailsDto Courier { get; set; }
        public string TaxNo { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal GstAmount { get; set; }
        public decimal WithholdingTaxPercentage { get; set; }
        public decimal WithholdingTaxAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public string Reference { get; set; }
        public string ExternalId { get; set; }
        public IEnumerable<TransactionLineDto> Lines { get; set; }
        public IEnumerable<DeductionDto> Deductions { get; set; }
    }
}
