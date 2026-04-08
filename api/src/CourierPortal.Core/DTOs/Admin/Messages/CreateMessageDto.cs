namespace CourierPortal.Core.DTOs.Admin.Messages
{
    public class CreateMessageDto
    {
        public int CourierId { get; set; }
        public int Type { get; set; }
        public string Message { get; set; }
    }
}
