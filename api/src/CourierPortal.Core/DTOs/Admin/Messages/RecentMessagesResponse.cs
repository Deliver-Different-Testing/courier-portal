using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Messages
{
    public class RecentMessagesResponse : BaseResponse
    {
        public RecentMessagesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<RecentMessagesDto> RecentMessages { get; set; }
    }
}
