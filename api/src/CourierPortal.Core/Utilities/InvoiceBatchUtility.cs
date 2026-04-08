using System;
using System.Collections.Generic;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.DTOs.Admin.Deductions;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Utilities
{
    public static class InvoiceBatchUtility
    {
        //private const string INVOICE_BATCH_PREFIX = "UAB-"; //Must not change prefix as it will change past batches as it uses the database ID currently instead of a counter.

        public static bool IsValidBatchNo(string batchNo)
        {
            if (string.IsNullOrWhiteSpace(batchNo) || !long.TryParse(batchNo, out long id) || id <= 0)
                return false;

            return true;
        }

        //public static long BatchNoToId(string batchNo)
        //{
        //    if (!IsValidBatchNo(batchNo))
        //        throw new Exception($"Invalid Batch No of '{batchNo}'");

        //    return long.Parse(batchNo.Substring(INVOICE_BATCH_PREFIX.Length));
        //}

        //public static string IdToBatchNo(long id)
        //{
        //    if (id <= 0)
        //        throw new Exception($"Invalid ID of '{id}'");

        //    return INVOICE_BATCH_PREFIX + id.ToString().PadLeft(4, '0');
        //}

        public static InvoiceBatchDto MapToInvoiceBatchDto(CourierInvoiceBatch batch)
        {
            var invoices = batch
                .CourierInvoiceBatchItems
                .Select(i => i.Invoice)
                .ToList();
            List<TransactionDto> invoiceDtos = invoices
                .Select(i => InvoiceUtility.MapToDto(i))
                .ToList();

            //Withholding Tax Invoices Summary
            decimal grossTaxable = invoiceDtos.Where(i => i.WithholdingTaxPercentage > 0).Sum(i => i.SubTotal);
            decimal withholdingTax = invoiceDtos.Where(i => i.WithholdingTaxPercentage > 0).Sum(i => i.WithholdingTaxAmount);
            decimal netPayment = grossTaxable - withholdingTax;

            //GST Invoices Summary
            decimal gst = invoiceDtos.Where(i => i.GstPercentage > 0).Sum(i => i.GstAmount);
            decimal gstExclusive = invoiceDtos.Where(i => i.GstPercentage > 0).Sum(i => i.SubTotal);
            decimal gstInclusive = gstExclusive + gst;

            return new InvoiceBatchDto()
            {
                BatchNo = batch.Id.ToString(),
                Created = batch.Created,
                PayPeriodStartDate = batch.PayPeriodStartDate,
                PayPeriodEndDate = batch.PayPeriodEndDate,
                Status = batch.Status.Name,
                GrossTaxable = grossTaxable,
                WithholdingTax = withholdingTax,
                NetPayment = netPayment,
                Gst = gst,
                GstExclusive = gstExclusive,
                GstInclusive = gstInclusive,
                Invoices = invoiceDtos
            };
        }
    }
}
