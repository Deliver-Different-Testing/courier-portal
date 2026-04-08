using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Fleets;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class FleetService(IDbContextFactory<DespatchContext> contextFactory):AdminBaseService(contextFactory)
    {
        public async Task<FleetsResponse> GetAll(Guid messageId)
        {
            return new FleetsResponse(messageId)
            {
                Fleets = await Context.TucCourierFleets
                    //.Where(f => f.AllowCourierPortalAccess.HasValue && f.AllowCourierPortalAccess.Value)
                    .OrderBy(f => f.UccfName)
                    .Select(f => new FleetDto() { Name = f.UccfName })
                    .ToListAsync(),
                Success = true
            };
        }
    }
}
