using Newtonsoft.Json;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public abstract class OfEntity
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }
    }
}
