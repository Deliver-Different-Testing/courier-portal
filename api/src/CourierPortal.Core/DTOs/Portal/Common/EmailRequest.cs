using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Common
{
    public class EmailRequest : BaseRequest
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public bool IsBodyHtml { get; set; } = false;
        public string Message { get; set; }
        public IEnumerable<EmailLinkedResourceDto> LinkedResources { get; set; }
        public IEnumerable<EmailAttachmentDto> Attachments { get; set; }
    }
}
