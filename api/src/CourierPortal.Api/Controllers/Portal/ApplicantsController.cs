using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Applications;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.Services.Portal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace CourierPortal.Api.Controllers.Portal
{
    [Route("api/portal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtBearer", Policy = "Applicant")]
    public class ApplicantsController : ControllerBase
    {

        private readonly PortalApplicantService _applicantService;
        public ApplicantsController(PortalApplicantService applicantService)
        {
            _applicantService = applicantService;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId}): {request.Email}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.Register(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception

                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [AllowAnonymous]
        [HttpPost("EmailVerification")]
        public async Task<IActionResult> Post([FromBody] EmailVerificationRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.EmailVerification(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await _applicantService.Get(messageId, GetCurrentApplicant()));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProfileUpdateRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.Update(GetCurrentApplicant(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpGet("Documents/{Id}/File")]
        public async Task<IActionResult> GetDocumentFile([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.DocumentGetFile(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Documents")]
        public async Task<IActionResult> GetDocuments()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await _applicantService.DocumentsGet(messageId, GetCurrentApplicant()));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpPost("Uploads")]
        public async Task<IActionResult> UploadPost([FromForm] UploadRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.UploadPost(GetCurrentApplicant(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpGet("Uploads/{Id}")]
        public async Task<IActionResult> UploadGet([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.UploadGet(GetCurrentApplicant(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpDelete("Uploads/{Id}")]
        public async Task<IActionResult> UploadDelete([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.UploadDelete(GetCurrentApplicant(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpPost("Declaration")]
        public async Task<IActionResult> PostDeclaration([FromBody] DeclarationUpdateRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _applicantService.PostDeclaration(GetCurrentApplicant(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("Locations")]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})");

                Guid messageId = Guid.NewGuid();
                return HandleResponse(await _applicantService.GetLocations(messageId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Information(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        #region "Private methods\functions"

        private int GetCurrentApplicant()
        {
            return Convert.ToInt32(User.FindFirst("CurrentId").Value);
        }

        private IActionResult HandleInvalidModelState(Guid messageId)
        {
            BaseResponse response = new BaseResponse(messageId);

            IEnumerable<MessageDto> messages = ModelState
                .SelectMany(x => x.Value.Errors.Select(e => new MessageDto() { Message = e.ErrorMessage, Params = new List<object>() { x.Key } }));

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

        #endregion
    }
}