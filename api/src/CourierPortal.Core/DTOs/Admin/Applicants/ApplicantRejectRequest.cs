using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantRejectRequest : IdentifierRequest
    {
        public string Reason { get; set; }
        public IEnumerable<int> ClearDocuments { get; set; }
    }
}
