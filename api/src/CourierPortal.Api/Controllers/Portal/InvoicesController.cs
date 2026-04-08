using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Invoices;
using CourierPortal.Core.Services.Portal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

// SHARED: This controller should call the Accounts API for invoice data.
// Portal self-service view - courier sees their own invoices.
// TODO: Replace direct DB access with HTTP calls to Accounts API.
namespace CourierPortal.Api.Controllers.Portal
{
    [Route("api/portal/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtBearer", Policy = "Courier")]
    public class InvoicesController(PortalInvoiceService invoiceService) : BaseController
    {
        [HttpGet("Recent")]
        public async Task<IActionResult> GetRecent()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await invoiceService.GetRecent(messageId, GetCurrentUser()));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpGet("Past")]
        public async Task<IActionResult> GetPast()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await invoiceService.GetPast(messageId, GetCurrentUser()));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        [HttpGet("{invoiceNo}")]
        public async Task<IActionResult> Get([FromRoute] InvoiceRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await invoiceService.Get(GetCurrentUser(), request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] InvoiceCreateRequest request)
        //{
        //    try
        //    {
        //        Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

        //        if (!ModelState.IsValid)
        //            return HandleInvalidModelState(request.MessageId);

        //        return HandleResponse(await _invoiceService.Create(GetCurrentCourier(), request));
        //    }
        //    catch (Exception e)
        //    {
        //        //Log exception to file and throw, Raygun should automatically record exception
        //        Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
        //        throw;
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await invoiceService.Create(GetCurrentUser(), messageId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e.ToString() + Environment.NewLine + e.InnerException.ToString());
                throw;
            }
        }

        // Get api/invoices/uninvoiced
        [HttpGet("Uninvoiced")]
        public async Task<IActionResult> Uninvoiced()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);


                var courierId = GetCurrentUser();
                var masterId = GetUser();

                return HandleResponse(await invoiceService.GetUninvoiced(messageId, courierId, masterId != courierId, masterId == courierId ? null : masterId));
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