using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Openforce;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace CourierPortal.Core.Services.Admin
{
    public class OpenforceService(IHttpClientFactory httpClientFactory, IDbContextFactory<DespatchContext> contextFactory) : AdminBaseService(contextFactory)
    {

        private DateTime _accessTokenAccessTime;
        private OfAccessToken _accessToken;
        private OfSettings _settings;

        public async Task<OfSettings> GetSettingsAsync(bool ignoreNotConfigured = false)
        {
            if (_settings == null)
            {
                _settings = await Context.TblSettings
                .Select(s => new OfSettings
                {
                    ClientId = s.OpenforceClientId,
                    ClientGuid = s.OpenforceClientGuid,
                    AccessKey = s.OpenforceAccessKey,
                    ApiKey = s.OpenforceApiKey
                })
                .FirstOrDefaultAsync();

                if (_settings != null)
                {
                    var courierTypes = await Context.CourierTypes
                        .Select(t => new { t.Id, t.OpenforceActivationCode })
                        .ToListAsync();

                    _settings.ActivationCode1 = courierTypes.First(t => t.Id == 1).OpenforceActivationCode;
                    _settings.ActivationCode2 = courierTypes.First(t => t.Id == 2).OpenforceActivationCode;
                    _settings.ActivationCode3 = courierTypes.First(t => t.Id == 3).OpenforceActivationCode;
                }
            }

            if (!ignoreNotConfigured
                && (string.IsNullOrWhiteSpace(_settings?.ClientId) || string.IsNullOrWhiteSpace(_settings?.ClientGuid) || string.IsNullOrWhiteSpace(_settings?.ApiKey) || string.IsNullOrWhiteSpace(_settings.AccessKey) || (string.IsNullOrWhiteSpace(_settings.ActivationCode1) && string.IsNullOrWhiteSpace(_settings.ActivationCode2) && string.IsNullOrWhiteSpace(_settings.ActivationCode3))))
                throw new Exception("Openforce is not been configured.");

            return _settings;
        }

        public async Task<BaseResponse> Save(OfSettingsSaveRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            if (string.IsNullOrWhiteSpace(request.ClientId))
            {
                response.Messages.Add(new MessageDto() { Message = $"Client ID is required." });
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.ClientGuid))
            {
                response.Messages.Add(new MessageDto() { Message = $"Client GUID is required." });
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.AccessKey))
            {
                response.Messages.Add(new MessageDto() { Message = $"Access Key is required." });
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.ApiKey))
            {
                response.Messages.Add(new MessageDto() { Message = $"API Key is required." });
                return response;
            }

            var dbData = await Context.TblSettings.FirstOrDefaultAsync();

            if (dbData is null)
            {
                response.Messages.Add(new MessageDto() { Message = "Settings not found." });
                return response;
            }

            dbData.OpenforceClientId = request.ClientId.Trim();
            dbData.OpenforceClientGuid = request.ClientGuid.Trim();
            dbData.OpenforceAccessKey = request.AccessKey.Trim();
            dbData.OpenforceApiKey = request.ApiKey.Trim();

            var courierTypes = await Context.CourierTypes
                .ToListAsync();

            courierTypes.First(t => t.Id == 1).OpenforceActivationCode = string.IsNullOrWhiteSpace(request.ActivationCode1)? null : request.ActivationCode1.Trim();
            courierTypes.First(t => t.Id == 2).OpenforceActivationCode = string.IsNullOrWhiteSpace(request.ActivationCode2) ? null : request.ActivationCode2.Trim();
            courierTypes.First(t => t.Id == 3).OpenforceActivationCode = string.IsNullOrWhiteSpace(request.ActivationCode3) ? null : request.ActivationCode3.Trim();

            await Context.SaveChangesAsync();

            response.Data = new OfSettings
            {
                ClientId = dbData.OpenforceClientId,
                ClientGuid = dbData.OpenforceClientGuid,
                AccessKey = dbData.OpenforceAccessKey,
                ApiKey = dbData.OpenforceApiKey,
                ActivationCode1 = courierTypes.First(t => t.Id == 1).OpenforceActivationCode,
                ActivationCode2 = courierTypes.First(t => t.Id == 2).OpenforceActivationCode,
                ActivationCode3 = courierTypes.First(t => t.Id == 3).OpenforceActivationCode
            };

            response.Success = true;
            return response;
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                return await GetSettingsAsync() != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GetAccessToken()
        {
            if (_accessToken != null && _accessTokenAccessTime.AddSeconds(_accessToken.expires_in) > DateTime.Now.AddSeconds(10))
                return _accessToken.access_token;

            var url = Environment.GetEnvironmentVariable("OpenforceAuthUrl");

            var settings = await GetSettingsAsync();

            var formData = new Dictionary<string, string>
            {
                { "client_id", settings.ClientId},
                { "grant_type", "client_credentials" }
            };

            var content = new FormUrlEncodedContent(formData);

            using (var client = httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("authorization", $"Basic {settings.AccessKey}");
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "MWhuODAyaWI4aWU5YjFyMHNuYnJwZ3Q1dmM6MTRuODBhbG02a2luZ3VvNjc3ZTNoOHZhczdmNnF0bDB2aXRrYnUxdGhiMWEyNDB0cWRmNA==");
                _accessToken = null;
                _accessTokenAccessTime = DateTime.Now;
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                _accessToken = JsonConvert.DeserializeObject<OfAccessToken>(responseBody);
                return _accessToken.access_token;
            }
        }

        public async Task<BaseResponse> SendInvitationAsync(OfInvitation data)
        {
            try
            {
                Log.Information("Starting SendInvitationAsync for individual: {@IndividualData}",
                    new { data.first_name, data.last_name, data.email });

                var settings = await GetSettingsAsync();
                Log.Debug("Retrieved OpenForce settings. ClientGuid: {ClientGuid}", settings.ClientGuid);

                //data.activation_code = settings.ActivationCode;
                //data.individual_data.activation_code = settings.ActivationCode;

                var serializedContent = JsonConvert.SerializeObject(data);
                Log.Debug("Request payload: {@Payload}", serializedContent);

                var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

                using var client = httpClientFactory.CreateClient();
                var baseUrl = Environment.GetEnvironmentVariable("OpenforceBaseUrl");
                Log.Debug("Using OpenForce base URL: {BaseUrl}", baseUrl);
                
                client.BaseAddress = new Uri(baseUrl);
                
                client.DefaultRequestHeaders.Clear();
                var accessToken = await GetAccessToken();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Add("X-API-KEY", settings.ApiKey);

                Log.Debug("Sending POST request to /clients/{ClientGuid}/invitations", settings.ClientGuid);

                var response = await client.PostAsync($"/clients/{settings.ClientGuid}/invitations", content);

                var result = new BaseResponse();

                var responseBody = await response.Content.ReadAsStringAsync();
                Log.Debug("Response status code: {StatusCode}, Response body: {@ResponseBody}",
                    response.StatusCode, responseBody);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<OfInvitationResponse>(responseBody);
                    result.Data = responseData.Data;
                    result.Success = true;
                    Log.Information("Successfully sent invitation. UserId: {UserId}", responseData.Data.user_id);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    Log.Warning(responseBody);
                    var responseData = JsonConvert.DeserializeObject<OfBadRequest>(responseBody);

                    if (responseData.Errors.Any())
                        result.Messages.AddRange(responseData.Errors.Select(e => new MessageDto() { Message = e }).ToList());

                    if (!result.Messages.Any())
                        result.Messages.Add(new MessageDto() { Message = "(400) Bad response from server." });

                    result.Success = false;
                }
                else
                {
                    Log.Error(
                        "OpenForce API request failed. Status: {StatusCode}, Response: {@Response}, Request: {@Request}",
                        response.StatusCode,
                        responseBody,
                        new
                        {
                            BaseUrl = baseUrl,
                            Path = $"/clients/{settings.ClientGuid}/invitations",
                            Payload = serializedContent,
                            Headers = new
                            {
                                Authorization = "Bearer " + accessToken.Substring(0, 10) + "...",
                                ApiKey = settings.ApiKey.Substring(0, 5) + "..."
                            }
                        }
                    );

                    result.Messages.Add(new MessageDto() { Message = $"OpenForce API request failed. Status: {response.StatusCode}" });

                    result.Success = false;
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex,
                    "HTTP request failed in SendInvitationAsync. Message: {Message}",
                    ex.Message);
                throw;
            }
            catch (JsonException ex)
            {
                Log.Error(ex,
                    "JSON serialization/deserialization failed in SendInvitationAsync. Message: {Message}",
                    ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Unexpected error in SendInvitationAsync. Message: {Message}",
                    ex.Message);
                throw;
            }
        }

        public async Task<List<OfContractor>> SearchContractors(SearchRequest request)
        {
            string url = string.IsNullOrWhiteSpace(request.SearchText)
                ? "/contractors?count=2147483647"
                : $"/contractors/search?count=2147483647;name={WebUtility.UrlEncode(request.SearchText ?? string.Empty)}";
            return await SendAsync<List<OfContractor>>(HttpMethod.Get, url);
        }

        public async Task<OfSettlement> GetSettlementAsync(string id)
        {
            return await SendAsync<OfSettlement>(HttpMethod.Get, $"/settlements/{id}");
        }
        public async Task<OfDeductionType> GetDeductionTypeAsync(string id)
        {
            return await SendAsync<OfDeductionType>(HttpMethod.Get, $"/deduction_types/{id}");
        }

        public async Task<List<OfCommission>> GetCommissionsAsync(string settlementId)
        {
            return await SendAsync<List<OfCommission>>(HttpMethod.Get, $"/settlements/{settlementId}/commissions");
        }

        public async Task<List<OfDeduction>> GetDeductionsAsync(string settlementId)
        {
            return await SendAsync<List<OfDeduction>>(HttpMethod.Get, $"/settlements/{settlementId}/deductions");
        }

        public async Task<OfSettlement> AddUpdateAsync(OfSettlement data)
        {
            return string.IsNullOrWhiteSpace(data.id)
                ? await SendAsync<OfSettlement>(HttpMethod.Post, "/settlements", data)
                : await SendAsync<OfSettlement>(HttpMethod.Put, $"/settlements/{data.id}", data);
        }

        public async Task<OfCommission> AddUpdate(OfCommission data)
        {
            return string.IsNullOrWhiteSpace(data.id)
                ? await SendAsync<OfCommission>(HttpMethod.Post, $"/settlements/{data.settlement_id}/commissions", data)
                : await SendAsync<OfCommission>(HttpMethod.Put, $"/settlements/{data.settlement_id}/commissions/{data.id}", data);
        }

        public async Task<OfDeduction> AddUpdate(OfDeduction data)
        {
            return string.IsNullOrWhiteSpace(data.id)
                ? await SendAsync<OfDeduction>(HttpMethod.Post, $"/settlements/{data.settlement_id}/deductions", data)
                : await SendAsync<OfDeduction>(HttpMethod.Put, $"/settlements/{data.settlement_id}/deductions/{data.id}", data);
        }

        private async Task<T> SendAsync<T>(HttpMethod method, string url, object data = null)// where T : OfEntity
        {
            try
            {
                var settings = await GetSettingsAsync();

                var sData = data == null ? null : JsonConvert.SerializeObject(data);
                var content = sData == null
                    ? null
                    : new StringContent(sData, Encoding.UTF8, "application/json");

                var sLog = sData == null ? $"({method} {url})" : $"({method} {url}): {sData}";
                Log.Information(sLog);
                using (var client = httpClientFactory.CreateClient())
                {
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("OpenforceBaseUrl"));
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
                    client.DefaultRequestHeaders.Add("X-API-KEY", settings.ApiKey);
                    var response = method == HttpMethod.Post
                        ? await client.PostAsync(url, content)
                        : method == HttpMethod.Put ? await client.PutAsync(url, content)
                        : await client.GetAsync(url);

                    if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.UnprocessableEntity)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        Log.Error($"({method} {url}) Response: [{response.StatusCode}] {error}");
                    }

                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<T>(responseBody);
                    Log.Information($"({method} {url}) Response: {responseBody}");
                    return result;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}
