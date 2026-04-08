using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Common
{
    public class MessageDto
    {
        public MessageDto()
        {
            Params = new List<object>();
        }
        //public MessageCode Code { get; set; }
        public string Message { get; set; }

        public List<object> Params { get; set; }
    }
}
