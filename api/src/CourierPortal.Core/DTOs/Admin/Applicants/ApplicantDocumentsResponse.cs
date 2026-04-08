using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantDocumentsResponse : BaseResponse
    {
        public ApplicantDocumentsResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<ApplicantDocumentDto> Documents { get; set; }
    }
}
