using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementsRecentResponse : BaseResponse
    {
        public InfringementsRecentResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<InfringementDto> Infringements { get; set; }

        public int Total { get; set; }

        public IEnumerable<InfringementDto> CancelledInfringements { get; set; }

        public int Cancelled { get; set; }

        public IEnumerable<InfringementSummaryDto> CourierSummaries { get; set; }

        public IEnumerable<InfringementSummaryDto> CategorySummaries { get; set; }

        public IEnumerable<InfringementSummaryDto> SeveritySummaries { get; set; }


    }
}
