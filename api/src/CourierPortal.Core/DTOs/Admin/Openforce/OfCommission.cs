using System;
using Newtonsoft.Json;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfCommission : OfTransaction
    {
        public int commission_type_id { get; set; }
    }
}
