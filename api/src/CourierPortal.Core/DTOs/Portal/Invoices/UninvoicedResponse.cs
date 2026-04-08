using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Invoices
{
    public class UninvoicedResponse : BaseResponse
    {
        public UninvoicedResponse(Guid messageId) : base(messageId)
        {
        }

        public string ToAddress { get; set; }

        public UninvoicedDto Courier { get; set; }

        public List<UninvoicedDto> Masters { get; set; } = new List<UninvoicedDto>();
    }
}
