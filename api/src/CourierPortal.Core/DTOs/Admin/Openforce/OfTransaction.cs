using Newtonsoft.Json;
using System;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public abstract class OfTransaction : OfEntity
    {
        public string settlement_id { get; set; }
        public string contractor_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? custom_date_field1 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? custom_date_field2 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? custom_date_field3 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? custom_date_field4 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? custom_date_field5 { get; set; }
        public decimal amount { get; set; }
        public string external_id { get; set; }
        public string description { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field1 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field2 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field3 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field4 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field5 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field6 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field7 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field8 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field9 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field10 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field11 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string custom_string_field12 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? stm_client_id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string contractor_external_id { get; set; }
        public bool use_contractor_external_id { get; set; }
    }
}
