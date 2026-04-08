using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Messages;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class MessageService(IDbContextFactory<DespatchContext> contextFactory, AdminTimeZoneService timeZoneService):AdminBaseService(contextFactory)
    {
        public async Task<RecentMessagesResponse> GetRecent(Guid messageId)
        {
            RecentMessagesResponse response = new RecentMessagesResponse(messageId);

            //Get List of All Active Couriers with Courier Portal Access
            List<TucCourier> couriers = await Context.TucCouriers
                .Include(c => c.Region)
                .Include(c => c.CourierLogInOut)
                .Where(c => c.Active)
                .ToListAsync();

            //Get subjects which will be used to filter messages
            List<string> courierSubjects = couriers
                .Select(c => $"Response from Courier: {c.Code}")
                .Union(couriers.Select(c => $"SMS to Courier: {c.Code}"))
                .ToList();
            //We currently filter out messages without a staff account to cut back on the noise but still show messages sent from this app.
            courierSubjects.Add("Courier Manager");

            //Get Messages
            var tenantTime = timeZoneService.GetTenantTime();
            var manualMessages = await Context.TucManualMessages
                .Where(m => m.UcmmDate >= tenantTime.Date.AddDays(-5)
                            && ((m.UcmmSendTo.HasValue && m.UcmmSendTo.Value > 0 && m.UcmmStaffId.HasValue && m.UcmmStaffId > 0) || courierSubjects.Contains(m.Subject)))
                .Select(m => new { m.UcmmId, m.UcmmDate, m.UcmmSendTo, m.Subject, m.UcmmMessage, m.UcmmSent, m.Read })
                .OrderBy(m => m.UcmmDate)
                .ToListAsync();

            response.RecentMessages = couriers
                .Where(c => manualMessages.Any(m => m.UcmmSendTo == c.UccrId || m.Subject == $"Response from Courier: {c.Code}" || m.Subject == $"SMS to Courier: {c.Code}"))
                .Select(c => new RecentMessagesDto()
                {
                    Courier = CourierUtility.MapToCourierDto(c),
                    LoggedIn = c.CourierLogInOut != null && c.CourierLogInOut.LogInTime.Date == tenantTime.Date && !c.CourierLogInOut.LogOutTime.HasValue,
                    CourierMessages = manualMessages
                        .Where(m => m.UcmmSendTo == c.UccrId || m.Subject == $"Response from Courier: {c.Code}" || m.Subject == $"SMS to Courier: {c.Code}")
                        .Select(m => new CourierMessageDto()
                        {
                            Id = m.UcmmId,
                            Created = m.UcmmDate,
                            Type = m.UcmmSendTo == c.UccrId ? 1 : (m.Subject == $"Response from Courier: {c.Code}" ? 2 : 3),
                            Message = m.UcmmMessage,
                            Sent = m.UcmmSent,
                            Read = m.Read
                        }),
                    LastMessage = manualMessages
                        .Where(m => m.UcmmSendTo == c.UccrId || m.Subject == $"Response from Courier: {c.Code}" || m.Subject == $"SMS to Courier: {c.Code}")
                        .Max(m => m.UcmmDate)
                })
                .OrderByDescending(r => r.LastMessage);

            response.Success = true;

            return response;
        }       

        public async Task<CourierMessagesResponse> GetCourierMessages(CourierMessagesRequest request)
        {
            CourierMessagesResponse response = new CourierMessagesResponse(request.MessageId);

            TucCourier courier = await Context.TucCouriers
                .Include(c => c.CourierLogInOut)
                .FirstOrDefaultAsync(c => c.Code == request.Code.Trim() && c.Active);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier code '{request.Code}' inactive or not found." });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            var manualMessages = await Context.TucManualMessages
                .Where(m => m.UcmmDate >= tenantTime.Date.AddDays(-5) && ((m.UcmmSendTo == courier.UccrId && ((m.UcmmStaffId.HasValue && m.UcmmStaffId.Value > 0) || m.Subject == "Courier Manager")) || m.Subject == $"Response from Courier: {courier.Code}" || m.Subject == $"SMS to Courier: {courier.Code}"))
                .Select(m => new { m.UcmmId, m.UcmmDate, m.UcmmSendTo, m.Subject, m.UcmmMessage, m.UcmmSent, m.Read })
                .OrderBy(m => m.UcmmDate)
                .ToListAsync();

            response.Courier = new CourierDto()
            {
                Id = courier.UccrId,
                Code = courier.Code,
                FirstName = courier.UccrName,
                Surname = courier.UccrSurname
            };

            response.LoggedIn = courier.CourierLogInOut != null && courier.CourierLogInOut.LogInTime.Date == tenantTime.Date && !courier.CourierLogInOut.LogOutTime.HasValue;

            response.CourierMessages = manualMessages.Select(m => new CourierMessageDto()
            {
                Id = m.UcmmId,
                Created = m.UcmmDate,
                Type = m.UcmmSendTo == courier.UccrId ? 1 : (m.Subject == $"Response from Courier: {courier.Code}" ? 2 : 3),
                Message = m.UcmmMessage,
                Sent = m.UcmmSent,
                Read = m.Read
            });

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> CreateMessages(CreateMessagesRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            List<int> courierIds = request.Messages.Select(m => m.CourierId).Distinct().ToList();

            List<TucCourier> couriers = await Context.TucCouriers
                .Where(c => courierIds.Contains(c.UccrId) && c.Active)
                .ToListAsync();

            if (courierIds.Count() != couriers.Count)
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid or inactive courier(s)." });
                return response;
            }

            //If there is any message type (3)Mixed, which sends a Urgent Drive message if courier is logged in and SMS if logged out then get login status for couriers
            List<TblCourierLogInOut> couriersLogInOuts = new List<TblCourierLogInOut>();
            if (request.Messages.Any(m => m.Type == 3))
            {
                //Get Login Status
                IEnumerable<int> couriersLogInOutIds = couriers
                    .Where(c => request.Messages.Where(m => m.Type == 3).Select(m=>m.CourierId).Distinct().Contains(c.UccrId) && c.CourierLogInOutId.HasValue && c.CourierLogInOutId.Value > 0)
                    .Select(c => c.CourierLogInOutId.Value)
                    .Distinct();
                couriersLogInOuts = await Context.TblCourierLogInOuts
                    .Where(x => couriersLogInOutIds.Contains(x.CourierLogInOutId))
                    .ToListAsync();
            }

            var tenantTime = timeZoneService.GetTenantTime();
            if (request.Messages
                .Where(m => m.Type == 2 || (m.Type == 3 && !couriersLogInOuts.Any(x => x.CourierId == m.CourierId && x.LogInTime.Date == tenantTime.Date && !x.LogOutTime.HasValue)))
                .Any(m=> string.IsNullOrWhiteSpace(couriers.Single(c => c.UccrId == m.CourierId).PersonalMobile) && string.IsNullOrWhiteSpace(couriers.Single(c => c.UccrId == m.CourierId).UccrMobile)))
            {
                response.Messages.Add(new MessageDto() { Message = $"Couriers must have a mobile number to send SMS." });
                return response;
            }

            IEnumerable<TucManualMessage> messages = request.Messages
                .Where(m=> m.Type == 1 || (m.Type == 3 && couriersLogInOuts.Any(x=>x.CourierId == m.CourierId && x.LogInTime.Date == tenantTime.Date && !x.LogOutTime.HasValue)))
                .Select(m => new TucManualMessage()
                {
                    UcmmDate = tenantTime,
                    UcmmSendTo = m.CourierId,
                    UcmmStaffId = 0,
                    UcmmAttempts = 0,
                    Subject = "Courier Manager",
                    UcmmMessage = m.Message
                });

            if (messages.Any())
                Context.TucManualMessages.AddRange(messages);

            IEnumerable<TucManualMessage> smsMessages = request.Messages
                .Where(m => m.Type == 2 || (m.Type == 3 && !couriersLogInOuts.Any(x => x.CourierId == m.CourierId && x.LogInTime.Date == tenantTime.Date && !x.LogOutTime.HasValue)))
                .Select(m => new TucManualMessage()
                {
                    UcmmDate = tenantTime,
                    UcmmStaffId = 0,
                    UcmmAttempts = 0,
                    SendToMobile = (string.IsNullOrWhiteSpace(couriers.Single(c => c.UccrId == m.CourierId).PersonalMobile)? couriers.Single(c => c.UccrId == m.CourierId).UccrMobile : couriers.Single(c => c.UccrId == m.CourierId).PersonalMobile).Replace("+64", "0").Replace(" ", string.Empty),
                    Subject = $"SMS to Courier: {couriers.Single(c => c.UccrId == m.CourierId).Code}",
                    UcmmMessage = m.Message
                });

            if (smsMessages.Any())
                Context.TucManualMessages.AddRange(smsMessages);

            if (messages.Any() || smsMessages.Any())
                await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

    }
}
