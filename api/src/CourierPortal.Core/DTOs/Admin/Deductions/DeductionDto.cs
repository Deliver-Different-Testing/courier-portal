

using CourierPortal.Core.DTOs.Admin.Invoices;

namespace CourierPortal.Core.DTOs.Admin.Deductions
{
    public class DeductionDto : TransactionDto
    {
        public string InvoiceNumber { get; set; }
    }
}
