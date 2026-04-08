using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Reports;
using CourierPortal.Core.Services.Portal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

// SHARED: This controller should call the Accounts API for report data.
// Portal self-service view - courier sees their own reports.
// TODO: Replace direct DB access with HTTP calls to Accounts API.
namespace CourierPortal.Api.Controllers.Portal
{
    [Route("api/portal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtBearer", Policy = "Courier")]
    public class ReportsController(PortalReportService reportService) : BaseController
    {
        [HttpGet("Settings")]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await reportService.GetSettings(messageId, GetCurrentUser()));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetReport([FromBody] ReportParametersDto reportParameters)
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await reportService.GetReportAsync(messageId, GetCurrentUser(), reportParameters));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

    }
}