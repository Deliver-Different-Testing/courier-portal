using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Runs;
using CourierPortal.Core.Services.Portal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace CourierPortal.Api.Controllers.Portal
{
    [Route("api/portal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtBearer", Policy = "Courier")]
    public class RunsController : BaseController
    {
        private readonly PortalRunService _runService;

        public RunsController(PortalRunService runService)
        {
            _runService = runService;
        }

        // Get api/runs
        [HttpGet]
        public async Task<IActionResult> Runs()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                var courierId = GetCurrentUser();
                var masterId = GetUser();

                return HandleResponse(await _runService.GetCourierRuns(messageId, courierId, masterId != courierId, masterId == courierId? null : masterId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        // Get api/runs/{BookDate}/{RunName}
        [HttpGet("{BookDate}/{RunName}")]
        public async Task<IActionResult> Run([FromRoute] RunRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                var courierId = GetCurrentUser();
                var masterId = GetUser();

                return HandleResponse(await _runService.GetCourierRun(GetCurrentUser(), request, masterId != courierId, masterId == courierId ? null : masterId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        //// Get api/runs/available
        //[HttpGet("Available")]
        //public async Task<IActionResult> AvailableRuns()
        //{
        //    try
        //    {
        //        Guid messageId = Guid.NewGuid();
        //        Log.Information($"({Request.Method} {Request.Path})({messageId})");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(messageId);

        //        return HandleResponse(await _runService.GetAvailableRuns(messageId, GetCurrentCourier()));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
        //        throw;
        //    }
        //}

        //// Get api/runs/available/{id}
        //[HttpGet("Available/{id}")]
        //public async Task<IActionResult> AvailableRun([FromRoute] AvailableRunRequest request)
        //{
        //    try
        //    {
        //        Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(request.MessageId);

        //        return HandleResponse(await _runService.GetAvailableRun(GetCurrentCourier(), request));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
        //        throw;
        //    }
        //}

        [HttpPost("SendEnquiry")]
        public async Task<IActionResult> SendEnquiry([FromBody] EnquiryRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await _runService.SendEnquiry(User, request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        //TODO:Remove this commented out code if still no plan to bring it back later
        // Get api/runs/apply
        //[HttpPost("Apply")]
        //public async Task<IActionResult> Apply([FromBody] AvailableRunRequest request)
        //{
        //    try
        //    {
        //        Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(request.MessageId);

        //        return HandleResponse(await _runService.ApplyForRun(GetCurrentCourier(), request));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null? e.ToString() : e + Environment.NewLine + e.InnerException);
        //        throw;
        //    }
        //}

    }
}