using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantsSummaryByLocationDto
    {
        public string Location { get; set; }
        public IEnumerable<ApplicantDto> Applicants { get; set; }
    }
}
