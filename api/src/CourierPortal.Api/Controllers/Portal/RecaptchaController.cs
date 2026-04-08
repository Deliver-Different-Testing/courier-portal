using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CourierPortal.Api.Controllers.Portal
{
    [ApiController]
    [Route("api/portal/[controller]")]
    public class RecaptchaController : ControllerBase
    {
        [HttpGet("sitekey")]
        public IActionResult GetSiteKey()
        {
            return Ok(Environment.GetEnvironmentVariable("GoogleRecaptchaSiteKey"));
        }

        private async Task<bool> VerifyRecaptchaToken(string token)
        {
            using var client = new HttpClient();
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"secret", Environment.GetEnvironmentVariable("GoogleRecaptchaSecretKey")},
                    {"response", token}
                })
            );

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();

            return result.Success && result.Score >= 0.5; // Adjust threshold as needed
        }
    }

}
