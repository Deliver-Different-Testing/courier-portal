namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class InvoiceLineDto
    {
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
