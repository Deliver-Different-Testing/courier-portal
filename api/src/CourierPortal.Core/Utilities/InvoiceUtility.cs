using System;
using System.Linq;
using CourierPortal.Core.Dtos.Invoices;
using CourierPortal.Core.Domain;
using CourierPortal.Web.Core.Domain.Despatch;

namespace CourierPortal.Core.Utilities
{
    public static class InvoiceUtility
    {
        public static bool IsValidInvoiceNo(string invoiceNo)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo) || !long.TryParse(invoiceNo, out long id) || id <= 0)
                return false;

            return true;
        }

        public static long InvoiceNoToId(string invoiceNo)
        {
            if (!IsValidInvoiceNo(invoiceNo))
                throw new Exception($"Invalid Invoice No of '{invoiceNo}'");

            return long.Parse(invoiceNo);
        }

        public static string IdToInvoiceNo(long invoiceId)
        {
            if (invoiceId <= 0)
                throw new Exception($"Invalid Invoice Id of '{invoiceId}'");

            return invoiceId.ToString().PadLeft(4, '0');
        }

        public static InvoiceDto MapInvoiceToInvoiceDto(CourierInvoice invoice, string toAddress = null)
        {
            decimal subtotal = invoice.CourierInvoiceLines.Sum(l => Math.Round(l.Quantity * Math.Round(l.UnitPrice, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero));
            decimal gstAmount = Math.Round(subtotal * invoice.GstPercentage, 2);
            decimal withholdingTaxAmount = Math.Round(subtotal * invoice.WithholdingTaxPercentage * -1m, 2);
            decimal total = subtotal + gstAmount + withholdingTaxAmount;

            foreach (var d in invoice.CourierDeductions)
            {
                decimal subtotalDeduction = d.CourierDeductionLines.Sum(l => Math.Round(l.Quantity * Math.Round(l.UnitPrice, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero));
                decimal gstAmountDeduction = Math.Round(subtotalDeduction * invoice.GstPercentage, 2);
                decimal withholdingTaxAmountDeduction = Math.Round(subtotalDeduction * invoice.WithholdingTaxPercentage * -1m, 2);
                decimal totalDeduction = subtotalDeduction + gstAmountDeduction + withholdingTaxAmountDeduction;

                subtotal -= subtotalDeduction;
                gstAmount -= gstAmountDeduction;
                withholdingTaxAmount -= withholdingTaxAmountDeduction;
                total -= totalDeduction;
            }

            return new InvoiceDto()
            {
                InvoiceNo = IdToInvoiceNo(invoice.Id),
                Created = invoice.Created,
                TaxNo = invoice.TaxNo,
                GstPercentage = invoice.GstPercentage,
                GstAmount = gstAmount,
                WithholdingTaxPercentage = invoice.WithholdingTaxPercentage,
                WithholdingTaxAmount = withholdingTaxAmount,
                Subtotal = subtotal,
                Total = total,
                Reference = invoice.Reference,
                ToAddress = toAddress,
                Lines = invoice
                    .CourierInvoiceLines
                    .Select(line => new InvoiceLineDto()
                    {
                        Description = line.Description,
                        Quantity = line.Quantity,
                        UnitPrice = Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero),
                        Total = Math.Round(line.Quantity * Math.Round(line.UnitPrice, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero)
                    })
                    .Concat(invoice.CourierDeductions
                        .SelectMany(d => d.CourierDeductionLines
                            .Select(line => new InvoiceLineDto()
                            {
                                Description = line.Description,
                                Quantity = line.Quantity,
                                UnitPrice = Math.Round(line.UnitPrice * -1, 2, MidpointRounding.AwayFromZero),
                                Total = Math.Round(line.Quantity * Math.Round(line.UnitPrice * -1, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero)
                            }))
                    )
            };
        }

        public static bool IsCompleted(RunItem runItem)
        {
            return !runItem.Void && runItem.JobDone;
        }

        public static bool CanInvoice(RunItem runItem)
        {
            if (runItem.Invoiced || !runItem.Archived || runItem.Void || runItem.Reprice)
                return false;

            //Verify that the job has been completed
            if (!IsCompleted(runItem))
                return false;

            if (!runItem.CourierPayment.HasValue || runItem.CourierPayment == 0)
                return false;


            return true;
        }
    }
}
