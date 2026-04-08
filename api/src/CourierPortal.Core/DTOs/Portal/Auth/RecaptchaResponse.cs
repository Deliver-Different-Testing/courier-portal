using System.Text.Json.Serialization;

namespace CourierPortal.Core.DTOs.Portal.Auth
{
    public class RecaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    
        [JsonPropertyName("score")]
        public float Score { get; set; }
    
        [JsonPropertyName("action")]
        public string Action { get; set; }
    
        [JsonPropertyName("error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
