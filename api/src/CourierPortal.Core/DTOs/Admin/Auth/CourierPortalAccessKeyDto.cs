using System;

namespace CourierPortal.Core.DTOs.Admin.Auth
{
    public class CourierPortalAccessKeyDto
    {
        public string AccessKey { get; set; }
        public DateTime Expiry { get; set; }
        public int Id { get; set; }
    }
}
