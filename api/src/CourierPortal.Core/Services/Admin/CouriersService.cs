using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Couriers;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Couriers;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminCourierService(IDbContextFactory<DespatchContext> contextFactory):AdminBaseService(contextFactory)
    {
        public async Task<CourierResponse> Get(IdentifierRequest request)
        {
            CourierResponse response = new CourierResponse(request.MessageId);

            TucCourier courier = await Context.TucCouriers
                .Include(c => c.Region)
                .SingleOrDefaultAsync(c => c.UccrId == request.Id && !c.UccrInternal);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier '{request.Id}' not found." });
                return response;
            }

            response.Courier = CourierUtility.MapToCourierDto(courier);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Search(SearchRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            List<TucCourier> couriers = await Context.TucCouriers
                .Include(c => c.Region)
                .Where(c => c.Active 
                        && !c.UccrInternal
                        && (c.Code.ToLower().Contains(request.SearchText.ToLower()) || (c.UccrName + " " + c.UccrSurname).ToLower().Contains(request.SearchText.ToLower()))
                )
                .OrderBy(c => c.UccrName + " " + c.UccrSurname)
                .ThenBy(c => c.Code)
                .ToListAsync();

            response.Data = couriers.Select(CourierUtility.MapToCourierDto);

            response.Success = true;
            return response;
        }

        public async Task<BaseResponse> LinkAsync(LinkRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var dbData = await Context.TucCouriers.FirstOrDefaultAsync(c => c.UccrId == request.Id);

            if (dbData == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Not found." });
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dbData.OpenForceNumber))
            {
                response.Messages.Add(new MessageDto() { Message = $"A link already exists." });
                return response;
            }

            dbData.OpenForceNumber = string.IsNullOrWhiteSpace(request.ExternalId) ? null : request.ExternalId.Trim();

            await Context.SaveChangesAsync();

            response.Success = true;
            return response;
        }

        public async Task<BaseResponse> GetAllUrgentArmy(Guid messageId)
        {
            BaseResponse response = new BaseResponse(messageId);

            var couriers = await Context.TucCouriers
                .Include(c => c.Region)
                .Where(c => c.Active && !c.UccrInternal)
                .OrderBy(c => c.UccrName + " " + c.UccrSurname)
                .ThenBy(c => c.Code)
                .ToListAsync();

            response.Data = couriers.Select(CourierUtility.MapToCourierDto);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetByLocation(CouriersByLocationRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            List<TucCourier> couriers = await Context.TucCouriers
                .Include(c => c.Region)
                .Where(c => c.Active && !c.UccrInternal && c.Region.Name == request.Location)
                .OrderBy(c => c.UccrName + " " + c.UccrSurname)
                .ThenBy(c => c.Code)
                .ToListAsync();

            response.Data = couriers.Select(CourierUtility.MapToCourierDto);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetByFleet(CouriersByFleetRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            List<TucCourier> couriers = await Context.TucCouriers
                .Include(c => c.Region)
                .Where(c => c.Active && !c.UccrInternal && c.CourierFleet.UccfName == request.Fleet)
                .ToListAsync();

            response.Data = couriers.Select(CourierUtility.MapToCourierDto);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetCourierTypes(Guid messageid)
        {
            return new BaseResponse(messageid)
            {
                Data = await Context.CourierTypes
                .Select(x => new NameIdDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(),

                Success = true
            };
        }


        public async Task<BaseResponse> GetMasters(Guid messageid)
        {
            return new BaseResponse(messageid)
            {
                Data = await Context.TucCouriers
                .Where(c => c.CourierTypeId == 2)
                .Select(x => new NameIdDto
                {
                    Id = x.UccrId,
                    Name = $"{x.UccrName} {x.UccrSurname}"
                })
                .ToListAsync(),

                Success = true
            };
        }

    }
}
