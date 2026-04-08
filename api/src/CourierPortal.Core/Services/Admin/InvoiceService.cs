using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminInvoiceService(InvoiceLineJobRepository invoiceLineJobRepository,IDbContextFactory<DespatchContext> contextFactory):AdminBaseService(contextFactory)
    {
        public async Task<BaseResponse> Get(TransactionRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierInvoice invoice = await Context.CourierInvoices
                .Include(i => i.Courier)
                .ThenInclude(c => c.Region)
                .Include(i => i.CourierInvoiceLines)
                .Include(i => i.CourierDeductions)
                .ThenInclude(d => d.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .SingleOrDefaultAsync(i => i.Id == InvoiceUtility.NumberToId(request.Number));

            if (invoice is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Invoice '{request.Number}' Not Found." });
                return response;
            }

            //Get job information for the invoice
            IEnumerable<TransactionLineJobDto> jobs = await invoiceLineJobRepository.GetJobsByInvoice(invoice.Id);

            response.Data = InvoiceUtility.MapToDto(invoice, jobs);

            response.Success = true;
            return response;
        }

        public async Task<BaseResponse> Search(SearchRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            //List<CourierInvoice> invoices = await _Context.CourierInvoice
            //    .Include(i => i.Courier)
            //    .Include(i => i.CourierInvoiceLine)
            //    .Where(i => InvoiceUtility.IdToInvoiceNo(i.Id).Contains(request.SearchText, StringComparison.OrdinalIgnoreCase)
            //                || i.Courier.Code.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase)
            //                || (i.Courier.UccrName + ' ' + i.Courier.UccrSurname).Contains(request.SearchText, StringComparison.OrdinalIgnoreCase))
            //    .OrderByDescending(i => i.Id)
            //    .ToListAsync();

            //List<TblSite> sites = await _Context.TblSite.ToListAsync();

            //response.Invoices = invoices.Select(i=> InvoiceUtility.MapToInvoiceDto(i, sites.SingleOrDefault(s=>s.SiteId == i.Courier.SiteId)));

            response.Data = await Context.CourierInvoices
                .Where(i => i.Id.ToString().Contains(request.SearchText)
                            || i.Courier.Code.Contains(request.SearchText)
                            || (i.Courier.UccrName + ' ' + i.Courier.UccrSurname).Contains(request.SearchText))
                .OrderByDescending(i => i.Id)
                .Select(i => new TransactionSearchDto()
                {
                    Number = i.Id.ToString(),
                    Created = i.Created,
                    Courier = new CourierDto()
                    {
                        Id = i.Courier.UccrId,
                        Code = i.Courier.Code,
                        FirstName = i.Courier.UccrName,
                        Surname = i.Courier.UccrSurname
                    }
                })
                .ToListAsync();

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetOnHold(Guid messageId)
        {
            BaseResponse response = new BaseResponse(messageId);

            var invoices = await Context.CourierInvoices
                .Include(i => i.Courier)
                .ThenInclude(c => c.Region)
                .Include(i => i.CourierInvoiceLines)
                .Include(i => i.CourierDeductions)
                .ThenInclude(i => i.CourierDeductionLines)
                .Where(i => i.CourierInvoiceBatchItem == null)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            response.Data = invoices.Select(i => InvoiceUtility.MapToDto(i));

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetByCourier(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var invoices = await Context.CourierInvoices
                .Include(i => i.Courier)
                .ThenInclude(c => c.Region)
                .Include(i => i.CourierInvoiceLines)
                .Include(i => i.CourierDeductions)
                .ThenInclude(i => i.CourierDeductionLines)
                .Where(i=>i.CourierId == request.Id)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            response.Data = invoices.Select(i=> InvoiceUtility.MapToDto(i));

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetPendingByCourier(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var invoices = await Context.CourierInvoices
                .Include(i => i.Courier)
                .ThenInclude(c => c.Region)
                .Include(i => i.CourierInvoiceLines)
                .Include(i => i.CourierDeductions)
                .ThenInclude(i => i.CourierDeductionLines)
                .Where(i => i.CourierId == request.Id && ( i.CourierInvoiceBatchItem == null || i.CourierInvoiceBatchItem.Batch.StatusId != 3))
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            response.Data = invoices.Select(i => InvoiceUtility.MapToDto(i));

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetCompletedByCourier(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var invoices = await Context.CourierInvoices
                .Include(i => i.Courier)
                .ThenInclude(c => c.Region)
                .Include(i => i.CourierInvoiceLines)
                .Include(i => i.CourierDeductions)
                .ThenInclude(d => d.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .Where(i => i.CourierId == request.Id && i.CourierInvoiceBatchItem.Batch.StatusId == 3)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            response.Data = invoices.Select(i => InvoiceUtility.MapToDto(i));

            response.Success = true;

            return response;
        }

        public async Task<UninvoicedJobsResponse> GetUninvoicedJobsByCourier(IdentifierRequest request)
        {
            return new UninvoicedJobsResponse(request.MessageId)
            {

                Jobs = await invoiceLineJobRepository.GetUninvoicedJobs(request.Id),
                Success = true
            };
        }
    }
}
