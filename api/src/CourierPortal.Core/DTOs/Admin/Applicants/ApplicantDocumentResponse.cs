using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantDocumentResponse : BaseResponse
    {
        public ApplicantDocumentResponse(Guid messageId) : base(messageId)
        {
        }

        public ApplicantDocumentDto Document { get; set; }
    }
}
