using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Couriers;
using CourierPortal.Core.DTOs.Portal.Invoices;
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
    public class CouriersController(PortalCourierService courierService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await courierService.Get(messageId, GetCurrentUser()));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(CourierUpdateRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await courierService.Update(GetCurrentUser(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpGet("Contractors")]
        public async Task<IActionResult> GetContractors()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await courierService.GetContractors(messageId, GetUser()));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpGet("Contractors/{Id}")]
        public async Task<IActionResult> GetContractor([FromRoute] IdentifierRequest request)
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await courierService.GetContractor(GetUser(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }


        [HttpPost("Contractors")]
        public async Task<IActionResult> UpdateContractor(SubcontractorUpdateRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await courierService.UpdateContractor(GetUser(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }
        //TODO: CourierAvailability may not be needed anymore, if this still isn't used for quite sometime then it can be removed and tables can be dropped.
        //[HttpGet("Availability", Name = "Availability")]
        //public async Task<IActionResult> Availability()
        //{
        //    try
        //    {
        //        Guid messageId = Guid.NewGuid();
        //        Log.Information($"({Request.Method} {Request.Path})({messageId})");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(messageId);

        //        return HandleResponse(await _courierService.GetCourierAvailability(messageId, GetCurrentCourier()));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
        //        throw;
        //    }
        //}

        //[HttpPost("Availability", Name = "Availability")]
        //public async Task<IActionResult> UpdateAvailability([FromBody] CourierAvailabilityUpdateRequest request)
        //{
        //    try
        //    {
        //        Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(request.MessageId);

        //        return HandleResponse(await _courierService.UpdateAvailability(GetCurrentCourier(), request));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
        //        throw;
        //    }
        //}

        //[HttpGet("Summary", Name = "Summary")]
        //public async Task<IActionResult> Summary()
        //{
        //    try
        //    {
        //        Guid messageId = Guid.NewGuid();
        //        Log.Information($"({Request.Method} {Request.Path})({messageId})");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(messageId);

        //        return HandleResponse(await _courierService.GetCourierSummary(messageId, GetCurrentCourier()));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
        //        throw;
        //    }
        //}

    }
}