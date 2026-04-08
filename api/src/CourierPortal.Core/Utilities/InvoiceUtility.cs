using System;
using System.Collections.Generic;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Utilities
{
    public static class InvoiceUtility
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
                throw new Exception($"Invalid number of '{number}'");

            return long.Parse(number);
        }

        //public static string IdToNumber(long id)
        //{
        //    if (id <= 0)
        //        throw new Exception($"Invalid ID of '{id}'");

        //    return id.ToString().PadLeft(4, '0');
        //}

        public static TransactionDto MapToDto(CourierInvoice invoice, IEnumerable<TransactionLineJobDto> jobs = null)
        {
            decimal subTotal = invoice.CourierInvoiceLines.Sum(CalculateLineSubtotal);
            decimal gstAmount = invoice.CourierInvoiceLines.Sum(CalculateLineGst);
            decimal withholdingTaxAmount = invoice.CourierInvoiceLines.Sum(CalculateLineWithholdingTax);
            decimal total = subTotal + gstAmount - withholdingTaxAmount;

            var deductions = invoice.CourierDeductions
                .Select(DeductionUtility.MapToDto)
                .ToList();

            return new TransactionDto()
            {
                Number = invoice.Id.ToString(),
                Created = invoice.Created,
                Courier = invoice.Courier == null ? null : CourierUtility.MapToCourierDto(invoice.Courier),
                TaxNo = invoice.TaxNo,
                GstPercentage = invoice.GstPercentage,
                GstAmount = gstAmount - deductions.Sum(d => d.GstAmount),
                WithholdingTaxPercentage = invoice.WithholdingTaxPercentage,
                WithholdingTaxAmount = withholdingTaxAmount - deductions.Sum(d => d.WithholdingTaxAmount),
                SubTotal = subTotal - deductions.Sum(d => d.SubTotal),
                Total = total - deductions.Sum(d => d.Total),
                Reference = invoice.Reference,
                ExternalId = invoice.ExternalId,
                Lines = invoice.CourierInvoiceLines.Select(line => new TransactionLineDto()
                {
                    Id = line.Id,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero),
                    Total = Math.Round(line.Quantity * Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero),
                    Jobs = jobs?.Where(j => j.LineId == line.Id)
                }),
                Deductions = deductions
            };
        }

        private static decimal CalculateLineSubtotal(CourierInvoiceLine line)
        {
            return Math.Round(
                line.Quantity * Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero)
            , 2, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateLineGst(CourierInvoiceLine line)
        {
            return Math.Round(
                line.Invoice.GstPercentage
                *
                CalculateLineSubtotal(line)
            , 2, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateLineWithholdingTax(CourierInvoiceLine line)
        {
            return Math.Round(
                line.Invoice.WithholdingTaxPercentage
                *
                CalculateLineSubtotal(line)
            , 2, MidpointRounding.AwayFromZero);
        }

    }
}
