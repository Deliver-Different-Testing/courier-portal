using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Reports.NoShows;
//using CourierPortal.Core.DTOs.Admin.Reports.Sustainability;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminReportsService(IDbContextFactory<DespatchContext> contextFactory, AdminTimeZoneService timeZoneService) : AdminBaseService(contextFactory)
    {
        //private DateTime _CourierPaymentFieldStartDate = new DateTime(2023, 07, 01);

        #region Sustainability

        //public async Task<SustainabilityResponse> Sustainability(SustainabilityRequest request)
        //{
        //    SustainabilityResponse response = new SustainabilityResponse(request.MessageId);

        //    var jobs = await GetJobs(request.Date);

        //    var loggedInHours = await (from l in Context.TblCourierLogInOuts
        //                               where l.LogInTime.Date == request.Date.Date
        //                               select new SustainabilityCourierLoginDto() { CourierId = l.CourierId, LogInTime = l.LogInTime, LogOutTime = l.LogOutTime }).ToListAsync();

        //    var courierSummary = jobs
        //        .GroupBy(j => new { j.CourierId, j.CourierCode, j.CourierVehicle })
        //        .OrderBy(x => x.Key.CourierCode)
        //        .Select(x => new
        //        {
        //            Courier = x.Key,
        //            Total = x.Sum(j => j.Total),
        //            //Hours = Math.Round((loggedInHours.Where(l => l.CourierId == x.Key.CourierId).Any(l => l.LogInTime.Date != l.LogOutTime?.Date) || x.Max(j => j.ComplTime) < loggedInHours.Where(l => l.CourierId == x.Key.CourierId).Max(l => l.LogOutTime)
        //            //        ? x.Max(j => j.ComplTime) : loggedInHours.Where(l => l.CourierId == x.Key.CourierId).Max(l => l.LogOutTime.Value))
        //            //    .Subtract(loggedInHours.Where(l => l.CourierId == x.Key.CourierId).Min(l => l.LogInTime) > x.Min(j => j.PickUpTime)
        //            //        ? loggedInHours.Where(l => l.CourierId == x.Key.CourierId).Min(l => l.LogInTime) : x.Min(j => j.PickUpTime)).TotalHours, 2, MidpointRounding.AwayFromZero),
        //            Hours = CalculateHoursByCourierAndDate(x.Key.CourierId, request.Date, loggedInHours, x.ToList()),
        //            JobCount = x.Count()
        //        })
        //        .Where(x => x.Hours >= 1)
        //        .ToList();

        //    response.Couriers = courierSummary
        //        .Select(x => new SustainabilitySummaryDto
        //        {
        //            Name = x.Courier.CourierCode,
        //            Total = x.Total,
        //            Hours = x.Hours <= 0 ? 0.01 : x.Hours,
        //            PerHour = Math.Round(x.Total / Convert.ToDecimal(x.Hours <= 0 ? 0.01 : x.Hours), 2, MidpointRounding.AwayFromZero),
        //            JobCount = x.JobCount,
        //            PerJob = Math.Round(x.Total / x.JobCount, 2, MidpointRounding.AwayFromZero)
        //        })
        //        .OrderBy(x => x.PerHour);

        //    response.Vehicles = courierSummary
        //        .GroupBy(j => j.Courier.CourierVehicle)
        //        .Select(x => new SustainabilitySummaryDto
        //        {
        //            Name = x.Key,
        //            Total = x.Sum(c => c.Total),
        //            Hours = x.Sum(c => c.Hours) <= 0 ? 0.01 : x.Sum(c => c.Hours),
        //            PerHour = Math.Round(x.Sum(c => c.Total) / Convert.ToDecimal(x.Sum(c => c.Hours) <= 0 ? 0.01 : x.Sum(c => c.Hours)), 2, MidpointRounding.AwayFromZero),
        //            JobCount = x.Sum(c => c.JobCount),
        //            PerJob = Math.Round(x.Sum(c => c.Total) / x.Sum(c => c.JobCount), 2, MidpointRounding.AwayFromZero)
        //        });

        //    var CourierIds = courierSummary.Select(s => s.Courier.CourierId).ToList();

        //    response.Regions = jobs
        //        .Where(j => CourierIds.Contains(j.CourierId))
        //        .GroupBy(x => x.ToRegion)
        //        .OrderBy(x => x.Key)
        //        .Select(x => new SustainabilitySummaryRegionDto()
        //        {
        //            Name = string.IsNullOrWhiteSpace(x.Key) ? "Undefined" : x.Key,
        //            Total = x.Sum(j => j.Total),
        //            JobCount = x.Count(),
        //            Suburbs = x
        //                .GroupBy(j => j.ToSuburb)
        //                .Select(y => new SustainabilitySummarySuburbDto()
        //                {
        //                    Name = string.IsNullOrWhiteSpace(y.Key) ? "Undefined" : y.Key,
        //                    Total = y.Sum(z => z.Total),
        //                    JobCount = y.Count()
        //                })
        //                .OrderBy(y => y.Name)
        //        });

        //    response.Total = response.Couriers.Sum(j => j.Total);
        //    response.MinPerHour = response.Couriers.Any() ? response.Couriers.Min(c => c.PerHour) : 0;
        //    response.MaxPerHour = response.Couriers.Any() ? response.Couriers.Max(c => c.PerHour) : 0;
        //    response.AveragePerHour = response.Couriers.Any() ? Math.Round(response.Couriers.Sum(c => c.PerHour) / response.Couriers.Count(), 2, MidpointRounding.AwayFromZero) : 0;
        //    response.MinHours = response.Couriers.Any() ? response.Couriers.Min(c => c.Hours) : 0;
        //    response.MaxHours = response.Couriers.Any() ? response.Couriers.Max(c => c.Hours) : 0;
        //    response.AverageHours = response.Couriers.Any() ? Math.Round(response.Couriers.Sum(c => c.Hours) / response.Couriers.Count(), 2, MidpointRounding.AwayFromZero) : 0;

        //    response.Success = true;

        //    return response;
        //}

        //public async Task<SustainabilityByCourierResponse> SustainabilityByCourier(SustainabilityByCourierRequest request)
        //{
        //    SustainabilityByCourierResponse response = new SustainabilityByCourierResponse(request.MessageId);

        //    TucCourier courier = await Context.TucCouriers
        //        .OrderBy(c => c.Code)
        //        .ThenByDescending(c => c.Active)
        //        .FirstOrDefaultAsync(c => c.Code == request.Code.Trim() && !c.UccrInternal);

        //    if (courier is null)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Courier Not Found." });
        //        return response;
        //    }

        //    var jobs = await GetJobs(request.FromDate, request.ToDate, courier.UccrId);

        //    var loggedInHours = await (from l in Context.TblCourierLogInOuts
        //                               where l.CourierId == courier.UccrId && l.LogInTime.Date >= request.FromDate.Date && l.LogInTime.Date <= request.ToDate.Date
        //                               select new SustainabilityCourierLoginDto() { CourierId = l.CourierId, LogInTime = l.LogInTime, LogOutTime = l.LogOutTime }).ToListAsync();

        //    response.Dates = jobs
        //        .GroupBy(j => j.ComplTime.Date)
        //        .OrderBy(x => x.Key)
        //        .Select(x => new
        //        {
        //            Name = x.Key.ToString("d MMM"),
        //            //Hours = Math.Round((loggedInHours.Where(l => l.LogInTime.Date == x.Key.Date).Any(l => l.LogInTime.Date != l.LogOutTime?.Date) || x.Max(j => j.ComplTime) < loggedInHours.Where(l => l.LogInTime.Date == x.Key.Date).Max(l => l.LogOutTime)
        //            //        ? x.Max(j => j.ComplTime) : loggedInHours.Where(l => l.LogInTime.Date == x.Key.Date).Max(l => l.LogOutTime.Value))
        //            //    .Subtract(loggedInHours.Where(l => l.LogInTime.Date == x.Key.Date).Min(l => l.LogInTime) > x.Min(j => j.PickUpTime)
        //            //        ? loggedInHours.Where(l => l.LogInTime.Date == x.Key.Date).Min(l => l.LogInTime) : x.Min(j => j.PickUpTime)).TotalHours, 2, MidpointRounding.AwayFromZero),
        //            Hours = CalculateHoursByCourierAndDate(courier.UccrId, x.Key, loggedInHours, x.ToList()),
        //            Total = x.Sum(j => j.Total),
        //            JobCount = x.Count()
        //        })
        //        .Select(x => new SustainabilitySummaryDto
        //        {
        //            Name = x.Name,
        //            Hours = x.Hours <= 0 ? 0.01 : x.Hours,
        //            Total = x.Total,
        //            PerHour = Math.Round(x.Total / Convert.ToDecimal(x.Hours <= 0 ? 0.01 : x.Hours), 2, MidpointRounding.AwayFromZero),
        //            JobCount = x.JobCount,
        //            PerJob = Math.Round(x.Total / x.JobCount, 2, MidpointRounding.AwayFromZero)
        //        });


        //    response.Regions = jobs
        //        .GroupBy(x => x.ToRegion)
        //        .OrderBy(x => x.Key)
        //        .Select(x => new SustainabilitySummaryRegionDto()
        //        {
        //            Name = string.IsNullOrWhiteSpace(x.Key) ? "Undefined" : x.Key,
        //            Total = x.Sum(j => j.Total),
        //            JobCount = x.Count(),
        //            Suburbs = x.GroupBy(j => j.ToSuburb)
        //                .Select(y => new SustainabilitySummarySuburbDto()
        //                {
        //                    Name = string.IsNullOrWhiteSpace(y.Key) ? "Undefined" : y.Key,
        //                    Total = y.Sum(j => j.Total),
        //                    JobCount = y.Count()
        //                })
        //                .OrderBy(y => y.Name)
        //        });

        //    response.Total = response.Dates.Sum(j => j.Total);
        //    response.MinPerHour = response.Dates.Any() ? response.Dates.Min(x => x.PerHour) : 0;
        //    response.MaxPerHour = response.Dates.Any() ? response.Dates.Max(x => x.PerHour) : 0;
        //    response.AveragePerHour = response.Dates.Any() ? Math.Round(response.Dates.Sum(c => c.PerHour) / response.Dates.Count(), 2, MidpointRounding.AwayFromZero) : 0;
        //    response.MinHours = response.Dates.Any() ? response.Dates.Min(x => x.Hours) : 0;
        //    response.MaxHours = response.Dates.Any() ? response.Dates.Max(x => x.Hours) : 0;
        //    response.AverageHours = response.Dates.Any() ? Math.Round(response.Dates.Sum(x => x.Hours) / response.Dates.Count(), 2, MidpointRounding.AwayFromZero) : 0;

        //    response.Success = true;

        //    return response;
        //}

        //private async Task<List<SustainabilityJobDto>> GetJobs(DateTime fromDate, DateTime? toDate = null, int? courierId = null)
        //{
        //    // First part of query that can be translated to SQL
        //    var query = from j in Context.TucJobArchives
        //                join c in Context.TucCouriers on j.UcjbCourierId equals c.UccrId
        //                join fleet in Context.TucCourierFleets on c.CourierFleetId equals fleet.UccfId
        //                join suburb in Context.TucSuburbs on j.UcjbTo equals suburb.UcsuId into suburbs
        //                from suburb in suburbs.DefaultIfEmpty()
        //                join site in Context.TblSites on suburb.SiteId equals site.SiteId into sites
        //                from site in sites.DefaultIfEmpty()
        //                where j.UcjbComplTime.HasValue
        //                    && ((toDate == null && j.UcjbComplTime.Value.Date == fromDate.Date)
        //                        || (toDate != null && j.UcjbComplTime.Value.Date >= fromDate.Date
        //                            && j.UcjbComplTime.Value.Date <= toDate.Value.Date))
        //                    && (courierId == null || j.UcjbCourierId == courierId.Value)
        //                    && !j.UcjbVoid
        //                    && j.UcjbAmount.HasValue
        //                    && j.UcjbAmount != 0
        //                    && j.PpdexclusiveAmount.HasValue
        //                    && j.PpdexclusiveAmount >= 0
        //                    && j.GrossProfitValue.HasValue
        //                select new
        //                {
        //                    j.UcjbAmount,
        //                    j.GrossProfitValue,
        //                    j.PpdexclusiveAmount,
        //                    j.UcjbDate,
        //                    j.CourierPayment,
        //                    j.CourierFuel,
        //                    j.FuelSurchargeAmount,
        //                    j.PickUpTime,
        //                    j.UcjbComplTime,
        //                    j.UcjbDispDate,
        //                    j.UcjbDispTime,
        //                    CourierId = c.UccrId,
        //                    CourierCode = c.Code,
        //                    CourierVehicle = c.UccrVehicle,
        //                    ToSuburb = suburb.UcsuName,
        //                    ToRegion = site.Name
        //                };

        //    // Get the data from the database asynchronously first
        //    var queryResults = await query.ToListAsync();

        //    // Then perform client-side operations
        //    var results = queryResults
        //        .Where(x =>
        //            (x.UcjbAmount > 0 && (x.GrossProfitValue >= 0
        //                || x.GrossProfitValue == Math.Round(x.UcjbAmount.Value * -0.02m, 4, MidpointRounding.AwayFromZero)))
        //            || (x.UcjbAmount < 0 && (x.GrossProfitValue <= 0
        //                || x.GrossProfitValue == Math.Round(x.UcjbAmount.Value * -0.02m, 4, MidpointRounding.AwayFromZero)))
        //            && Math.Round(x.UcjbAmount.Value, 2) != Math.Round(
        //                x.UcjbAmount.Value - x.PpdexclusiveAmount.Value - x.GrossProfitValue.Value - (x.UcjbAmount.Value * 0.02m),
        //                2,
        //                MidpointRounding.AwayFromZero)
        //        )
        //        .Select(x => new SustainabilityJobDto
        //        {
        //            CourierId = x.CourierId,
        //            CourierCode = x.CourierCode,
        //            CourierVehicle = x.CourierVehicle,
        //            ToSuburb = x.ToSuburb,
        //            ToRegion = x.ToRegion,
        //            PickUpTime = x.PickUpTime.HasValue && x.PickUpTime.Value.Date == x.UcjbComplTime.Value.Date
        //                && x.PickUpTime < x.UcjbComplTime
        //                    ? x.PickUpTime.Value
        //                    : x.UcjbDispDate.HasValue && x.UcjbDispTime.HasValue
        //                        && x.UcjbDispDate.Value.Date == x.UcjbComplTime.Value.Date
        //                        && x.UcjbDispTime.Value.TimeOfDay < x.UcjbComplTime.Value.TimeOfDay
        //                        && x.UcjbComplTime.Value.TimeOfDay.Subtract(x.UcjbDispTime.Value.TimeOfDay).TotalHours <= 1
        //                            ? x.UcjbDispDate.Value.Date + x.UcjbDispTime.Value.TimeOfDay
        //                            : x.UcjbComplTime.Value.AddMinutes(
        //                                Convert.ToInt32(x.UcjbAmount > 0 ? x.UcjbAmount * -1 : x.UcjbAmount)),
        //            ComplTime = x.UcjbComplTime.Value,
        //            Total = x.UcjbDate >= _CourierPaymentFieldStartDate
        //                ? (x.CourierPayment ?? 0m) + (x.CourierFuel ??
        //                    ((x.CourierPayment ?? 0m) == 0m ? 0m : x.FuelSurchargeAmount))
        //                : Math.Round(
        //                    x.UcjbAmount.Value - x.PpdexclusiveAmount.Value - x.GrossProfitValue.Value - (x.UcjbAmount.Value * 0.02m),
        //                    2,
        //                    MidpointRounding.AwayFromZero)
        //        })
        //        .ToList();

        //    return results;
        //}

        //private double CalculateHoursByCourierAndDate(int courierId, DateTime date, List<SustainabilityCourierLoginDto> logins, List<SustainabilityJobDto> jobs)
        //{
        //    var filteredLogins = logins.Where(l => l.CourierId == courierId && l.LogInTime.Date == date.Date).ToList();
        //    var filteredJobs = jobs.Where(j => j.CourierId == courierId && j.ComplTime.Date == date.Date).ToList();

        //    if (!filteredLogins.Any() || !filteredJobs.Any())
        //        return 0;

        //    var maxComplTime = filteredJobs.Max(j => j.ComplTime);
        //    var maxLogOutTime = filteredLogins.Any(l => l.LogInTime.Date != l.LogOutTime?.Date) ? (DateTime?)null : filteredLogins.Max(l => l.LogOutTime.Value);
        //    DateTime maxTime = !maxLogOutTime.HasValue || maxComplTime < maxLogOutTime ? maxComplTime : maxLogOutTime.Value;

        //    var minLoginTime = filteredLogins.Min(l => l.LogInTime);
        //    var minPickupTime = filteredJobs.Min(j => j.PickUpTime);
        //    DateTime minTime = minLoginTime > minPickupTime && minLoginTime < maxTime ? minLoginTime : minPickupTime;

        //    return Math.Round((maxTime - minTime).TotalHours, 2, MidpointRounding.AwayFromZero);
        //}

        #endregion

        public async Task<RecentNoShowsResponse> RecentNoShows(Guid messageId)
        {

            RecentNoShowsResponse response = new RecentNoShowsResponse(messageId);

            int daysLimit = -90;

            var tenantTime = timeZoneService.GetTenantTime();
            var scheduleResponses = await Context.CourierScheduleResponses
                .Where(r => r.Schedule.BookDate.Date >= tenantTime.Date.AddDays(daysLimit)
                            && r.Schedule.BookDate.Date < tenantTime.Date
                            && r.StatusId == 1
                            && r.Courier.Active)
                .Select(r => new
                {
                    r.Schedule.BookDate.Date,
                    r.StatusId,
                    Courier = new
                    {
                        Id = r.Courier.UccrId,
                        Code = r.Courier.Code,
                        FirstName = r.Courier.UccrName,
                        Surname = r.Courier.UccrSurname
                    }
                })
                .Distinct()
                .ToListAsync();

            var courierJobCompletedDates = await Context.TblJobs
                .Where(j => j.CompletedTime.HasValue && j.CompletedTime >= tenantTime.Date.AddDays(daysLimit) && j.CompletedTime < tenantTime.Date && j.CourierId.HasValue)
                .Select(j => new
                {
                    j.CourierId,
                    j.CompletedTime.Value.Date
                })
                .Distinct()
                .ToListAsync();

            var courierLoginDates = await Context.TblCourierLogInOuts
                .Where(l => l.LogInTime >= tenantTime.Date.AddDays(daysLimit) && l.LogInTime < tenantTime.Date)
                .Select(l => new
                {
                    l.CourierId,
                    l.LogInTime.Date
                })
                .Distinct()
                .ToListAsync();

            var noShows = scheduleResponses
                .Where(r => !courierLoginDates.Any(l => l.CourierId == r.Courier.Id && l.Date == r.Date) ||
                            !courierJobCompletedDates.Any(j => j.CourierId == r.Courier.Id && j.Date == r.Date))
                .OrderByDescending(r => r.Date)
                .ThenBy(s => s.Courier.Code)
                .ToList();

            var shows = scheduleResponses
                .Where(r => !noShows.Contains(r))
                .ToList();

            response.Couriers = noShows
                .GroupBy(c => c.Courier)
                .Select(x => new
                {
                    Courier = x.Key,
                    LatestNoShow = x.Max(r => r.Date),
                    NoShows = x.Count(),
                    Shows = shows.Count(r => r.Courier.Id == x.Key.Id)
                })
                .OrderByDescending(x => x.LatestNoShow)
                .ThenBy(x => x.Courier.Code)
                .Select(x => new NoShowCourierDto()
                {
                    Courier = new CourierDto()
                    {
                        Id = x.Courier.Id,
                        Code = x.Courier.Code,
                        FirstName = x.Courier.FirstName,
                        Surname = x.Courier.Surname
                    },
                    LatestNoShow = x.LatestNoShow,
                    NoShows = x.NoShows,
                    Shows = x.Shows,
                    Percentage = Math.Round(x.NoShows / Convert.ToDouble(x.NoShows + x.Shows) * 100, 2, MidpointRounding.AwayFromZero)
                });

            //Calculate overall summary
            response.NoShows = noShows.Count();
            response.Shows = shows.Count();
            response.Percentage = response.NoShows + response.Shows == 0
                ? 0.00
                : Math.Round(response.NoShows / Convert.ToDouble(response.NoShows + response.Shows) * 100, 2, MidpointRounding.AwayFromZero);

            response.Success = true;

            return response;
        }

    }
}
