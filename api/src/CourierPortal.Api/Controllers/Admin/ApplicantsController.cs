using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Admin.Applicants;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CourierPortal.Api.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    public class ApplicantsController(AdminApplicantsService applicantsService) : Controller
    {
        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.Search(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // GET api/<controller>/{Id}
        [HttpGet("{Id}")]
        public async Task<IActionResult> Get([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.Get(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // POST api/<controller>/{Id}/Delete
        [HttpPost("{Id}/Delete")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromBody] ApplicantDeleteRequest request)
        {
            try
            {
                request.Id = id;
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.Delete(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // POST api/<controller>/{Id}/TrainingCompleted
        [HttpPost("{Id}/TrainingCompleted")]
        public async Task<IActionResult> PostTrainingCompleted([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.PostTrainingCompleted(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // POST api/<controller>/{Id}/Reject
        [HttpPost("{Id}/Reject")]
        public async Task<IActionResult> PostReject([FromRoute] int id, [FromBody] ApplicantRejectRequest request)
        {
            try
            {
                request.Id = id;
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.PostReject(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // POST api/<controller>/{Id}/Approve
        [HttpPost("{Id}/Approve")]
        public async Task<IActionResult> PostApprove([FromRoute] int id, [FromBody] ApplicantApproveRequest request)
        {
            try
            {
                request.Id = id;
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.PostApprove(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // POST api/<controller>/{Id}/PasswordReset
        [HttpPost("{Id}/PasswordReset")]
        public async Task<IActionResult> PostPasswordReset([FromRoute] int id, [FromBody] ApplicantPasswordResetRequest request)
        {
            try
            {
                request.Id = id;
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.PostPasswordReset(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        // GET api/<controller>/Summaries/Location
        [HttpGet("Summaries/Location")]
        public async Task<IActionResult> GetSummariesByLocation()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await applicantsService.GetSummariesByLocation(messageId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpPost("Documents")]
        public async Task<IActionResult> CreateDocument([FromBody] ApplicantDocumentCreateRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.CreateDocument(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Documents")]
        public async Task<IActionResult> GetDocuments()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await applicantsService.GetDocuments(messageId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Documents/{Id}")]
        public async Task<IActionResult> GetDocument([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.GetDocument(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Documents/{Id}/File")]
        public async Task<IActionResult> GetDocumentFile([FromRoute] IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.GetDocumentFile(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Documents/Active")]
        public async Task<IActionResult> GetDocumentsActive()
        {
            try
            {
                Guid messageId = Guid.NewGuid();
                Log.Information($"({Request.Method} {Request.Path})({messageId})");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(messageId);

                return HandleResponse(await applicantsService.GetDocumentsActive(messageId));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("{Id}/Documents")]
        public async Task<IActionResult> GetDocuments(IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.GetDocumentsByApplicant(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpPost("Documents/{Id}")]
        public async Task<IActionResult> PostDocument([FromBody] ApplicantDocumentUpdateRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.PostDocument(request));
            }
            catch (Exception e)
            {
                //Log exception to file and throw, Raygun should automatically record exception
                Log.Error(e.InnerException == null ? e.ToString() : e + Environment.NewLine + e.InnerException);
                throw;
            }
        }

        [HttpGet("Uploads/{Id}/File")]
        public async Task<IActionResult> GetUploadFile(IdentifierRequest request)
        {
            try
            {
                Log.Information($"({Request.Method} {Request.Path}): {JsonConvert.SerializeObject(request)}");

                if (!ModelState.IsValid)
                    return HandleInvalidModelState(request.MessageId);

                return HandleResponse(await applicantsService.GetUploadFile(request));
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
            Log.Information(response is FileResponse
                ? $"Response ({response.MessageId}): {JsonConvert.SerializeObject(new BaseResponse(response.MessageId) { Success = response.Success, Messages = response.Messages })}"
                : $"Response ({response.MessageId}): {JsonConvert.SerializeObject(response)}");

            if (response.Success)
            {
                if (response is FileResponse fileResponse)
                    return File(fileResponse.File, fileResponse.FileType, fileResponse.FileName);

                return Ok(response);
            }

            return BadRequest(response);
        }

    }
}
