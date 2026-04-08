using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Core.Domain.Bnz;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Core.Domain.Ird.PaydayFiling;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using CourierPortal.Core.DTOs.Admin.Openforce;
using Serilog;

namespace CourierPortal.Core.Services.Admin
{
    public class InvoiceBatchService(IWebHostEnvironment hostingEnvironment, AdminTimeZoneService timeZoneService, OpenforceService openforceService, IDbContextFactory<DespatchContext> contextFactory):AdminBaseService(contextFactory)
    {
        public async Task<InvoiceBatchResponse> Get(InvoiceBatchRequest request)
        {
            InvoiceBatchResponse response = new InvoiceBatchResponse(request.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
                return response;
            }

            response.InvoiceBatch = InvoiceBatchUtility.MapToInvoiceBatchDto(batch);

            response.Success = true;

            return response;
        }

        public async Task<InvoiceBatchesResponse> Search(SearchRequest request)
        {
            InvoiceBatchesResponse response = new InvoiceBatchesResponse(request.MessageId);

            List<CourierInvoiceBatch> invoiceBatches = await Context.CourierInvoiceBatches
                .Include(b => b.Status)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice.CourierDeductions)
                .Where(b => b.Id.ToString().Contains(request.SearchText) || b.CourierInvoiceBatchItems.Count(i => i.InvoiceId.ToString().Contains(request.SearchText)) > 0)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            response.InvoiceBatches = invoiceBatches.Select(b => InvoiceBatchUtility.MapToInvoiceBatchDto(b));

            response.Success = true;

            return response;
        }

        public async Task<InvoiceBatchResponse> AddInvoices(InvoiceBatchRequest request, AddRemoveInvoiceBatchRequest addRemoveRequest)
        {
            InvoiceBatchResponse response = new InvoiceBatchResponse(addRemoveRequest.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch == null)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
            else if (batch.StatusId != 1)
                response.Messages.Add(new MessageDto() { Message = $"Adding invoices to a '{batch.Status.Name}' batch is not permitted." });

            if (response.Messages.Count > 0)
                return response;

            IEnumerable<long> invoiceNos = addRemoveRequest.InvoiceNos.Select(InvoiceUtility.NumberToId).Distinct();

            IEnumerable<CourierInvoice> invoices = await Context.CourierInvoices
                .Include(i => i.CourierInvoiceLines)
                .Include(i => i.Courier)
                .Where(i => invoiceNos.Contains(i.Id) && i.CourierInvoiceBatchItem == null)
                .ToListAsync();

            if (invoices.Count() != invoiceNos.Count())
            {
                IEnumerable<string> foundInvoiceNos = invoices.Select(i => i.Id.ToString());
                IEnumerable<string> missingInvoiceNos = addRemoveRequest.InvoiceNos.Where(i => !foundInvoiceNos.Contains(i));
                response.Messages.Add(new MessageDto() { Message = $"Invalid invoice(s) '{missingInvoiceNos}'." });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            foreach (var aInvoice in invoices)
            {
                batch.CourierInvoiceBatchItems.Add(new CourierInvoiceBatchItem()
                {
                    Created = tenantTime,
                    Invoice = aInvoice
                });

                //TODO: There is currently a problem adding many batch items at a time, needs investigation, for now just save for each one
                await Context.SaveChangesAsync();
            }

            response.InvoiceBatch = InvoiceBatchUtility.MapToInvoiceBatchDto(batch);

            response.Success = true;
            return response;
        }

        public async Task<InvoiceBatchResponse> RemoveInvoices(InvoiceBatchRequest request, AddRemoveInvoiceBatchRequest addRemoveRequest)
        {
            InvoiceBatchResponse response = new InvoiceBatchResponse(addRemoveRequest.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
            else if (batch.StatusId != 1)
                response.Messages.Add(new MessageDto() { Message = $"Removing record from a '{batch.Status.Name}' batch is not permitted." });

            if (response.Messages.Count > 0)
                return response;

            IEnumerable<long> invoiceNos = addRemoveRequest.InvoiceNos.Select(InvoiceUtility.NumberToId).Distinct();

            IEnumerable<CourierInvoiceBatchItem> itemsToRemove = batch.CourierInvoiceBatchItems.Where(i => invoiceNos.Contains(i.InvoiceId));

            if (invoiceNos.Count() != itemsToRemove.Count())
            {
                IEnumerable<string> foundInvoiceNos = itemsToRemove.Select(i => i.InvoiceId.ToString());
                IEnumerable<string> missingInvoiceNos = addRemoveRequest.InvoiceNos.Where(i => !foundInvoiceNos.Contains(i));
                response.Messages.Add(new MessageDto() { Message = $"Records '{missingInvoiceNos}' not found in Batch '{request.BatchNo}'" });
                return response;
            }

            Context.CourierInvoiceBatchItems.RemoveRange(itemsToRemove);

            await Context.SaveChangesAsync();

            response.InvoiceBatch = InvoiceBatchUtility.MapToInvoiceBatchDto(batch);

            response.Success = true;
            return response;
        }

        public async Task<InvoiceBatchResponse> MarkBatchAsInProgress(InvoiceBatchRequest request)
        {
            InvoiceBatchResponse response = new InvoiceBatchResponse(request.MessageId);

            CourierInvoiceBatchStatus inProgressStatus = await Context.CourierInvoiceBatchStatuses.SingleOrDefaultAsync(s => s.Id == 2);

            if (inProgressStatus == null)
            {
                response.Messages.Add(new MessageDto() { Message = "Status not found." });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
            else if (batch.StatusId == inProgressStatus.Id)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' status is already '{inProgressStatus.Name}'." });
            else if (batch.StatusId != 1)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' status has been updated to '{batch.Status.Name}'." });
            //else if (batch.PayPeriodEndDate.Date.AddDays(1).AddHours(17) >= tenantTime) //5pm Tuesday, See SQL Job Despatch_CM_CreateNewWeeklyInvoiceBatch
            else if (batch.PayPeriodEndDate.Date >= tenantTime.Date) //End date has past
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' pay period window has not ended." });

            if (response.Messages.Count > 0)
                return response;

            batch.Status = inProgressStatus;
            await Context.SaveChangesAsync();

            response.InvoiceBatch = InvoiceBatchUtility.MapToInvoiceBatchDto(batch);

            response.Success = true;
            return response;
        }

        public async Task<InvoiceBatchResponse> CompleteBatch(InvoiceBatchRequest request)
        {
            InvoiceBatchResponse response = new InvoiceBatchResponse(request.MessageId);

            CourierInvoiceBatchStatus completedStatus = await Context.CourierInvoiceBatchStatuses.SingleOrDefaultAsync(s => s.Id == 3);

            if (completedStatus == null)
            {
                response.Messages.Add(new MessageDto() { Message = "Status not found." });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
            else if (batch.StatusId == completedStatus.Id)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' status is already '{completedStatus.Name}'." });
            else if (batch.PayPeriodEndDate >= tenantTime)
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' pay period has not ended." });

            if (response.Messages.Count > 0)
                return response;

            //Update couriers that have been marked as require payday filing registration
            IEnumerable<TucCourier> couriers = batch.CourierInvoiceBatchItems
                .Where(bi => (!bi.Invoice.Courier.PaydayFileRegistration.HasValue || !bi.Invoice.Courier.PaydayFileRegistration.Value) && bi.Invoice.WithholdingTaxPercentage > 0)
                .Select(x => x.Invoice.Courier)
                .Distinct();

            foreach (TucCourier courier in couriers)
            {
                courier.PaydayFileRegistration = true;
            }

            batch.Status = completedStatus;
            await Context.SaveChangesAsync();

            response.InvoiceBatch = InvoiceBatchUtility.MapToInvoiceBatchDto(batch);

            response.Success = true;
            return response;
        }

        public async Task<InvoiceBatchesResponse> GetPendingBatches(Guid messageId)
        {
            InvoiceBatchesResponse response = new InvoiceBatchesResponse(messageId);

            List<CourierInvoiceBatch> invoiceBatches = await Context.CourierInvoiceBatches
                .Include(b => b.Status)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice.CourierInvoiceLines)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice.CourierDeductions)
                .ThenInclude(d => d.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .Where(b => b.StatusId != 3)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            response.InvoiceBatches = invoiceBatches.Select(b => InvoiceBatchUtility.MapToInvoiceBatchDto(b));

            response.Success = true;

            return response;
        }

        #region "Downloads"

        //public async Task<FileResponse> GetBankAnzGstUploadFile(InvoiceBatchRequest request)
        //{
        //    FileResponse response = new FileResponse(request.MessageId);

        //    CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

        //    if (batch is null)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
        //        return response;
        //    }

        //    if (batch.StatusId != 2)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' is not in progress." });
        //        return response;
        //    }

        //    //Get GST Invoices from batch, exclude withholding tax invoices
        //    IEnumerable<CourierInvoice> invoices = batch.CourierInvoiceBatchItem.Where(bi => bi.Invoice.GstPercentage > 0).Select(bi => bi.Invoice);

        //    List<TblSite> sites = await Context.TblSite.ToListAsync();

        //    AnzDomestic anzDomestic = new AnzDomestic();

        //    foreach (var invoice in invoices)
        //    {
        //        var invoiceDto = InvoiceUtility.MapToInvoiceDto(invoice, invoice.Courier == null ? null : sites.SingleOrDefault(s => s.SiteId == invoice.Courier.SiteId));

        //        anzDomestic.Lines.Add(new AnzDomesticLine()
        //        {
        //            TransactionAmount = invoiceDto.Total,
        //            AccountNo = invoice.Courier.UccrBankAccountNo,
        //            OtherPartyName = invoice.Courier.UccrName + " " + invoice.Courier.UccrSurname,
        //            OriginatorReference = request.BatchNo,
        //            OriginatorAnalysisCode = string.Empty,
        //            OriginatorParticulators = invoiceDto.InvoiceNo,
        //            OtherPartyReference = invoiceDto.InvoiceNo,
        //            OtherPartyAnalysisCode = string.Empty,
        //            OtherPartyParticulars = "Urgent Couri"
        //        });
        //    }

        //    if (!anzDomestic.IsValid(response.Messages))
        //        return response;

        //    byte[] result = anzDomestic.ToCsv();
        //    string fileName = $"Bank {request.BatchNo} Gst.csv";

        //    //Save to file for reference
        //    await System.IO.File.WriteAllBytesAsync($"{_hostingEnvironment.ContentRootPath}\\Downloads\\{fileName}", result);

        //    response.FileType = "text/csv";
        //    response.FileName = fileName;
        //    response.File = result;

        //    response.Success = true;

        //    return response;
        //}

        //public async Task<FileResponse> GetBankAnzWithholdingTaxUploadFile(InvoiceBatchRequest request)
        //{
        //    FileResponse response = new FileResponse(request.MessageId);

        //    CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

        //    if (batch is null)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
        //        return response;
        //    }

        //    if (batch.StatusId != 2)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' is not in progress." });
        //        return response;
        //    }

        //    //Get Withholding Tax Invoices from batch, exclude GST invoices
        //    IEnumerable<CourierInvoice> invoices = batch.CourierInvoiceBatchItem.Where(bi => bi.Invoice.WithholdingTaxPercentage > 0).Select(bi => bi.Invoice);

        //    List<TblSite> sites = await Context.TblSite.ToListAsync();

        //    AnzDomestic anzDomestic = new AnzDomestic();

        //    foreach (var invoice in invoices)
        //    {
        //        var invoiceDto = InvoiceUtility.MapToInvoiceDto(invoice, invoice.Courier == null ? null : sites.SingleOrDefault(s => s.SiteId == invoice.Courier.SiteId));

        //        anzDomestic.Lines.Add(new AnzDomesticLine()
        //        {
        //            TransactionAmount = invoiceDto.Total,
        //            AccountNo = invoice.Courier.UccrBankAccountNo,
        //            OtherPartyName = invoice.Courier.UccrName + " " + invoice.Courier.UccrSurname,
        //            OriginatorReference = request.BatchNo,
        //            OriginatorAnalysisCode = string.Empty,
        //            OriginatorParticulators = invoiceDto.InvoiceNo,
        //            OtherPartyReference = invoiceDto.InvoiceNo,
        //            OtherPartyAnalysisCode = string.Empty,
        //            OtherPartyParticulars = "Urgent Couri"
        //        });
        //    }

        //    if (!anzDomestic.IsValid(response.Messages))
        //        return response;

        //    byte[] result = anzDomestic.ToCsv();
        //    string fileName = $"Bank {request.BatchNo} Withholding Tax.csv";

        //    //Save to file for reference
        //    await System.IO.File.WriteAllBytesAsync($"{_hostingEnvironment.ContentRootPath}\\Downloads\\{fileName}", result);

        //    response.FileType = "text/csv";
        //    response.FileName = fileName;
        //    response.File = result;
        //    response.Success = true;

        //    return response;
        //}

        public async Task<InvoiceBatchResponse> ProcessSettlements(InvoiceBatchRequest request)
        {
            InvoiceBatchResponse response = new InvoiceBatchResponse(request.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
                return response;
            }

            if (batch.StatusId != 2)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' is not in progress." });
                return response;
            }

            if (!batch.CourierInvoiceBatchItems.Any(i => !string.IsNullOrWhiteSpace(i.Invoice.Courier.OpenForceNumber)))
            {
                CourierInvoiceBatchStatus completedStatus = await Context.CourierInvoiceBatchStatuses.SingleOrDefaultAsync(s => s.Id == 3);

                if (completedStatus == null)
                {
                    response.Messages.Add(new MessageDto() { Message = "Status not found." });
                    return response;
                }

                batch.Status = completedStatus;
                await Context.SaveChangesAsync();

                response.InvoiceBatch = InvoiceBatchUtility.MapToInvoiceBatchDto(batch);
                response.Success = true;
                return response;
            }

            OfSettlement settlement = null;
            if (batch.CourierInvoiceBatchItems.Any(i => !string.IsNullOrWhiteSpace(i.Invoice.Courier.OpenForceNumber)))
            {
                List<OfCommission> commissions = null;
                List<OfDeduction> deductions = null;
                if (string.IsNullOrWhiteSpace(batch.ExternalId))
                {
                    var paymentTerm = await Context.TblSettings.Select(s => s.CourierInvoiceBatchPaymentTerm).FirstOrDefaultAsync();
                    if (paymentTerm == null)
                    {
                        response.Messages.Add(new MessageDto() { Message = "Settlement batch payment term has not been setup." });
                        return response;
                    }

                    var requestedDisbursementDate = batch.PayPeriodEndDate.Date.AddDays(paymentTerm.Days);

                    if (requestedDisbursementDate.Date <= DateTime.Today)
                    {
                        requestedDisbursementDate = DateTime.Today.AddDays(1);
                        Log.Warning("Courier invoice batch disbursement date is in the past, this has been readjusted to tomorrow");
                    }

                    settlement = await openforceService.AddUpdateAsync(new OfSettlement()
                    {
                        external_id = batch.Id.ToString(),
                        invoice_id = batch.Id,
                        start_date = batch.PayPeriodStartDate,
                        end_date = batch.PayPeriodEndDate,
                        requested_disbursement_date = requestedDisbursementDate,
                        status = 0
                    });
                    commissions = new List<OfCommission>();
                    deductions = new List<OfDeduction>();
                    batch.ExternalId = settlement.id;
                    await Context.SaveChangesAsync();
                }
                else
                {
                    settlement = await openforceService.GetSettlementAsync(batch.ExternalId);
                }

                int commissionTypeId = int.Parse(Environment.GetEnvironmentVariable("OpenforceCommissionTypeId"));
                int deductionTypeId = int.Parse(Environment.GetEnvironmentVariable("OpenforceDeductionTypeId"));
                commissions = commissions ?? await openforceService.GetCommissionsAsync(batch.ExternalId);
                foreach (var i in batch.CourierInvoiceBatchItems)
                {
                    if (settlement.status >= 2)
                    {
                        response.Messages.Add(new MessageDto() { Message = "Settlement has been marked for processing and commissions cannot be updated." });
                        return response;
                    }

                    if (string.IsNullOrWhiteSpace(i.Invoice.ExternalId)
                        && !string.IsNullOrWhiteSpace(i.Invoice.Courier.OpenForceNumber))
                    {
                        var commission =
                            commissions.FirstOrDefault(c => c.contractor_id == i.Invoice.Courier.OpenForceNumber && c.external_id == i.InvoiceId.ToString())
                            ??
                            await openforceService.AddUpdate(new OfCommission()
                            {
                                stm_client_id = settlement.stm_client_id,
                                settlement_id = batch.ExternalId,
                                commission_type_id = commissionTypeId,
                                contractor_id = i.Invoice.Courier.OpenForceNumber,
                                contractor_external_id = i.Invoice.CourierId.ToString(),
                                external_id = i.InvoiceId.ToString(),
                                description = i.InvoiceId.ToString(),
                                amount = i.Invoice.CourierInvoiceLines.Sum(l => l.Quantity * l.UnitPrice)
                            });

                        i.Invoice.ExternalId = commission.id;
                        await Context.SaveChangesAsync();
                    }

                    deductions = deductions ?? await openforceService.GetDeductionsAsync(batch.ExternalId);
                    foreach (var d in i.Invoice.CourierDeductions)
                    {
                        if (settlement.status >= 2)
                        {
                            response.Messages.Add(new MessageDto() { Message = "Settlement has been marked for processing and deductions cannot be updated." });
                            return response;
                        }

                        if (!string.IsNullOrWhiteSpace(d.ExternalId) || string.IsNullOrWhiteSpace(d.Courier.OpenForceNumber))
                            continue;

                        var deduction =
                            deductions.FirstOrDefault(c => c.contractor_id == i.Invoice.Courier.OpenForceNumber && c.external_id == i.InvoiceId.ToString())
                            ??
                            await openforceService.AddUpdate(new OfDeduction()
                            {
                                stm_client_id = settlement.stm_client_id,
                                settlement_id = batch.ExternalId,
                                deduction_type_id = deductionTypeId,
                                contractor_id = d.Courier.OpenForceNumber,
                                contractor_external_id = d.CourierId.ToString(),
                                external_id = d.Id.ToString(),
                                description = d.Id.ToString(),
                                amount = d.CourierDeductionLines.Sum(l => l.Quantity * l.UnitPrice)
                            });

                        d.ExternalId = deduction.id;
                        await Context.SaveChangesAsync();
                    }
                }
            }

            if (batch.CourierInvoiceBatchItems.All(i => string.IsNullOrWhiteSpace(i.Invoice.Courier.OpenForceNumber) || (!string.IsNullOrWhiteSpace(i.Invoice.ExternalId) && i.Invoice.CourierDeductions.All(d => !string.IsNullOrWhiteSpace(d.ExternalId)))))
            {
                if (settlement != null && settlement.status < 2)
                {
                    settlement.status = 2;
                    settlement = await openforceService.AddUpdateAsync(settlement);
                }

                CourierInvoiceBatchStatus completedStatus = await Context.CourierInvoiceBatchStatuses.SingleOrDefaultAsync(s => s.Id == 3);

                if (completedStatus == null)
                {
                    response.Messages.Add(new MessageDto() { Message = "Status not found." });
                    return response;
                }

                batch.Status = completedStatus;
                await Context.SaveChangesAsync();

                response.InvoiceBatch = InvoiceBatchUtility.MapToInvoiceBatchDto(batch);
                response.Success = true;
            }

            return response;
        }

        public async Task<FileResponse> GetBankBnzGstUploadFile(InvoiceBatchRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
                return response;
            }

            if (batch.StatusId != 2)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' is not in progress." });
                return response;
            }

            //Get GST Invoices from batch, exclude withholding tax invoices
            IEnumerable<CourierInvoice> invoices = batch.CourierInvoiceBatchItems.Where(bi => bi.Invoice.GstPercentage > 0).Select(bi => bi.Invoice);

            BnzPayroll bnzPayroll = new()
            {
                DueDate = batch.PayPeriodEndDate.Date.AddDays(8)
            };

            foreach (var invoice in invoices)
            {
                var invoiceDto = InvoiceUtility.MapToDto(invoice);

                bnzPayroll.Transactions.Add(new BnzPayrollTransaction()
                {
                    AccountNo = invoice.Courier.UccrBankAccountNo,
                    TransactionAmount = invoiceDto.Total,
                    OtherPartyName = invoice.Courier.UccrName + " " + invoice.Courier.UccrSurname,
                    OtherPartyReference = invoiceDto.Number,
                    OtherPartyParticulars = "Urgent Couri",
                    OtherPartyCode = string.Empty,

                    YourName = "Urgent Couriers",
                    YourReference = request.BatchNo,
                    YourParticulars = invoiceDto.Number,
                    YourCode = string.Empty
                });
            }

            if (!bnzPayroll.IsValid(response.Messages))
                return response;

            byte[] result = bnzPayroll.ToCsv();
            string fileName = $"Bank BNZ {request.BatchNo} Gst.txt";

            //Save to file for reference
            //await System.IO.File.WriteAllBytesAsync($"{hostingEnvironment.ContentRootPath}\\Downloads\\{fileName}", result);

            response.FileType = "text/plain";
            response.FileName = fileName;
            response.File = result;

            response.Success = true;

            return response;
        }

        public async Task<FileResponse> GetBankBnzWithholdingTaxUploadFile(InvoiceBatchRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
                return response;
            }

            if (batch.StatusId != 2)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' is not in progress." });
                return response;
            }

            //Get Withholding Tax Invoices from batch, exclude GST invoices
            IEnumerable<CourierInvoice> invoices = batch.CourierInvoiceBatchItems.Where(bi => bi.Invoice.WithholdingTaxPercentage > 0).Select(bi => bi.Invoice);

            BnzPayroll bnzPayroll = new()
            {
                DueDate = batch.PayPeriodEndDate.Date.AddDays(8)
            };

            foreach (var invoice in invoices)
            {
                var invoiceDto = InvoiceUtility.MapToDto(invoice);

                bnzPayroll.Transactions.Add(new BnzPayrollTransaction()
                {
                    AccountNo = invoice.Courier.UccrBankAccountNo,
                    TransactionAmount = invoiceDto.Total,
                    OtherPartyName = invoice.Courier.UccrName + " " + invoice.Courier.UccrSurname,
                    OtherPartyReference = invoiceDto.Number,
                    OtherPartyParticulars = "Urgent Couri",
                    OtherPartyCode = string.Empty,

                    YourName = "Urgent Couriers",
                    YourReference = request.BatchNo,
                    YourParticulars = invoiceDto.Number,
                    YourCode = string.Empty
                });
            }

            if (!bnzPayroll.IsValid(response.Messages))
                return response;

            byte[] result = bnzPayroll.ToCsv();
            string fileName = $"Bank BNZ {request.BatchNo} Withholding Tax.txt";

            //Save to file for reference
            //await System.IO.File.WriteAllBytesAsync($"{hostingEnvironment.ContentRootPath}\\Downloads\\{fileName}", result);

            response.FileType = "text/plain";
            response.FileName = fileName;
            response.File = result;
            response.Success = true;

            return response;
        }

        public async Task<FileResponse> GetIrdEmployeeDetailsFile(InvoiceBatchRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
                return response;
            }

            if (batch.StatusId != 2)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' is not in progress." });
                return response;
            }

            IEnumerable<TucCourier> couriers = batch.CourierInvoiceBatchItems
                .Where(bi => (!bi.Invoice.Courier.PaydayFileRegistration.HasValue || !bi.Invoice.Courier.PaydayFileRegistration.Value) && bi.Invoice.WithholdingTaxPercentage > 0)
                .Select(x => x.Invoice.Courier)
                .Distinct();

            var tenantTime = timeZoneService.GetTenantTime();
            if (couriers.Count(c=> !c.UccrStartDate.HasValue || c.UccrStartDate.Value > tenantTime) > 0)
            {
                response.Messages.Add(new MessageDto() { Message = "A valid start date is required for all couriers." });
                return response;
            }

            EmployeeDetails employeeDetails = new();

            employeeDetails.Lines.AddRange(couriers.Select(x => new EmployeeDetailsLine()
            {
                Ded = new Ded()
                {
                    EmployeeIrdNo = x.UccrGst,
                    EmployeeFirstName = x.UccrName?.Trim(),
                    EmployeeLastName = x.UccrSurname?.Trim(),
                    EmploymentStartDate = x.UccrStartDate.Value
                }
            }));

            if (!employeeDetails.IsValid(response.Messages))
                return response;

            byte[] result = employeeDetails.ToCsv();
            string fileName = $"Payday Filing {request.BatchNo} Employee Details.csv";

            //Save to file for reference
            //await System.IO.File.WriteAllBytesAsync($"{hostingEnvironment.ContentRootPath}\\Downloads\\{fileName}", result);

            response.FileType = "text/csv";
            response.FileName = fileName;
            response.File = result;
            response.Success = true;

            return response;
        }

        public async Task<FileResponse> GetIrdEmployeeInformationFile(InvoiceBatchRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierInvoiceBatch batch = await GetCourierInvoiceBatch(request);

            if (batch is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' not found." });
                return response;
            }

            if (batch.StatusId != 2)
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' is not in progress." });
                return response;
            }

            var invoicesGroupedByCouriers = batch.CourierInvoiceBatchItems
                .Where(bi => bi.Invoice.WithholdingTaxPercentage > 0)
                .Select(bi => InvoiceUtility.MapToDto(bi.Invoice))
                .GroupBy(i => i.Courier.Code);



            if (invoicesGroupedByCouriers.Any(x => x.Any(i => i.Total <= 0m)))
            {
                response.Messages.Add(new MessageDto() { Message = $"Batch '{request.BatchNo}' contains zero or negative invoices." });
                return response;
            }

            //Batch end date is always Monday, payDate is short date and will be second tuesday from enddate
            DateTime payDate = DateTime.Parse(batch.PayPeriodEndDate.AddDays(8).ToString("yyyy-MM-dd"));

            ////payDate will be second tuesday from enddate
            //int delta = batch.PayPeriodEndDate.DayOfWeek == DayOfWeek.Sunday ? 9 : DayOfWeek.Tuesday - batch.PayPeriodEndDate.DayOfWeek + 7;
            //DateTime payDate = DateTime.Parse(batch.PayPeriodEndDate.AddDays(delta).ToString("yyyy-MM-dd"));

            EmployeeInformation employeeInformation = new()
            {
                PayDate = payDate
            };

            employeeInformation.Lines.AddRange(invoicesGroupedByCouriers.Select(x => new Dei()
            {
                EmployeeIrdNo = x.First().Courier.IrdNumber ?? x.First().Courier.GstNumber,
                EmployeeName = $"{x.First().Courier.Surname} {x.First().Courier.FirstName}", //Format must be surname then first
                PayPeriodStartDate = batch.PayPeriodStartDate,
                PayPeriodEndDate = batch.PayPeriodEndDate,
                GrossEarningsAndOrSchedularPayments = x.Sum(i => i.SubTotal),
                EarningsAndOrSchedularPaymentsNotLiableForAccEarnersLevy = x.Sum(i => i.SubTotal),
                PayeTax = x.Sum(i => i.WithholdingTaxAmount)
            }));

            if (!employeeInformation.IsValid(response.Messages))
                return response;

            byte[] result = employeeInformation.ToCsv();
            string fileName = $"Payday Filing {request.BatchNo} Employee Information.csv";

            //Save to file for reference
            //await System.IO.File.WriteAllBytesAsync($"{hostingEnvironment.ContentRootPath}\\Downloads\\{fileName}", result);

            response.FileType = "text/csv";
            response.FileName = fileName;
            response.File = result;
            response.Success = true;

            return response;
        }

        #endregion

        #region "Private functions and methods"

        private async Task<CourierInvoiceBatch> GetCourierInvoiceBatch(InvoiceBatchRequest request)
        {
            return await Context.CourierInvoiceBatches
                .Include(b => b.Status)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice.CourierInvoiceLines)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice.Courier)
                .ThenInclude(c => c.Region)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice.CourierDeductions)
                .ThenInclude(d => d.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .Include(b => b.CourierInvoiceBatchItems)
                .ThenInclude(i => i.Invoice.CourierDeductions)
                .ThenInclude(d => d.Courier)
                .ThenInclude(c => c.Region)
                .SingleOrDefaultAsync(b => b.Id == long.Parse(request.BatchNo));
        }

        #endregion
    }
}
