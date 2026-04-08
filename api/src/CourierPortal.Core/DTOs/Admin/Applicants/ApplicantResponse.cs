using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantResponse : BaseResponse
    {
        public ApplicantResponse(Guid messageId) : base(messageId)
        {
        }

        public ApplicantDto Applicant { get; set; }
    }
}
