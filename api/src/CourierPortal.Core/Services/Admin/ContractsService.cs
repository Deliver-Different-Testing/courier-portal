using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Contracts;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminContractsService(IDbContextFactory<DespatchContext> contextFactory, AdminTimeZoneService timeZoneService):AdminBaseService(contextFactory)
    {
        public async Task<BaseResponse> Create(ContractCreateRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            byte[] file = Convert.FromBase64String(request.File.DataUrl.Substring(request.File.DataUrl.IndexOf(',') + 1));

            if (file.Length < 1)
            {
                response.Messages.Add(new MessageDto() { Message = $"'File' must not be empty" });
                return response;
            } 

            if (file.Length > 10000000)
            {
                response.Messages.Add(new MessageDto() { Message = $"'File' size exceeds 10MB" });
                return response;
            }

            var tenantTime = timeZoneService.GetTenantTime();
            Context.Add(new CourierContract()
            {
                Created = tenantTime,
                FileName = request.File.FileName.Trim(),
                Type = request.File.DataUrl.Substring(request.File.DataUrl.IndexOf(':') + 1, request.File.DataUrl.IndexOf(';') - request.File.DataUrl.IndexOf(':') - 1),
                Length = file.LongLength,
                Data = file
            });

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        public async Task<FileResponse> GetFile(IdentifierRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierContract contract = await Context.CourierContracts.SingleOrDefaultAsync(c => c.Id == request.Id);

            if (contract is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Contract '{request.Id}' not found." });
                return response;
            }

            response.FileName = contract.FileName;
            response.FileType = contract.Type;
            response.File = contract.Data;

            response.Success = true;

            return response;
        }

        //public async Task<ContractResponse> GetData(IdentifierRequest request)
        //{
        //    ContractResponse response = new ContractResponse(request.MessageId);

        //    CourierContract contract = await _Context.CourierContract.SingleOrDefaultAsync(c => c.Id == request.Id);

        //    if (contract is null)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Contract '{request.Id}' not found." });
        //        return response;
        //    }

        //    response.Contract = new ContractDataDto()
        //    {
        //        Id = contract.Id,
        //        Created = contract.Created,
        //        FileName = contract.FileName,
        //        Type = contract.Type,
        //        Length = contract.Length,
        //        Data = contract.Data == null ? null : $"data:{contract.Type};base64,{Convert.ToBase64String(contract.Data)}"
        //    };

        //    response.Success = true;

        //    return response;
        //}

        public async Task<ContractsResponse> GetAll(Guid messageId)
        {
            return new ContractsResponse(messageId)
            {
                Contracts = await Context.CourierContracts
                    .Select(c => new ContractDto() { Id = c.Id, Created = c.Created, FileName = c.FileName, Type = c.Type, Length = c.Length })
                    .ToListAsync(),
                Success = true
            };
        }

    }
}
