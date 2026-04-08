using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class UploadsResponse : BaseResponse
    {
        public UploadsResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<UploadDto> Uploads { get; set; }
    }
}
