using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class DocumentsResponse : BaseResponse
    {
        public DocumentsResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<DocumentDto> Documents { get; set; }
    }
}
