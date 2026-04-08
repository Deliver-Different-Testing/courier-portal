using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Common
{
    public class BaseResponse
    {
        public BaseResponse(Guid messageId)
        {
            MessageId = messageId;
            Messages = new List<MessageDto>();
        }
        public Guid MessageId { get; }

        public object Data  { get; set; }

        public bool Success { get; set; }
        public List<MessageDto> Messages { get; set; }
    }
}
