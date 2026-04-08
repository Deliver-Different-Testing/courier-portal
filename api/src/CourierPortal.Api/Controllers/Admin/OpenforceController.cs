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
    public class OpenforceController(AdminApplicantsService applicantsService, OpenforceService openforceService, IConnectionStringManager connectionStringManager) : Controller
    {


        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path}): {messageId}");

                return HandleResponse(new BaseResponse(messageId)
                {
                    Data = await openforceService.GetSettingsAsync(true),
                    Success = true
                });
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings([FromBody] OfSettingsSaveRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await openforceService.Save(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Connected")]
        public async Task<IActionResult> Connected()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path}): {messageId}");

                return HandleResponse(new BaseResponse(messageId)
                {
                    Data = await openforceService.IsConnectedAsync(),
                    Success = true
                });
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpPost("Webhooks")]
        public async Task<IActionResult> Webhooks([FromBody] OfWebhookEvent webhookEvent)
        {
            try
            {
                var key = Environment.GetEnvironmentVariable("JWTSecretKey");
                var authHeader = Request.Headers["Authorization"].ToString();
                string bearerToken = null;

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                    Log.Information($"Received webhook with bearer token: {bearerToken}");
                }
                else
                {
                    Log.Warning("No bearer token found in webhook request");
                    return Unauthorized("Bearer token is required");
                }

                try
                {
                    IJsonSerializer serializer = new JsonNetSerializer();
                    IDateTimeProvider provider = new UtcDateTimeProvider();
                    IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                    IJwtAlgorithm algorithm = new HMACSHA256Algorithm();

                    // Create our custom validator that skips expiration checks
                    IJwtValidator validator = new NonExpiringJwtValidator(serializer, provider);

                    // Create the decoder with our custom validator
                    IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

                    var json = decoder.Decode(bearerToken, key, verify: true);
                    
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    var encryptedClaims = result["SC"];

                    var sensitiveClaims = DecryptClaims(encryptedClaims, Environment.GetEnvironmentVariable("ClaimsKey"));
                    var parsedClaims = ParseDecryptedClaims(sensitiveClaims);
                    
                    var tenantId = parsedClaims["TenantId"];
                    var baseConnection = parsedClaims["Connection"];
                    var contactId = parsedClaims["ContactId"];

                    if (string.IsNullOrEmpty(baseConnection) || string.IsNullOrEmpty(tenantId))
                    {
                        Log.Error("Connection string or tenant ID is missing.");
                        return BadRequest(new { error ="Connection string or tenant ID is missing." });
                    }
                
                    Log.Debug($"Found Identity for ContactID:{contactId}");
                    
                    
                    // Create claims based on webhook data
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "WebhookUser"), 
                        new Claim("CurrentTenantID", tenantId),
                        new Claim("ContactID", contactId)
                    };
    
                    // Create a ClaimsIdentity
                    var identity = new ClaimsIdentity(claims, "WebhookAuthentication");
    
                    // Create a ClaimsPrincipal
                    var principal = new ClaimsPrincipal(identity);
    
                    // Set the User on the current HttpContext
                    HttpContext.User = principal;
                    
                    var credentials = Environment.GetEnvironmentVariable("SQLCredentials") ?? "";
                    if (string.IsNullOrEmpty(credentials))
                    {
                        throw new InvalidOperationException(
                            "Could not find a environment variable string named 'SQLCredentials'.");
                    }
                    await connectionStringManager.SetConnectionStringAsync($"{tenantId}-CourierManager-Connection", baseConnection +credentials);

                    var maskedConnectionString = MaskSensitiveInfo(baseConnection+credentials);
                    Log.Debug($"Connection String Set: {maskedConnectionString}");               

                    
                }
                catch (TokenExpiredException tex)
                {
                    Log.Error(tex, $"Bearer token expired: {tex.Message}");
                    return Unauthorized("Bearer token expired");
                }
                catch (SignatureVerificationException sve)
                {
                    Log.Error(sve, $"Bearer token invalid: {sve.Message}");
                    return Unauthorized("Bearer token invalid");
                }


                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(webhookEvent)}");

                var sData = JsonConvert.SerializeObject(webhookEvent.data);
                switch (webhookEvent.event_type)
                {
                    case "contractors/events/contract_activated":
                        return HandleResponse(await applicantsService.ProcessOfContractorContracted(JsonConvert.DeserializeObject<OfContractorContracted>(sData)));
                    default:
                        return Ok();
                }
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        static Dictionary<string, string> ParseDecryptedClaims(string decryptedClaims)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedClaims);
            }
            catch (JsonException ex)
            {
                Log.Error($"Error parsing decrypted claims: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }
        static string DecryptClaims(string encryptedClaims, string key)
        {
            var normalBase64 = encryptedClaims.Replace("-", "+").Replace("_", "/");
            var fullCipherText = Convert.FromBase64String(normalBase64);
            using var aesAlg = Aes.Create();
            var keyBytes = Convert.FromBase64String(key);
            aesAlg.Key = keyBytes;

            // Read the IV from the beginning of the ciphertext
            var iv = new byte[aesAlg.BlockSize / 8];
            Array.Copy(fullCipherText, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            var cipherTextWithoutIv = new byte[fullCipherText.Length - iv.Length];
            Array.Copy(fullCipherText, iv.Length, cipherTextWithoutIv, 0, cipherTextWithoutIv.Length);

            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using var msDecrypt = new MemoryStream(cipherTextWithoutIv);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        private string MaskSensitiveInfo(string connectionString)
        {
            // Mask password
            var maskedString = Regex.Replace(connectionString, 
                @"(Password|Pwd)=[^;]*", "$1=********", 
                RegexOptions.IgnoreCase);

            // Mask user id if present
            maskedString = Regex.Replace(maskedString, 
                @"(User ID|Uid)=[^;]*", "$1=********", 
                RegexOptions.IgnoreCase);

            return maskedString;
        }

        [HttpPost("Contractors/Search")]
        public async Task<IActionResult> SearchContractors([FromBody] SearchRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                var data = await openforceService.SearchContractors(request);

                return HandleResponse(new BaseResponse(request.MessageId)
                {
                    Data = data.Select(d => new NameIdStringDto()
                    {
                        Id = d.id,
                        Name = $"{d.company_name} {d.first_name} {d.middle_name} {d.last_name}".Trim()
                    }),
                    Success = true
                });
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
