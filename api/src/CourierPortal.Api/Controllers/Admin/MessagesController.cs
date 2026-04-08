using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.DTOs.Admin.Messages;
using CourierPortal.Core.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace CourierPortal.Api.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class MessagesController(MessageService messageService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await messageService.GetRecent(messageId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Couriers/{Code}")]
        public async Task<IActionResult> GetCourierMessages([FromRoute] CourierMessagesRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await messageService.GetCourierMessages(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessages(CreateMessagesRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await messageService.CreateMessages(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

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
            Log.Information($"Response ({response.MessageId}): {JsonConvert.SerializeObject(response)}");

            if (response.Success)
            {
                if (response is FileResponse)
                {
                    FileResponse fileResponse = (FileResponse)response;
                    return File(fileResponse.File, fileResponse.FileType, fileResponse.FileName);
                }

                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}