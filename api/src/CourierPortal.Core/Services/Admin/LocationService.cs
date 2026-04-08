using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Locations;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminLocationService(IDbContextFactory<DespatchContext> contextFactory):AdminBaseService(contextFactory)
    {
        public async Task<BaseResponse> GetAll(Guid messageId)
        {
            return new BaseResponse(messageId)
            {
                Data = await Context.TblBulkRegions
                        .OrderBy(r => r.Name)
                        .Select(r => new LocationDto() { Id = r.BulkRegionId, Name = r.Name, ApplicantEnabled = r.CourierApplicantEnabled.HasValue && r.CourierApplicantEnabled.Value })
                        .ToListAsync(),
                Success = true
            };
        }

        public async Task<BaseResponse> Enable(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            TblBulkRegion location = await Context.TblBulkRegions.SingleOrDefaultAsync(s => s.BulkRegionId == request.Id);

            if (location is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Location '{request.Id}' not found." });
                return response;
            }

            if (location.CourierApplicantEnabled != true)
            {
                location.CourierApplicantEnabled = true;
                await Context.SaveChangesAsync();
            }

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Disable(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            TblBulkRegion location = await Context.TblBulkRegions.SingleOrDefaultAsync(s => s.BulkRegionId == request.Id);

            if (location is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Location '{request.Id}' not found." });
                return response;
            }

            if (location.CourierApplicantEnabled != false)
            {
                location.CourierApplicantEnabled = false;
                await Context.SaveChangesAsync();
            }

            response.Success = true;

            return response;
        }
    }
}
