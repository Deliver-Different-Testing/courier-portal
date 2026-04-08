using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Couriers;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class SettingsService(IDbContextFactory<DespatchContext> contextFactory, OpenforceService openforceService) : AdminBaseService(contextFactory)
    {
        public async Task<BaseResponse> GetAsync(Guid messageId)
        {
            return new BaseResponse(messageId)
            {
                Data = new
                {
                    CountryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstOrDefaultAsync(),
                    OpenforceConnected = await openforceService.IsConnectedAsync()
                },
                Success = true
            };
        }
    }
}
