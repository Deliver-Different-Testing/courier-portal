using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Common
{
    public class BaseResponse
    {
        public BaseResponse(Guid messageId)
        {
            MessageId = messageId;
            Messages = new List<MessageDto>();
        }
        public BaseResponse()
        {
            MessageId = Guid.NewGuid();
            Messages = new List<MessageDto>();
        }
        public object Data { get; set; }
        public Guid MessageId { get; }
        public bool Success { get; set; }
        public List<MessageDto> Messages { get; set; }
    }
}
