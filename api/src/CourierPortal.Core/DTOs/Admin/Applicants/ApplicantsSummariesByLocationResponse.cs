using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantsSummariesByLocationResponse : BaseResponse
    {
        public ApplicantsSummariesByLocationResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<ApplicantsSummaryByLocationDto> Summaries { get; set; }
    }
}
