using System;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalContractService(IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        public async Task<FileResponse> GetCurrentFile(int applicantId, Guid messageId)
        {
            FileResponse response = new FileResponse(messageId);

            CourierContract contract = await Context.CourierContracts.OrderByDescending(c => c.Id).FirstAsync();

            CourierApplicant applicant = await Context.CourierApplicants.SingleOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Applicant {applicantId} not found." });
                return response;
            }

            if (!ApplicantUtility.HasCompletedDeclaration(applicant))
            {
                applicant.ContractId = contract.Id;
                await Context.SaveChangesAsync();
            }


            response.FileName = contract.FileName;
            response.FileType = contract.Type;
            response.File = contract.Data;

            response.Success = true;

            return response;
        }

        //public async Task<ContractResponse> GetCurrentData(Guid messageId)
        //{
        //    ContractResponse response = new ContractResponse(messageId);

        //    CourierContract contract = await _Context.CourierContracts.OrderByDescending(c => c.Id).FirstAsync();

        //    response.Contract = new ContractDataDto()
        //    {
        //        Id = contract.Id,
        //        FileName = contract.FileName,
        //        Type = contract.Type,
        //        Length = contract.Length,
        //        Data = contract.Data == null ? null : $"data:{contract.Type};base64,{Convert.ToBase64String(contract.Data)}"
        //    };

        //    response.Success = true;

        //    return response;
        //}

        //public async Task<ContractResponse> GetData(int applicantId, IdentifierRequest request)
        //{
        //    ContractResponse response = new ContractResponse(request.MessageId);

        //    if (!await _Context.CourierApplicants.AnyAsync(a => a.Id == applicantId && a.ContractId.HasValue && a.ContractId.Value == a.ContractId))
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Applicant '{request.Id}' hasn't agreed to a contract." });
        //        return response;
        //    }

        //    CourierContract contract = await _Context.CourierContracts.SingleOrDefaultAsync(c => c.Id == request.Id);

        //    if (contract is null)
        //    {
        //        response.Messages.Add(new MessageDto() { Message = $"Contract '{request.Id}' not found." });
        //        return response;
        //    }

        //    response.Contract = new ContractDataDto()
        //    {
        //        Id = contract.Id,
        //        FileName = contract.FileName,
        //        Type = contract.Type,
        //        Length = contract.Length,
        //        Data = contract.Data == null ? null : $"data:{contract.Type};base64,{Convert.ToBase64String(contract.Data)}"
        //    };

        //    response.Success = true;

        //    return response;
        //}
    }
}
