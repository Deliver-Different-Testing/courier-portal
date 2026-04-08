using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Common;
using Microsoft.Extensions.Configuration;

namespace CourierPortal.Core.Domain.Ird.PaydayFiling
{
    public class EmployeeDetails
    {
    //    private readonly char[] _specialChars = new char[] { ',', '\n', '\r', '"' };
        private readonly char[] _specialChars = new char[] { ',', '\\', '[', ']', '"', '\n', '\r' };


        public EmployeeDetails()
        {

        }

        #region "HED header record Fields"

        public string Identifier { get; } = "HED2";
        public string EmployerIrdNo => Environment.GetEnvironmentVariable("UrgentIrdNo");
        public string PayrollPackageAndVersionNoIdentifier => Environment.GetEnvironmentVariable("PayrollPackageAndVersionNoIdentifier");
        public int TotalEmployeeLines => Lines == null ? 0 : Lines.Count();

        #endregion

        public List<EmployeeDetailsLine> Lines { get; } = new List<EmployeeDetailsLine>();

        public bool IsValid(List<MessageDto> messages = null)
        {
            //TODO: Add more validations
            bool result = true;

            if (Lines != null)
            {
                foreach (var line in Lines)
                {
                    if (line.Ded == null || line.Ted == null)
                    {
                        if (messages != null)
                            messages.Add(new MessageDto() { Message = $"Line requires a DED and TED record." });

                        result = false;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line.Ded.EmployeeName))
                    {
                        if (messages != null)
                            messages.Add(new MessageDto() { Message = $"Employee name is required." });

                        result = false;
                    }

                    //DB currently doesn't have proper formatting and validations on IRD numbers
                    //Check if there is enough digits for a IRD number, ranges from 10-000-000 to 999-999-999.
                    string irdGstNoDigitsOnly = string.IsNullOrWhiteSpace(line.Ded.EmployeeIrdNo) ? string.Empty : new string(line.Ded.EmployeeIrdNo.Where(c => char.IsDigit(c)).ToArray());
                    if (irdGstNoDigitsOnly.Length != 8 && irdGstNoDigitsOnly.Length != 9)
                    {
                        if (messages != null)
                            messages.Add(new MessageDto() { Message = $"Employee '{line.Ded.EmployeeName}' has an invalid IRD number." });

                        result = false;
                    }
                }
            }

            return result;
        }

        private string EscapeAndFormat(object obj)
        {
            if (obj == null)
                return string.Empty;

            string field;

            if (obj is string)
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

        public byte[] ToCsv()
        {
            if (!IsValid())
                throw new Exception("Invalid header or lines");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    string formattedHeader = string.Format("{0},{1},{2},{3}",
                        EscapeAndFormat(Identifier),
                        EscapeAndFormat(new string(EmployerIrdNo.Where(char.IsDigit).ToArray()).PadLeft(9, '0')),
                        EscapeAndFormat(PayrollPackageAndVersionNoIdentifier),
                        EscapeAndFormat(TotalEmployeeLines));

                    streamWriter.WriteLine(formattedHeader);

                    if (Lines != null)
                    {
                        foreach (var line in Lines)
                        {
                            string formattedDed = $"{EscapeAndFormat(line.Ded.Identifier)},{EscapeAndFormat(new string(line.Ded.EmployeeIrdNo.Where(c => char.IsDigit(c)).ToArray()).PadLeft(9, '0'))}," +
                                $"{EscapeAndFormat(line.Ded.EmployeeName.Length > 255 ? line.Ded.EmployeeName.Substring(0, 255) : line.Ded.EmployeeName)},,{EscapeAndFormat(line.Ded.EmployeeFirstName.Length > 50 ? line.Ded.EmployeeFirstName.Substring(0, 50) : line.Ded.EmployeeFirstName)},,{EscapeAndFormat(line.Ded.EmployeeLastName.Length > 50 ? line.Ded.EmployeeLastName.Substring(0, 50) : line.Ded.EmployeeLastName)},,{EscapeAndFormat(line.Ded.EmploymentStartDate)}," +
                                $",{EscapeAndFormat(line.Ded.KiwiSaverEligibility)},{EscapeAndFormat(line.Ded.KiwiSaverStatus)},,,,,,,,,{EscapeAndFormat(line.Ded.CountryCode)},,,,,,,,,,,,,,,,,,,,";
                            streamWriter.WriteLine(formattedDed);

                            string formattedTed = $"{EscapeAndFormat(line.Ted.Identifier)},{EscapeAndFormat(line.Ted.EmployeeTaxCode)}";
                            streamWriter.WriteLine(formattedTed);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}

