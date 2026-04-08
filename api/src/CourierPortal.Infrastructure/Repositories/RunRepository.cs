using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Repositories
{
    public class RunRepository(IDbContextFactory<DespatchContext> contextFactory) : BaseRepository(contextFactory)
    {
        public async Task<List<RunItem>> GetAvailableRuns(int courierId)
        {
            List<RunItem> results = null;

            await Context.LoadStoredProc("CP_stpAvailableRuns")
                .WithSqlParam("CourierId", courierId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<RunItem>().ToList();
                });

            return results;
        }

        public async Task<List<RunItem>> GetRunsByCourier(int courierId)
        {
            List<RunItem> results = null;

            await Context.LoadStoredProc("CP_stpCourierRuns")
                .WithSqlParam("CourierId", courierId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<RunItem>().ToList();
                });

            return results;
        }

        //public async Task<List<RunItem>> GetPendingRunsByCourier(int courierId)
        //{
        //    List<RunItem> results = null;

        //    await _Context.LoadStoredProc("CP_stpPendingRuns")
        //        .WithSqlParam("CourierId", courierId)
        //        .ExecuteStoredProcAsync((handler) =>
        //        {
        //            results = handler.ReadToList<RunItem>().ToList();
        //        });

        //    return results;
        //}

        public async Task<List<RunItem>> GetRunByCourier(int courierId, DateTime bookDate, string runName)
        {
            List<RunItem> results = null;

            await Context.LoadStoredProc("CP_stpCourierRun")
                .WithSqlParam("CourierId", courierId)
                .WithSqlParam("BookDate", bookDate)
                .WithSqlParam("RunName", runName)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<RunItem>().ToList();
                });

            return results;
        }

        public async Task<List<RunItem>> GetAvailableRun(int courierId, int runId)
        {
            List<RunItem> results = null;

            await Context.LoadStoredProc("CP_stpAvailableRun")
                .WithSqlParam("CourierId", courierId)
                .WithSqlParam("RunId", runId)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<RunItem>().ToList();
                });

            return results;
        }

        //TODO:Remove this commented out code and stored proc if still no plan to bring it back later
        //public async Task ApplyForRun(int courierId, int runId)
        //{
        //    await _Context.LoadStoredProc("CP_stpApplyForRun_Insert")
        //        .WithSqlParam("CourierId", courierId)
        //        .WithSqlParam("RunId", runId)
        //        .ExecuteStoredProcAsync((handler) => {});
        //}
    }
}
