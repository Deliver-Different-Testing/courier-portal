using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantsSearchResponse : BaseResponse
    {
        public ApplicantsSearchResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<ApplicantSearchDto> Applicants { get; set; }
    }
}
