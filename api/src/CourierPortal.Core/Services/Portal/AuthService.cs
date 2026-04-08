using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Auth;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Infrastructure.Models;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalAuthService(IHttpClientFactory httpClientFactory, IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        public async Task<TokenResponse> Login(TokenRequest request)
        {
            var response = new TokenResponse(request.MessageId);

            var courier = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.Active
                    && (c.Code == request.Username || (c.UccrEmail != null && c.UccrEmail.Trim().ToLower() == request.Username.ToLower())));

            CourierApplicant applicant = null;

            if (courier == null)
            {
                applicant = await Context.CourierApplicants.FirstOrDefaultAsync(a => a.Email.ToLower().Trim() == request.Username.ToLower().Trim());

                if (applicant == null || applicant.CourierId.HasValue || !applicant.EmailVerified || applicant.Password != request.Password)
                {
                    response.Messages.Add(new MessageDto() { Message = $"Invalid username or password." });
                    return response;
                }
            }
            else if (string.IsNullOrWhiteSpace(courier.UccrPassword) || courier.UccrPassword != request.Password)
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid username or password." });
                return response;
            }

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstAsync();
            response.Results = GenerateToken(courier == null ? AccountType.Applicant : AccountType.Courier, courier?.CourierTypeId == 2? courier.UccrId : null, courier?.UccrId ?? applicant.Id, courier?.CourierTypeId, courier?.Code, courier?.UccrEmail ?? applicant?.Email, courier != null && (courier.Code == courier.UccrPassword || (courier.PasswordResetRequired.HasValue && courier.PasswordResetRequired.Value)), countryCode);

            response.Success = true;

            return response;
        }

        public async Task<TokenResponse> LoginFromHub(string email, Guid messageId)
        {
            var response = new TokenResponse(messageId);

            // Look up courier by email - no password check needed as user is already authenticated by Hub
            var courier = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.Active && c.UccrEmail != null && c.UccrEmail.Trim().ToLower() == email.ToLower().Trim());

            if (courier == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"No active courier found for email {email}." });
                return response;
            }

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstAsync();

            response.Results = GenerateToken(
                AccountType.Courier,
                courier.CourierTypeId == 2 ? courier.UccrId : null,
                courier.UccrId,
                courier.CourierTypeId,
                courier.Code,
                courier.UccrEmail,
                courier.Code == courier.UccrPassword || (courier.PasswordResetRequired.HasValue && courier.PasswordResetRequired.Value),
                countryCode
            );

            response.Success = true;

            return response;
        }

        public async Task<TokenResponse> LoginAccessKey(TokenAccessKeyRequest request)
        {
            var response = new TokenResponse(request.MessageId);

            CourierManagerAccessResponse courierManagerAccessResponse = null; 
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CourierPortalApiBase"));
                var courierManagerResponse = await httpClient.PostAsJsonAsync("Auth/CourierPortal/AccessValidation", request);
                
                if (!courierManagerResponse.IsSuccessStatusCode)
                {
                    response.Messages.Add(new MessageDto() { Message = $"Invalid access key." });
                    return response;
                }

                var jsonResult = await courierManagerResponse.Content.ReadAsStringAsync();
                courierManagerAccessResponse = JsonConvert.DeserializeObject<CourierManagerAccessResponse>(jsonResult);
            }

            if (courierManagerAccessResponse == null || !courierManagerAccessResponse.Success || courierManagerAccessResponse.Id < 1)
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid access key." });
                return response;
            }

            var courier = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.UccrId == courierManagerAccessResponse.Id && c.Active);

            if (courier == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid access key." });
                return response;
            }

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstAsync();
            response.Results = GenerateToken(AccountType.Courier, courier.CourierTypeId == 2 ? courier.UccrId : null, courier.UccrId, courier.CourierTypeId, courier?.Code, courier?.UccrEmail, courier.Code == courier.UccrPassword || (courier.PasswordResetRequired.HasValue && courier.PasswordResetRequired.Value), countryCode);

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> ChangePassword(int courierId, ChangePasswordRequest request)
        {
            var response = new BaseResponse(request.MessageId);

            var courier = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.UccrId == courierId && c.Active);

            if (courier == null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier {courierId} not found or access has been revoked." });
                return response;
            }

            if (courier.Code == request.NewPassword)
            {
                var message = new MessageDto() { Message = $"'New password' must not match your courier code." };
                message.Params.Add("NewPassword");
                response.Messages.Add(message);
                return response;
            }

            if (courier.UccrPassword != request.OldPassword)
            {
                var message = new MessageDto() { Message = $"'Old password' is invalid." };
                message.Params.Add("OldPassword");
                response.Messages.Add(message);
                return response;
            }

            courier.UccrPassword = request.NewPassword;
            courier.PasswordResetRequired = false;

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        public async Task<TokenResponse> ChangeUser(int masterCourierId, IdentifierRequest request)
        {
            var response = new TokenResponse(request.MessageId);

            var courier = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.UccrId == request.Id 
                     && (request.Id == masterCourierId || c.MasterCourierId == masterCourierId)
                     && c.Active);

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstAsync();
            response.Results = GenerateToken(AccountType.Courier, masterCourierId, courier.UccrId, courier.CourierTypeId, courier.Code, courier.UccrEmail, false, countryCode);

            response.Success = true;

            return response;
        }

        private TokenDto GenerateToken(AccountType accountType, int? masterId, int id, int? courierType, string code, string email, bool passwordResetRequired, string countryCode)
        {
            var refreshToken = GenerateRefreshToken();

            var token = CreateToken(accountType, masterId, id, code, email);

            return new TokenDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = token.ValidTo,
                RefreshToken = refreshToken,
                AccountType = accountType,
                CourierType = courierType,
                PasswordResetRequired = passwordResetRequired,
                CountryCode = countryCode
            };
        }

        private JwtSecurityToken CreateToken(AccountType accountType, int? masterId, int id, string code, string email)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTSecretKey")));

            var claims = new Claim[]
            {
                new Claim("AccountType", accountType.ToString()),
                new Claim(JwtRegisteredClaimNames.NameId, (masterId ?? id).ToString()),
                new Claim("CurrentId", id.ToString()),
                new Claim("Code", code ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, email?.Trim() ?? string.Empty)
            };

            return new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("Issuer"),
                audience: Environment.GetEnvironmentVariable("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256)
            );
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<TokenResponse> RefreshToken(TokenRefreshRequest request)
        {
            var response = new TokenResponse(request.MessageId);

            var claimsPrincipal = GetPrincipalFromExpiredToken(request.Token);

            if (!Enum.TryParse(claimsPrincipal.FindFirst("AccountType").Value, out AccountType accountType) 
                || !int.TryParse(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value, out var id))
            {
                response.Messages.Add(new MessageDto() { Message = $"Invalid Claims." });
                return response;
            }

            TucCourier courier = null;
            if (accountType == AccountType.Courier)
            {
                courier = await Context.TucCouriers
                    .FirstOrDefaultAsync(c => c.UccrId == id && c.Active);

                if (courier == null)
                {
                    response.Messages.Add(new MessageDto() { Message = $"Courier {id} not found." });
                    return response;
                }
            }

            if (accountType == AccountType.Applicant)
            {
                var applicant = await Context.CourierApplicants.FirstOrDefaultAsync(a => a.Id == id);

                if (applicant == null)
                {
                    response.Messages.Add(new MessageDto() { Message = $"Applicant {id} not found." });
                    return response;
                }
            }

            //TODO: Validate refreshToken and invalidate old and save new, temporarily only validating on old token
            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstAsync();
            response.Results = GenerateToken(accountType, courier?.CourierTypeId == 3 ? courier.MasterCourierId : null, id, courier?.CourierTypeId, courier?.Code, courier?.UccrEmail, accountType == AccountType.Courier && (courier.Code == courier.UccrPassword || (courier.PasswordResetRequired.HasValue && courier.PasswordResetRequired.Value)), countryCode);

            response.Success = true;

            return response;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTSecretKey")));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = symmetricSecurityKey,
                ValidateAudience = true,
                ValidAudience = Environment.GetEnvironmentVariable("Audience"),
                ValidateIssuer = true,
                ValidIssuer = Environment.GetEnvironmentVariable("Issuer"),
                ValidateLifetime = false //Ignore token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

    }
}
