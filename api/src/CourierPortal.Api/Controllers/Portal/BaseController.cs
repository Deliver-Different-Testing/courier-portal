using CourierPortal.Core.DTOs.Portal.Common;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Security.Claims;

namespace CourierPortal.Api.Controllers.Portal
{
    public abstract class BaseController : ControllerBase
    {
        protected int GetCurrentUser()
        {
            return Convert.ToInt32(User.FindFirst("CurrentId").Value);
        }
        protected int GetUser()
        {
            return Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        protected virtual IActionResult HandleInvalidModelState(Guid messageId)
        {
            BaseResponse response = new BaseResponse(messageId);

            IEnumerable<MessageDto> messages = ModelState
                .SelectMany(x => x.Value.Errors.Select(e => new MessageDto() { Message = e.ErrorMessage, Params = new List<object>() { x.Key } }));

            response.Messages.AddRange(messages);

            Log.Information($"Response ({response.MessageId}): {JsonConvert.SerializeObject(response)}");

            return BadRequest(response);
        }

        protected virtual IActionResult HandleResponse(BaseResponse response)
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
