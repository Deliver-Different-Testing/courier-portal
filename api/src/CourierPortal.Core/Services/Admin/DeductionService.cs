using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Deductions;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CourierPortal.Core.Services.Admin
{
    public class DeductionService(IDbContextFactory<DespatchContext> contextFactory, AdminTimeZoneService timeZoneService):AdminBaseService(contextFactory) 
    {
        public async Task<BaseResponse> Get(TransactionRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierDeduction deduction = await Context.CourierDeductions
                .Include(i => i.Courier)
                .ThenInclude(c => c.Region)
                .Include(i => i.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .SingleOrDefaultAsync(i => i.Id == DeductionUtility.NumberToId(request.Number));

            if (deduction is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Deduction '{request.Number}' not found." });
                return response;
            }

            response.Data = DeductionUtility.MapToDto(deduction);

            response.Success = true;
            return response;
        }


        public async Task<BaseResponse> GetRecurring(TransactionRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierDeductionRecurring dbData = await Context.CourierDeductionRecurrings
                .Include(d => d.Courier)
                .Include(d => d.CourierDeductionRecurringLines)
                .ThenInclude(l => l.DeductionType)
                .SingleOrDefaultAsync(d => d.Id == DeductionUtility.NumberToId(request.Number));

            if (dbData is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Deduction '{request.Number}' not found." });
                return response;
            }

            response.Data = DeductionUtility.MapToDto(dbData);

            response.Success = true;
            return response;
        }

        public async Task<BaseResponse> GetTypes(Guid messageId)
        {
            return new BaseResponse(messageId)
            {
                Data = await Context.CourierDeductionTypes
                .Select(x => new NameIdDto()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(),
                Success = true
            };
        }

        public async Task<BaseResponse> GetType(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierDeductionType result = await Context.CourierDeductionTypes
                .SingleOrDefaultAsync(i => i.Id == request.Id);

            if (result is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Deduction type '{request.Id}' not found." });
                return response;
            }

            response.Data = DeductionUtility.MapToDto(result);

            response.Success = true; 
            return response;
        }

        public async Task<BaseResponse> GetUnassigned(Guid messageId)
        {
            BaseResponse response = new BaseResponse(messageId);

            var dbData = await Context.CourierDeductions
                .Include(i => i.Courier)
                .ThenInclude(c => c.Region)
                .Include(i => i.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .Where(d => !d.InvoiceId.HasValue)
                .ToListAsync();

            response.Data = dbData.Select(DeductionUtility.MapToDto).ToList();

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Save(DeductionSaveRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            if (request.Courier == null || request.Courier.Id <= 0)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier is required." });
                return response;
            }

            if (request.Lines == null || !request.Lines.Any())
            {
                response.Messages.Add(new MessageDto() { Message = $"At least one line is required." });
                return response;
            }

            if (request.Lines.Any(l => l.Type == null || string.IsNullOrWhiteSpace(l.Description) || !l.Quantity.HasValue || Math.Round(l.Quantity.Value, 2, MidpointRounding.AwayFromZero) <= 0 || !l.UnitPrice.HasValue || Math.Round(l.UnitPrice.Value, 2, MidpointRounding.AwayFromZero) <= 0))
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid line." });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            var dbData = string.IsNullOrWhiteSpace(request.Number)
                ? new CourierDeduction()
                {
                    Created = tenantTime
                }
                : await Context.CourierDeductions
                    .Include(d => d.Courier)
                    .ThenInclude(c => c.Region)
                    .Include(d => d.CourierDeductionLines)
                    .ThenInclude(l => l.DeductionType)
                    .SingleOrDefaultAsync(i => i.Id == DeductionUtility.NumberToId(request.Number));

            if (dbData is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Deduction '{request.Number}' not found." });
                return response;
            }

            if (dbData.InvoiceId.HasValue)
            {
                response.Messages.Add(new MessageDto() { Message = $"Deduction can no longer be updated as it has been assigned." });
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dbData.ExternalId))
            {
                response.Messages.Add(new MessageDto() { Message = $"Deduction can no longer be updated as it has been processed." });
                return response;
            }

            if (dbData.Courier == null || dbData.Courier.UccrId != request.Courier.Id)
            {

                dbData.Courier = await Context.TucCouriers.FirstOrDefaultAsync(c => c.UccrId == request.Courier.Id);

                if (dbData.Courier == null)
                {
                    response.Messages.Add(new MessageDto() { Message = $"Courier Not Found." });
                    return response;
                }
            }

            dbData.Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim();

            var deductionTypeIds = request.Lines.Select(l => l.Type.Id).Distinct().ToList();
            var deductionsTypes = await Context.CourierDeductionTypes
                .Where(t => deductionTypeIds.Contains(t.Id))
                .ToListAsync();

            if (!deductionTypeIds.Any(x => deductionsTypes.Any(t => t.Id == x)))
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid deduction type." });
                return response;
            }

            foreach (var line in request.Lines)
            {
                var dbLine = (
                        string.IsNullOrWhiteSpace(request.Number) || !line.Id.HasValue
                        ? null
                        : dbData.CourierDeductionLines.FirstOrDefault(l => l.Id == line.Id)
                    )
                    ?? new CourierDeductionLine() { Created = tenantTime };

                dbLine.DeductionType = deductionsTypes.First(t => t.Id == line.Type.Id);
                dbLine.Description = line.Description.Trim();
                dbLine.Quantity = Math.Round(line.Quantity.Value, 2, MidpointRounding.AwayFromZero);
                dbLine.UnitPrice = Math.Round(line.UnitPrice.Value, 2, MidpointRounding.AwayFromZero);

                if (dbLine.Id < 1)
                    dbData.CourierDeductionLines.Add(dbLine);
            }

            if (dbData.Id <= 0)
                Context.CourierDeductions.Add(dbData);

            await Context.SaveChangesAsync();

            response.Data = DeductionUtility.MapToDto(dbData);

            response.Success = true;
            return response;
        }

        public async Task<BaseResponse> Save(DeductionRecurringSaveRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);
            var tenantTime = timeZoneService.GetTenantTime();

            request.StartDate = request.StartDate.HasValue && request.StartDate.Value.Kind == DateTimeKind.Utc
                ? timeZoneService.ConvertUtcToTenantTime(request.StartDate.Value)
                : request.StartDate;

            request.EndDate = request.EndDate.HasValue && request.EndDate.Value.Kind == DateTimeKind.Utc
                ? timeZoneService.ConvertUtcToTenantTime(request.EndDate.Value)
                : request.EndDate;

            if (!request.StartDate.HasValue)
            {
                response.Messages.Add(new MessageDto() { Message = $"Start date is required." });
                return response;
            }

            if (request.EndDate.HasValue && request.EndDate.Value.Date < request.StartDate.Value.Date)
            {
                response.Messages.Add(new MessageDto() { Message = $"Start date must be before end date." });
                return response;
            }

            if (request.EndDate.HasValue && request.EndDate.Value.Date < tenantTime.Date)
            {
                response.Messages.Add(new MessageDto() { Message = $"End date must be in the future or today." });
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Number) && request.StartDate.Value.Date <= tenantTime.Date)
            {
                response.Messages.Add(new MessageDto() { Message = $"Start date must be in the future." });
                return response;
            }

            if (request.Day < 1 || request.Day > 31)
            {
                response.Messages.Add(new MessageDto() { Message = $"Day must be between 1 and 31." });
                return response;
            }

            if (request.Courier == null || request.Courier.Id <= 0)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier is required." });
                return response;
            }

            if (request.Lines == null || !request.Lines.Any())
            {
                response.Messages.Add(new MessageDto() { Message = $"At least one line is required." });
                return response;
            }

            if (request.Lines.Any(l => l.Type == null || string.IsNullOrWhiteSpace(l.Description) || !l.Quantity.HasValue || Math.Round(l.Quantity.Value, 2, MidpointRounding.AwayFromZero) <= 0 || !l.UnitPrice.HasValue || Math.Round(l.UnitPrice.Value, 2, MidpointRounding.AwayFromZero) <= 0))
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid line." });
                return response;
            }

            var dbData = string.IsNullOrWhiteSpace(request.Number)
                ? new CourierDeductionRecurring()
                {
                    Created = tenantTime,
                    RecurringType = request.RecurringType,
                    StartDate = request.StartDate.Value
                }
                : await Context.CourierDeductionRecurrings
                    .Include(d => d.Courier)
                    .Include(d => d.CourierDeductionRecurringLines)
                    .ThenInclude(l => l.DeductionType)
                    .SingleOrDefaultAsync(i => i.Id == DeductionUtility.NumberToId(request.Number));

            if (dbData is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Recurring deduction '{request.Number}' not found." });
                return response;
            }

            if ((dbData.LastProcessed.HasValue || dbData.StartDate.Date <= tenantTime.Date) && dbData.RecurringType != request.RecurringType)
            {
                response.Messages.Add(new MessageDto() { Message = $"Recurring type can no longer be changed." });
                return response;
            }

            if ((dbData.LastProcessed.HasValue || dbData.StartDate.Date <= tenantTime.Date) && dbData.StartDate != request.StartDate)
            {
                response.Messages.Add(new MessageDto() { Message = $"Start date can no longer be changed." });
                return response;
            }

            if (dbData.EndDate.HasValue && dbData.EndDate.Value <= tenantTime.Date)
            {
                response.Messages.Add(new MessageDto() { Message = $"Recurring deduction has been completed and can no longer be updated." });
                return response;
            }

            if (dbData.Courier == null || dbData.Courier.UccrId != request.Courier.Id)
            {
                dbData.Courier = await Context.TucCouriers.FirstOrDefaultAsync(c => c.UccrId == request.Courier.Id);

                if (dbData.Courier == null)
                {
                    response.Messages.Add(new MessageDto() { Message = $"Courier Not Found." });
                    return response;
                }
            }

            dbData.RecurringType = request.RecurringType;
            dbData.StartDate = request.StartDate.Value.Date;
            dbData.EndDate = request.EndDate?.Date;
            dbData.DayValue = request.Day;
            dbData.Paused = request.Paused;
            dbData.Reference = string.IsNullOrWhiteSpace(request.Reference) ? null : request.Reference.Trim();

            var deductionTypeIds = request.Lines.Select(l => l.Type.Id).Distinct().ToList();
            var deductionsTypes = await Context.CourierDeductionTypes
                .Where(t => deductionTypeIds.Contains(t.Id))
                .ToListAsync();

            if (!deductionTypeIds.Any(x => deductionsTypes.Any(t => t.Id == x)))
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid deduction type." });
                return response;
            }

            foreach (var line in request.Lines)
            {
                var dbLine = (
                        string.IsNullOrWhiteSpace(request.Number) || !line.Id.HasValue
                        ? null
                        : dbData.CourierDeductionRecurringLines.FirstOrDefault(l => l.Id == line.Id)
                    )
                    ?? new CourierDeductionRecurringLine() { Created = tenantTime };

                dbLine.DeductionType = deductionsTypes.First(t => t.Id == line.Type.Id);
                dbLine.Description = line.Description.Trim();
                dbLine.Quantity = Math.Round(line.Quantity.Value, 2, MidpointRounding.AwayFromZero);
                dbLine.UnitPrice = Math.Round(line.UnitPrice.Value, 2, MidpointRounding.AwayFromZero);

                if (dbLine.Id < 1)
                    dbData.CourierDeductionRecurringLines.Add(dbLine);
            }

            if (dbData.Id <= 0)
                Context.CourierDeductionRecurrings.Add(dbData);

            await Context.SaveChangesAsync();

            response.Data = DeductionUtility.MapToDto(dbData);

            response.Success = true;
            return response;
        }

        public async Task<BaseResponse> SaveType(DeductionTypeSaveRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                response.Messages.Add(new MessageDto() { Message = $"Name is required." });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            var dbData = request.Id.HasValue
                ? await Context.CourierDeductionTypes.SingleOrDefaultAsync(i => i.Id == request.Id)
                : new CourierDeductionType()
                {
                    Created = tenantTime
                };

            if (dbData is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Deduction type '{request.Id}' not found." });
                return response;
            }

            dbData.Name = request.Name.Trim();

            if (dbData.Id <= 0)
                Context.CourierDeductionTypes.Add(dbData);

            await Context.SaveChangesAsync();

            response.Data = DeductionUtility.MapToDto(dbData);

            response.Success = true;
            return response;
        }

        public async Task<BaseResponse> Search(SearchRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);
    
            var searchPattern = $"%{request.SearchText?.Trim()}%";

            response.Data = await Context.CourierDeductions
                .Where(d => EF.Functions.Like(d.Id.ToString(), searchPattern)
                            || EF.Functions.Like(d.Courier.Code, searchPattern)
                            || EF.Functions.Like(
                                d.Courier.UccrName + " " + d.Courier.UccrSurname, 
                                searchPattern))
                .OrderByDescending(d => d.Id)
                .Select(d => new TransactionSearchDto()
                {
                    Number = d.Id.ToString(),
                    Created = d.Created,
                    Courier = new CourierDto()
                    {
                        Id = d.Courier.UccrId,
                        Code = d.Courier.Code,
                        FirstName = d.Courier.UccrName,
                        Surname = d.Courier.UccrSurname
                    }
                })
                .ToListAsync();

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> SearchRecurring(SearchRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var searchPattern = $"%{request.SearchText?.Trim()}%";

            var tenantTime = timeZoneService.GetTenantTime();
            response.Data = await Context.CourierDeductionRecurrings
                .Where(d => (!d.EndDate.HasValue || d.EndDate.Value.Date > tenantTime.Date)
                            && (EF.Functions.Like(d.Id.ToString(), searchPattern)
                            || EF.Functions.Like(d.Courier.Code, searchPattern)
                            || EF.Functions.Like(
                                d.Courier.UccrName + " " + d.Courier.UccrSurname,
                                searchPattern)))
                .OrderByDescending(d => d.Id)
                .Select(d => new TransactionSearchDto()
                {
                    Number = d.Id.ToString(),
                    Created = d.Created,
                    Courier = new CourierDto()
                    {
                        Id = d.Courier.UccrId,
                        Code = d.Courier.Code,
                        FirstName = d.Courier.UccrName,
                        Surname = d.Courier.UccrSurname
                    }
                })
                .ToListAsync();

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> SearchTypes(SearchRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            response.Data = await Context.CourierDeductionTypes
                .Where(x => EF.Functions.Like(x.Name.ToLower(), $"%{request.SearchText.ToLower()}%"))
                .OrderBy(x => x.Name)
                .Select(x => new NameIdDto()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetByCourier(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var deductions = await Context.CourierDeductions
                .Include(d => d.Courier)
                .ThenInclude(c => c.Region)
                .Include(d => d.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .Where(d => d.CourierId == request.Id)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            response.Data = deductions.Select(DeductionUtility.MapToDto);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetPendingByCourier(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var deductions = await Context.CourierDeductions
                .Include(d => d.Courier)
                .ThenInclude(c => c.Region)
                .Include(d => d.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .Where(d => d.CourierId == request.Id && (!d.InvoiceId.HasValue || d.Invoice.CourierInvoiceBatchItem.Batch.StatusId != 3))
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            // Explicitly load missing DeductionTypes
            foreach (var deduction in deductions)
            {
                foreach (var line in deduction.CourierDeductionLines)
                {
                    if (line.DeductionType == null && line.DeductionTypeId > 0)
                    {
                        Log.Debug($"Loading missing DeductionType for Line ID: {line.Id}, DeductionTypeId: {line.DeductionTypeId}");
                        line.DeductionType = await Context.CourierDeductionTypes.FindAsync(line.DeductionTypeId);
                    }
                }
            }
            response.Data = deductions.Select(DeductionUtility.MapToDto);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> GetCompletedByCourier(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            var deductions = await Context.CourierDeductions
                .Include(d => d.Courier)
                .ThenInclude(c => c.Region)
                .Include(d => d.CourierDeductionLines)
                .ThenInclude(l => l.DeductionType)
                .Where(d => d.CourierId == request.Id && d.Invoice.CourierInvoiceBatchItem.Batch.StatusId == 3)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            response.Data = deductions.Select(DeductionUtility.MapToDto);

            response.Success = true;

            return response;
        }

    }
}
