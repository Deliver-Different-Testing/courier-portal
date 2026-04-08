using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Vehicles;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalVehicleService(IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        public async Task<VehicleTypesResponse> GetTypes(Guid messageId)
        {
            return new VehicleTypesResponse(messageId)
            {
                VehicleTypes = await Context.VehicleTypes
                    .OrderBy(v => v.Name)
                    .Select(v => new VehicleTypeDto() { Id = v.Id, Name = v.Name })
                    .ToListAsync(),
                Success = true
            };
        }
    }
}
