using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class TransactionLineDto
    {
        public long? Id { get; set; }
        public NameIdDto Type { get; set; }
        public string Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal Total { get; set; }
        public IEnumerable<TransactionLineJobDto> Jobs { get; set; }
    }
}
