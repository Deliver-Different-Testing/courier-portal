using System;

namespace CourierPortal.Core.Domain.Ird.PaydayFiling
{
    public class Dei
    {
        public string Identifier { get; } = "DEI";
        public string EmployeeIrdNo { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeTaxCode { get; } = "WT"; //Scheduler Payments
        public DateTime PayPeriodStartDate { get; set; }
        public DateTime PayPeriodEndDate { get; set; }
        public string EmployeePayCycle { get; } = "WK"; //Weekly
        public decimal GrossEarningsAndOrSchedularPayments { get; set; }
        public decimal EarningsAndOrSchedularPaymentsNotLiableForAccEarnersLevy { get; set; }
        public decimal LumpSum { get; } = 0;
        public decimal PayeTax { get; set; }
        public decimal ChildSupportDeductions { get; } = 0;
        public decimal StudentLoanDeductions { get; } = 0;
        public decimal KiwiSaverDeductions { get; } = 0;
        public decimal NetKiwiSaverEmployerContributions { get; } = 0;
        public decimal EsctDeducted { get; } = 0;
        public decimal TaxCreditsForPayrollDonations { get; } = 0;
        public decimal FamilyTaxCredits { get; } = 0;

    }
}
