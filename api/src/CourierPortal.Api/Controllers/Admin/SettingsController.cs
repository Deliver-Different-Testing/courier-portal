using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Openforce;
using CourierPortal.Core.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using System.Text.RegularExpressions;
using CourierPortal.Core.Validators.Admin;
using JsonException = System.Text.Json.JsonException;

namespace CourierPortal.Api.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    public class SettingsController(SettingsService service) : Controller
    {


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path}): {messageId}");

                return HandleResponse(await service.GetAsync(messageId));
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
                return Ok(response);

            return BadRequest(response);
        }

    }
}
