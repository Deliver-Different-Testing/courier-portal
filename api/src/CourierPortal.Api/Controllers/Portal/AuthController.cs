using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Auth;
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
    public class AuthController(PortalAuthService authService) : BaseController
    {
        // POST api/auth/token
        [HttpPost("Token")]
        public async Task<IActionResult> Token([FromBody] TokenRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})(Username: {(string.IsNullOrWhiteSpace(request.Username)? string.Empty : request.Username)})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await authService.Login(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        // POST api/auth/token
        [HttpPost("Token/AccessKey")]
        public async Task<IActionResult> TokenAccessKey([FromBody] TokenAccessKeyRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await authService.LoginAccessKey(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        // POST api/auth/password
        [Authorize(AuthenticationSchemes = "JwtBearer", Policy = "Courier")]
        [HttpPost("Password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                var response = await authService.ChangePassword(GetCurrentUser(), request);

                Log.Information($"Response ({response.MessageId})({response.MessageId})");

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        // POST api/auth/token
        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await authService.RefreshToken(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [Authorize(AuthenticationSchemes = "JwtBearer", Policy = "Courier")]
        [HttpPost("ChangeUser")]
        public async Task<IActionResult> ChangeUser([FromBody] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path})({request.MessageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await authService.ChangeUser(GetUser(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        // POST api/auth/token/hub
        [HttpPost("Token/Hub")]
        [Authorize(AuthenticationSchemes = "Identity.Application")]
        public async Task<IActionResult> TokenFromHub()
        {
            try
            {
                var messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                // Get email from Hub authentication claims
                var email = User.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
                var isCourierClaim = User.Claims.FirstOrDefault(x => x.Type == "IsCourier")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    Log.Warning($"({messageId}) No email claim found in Hub authentication");
                    return Unauthorized(new { success = false, message = "No email found in authentication" });
                }

                // Verify this is actually a courier login
                bool isCourier = !string.IsNullOrEmpty(isCourierClaim) && bool.TryParse(isCourierClaim, out var courierFlag) && courierFlag;
                if (!isCourier)
                {
                    Log.Warning($"({messageId}) User {email} is not a courier - IsCourier claim: {isCourierClaim}");
                    return Unauthorized(new { success = false, message = "Not authorized as courier" });
                }

                // Use Hub authentication to generate CourierPortal token
                var response = await authService.LoginFromHub(email, messageId);

                return HandleResponse(response);
            }
            catch (Exception e)
            {
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        protected override IActionResult HandleResponse(BaseResponse response)
        {
            if (!response.Success)
                return Unauthorized(response);

            return base.HandleResponse(response); ;
        }

    }
}