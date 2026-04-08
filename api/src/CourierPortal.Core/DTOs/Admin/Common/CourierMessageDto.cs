using System;

namespace CourierPortal.Core.DTOs.Admin.Common
{
    public class CourierMessageDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int Type { get; set; }
        public string Message { get; set; }
        public bool Sent { get; set; }
        public bool Read { get; set; }
    }
}
