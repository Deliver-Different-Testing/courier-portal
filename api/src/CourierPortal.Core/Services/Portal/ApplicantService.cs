using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CourierPortal.Api.Core.Application.Dtos.Common;
using CourierPortal.Core.DTOs.Portal.Applications;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.Utilities;
using CourierPortal.Infrastructure.Services;
using CourierPortal.Infrastructure.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalApplicantService(IWebHostEnvironment hostingEnvironment, PortalTimeZoneService timeZoneService, EmailService emailService, IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        public async Task<BaseResponse> Register(RegisterRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            if (await Context.CourierApplicants.AnyAsync(a => a.Email.ToLower().Trim() == request.Email.ToLower().Trim())
                || await Context.TucCouriers.AnyAsync(c => c.UccrEmail != null && c.UccrEmail.ToLower().Trim() == request.Email.ToLower().Trim()))
            {
                var message = new MessageDto() { Message = $"'Email' already in use." };
                message.Params.Add("Email");
                response.Messages.Add(message);
                return response;
            }

            TblBulkRegion location = request.Location == null
                ? null
                : await Context.TblBulkRegions.SingleOrDefaultAsync(r => r.BulkRegionId == request.Location.Id);

            if (location == null)
            {
                var message = new MessageDto() { Message = $"'Location' is invalid." };
                message.Params.Add("Location");
                response.Messages.Add(message);
                return response;
            }

            if (!location.CourierApplicantEnabled.HasValue || !location.CourierApplicantEnabled.Value)
            {
                var message = new MessageDto() { Message = $"Registrations for '{location.Name}' is full." };
                message.Params.Add("Location");
                response.Messages.Add(message);
                return response;
            }

            VehicleType vehicleType = await Context.VehicleTypes.FirstOrDefaultAsync(v => v.Name.Trim().ToLower() == request.VehicleType.Trim().ToLower());
            if (vehicleType == null)
            {
                var message = new MessageDto() { Message = $"'VehicleType' is invalid." };
                message.Params.Add("VehicleType");
                response.Messages.Add(message);
                return response;
            }

            string verificationCode = RandomNumber.Next(100000, 999999).ToString();
            string emailMessage = await GenerateRegisteredEmailVerificationTemplate(verificationCode, request.Email.Trim());

            var tenantTime = timeZoneService.GetTenantTime();
            Context.CourierApplicants.Add(new CourierApplicant()
            {
                Created = tenantTime,
                Email = request.Email.Trim(),
                Password = request.NewPassword,
                FirstName = request.FirstName?.Trim(),
                Surname = request.Surname?.Trim(),
                Phone = request.Phone?.Trim(),
                Mobile = request.Mobile?.Trim(),
                VehicleType = vehicleType.Name, //TODO: Change over to VehicleTypeId
                RegionId = request.Location.Id,
                EmailVerificationCode = verificationCode
            });

            //Use existing email system for sending email
            Context.TucManualMessages.Add(new TucManualMessage()
            {
                UcmmDate = tenantTime,
                UcmmAttempts = 0,
                SendToEmailAddress = request.Email.Trim(),
                ReplyToEmailAddress = Environment.GetEnvironmentVariable("UrgentArmyEmail"),
                Subject = "Applicant Registration",
                UcmmMessage = emailMessage
            });

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> EmailVerification(EmailVerificationRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants
                .SingleOrDefaultAsync(a => a.Email.Trim().ToLower() == request.Email.Trim().ToLower());

            if (applicant == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Invalid verification code.");

            if (applicant.EmailVerificationAttempts >= 5)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Verification attempts exceeded for '{request.Email}'.");

            if (applicant.EmailVerificationCode != request.VerificationCode)
            {
                applicant.EmailVerificationAttempts += 1;
                await Context.SaveChangesAsync();
                return ResponseUtility.AddMessageAndReturnResponse(response, "Invalid verification code.");
            }

            if (!applicant.EmailVerified)
            {
                applicant.EmailVerified = true;
                await Context.SaveChangesAsync();
            }

            response.Success = true;

            return response;
        }

        public async Task<ProfileResponse> Get(Guid messageId, int applicantId)
        {
            ProfileResponse response = new ProfileResponse(messageId);

            CourierApplicant applicant = await Context.CourierApplicants
                .Include(a => a.Region)
                .SingleOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} not found.");

            response.Profile = ApplicantUtility.CreateProfileDto(applicant);

            response.Success = true;

            return response;
        }

        public async Task<ProfileResponse> Update(int applicantId, ProfileUpdateRequest request)
        {
            ProfileResponse response = new ProfileResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants.SingleOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null || !applicant.EmailVerified)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} has already been approved.");

            if (ApplicantUtility.HasCompletedDeclaration(applicant))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} is currently being reviewed.");

            TblBulkRegion location = request.Location == null
                ? null
                : Context.TblBulkRegions.SingleOrDefault(r => r.BulkRegionId == request.Location.Id);

            if (location == null)
            {
                var message = new MessageDto() { Message = $"'Location' is invalid." };
                message.Params.Add("Location");
                response.Messages.Add(message);
                return response;
            }

            if (!location.CourierApplicantEnabled.HasValue || !location.CourierApplicantEnabled.Value)
            {
                var message = new MessageDto() { Message = $"Registrations for '{location.Name}' is full." };
                message.Params.Add("Location");
                response.Messages.Add(message);
                return response;
            }

            VehicleType vehicleType = await Context.VehicleTypes.FirstOrDefaultAsync(v => v.Name.Trim().ToLower() == request.VehicleType.Trim().ToLower());
            if (vehicleType == null)
            {
                var message = new MessageDto() { Message = $"'VehicleType' is invalid." };
                message.Params.Add("VehicleType");
                response.Messages.Add(message);
                return response;
            }

            applicant.RegionId = request.Location.Id;
            applicant.FirstName = request.FirstName?.Trim();
            applicant.Surname = request.Surname?.Trim();
            applicant.DateOfBirth = request.DateOfBirth.HasValue && request.DateOfBirth?.Kind == DateTimeKind.Utc ? timeZoneService.ConvertUtcToTenantTime(request.DateOfBirth.Value) : request.DateOfBirth;
            applicant.Phone = request.Phone?.Trim();
            applicant.Mobile = request.Mobile?.Trim();
            applicant.DriversLicenceNo = request.DriversLicenceNo?.Trim();
            applicant.VehicleRegistrationNo = request.VehicleRegistrationNo.Trim();
            applicant.VehicleType = vehicleType.Name; //TODO: Change over to VehicleTypeId
            applicant.GstRegistered = request.GstRegistered;
            applicant.TaxNo = request.TaxNo?.Trim();
            applicant.BankRoutingNumber = request.BankRoutingNumber?.Trim();
            applicant.BankAccountNo = request.BankAccountNo?.Trim();
            applicant.NextOfKin = request.NextOfKin?.Trim();
            applicant.NextOfKinRelationship = request.NextOfKinRelationship?.Trim();
            applicant.NextOfKinPhone = request.NextOfKinPhone?.Trim();
            applicant.NextOfKinAddress = request.NextOfKinAddress?.Trim();
            applicant.AddressLine1 = request.AddressLine1?.Trim();
            applicant.AddressLine2 = request.AddressLine2?.Trim();
            applicant.AddressLine3 = request.AddressLine3?.Trim();
            applicant.AddressLine4 = request.AddressLine4?.Trim();
            applicant.AddressLine5 = request.AddressLine5?.Trim();
            applicant.AddressLine6 = request.AddressLine6?.Trim();
            applicant.AddressLine7 = request.AddressLine7?.Trim();
            applicant.AddressLine8 = request.AddressLine8?.Trim();
            //Reset contract and declaration as profile information has been updated.
            ApplicantUtility.ClearDeclarationNoSave(applicant);

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, "Record has been updated by another user.");
            }

            return await Get(request.MessageId, applicant.Id);
        }

        public async Task<BaseResponse> GetLocations(Guid messageId)
        {
            return new BaseResponse(messageId)
            {
                Data = await Context.TblBulkRegions
                    .Where(r => r.CourierApplicantEnabled.HasValue && r.CourierApplicantEnabled.Value)
                    .OrderBy(r => r.Name)
                    .Select(r => new NameId()
                    {
                        Id = r.BulkRegionId,
                        Name = r.Name
                    })
                    .ToListAsync(),
                Success = true
            };
        }

        #region "Documents"

        public async Task<FileResponse> DocumentGetFile(IdentifierRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            var document = await Context.CourierApplicantDocuments
                .Select(d => new { d.Id, d.FileName, d.Type, d.Data })
                .SingleOrDefaultAsync(d => d.Id == request.Id);

            if (document == null || string.IsNullOrWhiteSpace(document.FileName) || string.IsNullOrWhiteSpace(document.Type) || document.Data == null || document.Data.Length < 1)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Document {request.Id} not found.");

            response.FileName = document.FileName;
            response.FileType = document.Type;
            response.File = document.Data;

            response.Success = true;

            return response;
        }

        public async Task<DocumentsResponse> DocumentsGet(Guid messageId, int applicantId)
        {
            DocumentsResponse response = new DocumentsResponse(messageId);

            var uploadInfos = await Context.CourierApplicantUploads
                .Where(u => u.ApplicantId == applicantId)
                .Select(u => new { u.Id, u.DocumentId, u.FileName, u.Type, u.Length })
                .ToListAsync();

            var documentInfos = await Context.CourierApplicantDocuments
                .Where(d => d.Active || d.CourierApplicantUploads.Any(u => u.ApplicantId == applicantId && u.DocumentId == d.Id))
                .Select(d => new { d.Id, d.Name, d.Instructions, d.Mandatory, d.FileName, d.Type, d.Length, d.Active })
                .ToListAsync();

            response.Documents = documentInfos.Select(d => new DocumentDto()
            {
                Id = d.Id,
                Name = d.Name,
                Instructions = d.Instructions,
                Mandatory = d.Mandatory,
                FileName = d.FileName,
                Type = d.Type,
                Length = d.Length,
                Active = d.Active,
                Uploads = uploadInfos
                    .Where(u => u.DocumentId == d.Id)
                    .Select(u => new UploadDto()
                    {
                        Id = u.Id,
                        DocumentId = u.DocumentId,
                        FileName = u.FileName,
                        Type = u.Type,
                        Length = u.Length
                    })
            });

            response.Success = true;

            return response;
        }

        #endregion

        #region "Uploads"

        public async Task<BaseResponse> UploadGet(int applicantId, IdentifierRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierApplicantUpload upload = await Context.CourierApplicantUploads
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.ApplicantId == applicantId);

            if (upload == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Upload {request.Id} not found for application {applicantId}.");

            response.FileType = upload.Type;
            response.FileName = upload.FileName;
            response.File = upload.Data;

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> UploadPost(int applicantId, UploadRequest request)
        {
            UploadResponse response = new UploadResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants.SingleOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null || !applicant.EmailVerified)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} has already been approved.");

            if (ApplicantUtility.HasCompletedDeclaration(applicant))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} is currently being reviewed.");

            //Reset contract and declaration as application information has been updated.
            ApplicantUtility.ClearDeclarationNoSave(applicant);

            var tenantTime = timeZoneService.GetTenantTime();
            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream s = request.File.OpenReadStream())
                {
                    s.CopyTo(ms);
                }

                CourierApplicantUpload courierApplicationUpload = new CourierApplicantUpload()
                {
                    Created = tenantTime,
                    FileName = request.File.FileName,
                    ApplicantId = applicantId,
                    DocumentId = request.DocumentId,
                    Type = request.File.ContentType,
                    Length = request.File.Length,
                    Data = ms.ToArray()
                };

                Context.CourierApplicantUploads.Add(courierApplicationUpload);

                await Context.SaveChangesAsync();

                response.Id = courierApplicationUpload.Id;
            }

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> UploadDelete(int applicantId, IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant =
                await Context.CourierApplicants.SingleOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null || !applicant.EmailVerified)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} has already been approved.");

            if (ApplicantUtility.HasCompletedDeclaration(applicant))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Application {applicantId} is currently being reviewed.");

            CourierApplicantUpload upload = await Context.CourierApplicantUploads
                .FirstOrDefaultAsync(u => u.Id == request.Id && u.ApplicantId == applicantId);

            if (upload == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Upload {request.Id} not found for applicant {applicantId}.");

            Context.CourierApplicantUploads.Remove(upload);

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        #endregion

        public async Task<BaseResponse> PostDeclaration(int applicantId, DeclarationUpdateRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant =
                await Context.CourierApplicants
                    .Include(a => a.Contract)
                    .SingleOrDefaultAsync(a => a.Id == applicantId);

            if (applicant == null || !applicant.EmailVerified)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} has already been approved.");

            if (ApplicantUtility.HasCompletedDeclaration(applicant))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} is currently being reviewed.");

            if (!ApplicantUtility.HasCompletedProfile(applicant))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} profile is incomplete.");

            if (await Context.CourierApplicantDocuments.AnyAsync(d => d.Active && d.Mandatory && d.CourierApplicantUploads.All(u => u.ApplicantId != applicant.Id)))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant {applicantId} documentation is incomplete.");

            if (!applicant.ContractId.HasValue)
            {
                var message = new MessageDto() { Message = $"You must read the contract." };
                message.Params.Add("Contract");
                response.Messages.Add(message);
                return response;
            }

            int currentContractId = await Context.CourierContracts.OrderByDescending(c => c.Id).Select(c => c.Id).FirstAsync();

            if (applicant.ContractId.Value != currentContractId)
            {
                var message = new MessageDto() { Message = $"Contract has been updated, you must read the new contract." };
                message.Params.Add("Contract");
                response.Messages.Add(message);
                return response;
            }

            if (request.Text != Environment.GetEnvironmentVariable("ApplicantDeclaration"))
            {
                var message = new MessageDto() { Message = $"'Declaration' has been updated." };
                message.Params.Add("Declaration");
                response.Messages.Add(message);
                return response;
            }

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstOrDefaultAsync();
            var tenantTime = timeZoneService.GetTenantTime();
            DateTime declarationDate = tenantTime;
            string declarationFileName = "Signature.png";
            string declarationType = request.Signature.Substring(request.Signature.IndexOf(':') + 1, request.Signature.IndexOf(';') - request.Signature.IndexOf(':') - 1);
            byte[] declarationSignature = Convert.FromBase64String(request.Signature.Substring(request.Signature.IndexOf(',') + 1));
            string declarationSignatureContentId = "declarationSignatureContentId";
            //A staff member will need to check the email with the contract as part of the approval process
            var emailResponse = await emailService.SendEmail(new EmailRequest()
            {
                To = $"{applicant.Email},{Environment.GetEnvironmentVariable("UrgentArmyEmail")}",
                Subject = "Deliver Different - Applicant Contract",
                IsBodyHtml = true,
                Message = await GenerateApplicantContractTemplate(declarationDate, request.Text, request.Name, declarationSignatureContentId, countryCode),
                Attachments = new EmailAttachmentDto[]
                {
                    new EmailAttachmentDto()
                    {
                        FileName = applicant.Contract.FileName,
                        Type = applicant.Contract.Type,
                        Data = applicant.Contract.Data
                    },
                    new EmailAttachmentDto()
                    {
                        ContentId = declarationSignatureContentId,
                        FileName = declarationFileName,
                        Type = declarationType, //always image/png
                        Data = declarationSignature
                    }
                }
            });

            if (!emailResponse.Success)
            {
                response.Messages = emailResponse.Messages;
                return response;
            }

            applicant.ContractId = currentContractId;
            applicant.DeclarationText = request.Text;
            applicant.DeclarationDate = declarationDate;
            applicant.DeclarationAgree = request.Agree;
            applicant.DeclarationName = request.Name;
            applicant.DeclarationSignatureFileName = declarationFileName;
            applicant.DeclarationSignatureType = declarationType;
            applicant.DeclarationSignatureLength = declarationSignature.LongLength;
            applicant.DeclarationSignature = declarationSignature;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, "Record has been updated by another user.");
            }

            response.Success = true;

            return response;
        }


        #region "Email Templates"

        private async Task<string> GenerateRegisteredEmailVerificationTemplate(string verificationCode, string email)
        {
            if (string.IsNullOrWhiteSpace(verificationCode) || string.IsNullOrWhiteSpace(email))
                throw new Exception("Template parameters must not be empty.");

            //Prepare email message
            string emailMessage;


            using (var reader = new StreamReader(Path.Combine(hostingEnvironment.ContentRootPath,
                       "EmailTemplates", "Applicants", "RegisteredEmailVerification.html")))
            {
                emailMessage = await reader.ReadToEndAsync();
            }

            //Replace special tags in email template
            emailMessage = emailMessage.Replace("{{basePath}}", HttpUtility.HtmlEncode(Environment.GetEnvironmentVariable("CourierPortalWebBase")));
            emailMessage = emailMessage.Replace("{{verificationLink}}", $"{Environment.GetEnvironmentVariable("CourierPortalWebBase")}account/verification/{HttpUtility.UrlEncode(email)}");
            emailMessage = emailMessage.Replace("{{verificationCode}}", HttpUtility.HtmlEncode(verificationCode));

            return emailMessage;
        }

        private async Task<string> GenerateApplicantContractTemplate(DateTime declarationDate, string declarationText, string declarationName, string declarationSignatureContentId, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(declarationText) || string.IsNullOrWhiteSpace(declarationName) || string.IsNullOrWhiteSpace(declarationSignatureContentId))
                throw new Exception("Template parameters must not be empty.");

            //Prepare email message
            string emailMessage;

            using (var reader = new StreamReader(Path.Combine(hostingEnvironment.ContentRootPath,
                           "EmailTemplates", "Applicants", "Contract.html")))
            {
                emailMessage = await reader.ReadToEndAsync();
            }

            //Replace special tags in email template
            emailMessage = emailMessage.Replace("{{basePath}}", HttpUtility.HtmlEncode(Environment.GetEnvironmentVariable("CourierPortalWebBase")));
            emailMessage = emailMessage.Replace("{{declarationText}}", HttpUtility.HtmlEncode(declarationText));
            emailMessage = emailMessage.Replace("{{declarationName}}", HttpUtility.HtmlEncode(declarationName));
            emailMessage = emailMessage.Replace("{{declarationDate}}", HttpUtility.HtmlEncode(declarationDate.ToString(countryCode == "NZ"? "dd/MM/yyyy" : "MM/dd/yyyy")));
            emailMessage = emailMessage.Replace("{{declarationSignatureContentId}}", declarationSignatureContentId);

            return emailMessage;
        }

        #endregion
    }
}
