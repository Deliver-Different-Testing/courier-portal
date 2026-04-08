using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Messages
{
    public class CourierMessagesResponse : BaseResponse
    {
        public CourierMessagesResponse(Guid messageId) : base(messageId)
        {
        }

        public CourierDto Courier { get; set; }
        public bool LoggedIn { get; set; }
        public IEnumerable<CourierMessageDto> CourierMessages { get; set; }
    }
}
