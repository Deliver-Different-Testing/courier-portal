using System;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfContractor : OfEntity
    {
        public string of_id { get; set; }
        public string company_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
    }
}
