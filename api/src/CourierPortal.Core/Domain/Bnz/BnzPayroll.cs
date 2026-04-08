using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using CourierPortal.Core.DTOs.Admin.Common;
using Microsoft.Extensions.Configuration;

namespace CourierPortal.Core.Domain.Bnz
{
    public class BnzPayroll
    {
        public decimal TransactionAmountTotal => Transactions?.Sum(t => Math.Round(t.TransactionAmount, 2, MidpointRounding.AwayFromZero)) ?? 0;
        public int TransactionRecordCount => Transactions?.Count() ?? 0;

        public DateTime? DueDate { get; set; }
        public List<BnzPayrollTransaction> Transactions { get; set; } = new();

        private string Escape(string value, int maxLength = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string formattedValue = value.Trim();

            //Remove any characters not permitted by bank (BNZ)
            for (int i = formattedValue.Length - 1; i >= 0; i--)
            {
                if ("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ /-?:().'+".IndexOf(char.ToUpper(formattedValue[i])) == -1)
                    formattedValue = formattedValue.Remove(i, 1).Insert(i, " ");
            }

            //Trim any white space
            formattedValue = formattedValue.Trim();

            formattedValue = maxLength > 0 && formattedValue.Length > maxLength ? formattedValue.Substring(0, maxLength) : formattedValue;

            return formattedValue;
        }

        public bool IsValid(List<MessageDto> messages = null)
        {
            if (Transactions == null)
                return true;

            bool result = true;

            if (!DueDate.HasValue)
            {
                messages?.Add(new MessageDto() { Message = $"Due Date is required." });
                result = false;
            }

            if (TransactionRecordCount > 99998)
            {
                messages?.Add(new MessageDto() { Message = $"Maximum Transactions allowed for bulk is 99,998." });
                result = false;
            }

            if (Math.Round(TransactionAmountTotal, 2, MidpointRounding.AwayFromZero) >= 10000000000.00m)
            {
                messages?.Add(new MessageDto() { Message = $"Total transaction amount must be less than $10,000,000,000.00" });
                result = false;
            }

            foreach (var transaction in Transactions)
            {
                decimal roundedTransactionAmount = Math.Round(transaction.TransactionAmount, 2, MidpointRounding.AwayFromZero);
                if (roundedTransactionAmount <= 0.00m || roundedTransactionAmount >= 10000000000.00m)
                {
                    messages?.Add(new MessageDto() { Message = $"Transaction amount must be greater than $0.00 and less than $10,000,000,000.00" });
                    result = false;
                }

                if (string.IsNullOrWhiteSpace(Escape(transaction.OtherPartyName)))
                {
                    messages?.Add(new MessageDto() { Message = $"Other Party Name is required." });
                    result = false;
                }

                if (string.IsNullOrWhiteSpace(Escape(transaction.YourName)))
                {
                    messages?.Add(new MessageDto() { Message = $"Your Name is required." });
                    result = false;
                }

                //DB currently doesn't have proper formatting and validations on bank account numbers
                //Check if there is enough digits for a NZ bank account number
                //NZ Bank Account Format: Bank(2) Branch(4) Account(7) Suffix(2 or 3)
                string bankAccountNoDigitsOnly = string.IsNullOrWhiteSpace(transaction.AccountNo) ? string.Empty : new string(transaction.AccountNo.Where(char.IsDigit).ToArray());
                if (bankAccountNoDigitsOnly.Length != 15 && bankAccountNoDigitsOnly.Length != 16)
                {
                    messages?.Add(new MessageDto() { Message = $"Other party name '{transaction.OtherPartyName}' has an invalid bank account number." });
                    result = false;
                }
            }

            return result;
        }

        public byte[] ToCsv()
        {
            if (!IsValid())
                throw new Exception("Invalid Transactions");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    //Write Header Record
                    streamWriter.WriteLine($"1,,,,{Environment.GetEnvironmentVariable("UrgentBankBnzAccountNo")},7,{DueDate:yyMMdd},{DateTime.Now:yyMMdd},");

                    //Write Transaction Record(s) and calculate hash total for Control Record
                    BigInteger hashTotal = 0;
                    foreach (var item in Transactions)
                    {
                        //Format bank account number.
                        string bankNoDigitsOnly = new string(item.AccountNo.Where(char.IsDigit).ToArray());
                        string bankNo = bankNoDigitsOnly.Substring(0, 13) + bankNoDigitsOnly.Substring(13).PadLeft(3, '0');

                        //Add to hash total
                        hashTotal += BigInteger.Parse(bankNo.Substring(2, 11));

                        //Write Transaction Record
                        streamWriter.WriteLine($"2,{bankNo},50,{Math.Round(item.TransactionAmount, 2, MidpointRounding.AwayFromZero) *100m:0}," +
                                               $"{Escape(item.OtherPartyName, 20)}," +
                                               $"{Escape(item.OtherPartyReference, 12)}," +
                                               $"{Escape(item.OtherPartyCode, 12)}," +
                                               "," +
                                               $"{Escape(item.OtherPartyParticulars, 12)}," +
                                               $"{Escape(item.YourName, 20)}," +
                                               $"{Escape(item.YourCode, 12)}," +
                                               $"{Escape(item.YourReference, 12)}," +
                                               $"{Escape(item.YourParticulars, 12)}");
                    }

                    //Format hash total for Control Record
                    string formattedHashTotal = hashTotal.ToString();
                    formattedHashTotal = formattedHashTotal.Length > 11
                                         ? formattedHashTotal.Substring(formattedHashTotal.Length - 11, 11)
                                         : formattedHashTotal.PadLeft(11, '0');

                    //Write Control Record
                    streamWriter.WriteLine($"3,{Math.Round(TransactionAmountTotal, 2, MidpointRounding.AwayFromZero) *100m:0},{TransactionRecordCount},{formattedHashTotal}");
                }

                return memoryStream.ToArray();
            }
        }
    }
}
