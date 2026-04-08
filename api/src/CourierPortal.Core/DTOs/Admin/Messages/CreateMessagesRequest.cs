using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Messages
{
    public class CreateMessagesRequest : BaseRequest
    {
        public IEnumerable<CreateMessageDto> Messages { get; set; }
    }
}
