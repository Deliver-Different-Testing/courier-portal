using System;

namespace CourierPortal.Core.DTOs.Admin.Common
{
    public class BaseRequest
    {
        public BaseRequest()
        {
            MessageId = Guid.NewGuid();
        }

        public Guid MessageId { get; set; }

        public object Data { get; set; }
    }
}
