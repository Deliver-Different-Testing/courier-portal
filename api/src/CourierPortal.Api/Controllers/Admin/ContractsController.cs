using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Contracts;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CourierPortal.Api.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    public class ContractsController(AdminContractsService contractsService) : Controller
    {
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractCreateRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await contractsService.Create(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // GET api/<controller>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await contractsService.GetAll(messageId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // GET api/<controller>/{Id}/File
        [HttpGet("{Id}/File")]
        public async Task<IActionResult> GetFile([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await contractsService.GetFile(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        //// GET api/<controller>/{Id}/Data
        //[HttpGet("{Id}/Data")]
        //public async Task<IActionResult> GetData([FromRoute] IdentifierRequest request)
        //{
        //    try
        //    {
        //        Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(request.MessageId);

        //        return HandleResponse(await _contractsService.GetData(request));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
        //        throw;
        //    }
        //}

        private IActionResult HandleInvalidModelState(Guid messageId)
        {
            BaseResponse response = new BaseResponse(messageId);

            IEnumerable<MessageDto> messages = ModelState.Values.SelectMany(x => x.Errors.Select(e => new MessageDto() { Message = e.ErrorMessage }));

            response.Messages.AddRange(messages);

            Log.Information($"Response ({response.MessageId}): {JsonConvert.SerializeObject(response)}");

            return BadRequest(response);
        }

        private IActionResult HandleResponse(BaseResponse response)
        {
            Log.Information(response is FileResponse
                ? $"Response ({response.MessageId}): {JsonConvert.SerializeObject(new BaseResponse(response.MessageId) { Success = response.Success, Messages = response.Messages })}"
                : $"Response ({response.MessageId}): {JsonConvert.SerializeObject(response)}");

            if (response.Success)
            {
                if (response is FileResponse fileResponse)
                    return File(fileResponse.File, fileResponse.FileType, fileResponse.FileName);

                return Ok(response);
            }

            return BadRequest(response);
        }

    }
}
