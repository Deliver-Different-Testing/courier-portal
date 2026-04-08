using System;

namespace CourierPortal.Core.Domain.Ird.PaydayFiling
{
    public class Ded
    {
        public string Identifier { get; } = "DED";
        public string EmployeeIrdNo { get; set; }
        public string EmployeeName => $"{EmployeeFirstName?.Trim()} {EmployeeLastName?.Trim()}".Trim();
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public string KiwiSaverEligibility { get; } = "NE";
        public string KiwiSaverStatus { get; } = "NK";
        public string CountryCode { get; } = "NZL";
    }
}
