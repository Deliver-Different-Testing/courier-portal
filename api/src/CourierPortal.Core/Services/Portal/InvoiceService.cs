using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Invoices;
using CourierPortal.Core.DTOs.Portal.Runs;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CourierPortal.Core.Services.Portal
{
    /// <summary>
    /// Invoice service refactored to call Accounts API via HttpClient.
    /// CourierInvoice entities have been moved to the Accounts app.
    /// </summary>
    public class PortalInvoiceService : PortalBaseService
    {
        private readonly PortalTimeZoneService _timeZoneService;
        private readonly PortalRunService _runService;
        private readonly HttpClient _httpClient;

        public PortalInvoiceService(
            PortalTimeZoneService timeZoneService,
            PortalRunService runService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IDbContextFactory<DespatchContext> contextFactory) : base(contextFactory)
        {
            _timeZoneService = timeZoneService;
            _runService = runService;
            _httpClient = httpClientFactory.CreateClient("AccountsApi");

            var baseUrl = configuration["AccountsApi:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
                _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        }

        public async Task<InvoicesResponse> GetRecent(Guid messageId, int courierId)
        {
            var response = new InvoicesResponse(messageId);

            try
            {
                var invoices = await _httpClient.GetFromJsonAsync<IEnumerable<InvoiceDto>>(
                    $"api/contractor-invoices?courierId={courierId}&take=10");

                response.Invoices = invoices ?? [];
                response.Success = true;
            }
            catch (HttpRequestException ex)
            {
                response.Messages.Add(new MessageDto { Message = $"Unable to retrieve invoices from Accounts API: {ex.Message}" });
            }

            return response;
        }

        public async Task<InvoicesPastResponse> GetPast(Guid messageId, int courierId)
        {
            var response = new InvoicesPastResponse(messageId);

            try
            {
                var pastInvoices = await _httpClient.GetFromJsonAsync<IEnumerable<InvoicePastDto>>(
                    $"api/contractor-invoices/past?courierId={courierId}");

                response.Invoices = pastInvoices ?? [];
                response.Success = true;
            }
            catch (HttpRequestException ex)
            {
                response.Messages.Add(new MessageDto { Message = $"Unable to retrieve past invoices from Accounts API: {ex.Message}" });
            }

            return response;
        }

        public async Task<InvoiceResponse> Get(int courierId, InvoiceRequest request)
        {
            var response = new InvoiceResponse(request.MessageId);

            try
            {
                var invoice = await _httpClient.GetFromJsonAsync<InvoiceDto>(
                    $"api/contractor-invoices/{request.InvoiceNo}?courierId={courierId}");

                if (invoice is null)
                {
                    response.Messages.Add(new MessageDto { Message = $"Invoice '{request.InvoiceNo}' not found." });
                    return response;
                }

                response.Invoice = invoice;
                response.Success = true;
            }
            catch (HttpRequestException ex)
            {
                response.Messages.Add(new MessageDto { Message = $"Unable to retrieve invoice from Accounts API: {ex.Message}" });
            }

            return response;
        }

        public async Task<InvoiceResponse> Create(int courierId, Guid messageId)
        {
            var response = new InvoiceResponse(messageId);

            // Validate courier is active and eligible
            var courier = await Context.TucCouriers
                .SingleOrDefaultAsync(c => c.UccrId == courierId && c.Active && !c.UccrInternal);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto { Message = $"Courier '{courierId}' not found, inactive or invoicing access has been revoked." });
                return response;
            }

            if (courier.CourierTypeId != 4)
            {
                response.Messages.Add(new MessageDto { Message = "Invoicing is not supported for this courier type." });
                return response;
            }

            // Get uninvoiced runs from the run service
            var uninvoiced = await _runService.GetCourierRuns(messageId, courier.UccrId, true);
            if (!uninvoiced.Success)
            {
                response.Messages.AddRange(uninvoiced.Messages);
                return response;
            }

            // Forward creation to Accounts API
            try
            {
                var createRequest = new { CourierId = courierId, Runs = uninvoiced.Uninvoiced };
                var httpResponse = await _httpClient.PostAsJsonAsync("api/contractor-invoices", createRequest);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var error = await httpResponse.Content.ReadAsStringAsync();
                    response.Messages.Add(new MessageDto { Message = $"Accounts API error: {error}" });
                    return response;
                }

                var invoice = await httpResponse.Content.ReadFromJsonAsync<InvoiceDto>();
                response.Invoice = invoice;
                response.Success = true;
            }
            catch (HttpRequestException ex)
            {
                response.Messages.Add(new MessageDto { Message = $"Unable to create invoice via Accounts API: {ex.Message}" });
            }

            return response;
        }

        public async Task<UninvoicedResponse> GetUninvoiced(Guid messageId, int courierId, bool calculateMaster = false, int? masterCourierId = null)
        {
            var response = new UninvoicedResponse(messageId);

            var courier = await Context.TucCouriers
                .FirstOrDefaultAsync(c => c.UccrId == courierId && c.Active && !c.UccrInternal);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto { Message = $"Courier '{courierId}' not found, inactive or invoicing access has been revoked." });
                return response;
            }

            var runsResponse = await _runService.GetCourierRuns(messageId, courier.UccrId, calculateMaster, masterCourierId);
            if (!runsResponse.Success)
            {
                response.Messages.AddRange(runsResponse.Messages);
                return response;
            }

            // Get deductions from Accounts API
            IEnumerable<RunDto> deductionRuns = [];
            try
            {
                deductionRuns = await _httpClient.GetFromJsonAsync<IEnumerable<RunDto>>(
                    $"api/contractor-deductions/uninvoiced?courierId={courierId}") ?? [];
            }
            catch (HttpRequestException)
            {
                // Deductions unavailable — continue with runs only
            }

            var uninvoiced = runsResponse.Uninvoiced.ToList();
            uninvoiced.AddRange(deductionRuns);

            var settings = await Context.TblSettings
                .Select(s => new { s.Gstrate, s.CompanyAddress })
                .FirstAsync();

            decimal withholdingTaxPercentage = courier.WithholdingTaxPercentage.HasValue && courier.WithholdingTaxPercentage > 0
                ? courier.WithholdingTaxPercentage.Value / 100
                : 0;
            decimal gstPercentage = courier.WithholdingTaxPercentage.HasValue && courier.WithholdingTaxPercentage > 0
                ? 0
                : settings.Gstrate ?? 0;

            decimal subTotal = uninvoiced.Sum(run => run.Amount);
            decimal gstAmount = Math.Round(subTotal * gstPercentage, 2, MidpointRounding.AwayFromZero);
            decimal withholdingTaxAmount = Math.Round(subTotal * withholdingTaxPercentage * -1m, 2, MidpointRounding.AwayFromZero);
            decimal total = subTotal + gstAmount + withholdingTaxAmount;

            response.ToAddress = settings.CompanyAddress;
            response.Courier = new UninvoicedDto
            {
                Runs = uninvoiced.Where(r => r.Amount != 0),
                Subtotal = subTotal,
                WithholdingTaxPercentage = withholdingTaxPercentage,
                WithholdingTaxAmount = withholdingTaxAmount,
                GstPercentage = gstPercentage,
                GstAmount = gstAmount,
                Total = total
            };

            response.Success = true;
            return response;
        }
    }
}
