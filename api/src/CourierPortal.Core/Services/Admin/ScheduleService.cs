using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Schedules;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminScheduleService(IDbContextFactory<DespatchContext> contextFactory, AdminTimeZoneService timeZoneService):AdminBaseService(contextFactory)
    {
        public async Task<ScheduleSummariesResponse> GetSummariesByBookDate(SchedulesByBookDateRequest request)
        {
            ScheduleSummariesResponse response = new ScheduleSummariesResponse(request.MessageId);

            request.BookDate = request.BookDate.Kind == DateTimeKind.Utc
                ? timeZoneService.ConvertUtcToTenantTime(request.BookDate)
                : request.BookDate;

            IEnumerable<CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.Location)
                .Include(s => s.CourierScheduleResponses)
                .Where(s => s.BookDate.Date == request.BookDate.Date)
                .ToListAsync();

            IEnumerable<CourierScheduleTimeSlot> timeSlots = await Context.CourierScheduleTimeSlots
                .Include(t => t.Location)
                .Include(t => t.CourierScheduleTimeSlotVehicleTypes)
                .ThenInclude(v => v.VehicleType)
                .Where(t => t.BookDateTime.Date == request.BookDate.Date)
                .ToListAsync();

            //var jobs = await Context.TblJobs
            //    .Where(j => j.Date.Value.Date == request.BookDate.Date)
            //    .Select(j => new LocationSummaryJobDto { JobId = j.JobId, RunName = j.RunName, Void = j.Void, CourierId = j.CourierId, LocationId = j.DepotId })
            //    .ToListAsync();

            List<TucCourier> couriers = await Context.TucCouriers
                .Where(c => c.Active && !c.UccrInternal)
                .ToListAsync();

            response.Summaries = schedules
                .Select(s => s.Location)
                .Distinct()
                .OrderBy(l => l?.Name ?? "Unassigned")
                .Select(l => new LocationSummaryDto()
                {
                    Location = l?.Name ?? "Unassigned",
                    TotalCouriers = couriers.Count(c => c.RegionId == l?.BulkRegionId),
                    TotalAvailable = couriers.Count(c => c.RegionId == l?.BulkRegionId && schedules.Any(s => s.LocationId == c.RegionId && s.CourierScheduleResponses.Any(r => r.CourierId == c.UccrId && r.StatusId == 1))),
                    //TotalRuns = jobs.Where(j => j.LocationId == l?.BulkRegionId && (!j.Void.HasValue || !j.Void.Value)).GroupBy(j => j.RunName).Count(x => !string.IsNullOrWhiteSpace(x.Key)),
                    //TotalPointToPoint = jobs.Where(j => j.LocationId == l?.BulkRegionId && (!j.Void.HasValue || !j.Void.Value)).Count(j => string.IsNullOrWhiteSpace(j.RunName)),
                    //TotalJobs = jobs.Count(j => j.LocationId == l?.BulkRegionId && (!j.Void.HasValue || !j.Void.Value)),
                    ScheduleSummaries = schedules
                        .Where(s => s.LocationId == l?.BulkRegionId)
                        .Select(s => new ScheduleSummaryDto()
                        {
                            Id = s.Id,
                            Created = s.Created,
                            BookDate = s.BookDate,
                            Location = s.Location?.Name ?? "Unassigned",
                            Name = s.Name,
                            NotificationSent = s.NotificationSent,
                            StartTime = s.StartTime.ToTimeSpan(),
                            EndTime = s.EndTime.ToTimeSpan(),
                            Wanted = s.Wanted,
                            Available = s.CourierScheduleResponses.Count(r => r.StatusId == 1 && couriers.Any(c => c.UccrId == r.CourierId)),
                            VehicleSummaries = couriers
                                .Where(c => c.RegionId == l?.BulkRegionId)
                                .GroupBy(c => c.UccrVehicle)
                                .Select(v => new VehicleSummaryDto()
                                {
                                    Vehicle = v.Key ?? "Unassigned",
                                    Available = s.CourierScheduleResponses.Count(r =>
                                        r.StatusId == 1 && v.Any(x => x.UccrId == r.CourierId)),
                                    Total = v.Count()
                                })
                        }),
                    TimeSlots = timeSlots
                        .Where(s => s.LocationId == l?.BulkRegionId)
                        .OrderBy(t => t.BookDateTime)
                        .Select(t => new TimeSlotVehicleDto()
                        {
                            Id = t.Id,
                            Location = t.Location?.Name ?? "Unassigned",
                            BookDateTime = t.BookDateTime,
                            Wanted = t.Wanted,
                            VehicleTypes = t.CourierScheduleTimeSlotVehicleTypes
                                .OrderBy(v => v.VehicleType.Name)
                                .Select(v => v.VehicleType.Name)
                        })
                });

            response.Success = true;

            return response;
        }

        public async Task<SchedulesResponse> Create(SchedulesCreateRequest request)
        {
            SchedulesResponse response = new SchedulesResponse(request.MessageId);

            //Get all schedules by book dates in the request
            IEnumerable<CourierSchedule> existingSchedulesByBookDate = Context.CourierSchedules
                .Include(s => s.Location)
                .Where(s => request.Schedules.Select(x => x.BookDate.Date).Contains(s.BookDate.Date));

            //Verify that the schedule does not already exists
            if (request.Schedules.Any(s => existingSchedulesByBookDate.Any(x => x.BookDate.Date == s.BookDate.Date && x.Location.Name == s.Location && x.Name.Trim().ToLower() == s.Name.Trim().ToLower())))
            {
                response.Messages.Add(new MessageDto() { Message = $"Schedule for book date, location and name already exists." });
                return response;
            }

            //Get all locations in the request
            List<TblBulkRegion> locations = await Context.TblBulkRegions
                .Where(s => request.Schedules.Select(x => x.Location).Contains(s.Name))
                .ToListAsync();

            //Verify all locations from the request is found
            if (request.Schedules.Select(s => s.Location).Distinct().Count() != locations.Count())
            {
                response.Messages.Add(new MessageDto() { Message = $"Unknown location." });
                return response;
            }

            //Create a list of new schedules to be added
            var tenantTime = timeZoneService.GetTenantTime();
            List<CourierSchedule> schedules = request.Schedules
                .Select(s => new CourierSchedule()
                {
                    Created = tenantTime,
                    BookDate = s.BookDate.Date, //Short Date only
                    Location = locations.Single(x => x.Name == s.Location),
                    Name = s.Name,
                    StartTime = TimeOnly.FromTimeSpan(s.StartTime),
                    EndTime = TimeOnly.FromTimeSpan(s.EndTime),
                    Wanted = s.Wanted
                })
                .ToList();

            //Add the new schedules and save
            if (schedules.Any())
            {
                Context.CourierSchedules.AddRange(schedules);
                await Context.SaveChangesAsync();
            }

            //Populate response and return
            response.Schedules = schedules.Select(s => new ScheduleDto()
            {
                Id = s.Id,
                Created = s.Created,
                BookDate = s.BookDate,
                Location = s.Location.Name,
                Name = s.Name,
                NotificationSent = s.NotificationSent,
                StartTime = s.StartTime.ToTimeSpan(),
                EndTime = s.EndTime.ToTimeSpan(),
                Wanted = s.Wanted
            });

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Delete(IdentifierLongRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            //Get all schedules by book dates in the request
            CourierSchedule schedule = Context.CourierSchedules
                .FirstOrDefault(s => s.Id == request.Id);

            //Verify that the schedule does not already exists
            if (schedule == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Schedule not found." });
                return response;
            }

            if (schedule.NotificationSent.HasValue)
            {
                response.Messages.Add(new MessageDto() { Message = $"Deleting a schedule when notification has been sent is not permitted." });
                return response;
            }

            Context.CourierSchedules.Remove(schedule);

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        public async Task<TimeSlotResponse> TimeSlotCreate(TimeSlotCreateRequest request)
        {
            TimeSlotResponse response = new TimeSlotResponse(request.MessageId);

            request.BookDateTime = request.BookDateTime.Kind == DateTimeKind.Utc
                ? timeZoneService.ConvertUtcToTenantTime(request.BookDateTime)
                : request.BookDateTime;

            List <CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.Location)
                .Where(s => s.BookDate.Date == request.BookDateTime.Date && s.Location.Name.Trim().ToLower() == request.Location.Trim().ToLower()).ToListAsync();

            if (!schedules.Any(s => s.StartTime.ToTimeSpan() <= request.BookDateTime.TimeOfDay && s.EndTime.ToTimeSpan() > request.BookDateTime.TimeOfDay))
                return ResponseUtility.AddMessageAndReturnResponse(response, "No schedules found at this time slot.");

            if (await Context.CourierScheduleTimeSlots
                .AnyAsync(t => t.BookDateTime == request.BookDateTime 
                          && (!request.VehicleTypes.Any() || !t.CourierScheduleTimeSlotVehicleTypes.Any() || t.CourierScheduleTimeSlotVehicleTypes.Any(v => request.VehicleTypes.Contains(v.VehicleType.Name)))))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Conflicting time slot.");

            List<VehicleType> vehicleTypes = await Context.VehicleTypes
                .Where(v => request.VehicleTypes.Contains(v.Name))
                .ToListAsync();

            if (request.VehicleTypes.Count() != vehicleTypes.Count())
                return ResponseUtility.AddMessageAndReturnResponse(response, "Invalid vehicle types.");

            CourierScheduleTimeSlot timeSlot = new CourierScheduleTimeSlot()
            {
                Location = schedules.First().Location,
                BookDateTime = request.BookDateTime,
                Wanted = request.Wanted,
                CourierScheduleTimeSlotVehicleTypes = vehicleTypes
                    .Select(v => new CourierScheduleTimeSlotVehicleType()
                    {
                        VehicleType = v
                    })
                    .ToList()
            };

            Context.CourierScheduleTimeSlots.Add(timeSlot);

            await Context.SaveChangesAsync();

            response.TimeSlot = new TimeSlotVehicleDto()
            {
                Id = timeSlot.Id,
                Location = schedules.First().Location.Name,
                BookDateTime = timeSlot.BookDateTime,
                Wanted = timeSlot.Wanted,
                VehicleTypes = timeSlot.CourierScheduleTimeSlotVehicleTypes
                    .OrderBy(v => v.VehicleType.Name)
                    .Select(v => v.VehicleType.Name)
            };

            response.Success = true;

            return response;
        }

        public async Task<TimeSlotResponse> TimeSlotUpdate(TimeSlotUpdateRequest request)
        {
            TimeSlotResponse response = new TimeSlotResponse(request.MessageId);

            request.BookDateTime = request.BookDateTime.Kind == DateTimeKind.Utc
                ? timeZoneService.ConvertUtcToTenantTime(request.BookDateTime)
                : request.BookDateTime;

            CourierScheduleTimeSlot timeSlot = Context.CourierScheduleTimeSlots
                .Include(t => t.Location)
                .Include(t => t.CourierScheduleTimeSlotVehicleTypes)
                .ThenInclude(v => v.VehicleType)
                .Include(t => t.CourierScheduleResponses)
                .ThenInclude(r => r.Schedule)
                .Include(t => t.CourierScheduleResponses)
                .ThenInclude(r => r.Courier)
                .FirstOrDefault(t => t.Id == request.Id);

            if (timeSlot == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot not found.");

            var tenantTime = timeZoneService.GetTenantTime();
            if (timeSlot.BookDateTime <= tenantTime)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot is in the past.");

            if (timeSlot.BookDateTime == request.BookDateTime)
            {
                response.Success = true;
                return response;
            }

            //Check for at least one schedule with this time
            if (!await Context.CourierSchedules.AnyAsync(s => s.BookDate.Date == request.BookDateTime.Date && s.LocationId == timeSlot.LocationId && s.StartTime.ToTimeSpan() <= request.BookDateTime.TimeOfDay && s.EndTime.ToTimeSpan() > request.BookDateTime.TimeOfDay))
                return ResponseUtility.AddMessageAndReturnResponse(response, "No schedules found at this time slot.");

            //Check for any conflicting schedules with same time and vehicle type 
            if (await Context.CourierScheduleTimeSlots
                .AnyAsync(t => t.Id != timeSlot.Id 
                               && t.BookDateTime == request.BookDateTime
                               && (!timeSlot.CourierScheduleTimeSlotVehicleTypes.Any() || !t.CourierScheduleTimeSlotVehicleTypes.Any() || t.CourierScheduleTimeSlotVehicleTypes.Any(v => timeSlot.CourierScheduleTimeSlotVehicleTypes.Select(x => x.VehicleTypeId).Contains(v.VehicleTypeId)))))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Conflicting time slot.");

            //Inform couriers via SMS of time slot change
            var messages = timeSlot.CourierScheduleResponses
                .Where(r => r.Courier.Active && (!string.IsNullOrWhiteSpace(r.Courier.PersonalMobile) || !string.IsNullOrWhiteSpace(r.Courier.UccrMobile)))
                .Select(r => new TucManualMessage()
                {
                    UcmmDate = tenantTime,
                    UcmmStaffId = 0,
                    UcmmAttempts = 0,
                    SendToMobile = (string.IsNullOrWhiteSpace(r.Courier.PersonalMobile) ? r.Courier.UccrMobile.Trim() : r.Courier.PersonalMobile.Trim()).Replace("+64", "0").Replace(" ", string.Empty),
                    Subject = $"SMS to Courier: {r.Courier.Code}",
                    UcmmMessage = $"Schedule: {request.BookDateTime.Date.ToLongDateString()} ({r.Schedule.Name?.Trim()}) time slot has been changed from {timeSlot.BookDateTime.ToShortTimeString()} to {request.BookDateTime.ToShortTimeString()}.  https://hub.urgent.deliverdifferent.com/"
                })
                .ToList();

            if (messages.Any())
                Context.TucManualMessages.AddRange(messages);

            timeSlot.BookDateTime = request.BookDateTime;
            await Context.SaveChangesAsync();

            response.TimeSlot = new TimeSlotVehicleDto()
            {
                Id = timeSlot.Id,
                Location = timeSlot.Location?.Name ?? "Unassigned",
                BookDateTime = timeSlot.BookDateTime,
                Wanted = timeSlot.Wanted,
                VehicleTypes = timeSlot.CourierScheduleTimeSlotVehicleTypes
                    .OrderBy(v => v.VehicleType.Name)
                    .Select(v => v.VehicleType.Name)
            };

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> TimeSlotDelete(IdentifierLongRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierScheduleTimeSlot timeSlot = await Context.CourierScheduleTimeSlots
                .Include(t => t.CourierScheduleResponses)
                .Include(t => t.CourierScheduleTimeSlotVehicleTypes)
                .FirstOrDefaultAsync(t => t.Id == request.Id);

            if (timeSlot == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot not found.");

            if (timeSlot.BookDateTime <= DateTime.Now)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Time Slot is in the past.");

            if (timeSlot.CourierScheduleTimeSlotVehicleTypes.Any())
                Context.CourierScheduleTimeSlotVehicleTypes.RemoveRange(timeSlot.CourierScheduleTimeSlotVehicleTypes);

            //Deleting a time slot will put any couriers with this time slot on reserve
            Context.CourierScheduleTimeSlots.Remove(timeSlot);

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        public async Task<CouriersByScheduleResponse> GetCouriersBySchedule(IdentifierLongRequest request)
        {
            CouriersByScheduleResponse response = new CouriersByScheduleResponse(request.MessageId);

            CourierSchedule schedule = await Context.CourierSchedules
                .Include(s => s.Location)
                .Include(s => s.CourierScheduleResponses)
                .ThenInclude(r => r.TimeSlot.Location)
                .Include(s => s.CourierScheduleResponses)
                .ThenInclude(r => r.Status)
                .FirstOrDefaultAsync(s => s.Id == request.Id);

            if (schedule == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Schedule not found." });
                return response;
            }

            //If no notification has been sent for this schedule then it is not active yet, no couriers
            if (!schedule.NotificationSent.HasValue)
            {
                response.Success = true;
                return response;
            }

            List<TucCourier> couriers = await Context.TucCouriers
                .Include(c => c.Region)
                .Where(c => c.Active 
                        && !c.UccrInternal
                        && c.RegionId == schedule.LocationId)
                .ToListAsync();

            //var jobs = await Context.TblJobs
            //    .Where(j => j.Date.Value.Date == schedule.BookDate.Date)
            //    .Select(j => new { j.JobId, j.CourierId })
            //    .ToListAsync();

            response.CourierResponses = couriers
                .Select(c => new CourierByScheduleDto()
                {
                    Courier = CourierUtility.MapToCourierDto(c),
                    ScheduleResponse = schedule.CourierScheduleResponses
                        .Where(r => r.CourierId == c.UccrId)
                        .Select(r => new ScheduleResponseDto()
                        {
                            Id = r.Id,
                            Created = r.Created,
                            Updated = r.Updated,
                            StatusId = r.Status.Id,
                            Status = r.Status.Name,
                            TimeSlot = r.TimeSlot != null
                                ? new TimeSlotDto()
                                {
                                    Id = r.TimeSlot.Id,
                                    Location = r.TimeSlot.Location?.Name ?? "Unassigned",
                                    BookDateTime = r.TimeSlot.BookDateTime,
                                    Wanted = r.TimeSlot.Wanted
                                }
                                : null
                        })
                        .FirstOrDefault()
                    //JobsAssigned = jobs.Count(j => j.CourierId == c.UccrId)
                });

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> SendNotifications(NotificationsRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var tenantTime = timeZoneService.GetTenantTime();
            List<CourierSchedule> schedules = await Context.CourierSchedules
                .Where(s => s.BookDate >= tenantTime.Date)
                .ToListAsync();

            //Filter out valid new schedules, couriers can only apply for schedules that has not started yet
            IEnumerable<CourierSchedule> newSchedules = schedules
                .Where(s => !ScheduleUtility.HasStarted(s, tenantTime)
                            && !s.NotificationSent.HasValue
                            && request.Ids.Contains(s.Id));

            if (newSchedules.Any())
            {
                //Update new schedules with a notification date and compile list of unique location Ids for SMS messages
                List<int> newScheduleLocationIds = new List<int>();
                foreach (var schedule in newSchedules)
                {
                    schedule.NotificationSent = tenantTime;

                    if (schedule.LocationId.HasValue && !newScheduleLocationIds.Contains(schedule.LocationId.Value))
                        newScheduleLocationIds.Add(schedule.LocationId.Value);
                }

                var courierMobiles = await Context.TucCouriers
                    .Where(c => c.Active 
                        && !c.UccrInternal
                        && c.RegionId.HasValue 
                        && newScheduleLocationIds.Contains(c.RegionId.Value))
                    .Select(c => new { c.Code, c.PersonalMobile, c.UccrMobile })
                    .ToListAsync();

                //Filter out couriers who have no mobile numbers and create message for couriers who will be notified about a new schedule in their region
                List<TucManualMessage> newScheduleMessages = courierMobiles
                    .Where(c => !string.IsNullOrWhiteSpace(c.PersonalMobile) || !string.IsNullOrWhiteSpace(c.UccrMobile))
                    .Select(c => new TucManualMessage()
                    {
                        UcmmDate = tenantTime,
                        UcmmStaffId = 0,
                        UcmmAttempts = 0,
                        SendToMobile = (string.IsNullOrWhiteSpace(c.PersonalMobile) ? c.UccrMobile : c.PersonalMobile).Replace("+64", "0").Replace(" ", string.Empty),
                        UcmmMessage = "The schedule for your location has been updated.  Please login to the Courier Portal to view and confirm your availability.  https://hub.urgent.deliverdifferent.com",
                        Subject = $"SMS to Courier: {c.Code}",
                    })
                    .ToList();

                if (newScheduleMessages.Any())
                    Context.TucManualMessages.AddRange(newScheduleMessages);

                await Context.SaveChangesAsync();
            }

            response.Success = true;

            return response;
        }

        public async Task<SchedulesResponse> GetScheduleNotifications(Guid messageId)
        {
            SchedulesResponse response = new SchedulesResponse(messageId);

            var tenantTime = timeZoneService.GetTenantTime();
            List<CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.Location)
                .Where(s => s.BookDate >= tenantTime.Date)
                .ToListAsync();

            response.Schedules = schedules
                .Where(s => !s.NotificationSent.HasValue && !ScheduleUtility.HasStarted(s, tenantTime))
                .Select(s => new ScheduleDto()
                {
                    Id = s.Id,
                    Created = s.Created,
                    BookDate = s.BookDate,
                    Location = s.Location?.Name ?? "Unassigned",
                    Name = s.Name,
                    NotificationSent = s.NotificationSent,
                    StartTime = s.StartTime.ToTimeSpan(),
                    EndTime = s.EndTime.ToTimeSpan(),
                    Wanted = s.Wanted
                });

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> ScheduleResponseTimeSlotAssign(ScheduleResponseTimeSlotAssignRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierScheduleResponse scheduleResponse = await Context.CourierScheduleResponses
                .Include(r => r.Schedule)
                .Include(r => r.TimeSlot)
                .Include(r => r.Courier)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (scheduleResponse == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Schedule response not found.");


            if (scheduleResponse.TimeSlotId == request.TimeSlotId)
            {
                response.Success = true;
                return response;
            }

            if (!scheduleResponse.Courier.Active)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Courier is inactive.");

            if (scheduleResponse.StatusId != 1)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Time slot assignment for a schedule response with a status of '{scheduleResponse.Status.Name}' is not permitted.");

            CourierScheduleTimeSlot timeSlot = request.TimeSlotId.HasValue
                ? await Context.CourierScheduleTimeSlots
                    .Include(t => t.CourierScheduleTimeSlotVehicleTypes)
                    .ThenInclude(v => v.VehicleType)
                    .FirstOrDefaultAsync(t => t.Id == request.TimeSlotId.Value)
                : null;

            if (request.TimeSlotId.HasValue && timeSlot == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Time slot not found.");

            var tenantTime = timeZoneService.GetTenantTime();
            if (timeSlot != null && timeSlot.BookDateTime <= tenantTime)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Time slot must not be in the past.");

            if (timeSlot != null 
                && (timeSlot.BookDateTime.Date != scheduleResponse.Schedule.BookDate.Date 
                    || timeSlot.BookDateTime.TimeOfDay < scheduleResponse.Schedule.StartTime.ToTimeSpan() 
                    || timeSlot.BookDateTime.TimeOfDay >= scheduleResponse.Schedule.EndTime.ToTimeSpan()))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Invalid time slot for schedule.");

            if (timeSlot != null && timeSlot.CourierScheduleTimeSlotVehicleTypes.Any() && timeSlot.CourierScheduleTimeSlotVehicleTypes.All(v => v.VehicleType.Name.Trim().ToLower() != scheduleResponse.Courier.UccrVehicle?.Trim().ToLower()))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Invalid time slot for vehicle type.");

            //If not changing to reserve then inform the courier of the new time slot if there is a mobile number
            if (timeSlot != null && !string.IsNullOrWhiteSpace(scheduleResponse.Courier.PersonalMobile) || !string.IsNullOrWhiteSpace(scheduleResponse.Courier.UccrMobile))
                Context.TucManualMessages.Add(new TucManualMessage()
                {
                    UcmmDate = tenantTime,
                    UcmmStaffId = 0,
                    UcmmAttempts = 0,
                    SendToMobile = (string.IsNullOrWhiteSpace(scheduleResponse.Courier.PersonalMobile) ? scheduleResponse.Courier.UccrMobile.Trim() : scheduleResponse.Courier.PersonalMobile.Trim()).Replace("+64", "0").Replace(" ", string.Empty),
                    Subject = $"SMS to Courier: {scheduleResponse.Courier.Code}",
                    UcmmMessage = $"Schedule: {scheduleResponse.Schedule.BookDate.Date.ToLongDateString()} ({scheduleResponse.Schedule.Name?.Trim()}) time slot has been changed"
                                  + (scheduleResponse.TimeSlot == null ? string.Empty : $" from {scheduleResponse.TimeSlot.BookDateTime.ToShortTimeString()}")
                                  + $" to {timeSlot.BookDateTime.ToShortTimeString()}.  https://hub.urgent.deliverdifferent.com/"
                });

            //Update time slot
            scheduleResponse.TimeSlotId = timeSlot?.Id;

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

        public async Task<BaseResponse> ScheduleResponseStatusUpdate(ScheduleResponseStatusUpdateRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            List<CourierScheduleResponse> scheduleResponsesToUpdate = await Context.CourierScheduleResponses
                .Include(r => r.Schedule)
                .Include(r => r.Courier)
                .Where(r => request.Ids.Contains(r.Id))
                .ToListAsync();

            if (request.Ids.Count() != scheduleResponsesToUpdate.Count())
            {
                response.Messages.Add(new MessageDto() { Message = $"Schedule response(s) not found." });
                return response;
            }

            IEnumerable<DateTime> dates = scheduleResponsesToUpdate.Select(r => r.Schedule.BookDate.Date).Distinct();

            List<CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.Location)
                .Include(s => s.CourierScheduleResponses)
                .Where(s => dates.Contains(s.BookDate.Date))
                .ToListAsync();

            //When updating responses to available, check that there is no conflicting schedule for the courier
            if (request.StatusId == 1)
            {
                var conflictingSchedules = scheduleResponsesToUpdate
                    .Where(r => ScheduleUtility.HasConflictingSchedule(r.Schedule, schedules.Where(s => s.NotificationSent.HasValue && s.CourierScheduleResponses.Any(x => x.CourierId == r.CourierId && x.StatusId == 1))));

                if (conflictingSchedules.Any())
                {
                    response.Messages.Add(new MessageDto() { Message = $"Conflicting schedule(s) found." });
                    return response;
                }
            }

            if (scheduleResponsesToUpdate.Any())
            {
                var tenantTime = timeZoneService.GetTenantTime();
                foreach (CourierScheduleResponse scheduleResponse in scheduleResponsesToUpdate)
                {
                    if (scheduleResponse.StatusId != request.StatusId)
                    {
                        scheduleResponse.Updated = tenantTime;
                        scheduleResponse.StatusId = request.StatusId;
                        //If cancelling then clear time slot
                        if (scheduleResponse.StatusId == 3)
                        {
                            scheduleResponse.TimeSlotId = null;

                            //Inform the user of a cancelled schedule if schedule is not in the past
                            if ((scheduleResponse.Schedule.BookDate.Date > tenantTime.Date || (scheduleResponse.Schedule.BookDate.Date == tenantTime.Date && scheduleResponse.Schedule.EndTime.ToTimeSpan() > tenantTime.TimeOfDay))
                                && scheduleResponse.Courier.Active && (!string.IsNullOrWhiteSpace(scheduleResponse.Courier.PersonalMobile) || !string.IsNullOrWhiteSpace(scheduleResponse.Courier.UccrMobile)))
                                Context.TucManualMessages.Add(new TucManualMessage()
                                {
                                    UcmmDate = tenantTime,
                                    UcmmStaffId = 0,
                                    UcmmAttempts = 0,
                                    SendToMobile = (string.IsNullOrWhiteSpace(scheduleResponse.Courier.PersonalMobile)? scheduleResponse.Courier.UccrMobile.Trim() : scheduleResponse.Courier.PersonalMobile.Trim()).Replace("+64", "0").Replace(" ", string.Empty),
                                    Subject = $"SMS to Courier: {scheduleResponse.Courier.Code}",
                                    UcmmMessage = $"Schedule: {scheduleResponse.Schedule.BookDate.ToLongDateString()} ({scheduleResponse.Schedule.Name?.Trim()}) has been cancelled.  https://hub.urgent.deliverdifferent.com/"
                                });
                        }
                    }
                }

                //Save changes
                await Context.SaveChangesAsync();
            }

            response.Success = true;

            return response;
        }

        public async Task<CouriersResponse> GetCouriersByResponseStatus(CouriersByResponseStatusRequest request)
        {
            CouriersResponse response = new CouriersResponse(request.MessageId);

            var tenantTime = timeZoneService.GetTenantTime();
            List<CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.Location)
                .Include(s => s.CourierScheduleResponses)
                .ThenInclude(r => r.Courier)
                .ThenInclude(c => c.Region)
                .Include(s => s.CourierScheduleResponses)
                .ThenInclude(r => r.Courier)
                .ThenInclude(r => r.CourierFleet)
                .Where(s => s.BookDate.Date >= tenantTime.Date)
                .ToListAsync();

            if (request.StatusId == 0)
            {
                List<TucCourier> couriers = await Context.TucCouriers
                    .Include(c => c.Region)
                    .Where(c => c.Active && !c.UccrInternal)
                    .ToListAsync();

                response.Couriers = couriers
                    .Where(c => schedules.Any(s => s.NotificationSent.HasValue
                                                   && s.LocationId == c.RegionId
                                                   && s.CourierScheduleResponses.All(r => r.CourierId != c.UccrId)
                                                   && !ScheduleUtility.HasStarted(s, tenantTime)))
                    .Select(CourierUtility.MapToCourierDto);
            }
            else
                response.Couriers = schedules
                    .Where(s => s.NotificationSent.HasValue && ScheduleUtility.HasNotEnded(s, tenantTime))
                    .SelectMany(s => s.CourierScheduleResponses)
                    .Where(r => r.Courier.Active && !r.Courier.UccrInternal && r.StatusId == request.StatusId)
                    .Select(r => r.Courier)
                    .Distinct()
                    .Select(CourierUtility.MapToCourierDto);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Copy(ScheduleCopyRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            if (request.SourceDate.Kind == DateTimeKind.Utc)
                request.SourceDate = timeZoneService.ConvertUtcToTenantTime(request.SourceDate);

            if (request.DestinationDate.Kind == DateTimeKind.Utc)
                request.DestinationDate = timeZoneService.ConvertUtcToTenantTime(request.DestinationDate);

            List<CourierSchedule> schedules = await Context.CourierSchedules
                .Include(s => s.Location)
                .Where(s => s.BookDate.Date == request.SourceDate.Date && request.Locations.Contains(s.Location.Name))
                .ToListAsync();

            List<CourierScheduleTimeSlot> timeSlots = await Context.CourierScheduleTimeSlots
                .Include(t => t.CourierScheduleTimeSlotVehicleTypes)
                .Where(t => t.BookDateTime.Date == request.SourceDate.Date && request.Locations.Contains(t.Location.Name))
                .ToListAsync();

            if (request.Locations.Any(l => schedules.All(s => s.Location?.Name != l)))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid scheduled location(s).");

            DateTime destinationDate = request.DestinationDate.Kind == DateTimeKind.Utc ? timeZoneService.ConvertUtcToTenantTime(request.DestinationDate).Date : request.DestinationDate.Date;

            if (await Context.CourierSchedules.AnyAsync(s => s.BookDate.Date == destinationDate && schedules.Select(x => x.Name).Contains(s.Name)))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Schedule name(s) already exists.");
            
            var tenantTime = timeZoneService.GetTenantTime();
            Context.CourierSchedules.AddRange(schedules.Select(s => new CourierSchedule()
            {
                Created = tenantTime,
                BookDate = destinationDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                LocationId = s.LocationId,
                SiteId = s.SiteId,
                Name = s.Name,
                Wanted = s.Wanted
            }));

            if (timeSlots.Any())
                Context.CourierScheduleTimeSlots.AddRange(timeSlots.Select(t => new CourierScheduleTimeSlot()
                {
                    LocationId = t.LocationId,
                    SiteId = t.SiteId,
                    BookDateTime = destinationDate.AddTicks(t.BookDateTime.TimeOfDay.Ticks),
                    Wanted = t.Wanted,
                    CourierScheduleTimeSlotVehicleTypes = t.CourierScheduleTimeSlotVehicleTypes
                        .Select(v => new CourierScheduleTimeSlotVehicleType()
                        {
                            VehicleTypeId = v.VehicleTypeId
                        })
                        .ToHashSet()
                }));

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> SendScheduleReminders(IdentifierLongRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierSchedule schedule = await Context.CourierSchedules
                .Include(s => s.CourierScheduleResponses)
                .Include(s => s.CourierScheduleResponses)
                .ThenInclude(r => r.Courier)
                .FirstOrDefaultAsync(s => s.Id == request.Id);

            if (schedule == null)
                return ResponseUtility.AddMessageAndReturnResponse(response,"Schedule not found.");

            //If no notification has been sent for this schedule then it is not active yet, no couriers
            if (!schedule.NotificationSent.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Schedule is inactive.");

            var tenantTime = timeZoneService.GetTenantTime();
            if (ScheduleUtility.HasStarted(schedule, tenantTime))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Schedule has started or is in the past.");

            var messages = schedule.CourierScheduleResponses
                .Where(r => r.StatusId == 1 && r.Courier.Active && (!string.IsNullOrWhiteSpace(r.Courier.PersonalMobile) || !string.IsNullOrWhiteSpace(r.Courier.UccrMobile)))
                .Select(r => new TucManualMessage()
                {
                    UcmmDate = tenantTime,
                    UcmmStaffId = 0,
                    UcmmAttempts = 0,
                    SendToMobile = (string.IsNullOrWhiteSpace(r.Courier.PersonalMobile) ? r.Courier.UccrMobile.Trim() : r.Courier.PersonalMobile.Trim()).Replace("+64", "0").Replace(" ", string.Empty),
                    Subject = $"SMS to Courier: {r.Courier.Code}",
                    UcmmMessage = $"Reminder: Schedule {r.Schedule.BookDate.ToLongDateString()} ({r.Schedule.Name?.Trim()}) is confirmed{(r.TimeSlot != null? " for " + r.TimeSlot.BookDateTime.ToString("h:mm tt") : "")}.  Please ensure you are on site and ready to go.  https://hub.urgent.deliverdifferent.com/"
                })
                .ToList();

            if (messages.Any())
            {
                Context.TucManualMessages.AddRange(messages);
                await Context.SaveChangesAsync();
            }

            response.Success = true;

            return response;
        }

    }
}
