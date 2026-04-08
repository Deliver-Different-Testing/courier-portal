using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Api.Core.Application.Dtos.Common;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Couriers;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalCourierService(IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        public async Task<CourierResponse> Get(Guid messageId, int courierId)
        {
            CourierResponse response = new CourierResponse(messageId);

            TucCourier courier = await Context.TucCouriers
                .SingleOrDefaultAsync(c=> c.UccrId == courierId && c.Active);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier {courierId} not found." });
                return response;
            }

            response.Courier = CourierUtility.MapToCourierDto(courier);

            response.Success = true;

            return response;
        }

        public async Task<CourierResponse> Update(int courierId, CourierUpdateRequest request)
        {
            CourierResponse response = new CourierResponse(request.MessageId);

            TucCourier courier = await Context.TucCouriers
                .SingleOrDefaultAsync(c => c.UccrId == courierId && c.Active);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier {courierId} not found." });
                return response;
            }

            //tucCourier has a unique index on name and surname for some reason, unsure if an old application is using this.  This should be removed if possible, but for now apply temp workaround.
            if (await Context.TucCouriers.AnyAsync(c => c.UccrId != courier.UccrId && c.UccrName == request.FirstName && c.UccrSurname == request.Surname))
                request.Surname += ".";

            courier.UccrName = request.FirstName?.Trim();
            courier.UccrSurname = request.Surname?.Trim();
            courier.UccrTel = request.Phone?.Trim();
            courier.PersonalMobile = request.Mobile?.Trim();
            courier.UccrEmail = request.Email?.Trim();
            courier.AddressLine1 = request.AddressLine1?.Trim();
            courier.AddressLine2 = request.AddressLine2?.Trim();
            courier.AddressLine3 = request.AddressLine3?.Trim();
            courier.AddressLine4 = request.AddressLine4?.Trim();
            courier.AddressLine5 = request.AddressLine5?.Trim();
            courier.AddressLine6 = request.AddressLine6?.Trim();
            courier.AddressLine7 = request.AddressLine7?.Trim();
            courier.AddressLine8 = request.AddressLine8?.Trim();
            courier.UccrDlno = request.DriversLicenceNo?.Trim();
            courier.UccrReg = request.VehicleRegistrationNo?.Trim();
            courier.BankRoutingNumber = request.BankRoutingNumber?.Trim();
            courier.UccrBankAccountNo = request.BankAccountNo?.Trim();
            courier.UccrGst = request.TaxNo?.Trim();

            await Context.SaveChangesAsync();

            response.Courier = CourierUtility.MapToCourierDto(courier);

            response.Success = true;

            return response;
        }
        
        public async Task<BaseResponse> GetContractors(Guid messageId, int masterCourierId)
        {
            var couriers = await Context.TucCouriers
                .Where(c => (c.UccrId == masterCourierId || (c.CourierTypeId == 3 && c.MasterCourierId == masterCourierId))
                    && c.Active
                    && !c.UccrInternal)
                .Select(c => new
                {
                    Id = c.UccrId,
                    Type = c.CourierTypeId,
                    Name = $"{c.UccrName} {c.UccrSurname}"
                })
                .ToListAsync();

            return new BaseResponse(messageId)
            {
                Data = couriers
                    .OrderBy(c => c.Name)
                    .ToList(),
                Success = true
            };
        }

        public async Task<BaseResponse> GetContractor(int masterCourierId, IdentifierRequest request)
        {
            var response = new BaseResponse(request.MessageId);

            var courier = await Context.TucCouriers
                .Where(c => c.UccrId == request.Id && c.UccrId != masterCourierId && c.CourierTypeId == 3 && c.MasterCourierId == masterCourierId)
                .Select(c => new SubcontractorDto
                {
                    Id = c.UccrId,
                    Code = c.Code,
                    FirstName = c.UccrName,
                    Surname = c.UccrSurname,
                    Percentage = c.SubContractorPercentage ?? 0,
                    FuelPercentage = c.SubContractorFuelPercentage ?? 0,
                    BonusPercentage = c.SubContractorBonusPercentage ?? 0
                })
                .FirstOrDefaultAsync();

            if (courier == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Invalid Id.");

            response.Data = courier;
            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> UpdateContractor(int masterCourierId, SubcontractorUpdateRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            if (request.Percentage < 0 || request.FuelPercentage < 0 || request.BonusPercentage < 0)
            {
                response.Messages.Add(new MessageDto() { Message = $"Percentage must not be negative" });
                return response;
            }

            if (request.Percentage > 1 || request.FuelPercentage > 1 || request.BonusPercentage > 1)
            {
                response.Messages.Add(new MessageDto() { Message = $"Percentage max value is 1" });
                return response;
            }

            if (!await Context.TucCouriers
                .AnyAsync(c => c.UccrId == masterCourierId && c.Active && c.CourierTypeId == 2))
            {
                response.Messages.Add(new MessageDto() { Message = $"Master courier not found" });
                return response;
            }

            var dbData = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.UccrId == request.Id && c.UccrId != masterCourierId && c.CourierTypeId == 3 && c.MasterCourierId == masterCourierId);
            
            if (dbData == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Subcontractor not found." });
                return response;
            }

            dbData.SubContractorPercentage = request.Percentage;

            dbData.SubContractorFuelPercentage = request.FuelPercentage;

            dbData.SubContractorBonusPercentage = request.BonusPercentage;

            await Context.SaveChangesAsync();

            response.Data = new SubcontractorDto
            {
                Id = dbData.UccrId,
                Code = dbData.Code,
                FirstName = dbData.UccrName,
                Surname = dbData.UccrSurname,
                Percentage = dbData.SubContractorPercentage ?? 0,
                FuelPercentage = dbData.SubContractorFuelPercentage ?? 0,
                BonusPercentage = dbData.SubContractorBonusPercentage ?? 0
            };

            response.Success = true;

            return response;
        }

        //public async Task<CourierAvailabilityResponse> GetCourierAvailability(Guid messageId, int courierId)
        //{
        //    CourierAvailabilityResponse response = new CourierAvailabilityResponse(messageId);

        //    List<CourierAvailability> availability = await _Context.CourierAvailability.Where(a=>a.CourierId == courierId).ToListAsync();
        //    List<CourierAvailabilityExclude> availabilityExclude = await _Context.CourierAvailabilityExclude.Where(a => a.CourierId == courierId && a.UnavailableDate >= DateTime.Today ).ToListAsync();

        //    response.Availability = availability.Select(a => new CourierAvailabilityDto()
        //    {
        //        Day = Enum.Parse<DayOfWeek>(a.Day),
        //        StartTime = a.StartTime,
        //        EndTime = a.EndTime
        //    });

        //    response.Unavailable = availabilityExclude.Select(a => a.UnavailableDate).OrderBy(a=>a);

        //    response.Success = true;

        //    return response;
        //}

        //public async Task<CourierAvailabilityResponse> UpdateAvailability(int courierId, CourierAvailabilityUpdateRequest request)
        //{
        //    CourierAvailabilityResponse response = new CourierAvailabilityResponse(request.MessageId);

        //    List<CourierAvailability> availability = await _Context.CourierAvailability.Where(a => a.CourierId == courierId).ToListAsync();
        //    List<CourierAvailabilityExclude> availabilityExclude = await _Context.CourierAvailabilityExclude.Where(a => a.CourierId == courierId && a.UnavailableDate >= DateTime.Today).ToListAsync();

        //    //Convert any utc dates to local time and remove duplicates from unavailable
        //    request.Unavailable = request.Unavailable.Select(u => u.Kind == DateTimeKind.Utc ? u.ToLocalTime() : u).Distinct();

        //    //Update weekly availability
        //    await _Context.CourierAvailability
        //        .AddRangeAsync(request.Availability.Where(a => !availability.Exists(x => Enum.Parse<DayOfWeek>(x.Day) == a.Day && x.StartTime == a.StartTime && x.EndTime == a.EndTime)).Select(a => new CourierAvailability()
        //        {
        //            CourierId = courierId,
        //            Day = a.Day.ToString(),
        //            StartTime = a.StartTime,
        //            EndTime = a.EndTime
        //        }));

        //    _Context.CourierAvailability.RemoveRange(availability.Where(a=> !request.Availability.ToList().Exists(x=> x.Day == Enum.Parse<DayOfWeek>(a.Day) && x.StartTime == a.StartTime && x.EndTime == a.EndTime)));

        //    //Update availability exclusions
        //    await _Context.CourierAvailabilityExclude.AddRangeAsync(request.Unavailable
        //        .Where(u => !availabilityExclude.Exists(a => a.UnavailableDate == u))
        //        .Select(u => new CourierAvailabilityExclude()
        //        {
        //            CourierId = courierId,
        //            UnavailableDate = u
        //        }));

        //    _Context.CourierAvailabilityExclude.RemoveRange(availabilityExclude.Where(a => !request.Unavailable.Contains(a.UnavailableDate)));

        //    //Save changes
        //    await _Context.SaveChangesAsync();

        //    response.Availability = request.Availability;

        //    response.Unavailable = request.Unavailable;

        //    response.Success = true;

        //    return response;
        //}

        //public async Task<CourierSummaryResponse> GetCourierSummary(Guid messageId, int courierId)
        //{
        //    CourierSummaryResponse response = new CourierSummaryResponse(messageId);

        //    //Get run items
        //    RunsResponse runsResponse = await _runService.GetCourierRuns(messageId, courierId);

        //    if (!runsResponse.Success)
        //    {
        //        response.Messages.AddRange(runsResponse.Messages);
        //        return response;
        //    }

        //    //Start of week is based on pay period which starts on a tuesday
        //    DateTime today = DateTime.Today;
        //    int delta = today.DayOfWeek < DayOfWeek.Tuesday ? DayOfWeek.Tuesday - today.DayOfWeek - 7 : DayOfWeek.Tuesday - today.DayOfWeek;
        //    DateTime startOfWeek = today.AddDays(delta);

        //    decimal uninvoicedTotal = 0.00m;

        //    bool allowInvoicing = await _Context.TucCouriers
        //        .AnyAsync(c => c.UccrId == courierId 
        //            && c.Active
        //            && c.UccrInternal
        //            && c.CourierFleet.AllowCourierPortalAccess.HasValue && c.CourierFleet.AllowCourierPortalAccess.Value
        //            && c.CourierFleet.AllowInvoicing.HasValue && c.CourierFleet.AllowInvoicing.Value);

        //    if (allowInvoicing)
        //    {
        //        //Get un-invoiced info, use existing runs response previously retrieved.
        //        var uninvoicedRunsResponse = await _invoiceService.GetUninvoiced(messageId, courierId, runsResponse);

        //        if (!uninvoicedRunsResponse.Success)
        //        {
        //            response.Messages.AddRange(uninvoicedRunsResponse.Messages);
        //            return response;
        //        }

        //        uninvoicedTotal = uninvoicedRunsResponse.Uninvoiced.Total;
        //    }

        //    response.Summary = new CourierSummaryDto()
        //    {
        //        CurrentRunCount = runsResponse.Current.Count(),
        //        Driven = Math.Round(runsResponse.Past.Where(r => r.BookDate >= startOfWeek).Sum(r => r.Kms), 2),
        //        Uninvoiced = uninvoicedTotal
        //    };

        //    response.Success = true;

        //    return response;
        //}

    }
}
