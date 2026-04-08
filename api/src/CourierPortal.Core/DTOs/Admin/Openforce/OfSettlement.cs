using System;
using Newtonsoft.Json;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfSettlement : OfEntity
    {
        public DateTime requested_disbursement_date { get; set; }
        public long? invoice_id { get; set; }
        public string external_id { get; set; }
        public int status { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public string source_file_uri { get; set; }
        public Int32? stm_client_id { get; set; }
    }
}
