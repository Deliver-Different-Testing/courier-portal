using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Reports
{
    public class ReportSettingsDto
    {
        public IEnumerable<ReportDto> Reports { get; set; }
        //public IEnumerable<string> Periods { get; set; }
    }
}
