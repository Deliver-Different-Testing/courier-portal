namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfInvitation
    {
        public string master_account_id { get; set; }
        public string activation_code { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string mobile_phone { get; set; }
        public string email { get; set; }
        public bool send_notification { get; set; } = true;
        public OpenforceIndividualData individual_data { get; set; }

    }
}
