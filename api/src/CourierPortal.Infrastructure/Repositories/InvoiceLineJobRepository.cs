using System.Collections.Generic;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Repositories
{
    public class InvoiceLineJobRepository(IDbContextFactory<DespatchContext> contextFactory) : BaseRepository(contextFactory)
    {
        public async Task<IEnumerable<TransactionLineJobDto>> GetJobsByInvoice(long invoiceId)
        {
            IEnumerable<TransactionLineJobDto> results = null;

            await Context.LoadStoredProc("CM_stpJobsByInvoice")
                .WithSqlParam("InvoiceId", invoiceId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<TransactionLineJobDto>();
                });

            return results;
        }

        public async Task<IEnumerable<UninvoicedJobDto>> GetUninvoicedJobs(int courierId)
        {
            IEnumerable<UninvoicedJobDto> results = null;

            await Context.LoadStoredProc("CM_stpUninvoicedJobs")
                .WithSqlParam("CourierId", courierId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<UninvoicedJobDto>();
                });

            return results;
        }
    }
}