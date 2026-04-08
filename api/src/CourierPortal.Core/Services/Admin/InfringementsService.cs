using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Infringements;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CourierPortal.Core.Services.Admin
{
    public class InfringementsService(IWebHostEnvironment hostingEnvironment, AdminTimeZoneService timeZoneService, IDbContextFactory<DespatchContext> contextFactory):AdminBaseService(contextFactory)
    {
        #region "Infringement"

        public async Task<InfringementResponse> Create(InfringementCreateRequest request)
        {

            InfringementResponse response = new InfringementResponse(request.MessageId);

            //Ensure category exist and is active
            InfringementCategory category = await Context.InfringementCategories
                .Include(c => c.InfringementCategoryLinks)
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.Active);

            if (category == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Infringement Category '{request.CategoryId}' not found or inactive.");

            //Ensure details has been provided if category flag requires it
            if (category.DetailsRequired && string.IsNullOrWhiteSpace(request.Details))
                return ResponseUtility.AddMessageAndReturnResponse(response, "'Details' must not be empty.");

            //Ensure courier exist and is active
            TucCourier courier = await Context.TucCouriers.FirstOrDefaultAsync(c => c.Code == request.CourierCode && c.Active);

            if (courier == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Courier with code '{request.CourierCode}' not found or inactive.");

            //If notify then ensure the courier has at least an email address set
            if (request.Notify && string.IsNullOrWhiteSpace(courier.UccrEmail))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"'Notify' requires the courier to have an email address.");

            //Add new infringement
            var tenantTime = timeZoneService.GetTenantTime();
            Infringement infringement = new Infringement()
            {
                Created = tenantTime,
                Courier = courier,
                Category = category,
                Severity = request.Severity,
                OccuredOn = request.OccurredOn.Value.Kind == DateTimeKind.Utc? timeZoneService.ConvertUtcToTenantTime(request.OccurredOn.Value).Date : request.OccurredOn.Value.Date,
                Details = request.Details?.Trim()
            };

            Context.Infringements.Add(infringement);

            //Notify the courier if required
            if (request.Notify)
            {
                Context.TucManualMessages.Add(new TucManualMessage()
                {
                    UcmmDate = tenantTime,
                    UcmmAttempts = 0,
                    SendToEmailAddress = courier.UccrEmail.Trim(),
                    ReplyToEmailAddress = Environment.GetEnvironmentVariable("UrgentArmyEmailSupport"),
                    Subject = "Infringement Notice",
                    UcmmMessage = await GenerateInfringementTemplate(infringement)
                });

                if (!string.IsNullOrWhiteSpace(courier.PersonalMobile) || !string.IsNullOrWhiteSpace(courier.UccrMobile))
                    _ = Context.TucManualMessages.Add(new TucManualMessage()
                    {
                        UcmmDate = tenantTime,
                        UcmmStaffId = 0,
                        UcmmAttempts = 0,
                        SendToMobile = (string.IsNullOrWhiteSpace(courier.PersonalMobile) ? courier.UccrMobile.Trim() : courier.PersonalMobile.Trim()).Replace("+64", "0").Replace(" ", string.Empty),
                        Subject = $"SMS to Courier: {courier.Code}",
                        UcmmMessage = "Infringement Notice: A new infringement has been issued, please check your email for details."
                    });
            }

            await Context.SaveChangesAsync();

            response.Infringement = new InfringementDto()
            {
                Id = infringement.Id,
                Created = infringement.Created,
                Courier = new CourierDto()
                {
                    Id = infringement.Courier.UccrId,
                    Code = infringement.Courier.Code,
                    FirstName = infringement.Courier.UccrName,
                    Surname = infringement.Courier.UccrSurname
                },
                Category = new InfringementCategoryDto()
                {
                    Id = infringement.Category.Id,
                    Name = infringement.Category.Name,
                    Severity = infringement.Category.Severity,
                    DetailsRequired = infringement.Category.DetailsRequired,
                    Active = infringement.Category.Active
                },
                Severity = infringement.Severity,
                OccurredOn = infringement.OccuredOn,
                Details = infringement.Details?.Trim(),
                Cancelled = infringement.Cancelled,
                CancelledReason = infringement.CancelledReason
            };

            response.Success = true;
            return response;
        }

        private async Task<string> GenerateInfringementTemplate(Infringement infringement)
        {
            //Prepare email message
            string emailMessage;

            using (StreamReader reader = new StreamReader(Path.Combine(hostingEnvironment.ContentRootPath, "EmailTemplates", "Infringements", "Infringement.html")))
            {
                emailMessage = await reader.ReadToEndAsync();
            }

            //Replace special tags in email template
            emailMessage = emailMessage.Replace("{{categoryName}}", HttpUtility.HtmlEncode(infringement.Category.Name));
            emailMessage = emailMessage.Replace("{{occuredOn}}", HttpUtility.HtmlEncode(infringement.OccuredOn.ToLongDateString()));
            emailMessage = emailMessage.Replace("{{infringementDetails}}", string.IsNullOrWhiteSpace(infringement.Details) ? string.Empty : HttpUtility.HtmlEncode(infringement.Details.Trim()));

            //Generate links and replace special link tag in email template
            string linksTemplate = string.Empty;

            List<InfringementCategoryLink> activeLinks = infringement.Category.InfringementCategoryLinks.Where(l => l.Active).ToList();

            if (activeLinks.Any())
            {
                linksTemplate = "<h2>Tips and Guidelines</h2>";

                foreach (InfringementCategoryLink categoryLink in activeLinks)
                {
                    linksTemplate += $"<h4>{HttpUtility.HtmlEncode(categoryLink.Name.Trim())}</h4>";
                    linksTemplate += $"<a href='{categoryLink.Link.Trim()}' target='_blank'>{HttpUtility.HtmlEncode(categoryLink.Link.Trim())}</a><br/><br/>";
                }
            }

            emailMessage = emailMessage.Replace("{{links}}", linksTemplate);

            return emailMessage;
        }

        public async Task<InfringementsRecentResponse> Recent(Guid messageId)
        {
            InfringementsRecentResponse response = new InfringementsRecentResponse(messageId);

            int daysLimit = -90;

            var tenantTime = timeZoneService.GetTenantTime();
            List<InfringementDto> infringements = await Context.Infringements
                .Where(i => i.Created.Date >= tenantTime.Date.AddDays(daysLimit))
                .OrderByDescending(i => i.Id)
                .Select(i => new InfringementDto()
                {
                    Id = i.Id,
                    Created = i.Created,
                    Courier = new CourierDto()
                    {
                        Id = i.Courier.UccrId,
                        Code = i.Courier.Code,
                        FirstName = i.Courier.UccrName,
                        Surname = i.Courier.UccrSurname
                    },
                    Category = new InfringementCategoryDto()
                    {
                        Id = i.Category.Id,
                        Name = i.Category.Name,
                        Severity = i.Category.Severity,
                        DetailsRequired = i.Category.DetailsRequired,
                        Active = i.Category.Active
                    },
                    Severity = i.Severity,
                    OccurredOn = i.OccuredOn,
                    Details = i.Details,
                    Cancelled = i.Cancelled,
                    CancelledReason = i.CancelledReason
                })
                .ToListAsync();

            var jobsByCourier = await Context.TblJobs
                .Where(j => j.CompletedTime.Value >= tenantTime.Date.AddDays(daysLimit) && j.CourierId.HasValue && !j.Void)
                .Select(j => new { CourierId = j.CourierId.Value, j.JobId })
                .GroupBy(j => j.CourierId)
                .Select(x => new { CourierId = x.Key, Count = x.Count() })
                .ToListAsync();

            response.Infringements = infringements.Where(i => !i.Cancelled);
            response.Total = response.Infringements.Count();
            response.CancelledInfringements = infringements.Where(i => i.Cancelled);
            response.Cancelled = response.CancelledInfringements.Count();

            response.CourierSummaries = infringements
                .Where(i => !i.Cancelled)
                .GroupBy(i => i.Courier.Id)
                .Select(x => new InfringementSummaryDto()
                {
                    Name = x.First().Courier.Code,
                    Total = x.Count(),
                    Jobs = jobsByCourier.FirstOrDefault(y => y.CourierId == x.Key)?.Count ?? 0
                })
                .OrderBy(x => x.Name);

            response.CategorySummaries = infringements
                .Where(i => !i.Cancelled)
                .GroupBy(i => i.Category.Id)
                .Select(x => new InfringementSummaryDto()
                {
                    Name = x.First().Category.Name,
                    Total = x.Count()
                })
                .OrderBy(x => x.Name);

            response.SeveritySummaries = infringements
                .Where(i => !i.Cancelled)
                .GroupBy(i => i.Severity)
                .OrderBy(x => x.Key)
                .Select(x => new InfringementSummaryDto()
                {
                    Name = x.Key.ToString(),
                    Total = x.Count()
                });

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Cancel(InfringementCancelRequest request)
        {

            BaseResponse response = new BaseResponse(request.MessageId);

            Infringement infringement = await Context.Infringements.SingleOrDefaultAsync(c => c.Id == request.Id);

            if (infringement is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Infringement '{request.Id}' not found.");

            if (infringement.Cancelled)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Infringement '{request.Id}' has already been cancelled.");

            infringement.Cancelled = true;
            infringement.CancelledReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

            await Context.SaveChangesAsync();

            response.Success = true;
            return response;
        }

        #endregion

        #region "Infringement Category"

        public async Task<InfringementCategoriesResponse> GetCategories(Guid messageId)
        {
            return new InfringementCategoriesResponse(messageId)
            {
                Categories = await Context.InfringementCategories
                    .Select(c => new InfringementCategoryDto()
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Severity = c.Severity,
                        DetailsRequired = c.DetailsRequired,
                        Active = c.Active
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                Success = true
            };
        }

        public async Task<InfringementCategoryResponse> GetCategory(IdentifierRequest request)
        {
            InfringementCategoryResponse response = new InfringementCategoryResponse(request.MessageId);

            InfringementCategory category = await Context.InfringementCategories.SingleOrDefaultAsync(c => c.Id == request.Id);

            if (category is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Infringement Category '{request.Id}' not found." });
                return response;
            }

            response.Category = new InfringementCategoryDto()
            {
                Id = category.Id,
                Name = category.Name,
                Severity = category.Severity,
                DetailsRequired = category.DetailsRequired,
                Active = category.Active
            };

            response.Success = true;

            return response;
        }

        public async Task<InfringementCategoryResponse> CreateCategory(InfringementCategoryCreateRequest request)
        {

            InfringementCategoryResponse response = new InfringementCategoryResponse(request.MessageId);

            var tenantTime = timeZoneService.GetTenantTime();
            InfringementCategory category = new InfringementCategory()
            {
                Created = tenantTime,
                Name = request.Name.Trim(),
                Severity = request.Severity,
                DetailsRequired = request.DetailsRequired,
                Active = request.Active
            };

            Context.InfringementCategories.Add(category);

            await Context.SaveChangesAsync();

            response.Category = new InfringementCategoryDto()
            {
                Id = category.Id,
                Name = category.Name,
                Severity = category.Severity,
                DetailsRequired = category.DetailsRequired,
                Active = category.Active
            };

            response.Success = true;
            return response;
        }

        public async Task<InfringementCategoryResponse> UpdateCategory(InfringementCategoryUpdateRequest request)
        {

            InfringementCategoryResponse response = new InfringementCategoryResponse(request.MessageId);

            InfringementCategory category = await Context.InfringementCategories.SingleOrDefaultAsync(c => c.Id == request.Id);

            if (category is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Infringement Category '{request.Id}' not found." });
                return response;
            }

            if (category.DetailsRequired != request.DetailsRequired || category.Active != request.Active || category.Severity != request.Severity)
            {
                category.Severity = request.Severity;
                category.DetailsRequired = request.DetailsRequired;
                category.Active = request.Active;

                await Context.SaveChangesAsync();
            }

            response.Category = new InfringementCategoryDto()
            {
                Id = category.Id,
                Name = category.Name,
                Severity = category.Severity,
                DetailsRequired = category.DetailsRequired,
                Active = category.Active
            };

            response.Success = true;
            return response;
        }

        #endregion

        #region "Infringement Category Link"

        public async Task<InfringementCategoryLinkResponse> GetCategoryLink(IdentifierRequest request)
        {
            InfringementCategoryLinkResponse response = new InfringementCategoryLinkResponse(request.MessageId);

            InfringementCategoryLink link = await Context.InfringementCategoryLinks.SingleOrDefaultAsync(l => l.Id == request.Id);

            if (link is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Infringement Category Link '{request.Id}' not found." });
                return response;
            }

            response.Link = new InfringementCategoryLinkDto()
            {
                Id = link.Id,
                Name = link.Name,
                Link = link.Link,
                Active = link.Active
            };

            response.Success = true;

            return response;
        }

        public async Task<InfringementCategoryLinksResponse> GetLinksByCategory(IdentifierRequest request)
        {
            return new InfringementCategoryLinksResponse(request.MessageId)
            {
                Links = await Context.InfringementCategoryLinks
                    .Where(l => l.CategoryId == request.Id)
                    .Select(l => new InfringementCategoryLinkDto()
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Link = l.Link,
                        Active = l.Active
                    })
                    .OrderBy(l => l.Name)
                    .ToListAsync(),
                Success = true
            };
        }

        public async Task<InfringementCategoryLinkResponse> CreateCategoryLink(InfringementCategoryLinkCreateRequest request)
        {

            InfringementCategoryLinkResponse response = new InfringementCategoryLinkResponse(request.MessageId);

            if (!await Context.InfringementCategories.AnyAsync(c => c.Id == request.CategoryId && c.Active))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Infringement Category '{request.CategoryId}' not found or inactive.");

            var tenantTime = timeZoneService.GetTenantTime();
            InfringementCategoryLink link = new InfringementCategoryLink()
            {
                Created = tenantTime,
                CategoryId = request.CategoryId,
                Name = request.Name.Trim(),
                Link = request.Link.Trim(),
                Active = request.Active
            };

            Context.InfringementCategoryLinks.Add(link);

            await Context.SaveChangesAsync();

            response.Link = new InfringementCategoryLinkDto()
            {
                Id = link.Id,
                Name = link.Name,
                Link = link.Link,
                Active = link.Active
            };

            response.Success = true;
            return response;
        }

        public async Task<InfringementCategoryLinkResponse> UpdateCategoryLink(InfringementCategoryLinkUpdateRequest request)
        {

            InfringementCategoryLinkResponse response = new InfringementCategoryLinkResponse(request.MessageId);

            InfringementCategoryLink link = await Context.InfringementCategoryLinks.SingleOrDefaultAsync(l => l.Id == request.Id);

            if (link == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Infringement Category Link '{request.Id}' not found.");

            link.Name = request.Name.Trim();
            link.Link = request.Link.Trim();
            link.Active = request.Active;

            await Context.SaveChangesAsync();

            response.Link = new InfringementCategoryLinkDto()
            {
                Id = link.Id,
                Name = link.Name,
                Link = link.Link,
                Active = link.Active
            };

            response.Success = true;
            return response;
        }

        #endregion

    }
}
