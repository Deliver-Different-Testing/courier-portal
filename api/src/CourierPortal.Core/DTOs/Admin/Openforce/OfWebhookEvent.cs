namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfWebhookEvent
    {
        public string event_type { get; set; }
        public object data { get; set; }
    }
}
