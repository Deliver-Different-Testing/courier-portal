using System;
using System.Collections;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfContractorContracted
    {
        public string id { get; set; }
        public int contractor_numeric_id { get; set; }
        public string account_id { get; set; }
        public string client_id { get; set; }
        public string of_id { get; set; }
        public string contract_id { get; set; }
        public string contractor_guid { get; set; }
        public string enrollment_id { get; set; }
        public string company_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string external_id { get; set; }
        public DateTime? date_of_birth { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string postal_code { get; set; }
        public string department_of_transportation_number { get; set; }
        public string motor_carrier_number { get; set; }
        public string profile_photo_uri { get; set; }
        public string drivers_license_number { get; set; }
        public string drivers_license_state { get; set; }
        public string drivers_license_expiration_date { get; set; }
        public string termination_date { get; set; }
        public string contracted_on { get; set; }
        public string location { get; set; }
        public string insured_id { get; set; }
        public string expected_termination_date { get; set; }
        public string tin { get; set; }
        public int status { get; set; }
        public OfPhone[] phones { get; set; }
        public object[] deposit_options { get; set; }
        public OfEnrolmentData enrollment_data { get; set; }
    }
}
