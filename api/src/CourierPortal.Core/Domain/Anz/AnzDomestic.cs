using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.Domain.Anz
{
    public class AnzDomestic
    {
        private static readonly char[] _bankSpecialChars = new char[] { '[', ']', '{', '}', '\\', '|', '`', '~', '^', ',', '\n', '\r', '"' };
        //private static char[] _specialChars = new char[] { ',', '\n', '\r', '"' };

        //private static char[] _bankSpecialChars = new char[] { '[', ']', '{', '}', '\\', '|', '`', '~', '^' };

        public AnzDomestic()
        {
            Lines = new List<AnzDomesticLine>();
        }

        public List<AnzDomesticLine> Lines { get; }

        private string Escape(object obj)
        {
            if (obj == null)
                return string.Empty;

            string field;

            if (obj is decimal)
                field = ((decimal)obj).ToString("f2");
            else
                field = obj.ToString();

            //Remove special characters not permitted by bank (ANZ)
            while (field.IndexOfAny(_bankSpecialChars) != -1)
                field = field.Replace(field[field.IndexOfAny(_bankSpecialChars)].ToString(), " ");

            // If any special CSV characters then delimit the entire field with quotes and replace embedded quotes with "".
            //if (field.IndexOfAny(_specialChars) != -1)
            //    return String.Format("\"{0}\"", field.Replace("\"", "\"\""));

            //Trim any white space
            field = field.Trim();

            return field;
        }

        public bool IsValid(List<MessageDto> messages = null)
        {
            bool result = true;

            if (Lines != null)
            {
                foreach (var line in Lines)
                {
                    if (line.TransactionAmount <= 0.00m)
                    {
                        messages?.Add(new MessageDto() { Message = $"Transaction amount must be greater than '$0.00'." });
                        result = false;
                    }

                    if (string.IsNullOrWhiteSpace(Escape(line.OtherPartyName)))
                    {
                        messages?.Add(new MessageDto() { Message = $"Other Party Name is required." });
                        result = false;
                    }
                    //DB currently doesn't have proper formatting and validations on bank account numbers
                    //Check if there is enough digits for a NZ bank account number
                    //NZ Bank Account Format: Bank(2) Branch(4) Account(7) Suffix(2 or 3)
                    string bankAccountNoDigitsOnly = string.IsNullOrWhiteSpace(line.AccountNo) ? string.Empty : new string(line.AccountNo.Where(c => char.IsDigit(c)).ToArray());
                    if (bankAccountNoDigitsOnly.Length != 15 && bankAccountNoDigitsOnly.Length != 16)
                    {
                        messages?.Add(new MessageDto() { Message = $"Other party name '{line.OtherPartyName}' has an invalid bank account number." });
                        result = false;
                    }
                }
            }

            return result;
        }

        public byte[] ToCsv()
        {
            if (!IsValid())
                throw new Exception("Invalid lines");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    if (Lines != null)
                    {
                        foreach (var line in Lines)
                        {

                            //Format bank number for ANZ upload, ANZ upload also requires a 8 digit account number instead of 7 digits.
                            string bankNoDigitsOnly = new string(line.AccountNo.Where(c => char.IsDigit(c)).ToArray());
                            string bankNo = bankNoDigitsOnly.Substring(0, 6) + "0" + bankNoDigitsOnly.Substring(6, 7) + bankNoDigitsOnly.Substring(13).PadLeft(3, '0');

                            string formattedLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                    Escape(line.TransactionAmount),
                                    Escape(bankNo),
                                    Escape(line.OtherPartyName.Length > 20 ? line.OtherPartyName.Substring(0, 20) : line.OtherPartyName),
                                    Escape(line.OriginatorReference.Length > 12 ? line.OriginatorReference.Substring(0, 12) : line.OriginatorReference),
                                    Escape(line.OriginatorAnalysisCode.Length > 12 ? line.OriginatorAnalysisCode.Substring(0, 12) : line.OriginatorAnalysisCode),
                                    Escape(line.OriginatorParticulators.Length > 12 ? line.OriginatorParticulators.Substring(0, 12) : line.OriginatorParticulators),
                                    Escape(line.OtherPartyReference.Length > 12 ? line.OtherPartyReference.Substring(0, 12) : line.OtherPartyReference),
                                    Escape(line.OtherPartyAnalysisCode.Length > 12 ? line.OtherPartyAnalysisCode.Substring(0, 12) : line.OtherPartyAnalysisCode),
                                    Escape(line.OtherPartyParticulars.Length > 12 ? line.OtherPartyParticulars.Substring(0, 12) : line.OtherPartyParticulars)
                                );
                            streamWriter.WriteLine(formattedLine);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}
