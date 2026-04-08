using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Schedules;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalScheduleService(IDbContextFactory<DespatchContext> contextFactory, PortalTimeZoneService timeZoneService) : PortalBaseService(contextFactory)
    {
        public async Task<SchedulesResponse> Get(Guid messageId, int courierId)
        {
            SchedulesResponse response = new SchedulesResponse(messageId);

            var courier = await Context.TucCouriers
                .Select(c => new { c.UccrId, c.RegionId, c.UccrVehicle, c.Active })
                .SingleOrDefaultAsync(c => c.UccrId == courierId && c.Active);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier {courierId} not found or inactive." });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            List<CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.Location)
                .Include(s => s.CourierScheduleResponses)
                .ThenInclude(r => r.TimeSlot)
                .Where(s => s.BookDate >= tenantTime.Date && s.LocationId.HasValue && s.LocationId == courier.RegionId)
                .ToListAsync();

            List<CourierScheduleTimeSlot> timeSlots = await Context.CourierScheduleTimeSlots
                .Include(t => t.CourierScheduleTimeSlotVehicleTypes)
                .ThenInclude(v => v.VehicleType)
                .Where(t => t.BookDateTime.Date >= tenantTime.Date && t.LocationId.HasValue && t.LocationId == courier.RegionId)
                .ToListAsync();

            response.Schedules = schedules
                .Where(s => s.NotificationSent.HasValue
                            && !ScheduleUtility.HasEnded(s, tenantTime)
                            && !(ScheduleUtility.HasStarted(s, tenantTime) && s.CourierScheduleResponses.All(r => r.CourierId != courier.UccrId)))
                .Select(s => new ScheduleDto()
                {
                    Id = s.Id,
                    BookDate = s.BookDate,
                    Location = s.Location?.Name ?? "Unassigned",
                    Name = s.Name,
                    StartTime = s.StartTime.ToTimeSpan(),
                    EndTime = s.EndTime.ToTimeSpan(),
                    Wanted = s.Wanted,
                    HasTimeSlots = timeSlots.Any(t => t.BookDateTime.Date == s.BookDate.Date && t.BookDateTime.TimeOfDay >= s.StartTime.ToTimeSpan() && t.BookDateTime.TimeOfDay < s.EndTime.ToTimeSpan()),
                    TimeSlots = timeSlots
                        .Where(t => 
                            t.BookDateTime.Date == s.BookDate.Date
                            && t.BookDateTime.TimeOfDay >= s.StartTime.ToTimeSpan()
                            && t.BookDateTime.TimeOfDay < s.EndTime.ToTimeSpan()
                            && (!t.CourierScheduleTimeSlotVehicleTypes.Any() || t.CourierScheduleTimeSlotVehicleTypes.Any(v => v.VehicleType.Name.Trim().ToLower() == courier.UccrVehicle?.Trim().ToLower()))
                        )
                        .OrderBy(t => t.BookDateTime)
                        .Select(t => new TimeSlotDto()
                        {
                            Id = t.Id,
                            BookDateTime = t.BookDateTime,
                            Wanted = t.Wanted,
                            Remaining = t.Wanted - schedules.SelectMany(x => x.CourierScheduleResponses).Count(r => r.StatusId == 1 && r.TimeSlotId == t.Id)
                        }),
                    Response = s.CourierScheduleResponses
                        .Where(r => r.CourierId == courier.UccrId)
                        .Select(r => new ScheduleResponseDto()
                        {
                            StatusId = r.StatusId,
                            TimeSlot = r.TimeSlot == null? null : new TimeSlotDto()
                            {
                                Id = r.TimeSlot.Id,
                                BookDateTime = r.TimeSlot.BookDateTime
                            }
                        })
                        .FirstOrDefault()
                });

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Available(int courierId, ScheduleAvailableRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            TucCourier courier = await Context.TucCouriers.SingleOrDefaultAsync(c => c.UccrId == courierId && c.Active);

            if (courier is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Courier {courierId} not found.");

            var tenantTime = timeZoneService.GetTenantTime();
            List<CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.CourierScheduleResponses)
                .Where(s => s.BookDate >= tenantTime.Date && s.LocationId.HasValue && s.LocationId == courier.RegionId && s.NotificationSent.HasValue)
                .ToListAsync();


            CourierSchedule schedule = schedules.FirstOrDefault(s => s.Id == request.ScheduleId);

            //If no schedule then reject as it is either invalid, expired or has been cancelled for this courier
            if (schedule == null || ScheduleUtility.HasStarted(schedule, tenantTime) || schedule.CourierScheduleResponses.Any(r => r.CourierId == courierId && r.StatusId != 1 && r.StatusId != 2))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Schedule expired or not available.");

            //Check if response already exist
            if (schedule.CourierScheduleResponses.Any(r => r.CourierId == courierId && r.StatusId == 1 && r.TimeSlotId == request.TimeSlotId))
            {
                response.Success = true;
                return response;
            }

            //Look for conflicting schedules
            List<CourierSchedule> conflictingSchedules = schedules
                .Where(s => s.NotificationSent.HasValue && ScheduleUtility.isConflictingSchedule(s, schedule))
                .ToList();

            //If the courier has responded with available for a conflicting schedule then reject
            if (conflictingSchedules.Any(s => s.CourierScheduleResponses.Any(r => r.CourierId == courier.UccrId && r.StatusId == 1)))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Conflicting schedule(s): Check your available schedules and contact uadriver@urgent.co.nz if you would like to cancel a conflicting schedule.");

            CourierScheduleTimeSlot timeSlot = request.TimeSlotId.HasValue
                ? await Context.CourierScheduleTimeSlots
                    .Include(t => t.CourierScheduleResponses)
                    .Include(t => t.CourierScheduleTimeSlotVehicleTypes)
                    .ThenInclude(v => v.VehicleType)
                    .FirstOrDefaultAsync(t => t.Id == request.TimeSlotId && t.LocationId.HasValue && t.LocationId == courier.RegionId)
                : null;

            //If required, check to see if time slot is valid
            if (request.TimeSlotId.HasValue)
            {
                if (timeSlot == null)
                    return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot not found.");

                if (timeSlot.BookDateTime.Date != schedule.BookDate.Date || timeSlot.BookDateTime.TimeOfDay < schedule.StartTime.ToTimeSpan() || timeSlot.BookDateTime.TimeOfDay >= schedule.EndTime.ToTimeSpan())
                    return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot is invalid for schedule.");

                if (timeSlot.CourierScheduleTimeSlotVehicleTypes.Any() && timeSlot.CourierScheduleTimeSlotVehicleTypes.All(v => v.VehicleType.Name.Trim().ToLower() != courier.UccrVehicle?.Trim().ToLower()))
                    return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot is invalid for vehicle type.");

                //if (timeSlot.Wanted.HasValue && timeSlot.Wanted.Value <= timeSlot.CourierScheduleResponse.Count())
                //    return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot has already been filled.");
            }

            CourierScheduleResponse scheduleResponse = schedule.CourierScheduleResponses.FirstOrDefault(r => r.CourierId == courier.UccrId);
            if (scheduleResponse == null)
            {
                //Add new response with status (1)Available
                Context.CourierScheduleResponses.Add(new CourierScheduleResponse()
                {
                    Courier = courier,
                    Schedule = schedule,
                    TimeSlotId = timeSlot?.Id,
                    StatusId = 1,
                    Created = tenantTime,
                    Updated = tenantTime
                });
            } 
            else
            {
                //Update existing response to status (1)Available
                scheduleResponse.StatusId = 1;
                scheduleResponse.TimeSlotId = timeSlot?.Id;
                scheduleResponse.Updated = tenantTime;
            }

            //for conflicting schedules, add new responses with status (2)Unavailable
            List<CourierScheduleResponse> unavailableScheduleResponses = conflictingSchedules
                .Where(s => !ScheduleUtility.HasStarted(s, tenantTime) && s.CourierScheduleResponses.All(r => r.CourierId != courierId))
                .Select(s => new CourierScheduleResponse()
                {
                    Courier = courier,
                    Schedule = s,
                    StatusId = 2,
                    Created = tenantTime,
                    Updated = tenantTime
                })
                .ToList();

            if (unavailableScheduleResponses.Any())
                Context.CourierScheduleResponses.AddRange(unavailableScheduleResponses);

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                if (e.Message == "Time Slot has already been filled." || e.InnerException?.Message == "Time Slot has already been filled.")
                    return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot has already been filled.");

                throw;
            }

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Unavailable(int courierId, ScheduleUnavailableRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            TucCourier courier = await Context.TucCouriers.SingleOrDefaultAsync(c => c.UccrId == courierId && c.Active);

            if (courier is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Courier {courierId} not found.");

            CourierSchedule schedule = await Context.CourierSchedules
                .Include(s => s.CourierScheduleResponses)
                .FirstOrDefaultAsync(s => s.Id == request.ScheduleId && s.NotificationSent.HasValue && s.CourierScheduleResponses.All(r => r.CourierId != courier.UccrId));

            var tenantTime = timeZoneService.GetTenantTime();
            if (schedule == null || ScheduleUtility.HasStarted(schedule, tenantTime))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Schedule not found or expired.");

            Context.CourierScheduleResponses.Add(new CourierScheduleResponse()
            {
                Courier = courier,
                Schedule = schedule,
                StatusId = 2,
                Created = tenantTime,
                Updated = tenantTime
            });

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

    }
}
