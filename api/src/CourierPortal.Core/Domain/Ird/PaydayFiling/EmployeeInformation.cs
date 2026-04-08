using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Common;
using Microsoft.Extensions.Configuration;

namespace CourierPortal.Core.Domain.Ird.PaydayFiling
{
    public class EmployeeInformation
    {

        //private readonly char[] _specialChars = new char[] { ',', '\n', '\r', '"' };
        private readonly char[] _specialChars = new char[] { ',', '\\', '[', ']', '"', '\n', '\r' };

        public EmployeeInformation()
        {

        }

        #region "HEI header record Fields"

        public string Identifier { get; } = "HEI2";
        public string EmployerIrdNo => Environment.GetEnvironmentVariable("UrgentIrdNo");
        public DateTime PayDate { get; set; }
        public bool FinalReturn { get; } = false;
        public bool NillReturn { get; } = false;
        public string PayrollContactName => Environment.GetEnvironmentVariable("PayrollContactName"); //First name followed by surname 
        public string PayrollContactPhone => Environment.GetEnvironmentVariable("PayrollContactPhone");
        public string PayrollContactEmail => Environment.GetEnvironmentVariable("PayrollContactEmail");
        public int TotalEmployeeLines => Lines == null? 0 : Lines.Count();
        public decimal TotalPayeTax => Lines == null ? 0 : Lines.Sum(x => x.PayeTax);
        public decimal TotalChildSupportDeductions => Lines == null ? 0 : Lines.Sum(x => x.ChildSupportDeductions);
        public decimal TotalStudentLoanDeductions => Lines == null ? 0 : Lines.Sum(x => x.StudentLoanDeductions);
        public decimal TotalKiwiSaverDeductions => Lines == null ? 0 : Lines.Sum(x => x.KiwiSaverDeductions);
        public decimal TotalNetKiwiSaverEmployerContributions => Lines == null ? 0 : Lines.Sum(x => x.NetKiwiSaverEmployerContributions);
        public decimal TotalEsctDeducted => Lines == null ? 0 : Lines.Sum(x => x.EsctDeducted);
        public decimal TotalAmountDeducted => TotalPayeTax - TotalTaxCreditsForPayrollDonations + TotalChildSupportDeductions + TotalStudentLoanDeductions + TotalKiwiSaverDeductions + TotalNetKiwiSaverEmployerContributions + TotalEsctDeducted;
        public decimal TotalTaxCreditsForPayrollDonations => Lines == null ? 0 : Lines.Sum(x => x.TaxCreditsForPayrollDonations);
        public decimal TotalFamilyTaxCredits => Lines == null ? 0 : Lines.Sum(x => x.FamilyTaxCredits);
        public decimal TotalGrossEarnings => Lines == null ? 0 : Lines.Sum(x => x.GrossEarningsAndOrSchedularPayments);
        public decimal TotalEarningsNotLiableForAccEarnersLevy => Lines == null ? 0 : Lines.Sum(x => x.EarningsAndOrSchedularPaymentsNotLiableForAccEarnersLevy);
        public string PayrollPackageAndVersionNoIdentifier => Environment.GetEnvironmentVariable("PayrollPackageAndVersionNoIdentifier");
        public string IrFormVersionNumber { get; } = "0001";

        #endregion

        public List<Dei> Lines { get; } = new List<Dei>();

        private string EscapeAndFormat(object obj)
        {
            if (obj == null)
                return string.Empty;

            string field;

            //Format decimal value for upload if required PDEC(in cents), e.g. 123.59 = 12359
            if (obj is decimal)
            {
                decimal value = ((decimal)obj);
                if (value == 0)
                    field = "0";
                else
                    field = value.ToString("f2").Replace(".", string.Empty);
            }
            else if (obj is string)
            {
                field = (string)obj;
                while (field.IndexOfAny(_specialChars) != -1)
                {
                    field = field.Remove(field.IndexOfAny(_specialChars), 1);
                }
            }
            else if (obj is DateTime)
                field = ((DateTime)obj).ToString("yyyyMMdd");
            else
                field = obj.ToString();

            //// If any special CSV characters then delimit the entire field with quotes and replace embedded quotes with "".
            //if (field.IndexOfAny(_specialChars) != -1)
            //    return String.Format("\"{0}\"", field.Replace("\"", "\"\""));

            //Trim any leading or trailing whitespace
            field = field.Trim();

            return field;
        }

        public bool IsValid(List<MessageDto> messages = null)
        {
            //TODO: Add more validations
            bool result = true;

            if (Lines != null)
            {
                foreach (var line in Lines)
                {
                    if (string.IsNullOrWhiteSpace(line.EmployeeName))
                    {
                        messages?.Add(new MessageDto() { Message = $"Employee name is required." });
                        result = false;
                    }

                    //DB currently doesn't have proper formatting and validations on IRD numbers
                    //Check if there is enough digits for a IRD number, ranges from 10-000-000 to 999-999-999.
                    string irdGstNoDigitsOnly = string.IsNullOrWhiteSpace(line.EmployeeIrdNo) ? string.Empty : new string(line.EmployeeIrdNo.Where(char.IsDigit).ToArray());
                    if (irdGstNoDigitsOnly.Length != 8 && irdGstNoDigitsOnly.Length != 9)
                    {
                        messages?.Add(new MessageDto() { Message = $"Courier '{line.EmployeeName}' has an invalid IRD number." });
                        result = false;
                    }
                }
            }

            return result;
        }

        public byte[] ToCsv()
        {
            if (!IsValid())
                throw new Exception("Invalid header or lines");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    string formattedHeader = $"{EscapeAndFormat(Identifier)},{EscapeAndFormat(new string(EmployerIrdNo.Where(char.IsDigit).ToArray()).PadLeft(9, '0'))},{EscapeAndFormat(PayDate)}," +
                        $"{EscapeAndFormat(FinalReturn? "Y" : "N")},{EscapeAndFormat(NillReturn ? "Y" : "N")}," +
                        $",{EscapeAndFormat(PayrollContactName.Length > 20 ? PayrollContactName.Substring(0, 20) : PayrollContactName)},{EscapeAndFormat(PayrollContactPhone)},{EscapeAndFormat(PayrollContactEmail)}," +
                        $"{EscapeAndFormat(TotalEmployeeLines)},{EscapeAndFormat(TotalGrossEarnings)},0,0,{EscapeAndFormat(TotalPayeTax)},0,{EscapeAndFormat(TotalChildSupportDeductions)}," +
                        $"{EscapeAndFormat(TotalStudentLoanDeductions)},0,0,{EscapeAndFormat(TotalKiwiSaverDeductions)},{EscapeAndFormat(TotalNetKiwiSaverEmployerContributions)}," +
                        $"{EscapeAndFormat(TotalEsctDeducted)},{EscapeAndFormat(TotalAmountDeducted)},{EscapeAndFormat(TotalTaxCreditsForPayrollDonations)},{EscapeAndFormat(TotalFamilyTaxCredits)}," +
                        $"0,{EscapeAndFormat(PayrollPackageAndVersionNoIdentifier)},{EscapeAndFormat(IrFormVersionNumber)}";
                    streamWriter.WriteLine(formattedHeader);

                    if (Lines != null)
                    {
                        foreach (var line in Lines)
                        {
                            string formattedLine = $"{EscapeAndFormat(line.Identifier)},{EscapeAndFormat(new string(line.EmployeeIrdNo.Where(char.IsDigit).ToArray()).PadLeft(9, '0'))},{EscapeAndFormat(line.EmployeeName.Length > 20 ? line.EmployeeName.Substring(0, 20) : line.EmployeeName)},{EscapeAndFormat(line.EmployeeTaxCode)}," +
                                $",,{EscapeAndFormat(line.PayPeriodStartDate)},{EscapeAndFormat(line.PayPeriodEndDate)},{EscapeAndFormat(line.EmployeePayCycle)}," +
                                $"0,{EscapeAndFormat(line.GrossEarningsAndOrSchedularPayments)},0,{EscapeAndFormat(line.EarningsAndOrSchedularPaymentsNotLiableForAccEarnersLevy)}," +
                                $"{EscapeAndFormat(line.LumpSum)},{EscapeAndFormat(line.PayeTax)},0,{EscapeAndFormat(line.ChildSupportDeductions)},," +
                                $"{EscapeAndFormat(line.StudentLoanDeductions)},0,0,{EscapeAndFormat(line.KiwiSaverDeductions)},{EscapeAndFormat(line.NetKiwiSaverEmployerContributions)}," +
                                $"{EscapeAndFormat(line.EsctDeducted)},{EscapeAndFormat(line.TaxCreditsForPayrollDonations)},{EscapeAndFormat(line.FamilyTaxCredits)},0";
                            streamWriter.WriteLine(formattedLine);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}
