using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Invoices;
using CourierPortal.Core.DTOs.Portal.Runs;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalInvoiceService(PortalTimeZoneService timeZoneService, PortalRunService runService, IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        public async Task<InvoicesResponse> GetRecent(Guid messageId, int courierId)
        {
            InvoicesResponse response = new InvoicesResponse(messageId);

            List<CourierInvoice> invoices = await Context.CourierInvoices
                    .Include(i => i.CourierInvoiceLines)
                    .Include(i => i.CourierDeductions)
                    .ThenInclude(d => d.CourierDeductionLines)
                    .Where(i => i.CourierId == courierId)
                    .OrderByDescending(i => i.Id)
                    .Take(10)
                    .ToListAsync();

            response.Invoices = invoices.Select(i => InvoiceUtility.MapInvoiceToInvoiceDto(i));

            response.Success = true;
            return response;
        }

        public async Task<InvoicesPastResponse> GetPast(Guid messageId, int courierId)
        {
            InvoicesPastResponse response = new InvoicesPastResponse(messageId);

            var invoicesPast = await Context.CourierInvoices
                .Where(i => i.CourierId == courierId)
                .Select(i => new { i.Id, i.Created })
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            response.Invoices = invoicesPast
                .Select(i => new InvoicePastDto()
                {
                    InvoiceNo = InvoiceUtility.IdToInvoiceNo(i.Id), 
                    Created = i.Created
                });

            response.Success = true;
            return response;
        }

        public async Task<InvoiceResponse> Get(int courierId, InvoiceRequest request)
        {
            InvoiceResponse response = new InvoiceResponse(request.MessageId);

            CourierInvoice invoice = await Context.CourierInvoices
                .Include(i => i.CourierInvoiceLines)
                .Include(i => i.CourierDeductions)
                .ThenInclude(i => i.CourierDeductionLines)
                .SingleOrDefaultAsync(i => i.Id == InvoiceUtility.InvoiceNoToId(request.InvoiceNo) && i.CourierId == courierId);

            if (invoice is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Invoice '{request.InvoiceNo}' not found." });
                return response;
            }

            string companyAddress = await Context.TblSettings
                .Select(s => s.CompanyAddress).FirstOrDefaultAsync();

            response.Invoice = InvoiceUtility.MapInvoiceToInvoiceDto(invoice, companyAddress);

            response.Success = true;
            return response;
        }

        public async Task<InvoiceResponse> Create(int courierId, Guid messageId)
        {
            InvoiceResponse response = new InvoiceResponse(messageId);

            //Ensure the courier is still marked as active and web enabled.
            TucCourier courier = await Context.TucCouriers
                .SingleOrDefaultAsync(c => c.UccrId == courierId && c.Active && !c.UccrInternal);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier '{courierId}' not found, inactive or invoicing access has been revoked." });
                return response;
            }

            if (courier.CourierTypeId != 4)
            {
                response.Messages.Add(new MessageDto() { Message = "Invoicing is not supported for this courier type." });
                return response;
            }

            //Block the user from being able to create invoices if they have and invalid bank account number, IRD or GST number.
            if (!CourierUtility.IsValidBankAccount(courier.BankRoutingNumber, courier.UccrBankAccountNo) || (!CourierUtility.IsValidTaxNo(courier.UccrGst) && !CourierUtility.IsValidSocialSecurityNo(courier.UccrGst)))
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier '{courierId}' has an invalid bank account number or IRD/GST Number." });
                return response;
            }

            UninvoicedResponse uninvoicedResponse = await GetUninvoiced(messageId, courier.UccrId, true);

            if (!uninvoicedResponse.Success)
            {
                response.Messages.AddRange(uninvoicedResponse.Messages);
                return response;
            }

            //If there is no un-invoiced runs then there is no need to create a invoice, return
            if (!uninvoicedResponse.Courier.Runs.Any())
            {
                response.Messages.Add(new MessageDto() { Message = $"No runs to invoice" });
                return response;
            }

            if (uninvoicedResponse.Courier.Total <= 0m && !uninvoicedResponse.Masters.Any())
            {
                response.Messages.Add(new MessageDto() { Message = $"Total must be greater than zero" });
                return response;
            }

            if (uninvoicedResponse.Masters.Any(x => x.Total <= 0m))
            {
                response.Messages.Add(new MessageDto() { Message = $"Master Totals must be greater than zero" });
                return response;
            }

            //Get settings, always only one record in this table
            TblSetting settings = await Context.TblSettings.FirstAsync();
            CourierInvoice invoice = await CreateInvoice(settings, courier, uninvoicedResponse);
            if (invoice != null)
                Context.CourierInvoices.Add(invoice);

            foreach (var x in uninvoicedResponse.Masters)
            {
                //Ensure the courier is still marked as active and web enabled.
                TucCourier courierMaster = await Context.TucCouriers
                    .SingleOrDefaultAsync(c => c.UccrId == x.MasterId && c.Active && !c.UccrInternal);

                if (courierMaster is null)
                {
                    response.Messages.Add(new MessageDto() { Message = $"Master Courier '{x.MasterId}' not found, inactive or invoicing access has been revoked." });
                    return response;
                }

                CourierInvoice invoiceMaster = await CreateInvoice(settings, courierMaster, uninvoicedResponse, true);

                if (invoiceMaster != null)
                    Context.CourierInvoices.Add(invoiceMaster);
            }

            await Context.SaveChangesAsync();

            response.Invoice = invoice == null ? null : InvoiceUtility.MapInvoiceToInvoiceDto(invoice, settings.CompanyAddress);
            response.Success = true;
            return response;
        }

        public async Task<UninvoicedResponse> GetUninvoiced(Guid messageId, int courierId, bool calculateMaster = false, int? masterCourierId = null)
        {
            UninvoicedResponse response = new UninvoicedResponse(messageId);

            TucCourier courier = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.UccrId == courierId && c.Active && !c.UccrInternal);

            if (courier is null) { 
                response.Messages.Add(new MessageDto() { Message = $"Courier '{courierId}' not found, inactive or invoicing access has been revoked." });
                return response;
            }

            RunsResponse runsResponse = await runService.GetCourierRuns(messageId, courier.UccrId, calculateMaster, masterCourierId);

            if (!runsResponse.Success)
            {
                response.Messages.AddRange(runsResponse.Messages);
                return response;
            }

            List<CourierDeduction> deductions = await Context.CourierDeductions
                .Include(i => i.CourierDeductionLines)
                .Where(d => d.CourierId == courier.UccrId && !d.InvoiceId.HasValue)
                .ToListAsync();

            var uninvoiced = runsResponse.Uninvoiced.ToList();
            var deductionLines = deductions.SelectMany(d => d.CourierDeductionLines).ToList();
            if (deductionLines.Any())
                uninvoiced.AddRange(deductionLines.Select(l => new RunDto()
                {
                    RunType = 2,
                    BookDate = l.Created.Date,
                    Amount = -1 * Math.Round(l.Quantity * l.UnitPrice, 2, MidpointRounding.AwayFromZero),
                    RunName = l.Description,
                    Jobs = []
                }));


            //Get settings, always only one record in this table
            var settings = await Context.TblSettings
                .Select(s => new { s.Gstrate, s.CompanyAddress })
                .FirstAsync();

            decimal withholdingTaxPercentage = courier.WithholdingTaxPercentage.HasValue && courier.WithholdingTaxPercentage > 0
                ? courier.WithholdingTaxPercentage.Value / 100
                : 0;
            decimal gstPercentage = courier.WithholdingTaxPercentage.HasValue && courier.WithholdingTaxPercentage > 0
                ? 0
                : settings.Gstrate.Value;

            decimal subTotal = uninvoiced.Sum(run => run.Amount);
            decimal gstAmount = Math.Round(subTotal * gstPercentage, 2, MidpointRounding.AwayFromZero);
            decimal withholdingTaxAmount = Math.Round(subTotal * withholdingTaxPercentage * -1m, 2, MidpointRounding.AwayFromZero);
            decimal total = subTotal + gstAmount + withholdingTaxAmount;

            response.ToAddress = settings.CompanyAddress;

            response.Courier = new UninvoicedDto()
            {
                Runs = uninvoiced.Where(r => r.Amount != 0),
                Subtotal = subTotal,
                WithholdingTaxPercentage = withholdingTaxPercentage,
                WithholdingTaxAmount = withholdingTaxAmount,
                GstPercentage = gstPercentage,
                GstAmount = gstAmount,
                Total = total
            };

            if (calculateMaster)
            {
                var masterIds = uninvoiced
                    .SelectMany(r => r.Jobs
                        .Where(j => j.MasterId.HasValue 
                            && (!masterCourierId.HasValue || masterCourierId.Value == j.MasterId.Value))
                        .Select(j => j.MasterId.Value))
                    .Distinct()
                    .ToList();

                foreach (var id in masterIds)
                {
                    TucCourier courierMaster = await Context.TucCouriers.FirstOrDefaultAsync(c => c.UccrId == id && c.Active && !c.UccrInternal)
;
                    if (courierMaster is null)
                    {
                        response.Messages.Add(new MessageDto() { Message = $"Master Courier '{id}' not found, inactive or invoicing access has been revoked." });
                        return response;
                    }

                    decimal masterWithholdingTaxPercentage = courierMaster != null && courierMaster.WithholdingTaxPercentage.HasValue && courierMaster.WithholdingTaxPercentage > 0
                        ? courierMaster.WithholdingTaxPercentage.Value / 100
                        : 0;

                    decimal masterGstPercentage = courierMaster != null && courierMaster.WithholdingTaxPercentage.HasValue && courierMaster.WithholdingTaxPercentage > 0
                        ? 0
                        : settings.Gstrate.Value;

                    decimal masterSubTotal = uninvoiced.Sum(r => r.Masters.Where(m => m.Id == id).Sum(m => m.Amount));
                    decimal masterGstAmount = Math.Round(masterSubTotal * masterGstPercentage, 2, MidpointRounding.AwayFromZero);
                    decimal masterWithholdingTaxAmount = Math.Round(masterSubTotal * masterWithholdingTaxPercentage * -1m, 2, MidpointRounding.AwayFromZero);
                    decimal masterTotal = masterSubTotal + masterGstAmount + masterWithholdingTaxAmount;

                    if (masterTotal == 0)
                        continue;

                    var runsByMaster = uninvoiced
                            .Where(r => r.Masters.Any(m => m.Id == id))
                            .Select(r => JsonConvert.DeserializeObject<RunDto>(JsonConvert.SerializeObject(r)))
                            .ToList();
                    runsByMaster.ForEach(r => r.Jobs = r.Jobs.Where(j => j.MasterId == id).ToList());
                    runsByMaster = runsByMaster.Where(r => r.Jobs.Any()).ToList();

                    response.Masters.Add(new UninvoicedDto()
                    {
                        MasterId = id,
                        Runs = runsByMaster,
                        Subtotal = masterSubTotal,
                        WithholdingTaxPercentage = masterWithholdingTaxPercentage,
                        WithholdingTaxAmount = masterWithholdingTaxAmount,
                        GstPercentage = masterGstPercentage,
                        GstAmount = masterGstAmount,
                        Total = masterTotal
                    });
                }
            }

            response.Success = true;
            return response;
        }

        private async Task<CourierInvoice> CreateInvoice(TblSetting settings, TucCourier courier, UninvoicedResponse uninvoiced, bool isMaster = false)
        {
            //Get All uninvoiced deductions for this courier;
            List<CourierDeduction> deductions = isMaster
                ? null 
                : await Context.CourierDeductions
                    .Include(d => d.CourierDeductionLines)
                    .Where(d => !d.InvoiceId.HasValue && d.CourierId == courier.UccrId)
                    .ToListAsync();


            //If the user did not define what runs they would like to invoice then create invoice line for all un-invoiced lines else just for the runs requested.
            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstOrDefaultAsync(); 
            var tenantTime = timeZoneService.GetTenantTime(); 
            var lines = uninvoiced.Courier.Runs
                .Where(r => r.RunType != 2 && (!isMaster || r.Masters.Any(m => m.Id == courier.UccrId))) // Exclude deductions
                .Select(r => new CourierInvoiceLine()
                {
                    Created = tenantTime,
                    Description = $"{r.BookDate.ToString(countryCode == "NZ" ? "dd/MM/yyyy" : "MM/dd/yyyy")} - {r.RunName}",
                    Quantity = 1,
                    UnitPrice = Math.Round(
                        isMaster
                            //r.Jobs.Where(j => j.MasterId == courier.UccrId).Sum(j => (j.MasterPayment ?? 0) + (j.MasterFuel ?? 0) + (j.MasterBonus ?? 0))
                            ? r.Masters.Where(m => m.Id == courier.UccrId).Sum(m => m.Amount)
                            : r.Amount
                        , 2),
                    CourierInvoiceLineJobs = isMaster
                        ? null
                        : r.Jobs
                            .Select(j => new CourierInvoiceLineJob() { JobId = j.JobId })
                            .ToHashSet(),
                    CourierInvoiceLineJobMasters = isMaster
                        ? r.Jobs
                            .Where(j => j.MasterId == courier.UccrId)
                            .Select(j => new CourierInvoiceLineJobMaster() { JobId = j.JobId })
                            .ToHashSet()
                        : null
                })
                .Where(l => l.UnitPrice * l.Quantity != 0)
                .ToHashSet();

            if (!lines.Any())
                return null;

            return new CourierInvoice()
            {
                Created = tenantTime,
                CourierId = courier.UccrId,
                TaxNo = courier.UccrGst,
                GstPercentage = courier.WithholdingTaxPercentage.HasValue && courier.WithholdingTaxPercentage > 0 ? 0 : settings.Gstrate.Value,
                WithholdingTaxPercentage = courier.WithholdingTaxPercentage.HasValue && courier.WithholdingTaxPercentage > 0 ? courier.WithholdingTaxPercentage.Value / 100 : 0,
                Reference = courier.Code,
                CourierInvoiceLines = lines,
                CourierDeductions = deductions
            };
        }
    }
}
