using System;

namespace CourierPortal.Core.DTOs.Portal.Common
{
    public class BaseRequest
    {
        public BaseRequest()
        {
            MessageId = Guid.NewGuid();
        }

        public Guid MessageId { get; set; }
    }
}
