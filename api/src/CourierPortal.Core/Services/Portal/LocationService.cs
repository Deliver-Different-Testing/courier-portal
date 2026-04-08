using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Locations;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalLocationService(IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        public async Task<LocationsResponse> GetAll(Guid messageId)
        {
            return new LocationsResponse(messageId)
            {
                Locations = await Context.TblBulkRegions
                    .OrderBy(r => r.Name)
                    .Select(r => new LocationDto() { Name = r.Name, ApplicantEnabled = r.CourierApplicantEnabled.HasValue && r.CourierApplicantEnabled.Value })
                    .ToListAsync(),
                Success = true
            };
        }
    }
}
