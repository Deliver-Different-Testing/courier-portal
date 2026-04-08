namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfInvitationResponseData
    {
        public string user_id { get; set; }
        public string client_id { get; set; }
        public string activation_code { get; set; }
        public string source { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string mobile_phone { get; set; }
        public string email { get; set; }
        public string invitation_id { get; set; }
        public string invitation_url { get; set; }
        public string master_account_id { get; set; }
        public string from_contractor_id { get; set; }
        public string external_id { get; set; }
        public string event_type { get; set; }
        public string enrolment_id { get; set; }
        public OpenforceIndividualData individual_data { get; set; }
    }
}
