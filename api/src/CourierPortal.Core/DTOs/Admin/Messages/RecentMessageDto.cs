using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Messages
{
    public class RecentMessagesDto
    {
        public CourierDetailsDto Courier { get; set; }
        public bool LoggedIn { get; set; }
        public DateTime LastMessage { get; set; }
        public IEnumerable<CourierMessageDto> CourierMessages { get; set; }
    }
}
