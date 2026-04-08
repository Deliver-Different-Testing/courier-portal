using System;
using System.Collections.Generic;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Auth;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace CourierPortal.Api.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AuthController(AdminAuthService authService) : ControllerBase
    {
        [HttpPost("CourierPortal/AccessValidation")]
        public IActionResult CourierPortalAccessValidation([FromBody] CourierPortalAccessValidationRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(authService.CourierPortalAccessValidation(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpPost("CourierPortal/AccessKey")]
        public IActionResult CreateCourierPortalAccessKey([FromBody] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(authService.CreateCourierPortalAccessKey(request));
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
            Log.Information($"Response ({response.MessageId})(Success: {response.Success})");

            if (!response.Success)
                return Unauthorized();

            return Ok(response);
        }
    }
}