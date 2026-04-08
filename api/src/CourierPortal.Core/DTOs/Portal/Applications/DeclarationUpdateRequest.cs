using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class DeclarationUpdateRequest : BaseRequest
    {
        public string Text { get; set; }
        public bool Agree { get; set; }
        public string Name { get; set; }
        public string Signature { get; set; }
    }
}
