using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Runs;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain;
using CourierPortal.Infrastructure.Repositories;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalRunService(PortalTimeZoneService timeZoneService, RunRepository runRepository, IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {

        public async Task<RunsResponse> GetCourierRuns(Guid messageId, int courierId, bool calculateMaster = false, int? masterCourierId = null)
        {
            RunsResponse response = new RunsResponse(messageId);

            //Get run items
            List<RunItem> runItems = await runRepository.GetRunsByCourier(courierId);
            //List<RunItem> pendingRunItems = await _runRepository.GetPendingRunsByCourier(courierId);

            //Group items by Run using (BookDate and RunName) or (RunId)
            var groupedRunItems = runItems.GroupBy(x => new { x.BookDate, x.RunName });
            //var groupedPendingRunItems = pendingRunItems.GroupBy(x => x.RunId);

            //Filter Runs by status
            var currentRuns = groupedRunItems.Where(x => x.Any(r => !InvoiceUtility.IsCompleted(r) && !r.Void));
            var pastRuns = groupedRunItems.Where(x => !x.Any(r => !InvoiceUtility.IsCompleted(r) && !r.Void));

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstOrDefaultAsync();

            //Map runs to response
            var tenantTime = timeZoneService.GetTenantTime(); 
            response.Current = currentRuns.Select(r => MapRunItemsToRun(tenantTime, countryCode, r.ToList(), calculateMaster, masterCourierId));

            response.Past = pastRuns.Select(r => MapRunItemsToRun(tenantTime, countryCode, r.ToList(), calculateMaster, masterCourierId));

            //response.Pending = groupedPendingRunItems
            //    .Select(r => MapRunItemsToRun(r.ToList()));

            bool allowInvoicing = await Context.TucCouriers
                .AnyAsync(c => c.UccrId == courierId && c.Active && !c.UccrInternal);

            if (allowInvoicing)
                response.Uninvoiced = pastRuns
                    .Where(r => r.All(x => x.Archived) && r.Any(InvoiceUtility.CanInvoice))
                    .Select(r => MapRunItemsToRun(tenantTime, countryCode, r.Where(InvoiceUtility.CanInvoice).ToList(), calculateMaster, masterCourierId));
            else
                response.Uninvoiced = new List<RunDto>();

            response.Success = true;
            return response;
        }

        public async Task<RunResponse> GetCourierRun(int courierId, RunRequest request, bool calculateMaster = false, int? masterCourierId = null)
        {
            RunResponse response = new RunResponse(request.MessageId);

            List<RunItem> runItems = await runRepository.GetRunByCourier(courierId, request.BookDate, request.RunName);

            if (!runItems.Any())
                return ResponseUtility.AddMessageAndReturnResponse(response, "Invalid Run.");

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstOrDefaultAsync();

            var tenantTime = timeZoneService.GetTenantTime();
            response.Run = MapRunItemsToRun(tenantTime, countryCode, runItems, calculateMaster, masterCourierId);

            response.Success = true;
            return response;
        }

        private RunDto MapRunItemsToRun(DateTime tenantTime, string countryCode, List<RunItem> runItems, bool calculateMaster = false, int? masterCourierId = null)
        {
            IEnumerable<JobDto> jobs = runItems.Select(x => new JobDto()
            {
                JobId = x.JobId,
                JobNumber = x.JobNumber,
                ClientCode = x.ClientCode,
                CourierPayment = Math.Round((x.CourierPayment ?? 0m) * (x.MasterCourierId.HasValue ? (x.SubContractorPercentage ?? 0) : 1), 4, MidpointRounding.AwayFromZero),
                CourierFuel = Math.Round((x.CourierFuel ?? 0m) * (x.MasterCourierId.HasValue ? (x.SubContractorFuelPercentage ?? 0) : 1), 4, MidpointRounding.AwayFromZero),
                CourierBonus = Math.Round((x.CourierBonus ?? 0m) * (x.MasterCourierId.HasValue ? (x.SubContractorBonusPercentage ?? 0) : 1), 4, MidpointRounding.AwayFromZero),
                MasterId = x.MasterCourierId,
                MasterPayment = calculateMaster && x.MasterCourierId.HasValue && (!masterCourierId.HasValue || x.MasterCourierId == masterCourierId)
                    ? (x.CourierPayment ?? 0m) - Math.Round((x.CourierPayment ?? 0m) * (x.MasterCourierId.HasValue ? (x.SubContractorPercentage ?? 0) : 1), 4, MidpointRounding.AwayFromZero)
                    : null,
                MasterFuel = calculateMaster && x.MasterCourierId.HasValue && (!masterCourierId.HasValue || x.MasterCourierId == masterCourierId)
                    ? (x.CourierFuel ?? 0m) - Math.Round((x.CourierFuel ?? 0m) * (x.MasterCourierId.HasValue ? (x.SubContractorFuelPercentage ?? 0) : 1), 4, MidpointRounding.AwayFromZero)
                    : null,
                MasterBonus = calculateMaster && x.MasterCourierId.HasValue && (!masterCourierId.HasValue || x.MasterCourierId == masterCourierId)
                    ? (x.CourierBonus ?? 0m) - Math.Round((x.CourierBonus ?? 0m) * (x.MasterCourierId.HasValue ? (x.SubContractorBonusPercentage ?? 0) : 1), 4, MidpointRounding.AwayFromZero)
                    : null,
                DeliveryAddressLine1 = x.DeliveryAddressLine1,
                DeliveryAddressLine2 = x.DeliveryAddressLine2,
                DeliveryAddressLine3 = x.DeliveryAddressLine3,
                DeliveryAddressLine4 = x.DeliveryAddressLine4,
                DeliveryAddressLine5 = x.DeliveryAddressLine5,
                DeliveryAddressLine6 = x.DeliveryAddressLine6,
                DeliveryAddressLine7 = x.DeliveryAddressLine7,
                DeliveryAddressLine8 = x.DeliveryAddressLine8,
                DeliveryLatitude = double.TryParse(x.DeliveryLatitude, out double deliveryLatitude)? deliveryLatitude : (double?)null,
                DeliveryLongitude = double.TryParse(x.DeliveryLongitude, out double deliveryLongitude)? deliveryLongitude : (double?)null,
                Void = x.Void,
                Status = x.StatusId != 6 && x.StatusId != 13 && x.JobDone
                    ? "Done" 
                    : x.Status
            });

            //Get total amount for all not void jobs in the run then calculate the total amount for the courier.
            //Ignore job if any values are negative as they will not be included during invoicing, negative values can be charges to the courier.
            //(r.Amount * 0.02m) part is courier bonus - See gross profit value calculations
            //decimal runAmount = runItems
            //    .Where(r => !r.Void && r.Amount != 0m && r.PPDExclusiveAmount >= 0 && (r.GrossProfitValue.HasValue ? (r.GrossProfitValue >= 0 || r.GrossProfitValue == Math.Round(r.Amount * -0.02m, 4)) : (r.FuelSurchargeAmount >= 0 && r.CourierPercentage >= 0)))
            //    .Sum(r => r.GrossProfitValue.HasValue ? r.Amount - r.PPDExclusiveAmount - r.GrossProfitValue.Value - (r.Amount * 0.02m) : (r.Amount - r.FuelSurchargeAmount - r.PPDExclusiveAmount) * Convert.ToDecimal(r.CourierPercentage) + r.FuelSurchargeAmount);

            List<RunMasterDto> masters = calculateMaster
                ? jobs
                    .Where(j => j.MasterId.HasValue && (!masterCourierId.HasValue || j.MasterId.Value == masterCourierId))
                    .GroupBy(j => j.MasterId.Value)
                    .Select(x => new RunMasterDto()
                    {
                        Id = x.Key,
                        Amount = Math.Round(x.Where(j => !j.Void).Sum(j => (j.MasterPayment ?? 0m) + (j.MasterFuel ?? 0m) + (j.MasterBonus ?? 0m)), 2, MidpointRounding.AwayFromZero)
                    })
                    .Where(y => y.Amount != 0)
                    .ToList()
                : [];


            return new RunDto()
            {
                Id = runItems.First().RunId,
                RunType = 1,
                BookDate = runItems.First().BookDate,
                DateDisplay = runItems.First().BookDate == tenantTime.Date ? "TODAY" : runItems.First().BookDate.ToString(countryCode == "NZ"? "dd/MM/yyyy" : "MM/dd/yyyy"),
                RunName = runItems.First().RunName,
                Kms = Math.Round(runItems.First().RunKms, 2, MidpointRounding.AwayFromZero),
                Time = runItems.First().RunMins,
                Amount = Math.Round(jobs.Where(j => !j.Void).Sum(j => j.CourierPayment + j.CourierFuel + j.CourierBonus), 2, MidpointRounding.AwayFromZero),
                Masters = masters,
                MasterAmount = Math.Round(masters.Sum(m => m.Amount), 2, MidpointRounding.AwayFromZero),
                Cities = string.Join(", ", jobs.Where(j => !string.IsNullOrWhiteSpace(j.DeliveryAddressLine5)).Select(j => j.DeliveryAddressLine5).Distinct(StringComparer.OrdinalIgnoreCase)),
                States = string.Join(", ", jobs.Where(j => !string.IsNullOrWhiteSpace(j.DeliveryAddressLine6)).Select(j => j.DeliveryAddressLine6).Distinct(StringComparer.OrdinalIgnoreCase)),
                Jobs = jobs
            };
        }

        public async Task<BaseResponse> SendEnquiry(ClaimsPrincipal user, EnquiryRequest request)
        {
            var response = new BaseResponse(request.MessageId);

            var settings = await Context.TblSettings
                .Select(s => new {
                    s.EnquiryEmail,
                    s.GoogleSmtpserver,
                    s.GoogleSmtpserverUser,
                    s.GoogleSmtpserverPass
                })
                .FirstOrDefaultAsync();

            var message = new MailMessage(
                Environment.GetEnvironmentVariable("UrgentArmyEmail"),
                settings.EnquiryEmail, 
                $"Courier Portal: Job Enquiry for {request.JobNumber.Trim()} for courier {user.FindFirst("Code").Value}", 
                request.Message.Trim());

            var courierEmail = user.FindFirst(ClaimTypes.Email).Value;
            if (IsValidEmail(courierEmail))
                message.ReplyToList.Add(courierEmail?.Trim());

            using (var smtp = new SmtpClient())
            {
                smtp.Host = Environment.GetEnvironmentVariable("SmtpServer");
                smtp.Port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort"));
                smtp.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("SmtpUsername"), Environment.GetEnvironmentVariable("SmtpPassword"));
                smtp.EnableSsl = true;
                smtp.Send(message);
            };

            response.Success = true;
            return response;
        }

        public bool IsValidEmail(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                return false;

            var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

            var regex = new Regex(pattern);
            return regex.IsMatch(emailAddress);
        }

        //TODO:Remove this commented out code if still no plan to bring it back later
        //public async Task<RunResponse> ApplyForRun(int courierId, AvailableRunRequest request)
        //{
        //    RunResponse response = new RunResponse(request.MessageId);

        //    List<RunItem> runItems = await _runRepository.GetAvailableRun(courierId, request.Id);

        //    if (runItems.Count < 1)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Available Run '{request.Id}' not found for courier '{courierId}'." });
        //        return response;
        //    }

        //    await _runRepository.ApplyForRun(courierId, request.Id);

        //    response.Success = true;
        //    return response;
        //}

    }
}
