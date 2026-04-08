using System;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class DeclarationDto
    {
        public string Text { get; set; }
        public DateTime? Date { get; set; }
        public bool Agree { get; set; }
        public string Name { get; set; }
        public string Signature { get; set; }
    }
}
