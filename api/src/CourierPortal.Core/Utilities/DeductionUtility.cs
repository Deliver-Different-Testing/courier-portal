using System;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Deductions;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Domain.Entities;
using Serilog;

namespace CourierPortal.Core.Utilities
{
    public static class DeductionUtility
    {
        public static bool IsValidNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number) || !long.TryParse(number, out long id) || id <= 0)
                return false;

            return true;
        }

        public static long NumberToId(string number)
        {
            if (!IsValidNumber(number))
                throw new Exception($"Invalid Number of '{number}'");

            return long.Parse(number);
        }

        //public static string IdToNumber(long id)
        //{
        //    if (id <= 0)
        //        throw new Exception($"Invalid ID of '{id}'");

        //    return id.ToString().PadLeft(4, '0');
        //}

        

        public static DeductionDto MapToDto(CourierDeduction data)
        {
            // Add initial validation logging
            if (data == null)
            {
                Log.Error("MapToDto called with null CourierDeduction");
                return null; // Or return an empty DTO with error info
            }

            Log.Debug($"Processing deduction ID: {data.Id}, Lines count: {data.CourierDeductionLines?.Count ?? 0}");

            // Log detailed line information before processing
            if (data.CourierDeductionLines != null)
            {
                foreach (var line in data.CourierDeductionLines)
                {
                    Log.Debug($"Line ID: {line.Id}, " +
                                     $"DeductionType: {(line.DeductionType == null ? "NULL" : "OK")}, " +
                                     $"Description: {line.Description ?? "NULL"}, " +
                                     $"Quantity: {line.Quantity}, " +
                                     $"UnitPrice: {line.UnitPrice}");
                }
            }
            else
            {
                Log.Warning($"CourierDeductionLines is null for deduction ID: {data.Id}");
            }
            decimal subTotal = data.CourierDeductionLines.Sum(l => CalculateLineSubtotal(l.Quantity, l.UnitPrice));
            decimal gstAmount = data.CourierDeductionLines.Sum(CalculateLineGst);
            decimal withholdingTaxAmount = data.CourierDeductionLines.Sum(CalculateLineWithholdingTax);
            decimal total = subTotal + gstAmount - withholdingTaxAmount;

           
            return new DeductionDto()
            {
                Number = data.Id.ToString(),
                Created = data.Created,
                Courier = data.Courier == null ? null : CourierUtility.MapToCourierDto(data.Courier),
                TaxNo = data.Invoice?.TaxNo,
                GstPercentage = data.Invoice?.GstPercentage ?? 0,
                GstAmount = gstAmount,
                WithholdingTaxPercentage = data.Invoice?.WithholdingTaxPercentage ?? 0,
                WithholdingTaxAmount = withholdingTaxAmount,
                SubTotal = subTotal,
                Total = total,
                Reference = data.Reference,
                InvoiceNumber = data.InvoiceId.HasValue? data.InvoiceId.Value.ToString() : null,
                ExternalId = data.ExternalId,
                Lines = data.CourierDeductionLines.Select(line => 
                {
                    // Get type information safely
                    int typeId = line.DeductionType?.Id ?? line.DeductionTypeId;
                    string typeName = line.DeductionType?.Name ?? $"Type ID: {line.DeductionTypeId}";
            
                    return new TransactionLineDto()
                    {
                        Id = line.Id,
                        Type = new NameIdDto() { Id = typeId, Name = typeName },
                        Description = line.Description,
                        Quantity = line.Quantity,
                        UnitPrice = Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero),
                        Total = Math.Round(line.Quantity * Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero),
                    };
                }).ToList()
            };
        }

        public static DeductionRecurringDto MapToDto(CourierDeductionRecurring data)
        {
            // Add initial validation logging
            if (data == null)
            {
                Log.Error("MapToDto called with null CourierDeductionRecurring");
                return null; // Or return an empty DTO with error info
            }

            Log.Debug($"Processing recurring deduction ID: {data.Id}, Lines count: {data.CourierDeductionRecurringLines?.Count ?? 0}");

            // Log detailed line information before processing
            if (data.CourierDeductionRecurringLines != null)
            {
                foreach (var line in data.CourierDeductionRecurringLines)
                {
                    Log.Debug($"Line ID: {line.Id}, " +
                                     $"DeductionType: {(line.DeductionType == null ? "NULL" : "OK")}, " +
                                     $"Description: {line.Description ?? "NULL"}, " +
                                     $"Quantity: {line.Quantity}, " +
                                     $"UnitPrice: {line.UnitPrice}");
                }
            }
            else
            {
                Log.Warning($"CourierDeductioRecurringnLines is null for recurring deduction ID: {data.Id}");
            }
            decimal subTotal = data.CourierDeductionRecurringLines.Sum(l => CalculateLineSubtotal(l.Quantity, l.UnitPrice));
            decimal total = subTotal;


            return new DeductionRecurringDto()
            {
                Number = data.Id.ToString(),
                Created = data.Created,
                RecurringType = data.RecurringType,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                Day = data.DayValue,
                Paused = data.Paused,
                LastProcessed = data.LastProcessed,
                Courier = data.Courier == null ? null : CourierUtility.MapToCourierDto(data.Courier),
                SubTotal = subTotal,
                Total = total,
                Reference = data.Reference,
                Lines = data.CourierDeductionRecurringLines.Select(line =>
                {
                    // Get type information safely
                    int typeId = line.DeductionType?.Id ?? line.DeductionTypeId;
                    string typeName = line.DeductionType?.Name ?? $"Type ID: {line.DeductionTypeId}";

                    return new TransactionLineDto()
                    {
                        Id = line.Id,
                        Type = new NameIdDto() { Id = typeId, Name = typeName },
                        Description = line.Description,
                        Quantity = line.Quantity,
                        UnitPrice = Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero),
                        Total = Math.Round(line.Quantity * Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero),
                    };
                }).ToList()
            };
        }

        public static DeductionTypeDto MapToDto(CourierDeductionType data)
        {
            return new DeductionTypeDto()
            {
                Id = data.Id,
                Name = data.Name,
                ExternalId = data.ExternalId
            };
        }

        private static decimal CalculateLineSubtotal(decimal quantity, decimal unitPrice)
        {
            return Math.Round(
                quantity * Math.Round(unitPrice, 2, MidpointRounding.AwayFromZero)
            , 2, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateLineGst(CourierDeductionLine line)
        {
            return Math.Round(
                (line.Deduction.Invoice?.GstPercentage ?? 0) 
                * 
                CalculateLineSubtotal(line.Quantity, line.UnitPrice)
            , 2, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateLineWithholdingTax(CourierDeductionLine line)
        {
            return Math.Round(
                (line.Deduction.Invoice?.WithholdingTaxPercentage ?? 0)
                *
                CalculateLineSubtotal(line.Quantity, line.UnitPrice)
            , 2, MidpointRounding.AwayFromZero);
        }

    }
}
