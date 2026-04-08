using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class UninvoicedJobsResponse : BaseResponse
    {
        public UninvoicedJobsResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<UninvoicedJobDto> Jobs { get; set; }
    }
}
