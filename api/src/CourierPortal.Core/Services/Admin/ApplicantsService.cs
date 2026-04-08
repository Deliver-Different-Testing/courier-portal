using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using CourierPortal.Core.DTOs.Admin.Applicants;
using CourierPortal.Core.DTOs.Admin.Auth;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Contracts;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.DTOs.Admin.Openforce;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Domain.Master;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace CourierPortal.Core.Services.Admin
{
    public class AdminApplicantsService(IWebHostEnvironment hostingEnvironment, AdminTimeZoneService timeZoneService, OpenforceService openforceService, IDbContextFactory<DespatchContext> contextFactory, MasterContext masterContext, IHttpContextAccessor httpContextAccessor):AdminBaseService(contextFactory)
    {
        public async Task<ApplicantsSearchResponse> Search(SearchRequest request)
        {
            return new ApplicantsSearchResponse(request.MessageId)
            {
                Applicants = await Context.CourierApplicants
                    .Where(a => a.Email.ToLower().Contains(request.SearchText.ToLower()) || (a.FirstName + " " + a.Surname).ToLower().Contains(request.SearchText.ToLower()))
                    .Select(a => new ApplicantSearchDto()
                    {
                        Id = a.Id,
                        FirstName = a.FirstName,
                        Surname = a.Surname,
                        Phone = a.Phone,
                        Mobile = a.Mobile,
                        Email = a.Email
                    })
                    .OrderBy(a => a.FirstName + " " + a.Surname)
                    .ToListAsync(),
                Success = true
            };
        }

        public async Task<ApplicantResponse> Get(IdentifierRequest request)
        {
            ApplicantResponse response = new ApplicantResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants
                .Include(a => a.Region)
                .Include(a => a.CourierFleet)
                .SingleOrDefaultAsync(a => a.Id == request.Id);

            if (applicant is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' not found.");

            ContractDto contract = null;

            if (applicant.ContractId.HasValue)
                contract = await Context.CourierContracts
                .Select(c => new ContractDto()
                {
                    Id = c.Id,
                    Created = c.Created,
                    FileName = c.FileName,
                    Type = c.Type,
                    Length = c.Length
                })
                .SingleAsync(c => c.Id == applicant.ContractId.Value);

            IEnumerable<UploadDto> uploads = await Context.CourierApplicantUploads
                .Where(u => u.ApplicantId == applicant.Id)
                .Select(u => new UploadDto()
                {
                    Id = u.Id,
                    DocumentId = u.DocumentId,
                    FileName = u.FileName,
                    Type = u.Type,
                    Length = u.Length
                })
                .ToListAsync();

            response.Applicant = new ApplicantDto()
            {
                Id = applicant.Id,
                Created = applicant.Created,
                Location = applicant.Region.Name,
                FirstName = applicant.FirstName,
                Surname = applicant.Surname,
                DateOfBirth = applicant.DateOfBirth,
                Phone = applicant.Phone,
                Mobile = applicant.Mobile,
                Email = applicant.Email,
                EmailVerificationCode = applicant.EmailVerificationCode,
                EmailVerified = applicant.EmailVerified,
                Address = applicant.Address,
                DriversLicenceNo = applicant.DriversLicenceNo,
                VehicleRegistrationNo = applicant.VehicleRegistrationNo,
                VehicleType = applicant.VehicleType,
                GstRegistered = applicant.GstRegistered,
                TaxNo = applicant.TaxNo,
                BankRoutingNumber = applicant.BankRoutingNumber,
                BankAccountNo = applicant.BankAccountNo,
                NextOfKin = applicant.NextOfKin,
                NextOfKinRelationship = applicant.NextOfKinRelationship,
                NextOfKinPhone = applicant.NextOfKinPhone,
                NextOfKinAddress = applicant.NextOfKinAddress,
                TrainingCompleted = applicant.TrainingCompleted,
                RejectDate = applicant.RejectDate,
                RejectReason = applicant.RejectReason,
                Contract = contract,
                Declaration = new DeclarationDto()
                {
                    Text = applicant.DeclarationText,
                    Date = applicant.DeclarationDate,
                    Agree = applicant.DeclarationAgree,
                    Name = applicant.DeclarationName,
                    Signature = applicant.DeclarationSignature == null
                        ? null
                        : $"data:{applicant.DeclarationSignatureType};base64,{Convert.ToBase64String(applicant.DeclarationSignature)}"
                },
                Uploads = uploads,
                CourierId = applicant.CourierId
            };

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> Delete(ApplicantDeleteRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants
                .Include(a => a.CourierApplicantUploads)
                .FirstOrDefaultAsync(a => a.Id == request.Id);

            if (applicant is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' has already been approved.");

            if (applicant.CourierApplicantUploads.Any())
                Context.CourierApplicantUploads.RemoveRange(applicant.CourierApplicantUploads);

            Context.CourierApplicants.Remove(applicant);

            //Use existing email system for sending email
            var tenantTime = timeZoneService.GetTenantTime();
            if (request.SendEmail)
                Context.TucManualMessages.Add(new TucManualMessage()
                {
                    UcmmDate = tenantTime,
                    UcmmAttempts = 0,
                    SendToEmailAddress = applicant.Email,
                    ReplyToEmailAddress = Environment.GetEnvironmentVariable("UrgentArmyEmail"),
                    Subject ="Applicant Application Deleted",
                    UcmmMessage = await GenerateApplicantDeleteTemplate(request.Reason, request.Reapply)
                });

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Record has been updated by another user.");
            }

            response.Success = true;

            return response;
        }

        public async Task<ApplicantsSummariesByLocationResponse> GetSummariesByLocation(Guid messageId)
        {
            ApplicantsSummariesByLocationResponse response = new ApplicantsSummariesByLocationResponse(messageId);

            List<CourierApplicant> courierApplications = await Context.CourierApplicants
                .Where(a => !a.CourierId.HasValue)
                .Include(a => a.Region)
                .ToListAsync();

            IEnumerable<int> applicantIds = courierApplications.Select(a => a.Id);
            IEnumerable<int> contractIds = courierApplications.Where(a => a.ContractId.HasValue).Select(a => a.ContractId.Value);


            List<ContractDto> contracts = await Context.CourierContracts
                .Where(c => contractIds.Contains(c.Id))
                .Select(c => new ContractDto()
                {
                    Id = c.Id,
                    Created = c.Created,
                    FileName = c.FileName,
                    Type = c.Type,
                    Length = c.Length
                })
                .ToListAsync();

            var uploads = await Context.CourierApplicantUploads
                .Where(u => applicantIds.Contains(u.ApplicantId))
                .Select(u => new
                {
                    u.Id,
                    u.ApplicantId,
                    u.DocumentId,
                    u.FileName,
                    u.Type,
                    u.Length
                })
                .ToListAsync();

            response.Summaries = courierApplications
                .Select(a => a.Region.Name)
                .Distinct()
                .OrderBy(s => s)
                .Select(s => new ApplicantsSummaryByLocationDto()
                {
                    Location = s,
                    Applicants = courierApplications
                        .Where(a => a.Region.Name == s)
                        .Select(a => new ApplicantDto()
                        {
                            Id = a.Id,
                            Created = a.Created,
                            Location = a.Region.Name,
                            FirstName = a.FirstName,
                            Surname = a.Surname,
                            DateOfBirth = a.DateOfBirth,
                            Phone = a.Phone,
                            Mobile = a.Mobile,
                            Email = a.Email,
                            EmailVerificationCode = a.EmailVerificationCode,
                            EmailVerified = a.EmailVerified,
                            Address = a.Address,
                            DriversLicenceNo = a.DriversLicenceNo,
                            VehicleRegistrationNo = a.VehicleRegistrationNo,
                            VehicleType = a.VehicleType,
                            GstRegistered = a.GstRegistered,
                            TaxNo = a.TaxNo,
                            BankRoutingNumber = a.BankRoutingNumber,
                            BankAccountNo = a.BankAccountNo,
                            NextOfKin = a.NextOfKin,
                            NextOfKinRelationship = a.NextOfKinRelationship,
                            NextOfKinPhone = a.NextOfKinPhone,
                            NextOfKinAddress = a.NextOfKinAddress,
                            TrainingCompleted = a.TrainingCompleted,
                            RejectDate = a.RejectDate,
                            RejectReason = a.RejectReason,
                            Contract = a.ContractId.HasValue ? contracts.Single(c => c.Id == a.ContractId.Value) : null,
                            Declaration = new DeclarationDto()
                            {
                                Text = a.DeclarationText,
                                Date = a.DeclarationDate,
                                Agree = a.DeclarationAgree,
                                Name = a.DeclarationName,
                                Signature = a.DeclarationSignature == null ? null : $"data:{a.DeclarationSignatureType};base64,{Convert.ToBase64String(a.DeclarationSignature)}"
                            },
                            Uploads = uploads
                                .Where(u => u.ApplicantId == a.Id)
                                .Select(u => new UploadDto()
                                {
                                    Id = u.Id,
                                    DocumentId = u.DocumentId,
                                    FileName = u.FileName,
                                    Type = u.Type,
                                    Length = u.Length
                                }),
                            CourierId = a.CourierId
                        })
                });

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> PostTrainingCompleted(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants
                .SingleOrDefaultAsync(a => a.Id == request.Id);

            if (applicant is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' has already been approved.");

            applicant.TrainingCompleted = true;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Record has been updated by another user.");
            }

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> PostReject(ApplicantRejectRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants.FirstOrDefaultAsync(a => a.Id == request.Id);

            if (applicant is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' has already been approved.");

            if (request.ClearDocuments.Any())
            {
                List<int> uploadIds = await Context.CourierApplicantUploads
                    .Where(u => u.ApplicantId == request.Id && request.ClearDocuments.Contains(u.DocumentId))
                    .Select(u => u.Id)
                    .ToListAsync();

                if (uploadIds.Any())
                {
                    List<CourierApplicantUpload> uploads = uploadIds.Select(x => new CourierApplicantUpload() { Id = x }).ToList();
                    Context.CourierApplicantUploads.AttachRange(uploads);
                    Context.CourierApplicantUploads.RemoveRange(uploads);
                }
            }

            var tenantTime = timeZoneService.GetTenantTime();
            ApplicantUtility.ClearDeclarationNoSave(applicant);
            applicant.RejectDate = tenantTime;
            applicant.RejectReason = request.Reason.Trim();

            //Use existing email system for sending email
            Context.TucManualMessages.Add(new TucManualMessage()
            {
                UcmmDate = tenantTime,
                UcmmAttempts = 0,
                SendToEmailAddress = applicant.Email,
                ReplyToEmailAddress = Environment.GetEnvironmentVariable("UrgentArmyEmail"),
                Subject = "Applicant Updates Needed",
                UcmmMessage = await GenerateApplicantRejectTemplate(applicant.RejectReason)
            });

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Record has been updated by another user.");
            }

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> PostApprove(ApplicantApproveRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants.FirstOrDefaultAsync(a => a.Id == request.Id);

            if (applicant is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' not found.");

            TucCourierFleet fleet = await Context.TucCourierFleets.FirstOrDefaultAsync(f => f.UccfName.Trim().ToLower() == request.Fleet.Trim().ToLower());

            if (fleet == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Fleet '{request.Fleet}' not found.");//tucCourier has a unique index on name and surname for some reason, unsure if an old application is using this.  This should be removed if possible, but for now apply temp workaround.

            applicant.CourierTypeId = request.CourierTypeId;
            applicant.MasterCourierId = request.CourierTypeId == 3 ? request.MasterCourierId : null;
            applicant.CourierCode = request.Code?.Trim();
            applicant.CourierFleetId = fleet.UccfId;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Record has been updated by another user.");
            }

            var idRequest = new IdentifierRequest() { MessageId = request.MessageId, Id = request.Id };

            return await openforceService.IsConnectedAsync() 
                ? await SendOpenforceInvitationAsync(idRequest) 
                : await CreateCourierAsync(idRequest);
        }

        public async Task<bool> ValidateApplicantAsync(BaseResponse response, CourierApplicant applicant, OfContractorContracted ofContractorContracted = null)
        {
            if (applicant is null)
                ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant not found.");
            else if (ofContractorContracted == null && string.IsNullOrWhiteSpace(applicant.CourierType?.OpenforceActivationCode))
                ResponseUtility.AddMessageAndReturnResponse(response, $"Courier Type missing activation code.");
            else if (ofContractorContracted == null && applicant.CourierTypeId == 3 && string.IsNullOrWhiteSpace(applicant.MasterCourier?.OpenForceNumber))
                ResponseUtility.AddMessageAndReturnResponse(response, $"Master courier is missing Openforce reference.");
            else if (ofContractorContracted != null && applicant.Email.Trim().ToLower() != ofContractorContracted.email?.Trim().ToLower())
                ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid contractor email.");
            else if (string.IsNullOrWhiteSpace(applicant.CourierCode))
                ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant courier code is required.");
            else if (!applicant.CourierFleetId.HasValue)
                ResponseUtility.AddMessageAndReturnResponse(response, $"Fleet is required.");
            else if (applicant.CourierId.HasValue)
                ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant has already been approved.");
            else if (!ApplicantUtility.HasCompletedDeclaration(applicant))
                ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant has not completed registration process.");
            else if (await Context.CourierApplicants.AnyAsync(a => a.Id != applicant.Id && !a.CourierId.HasValue && a.CourierCode.Trim().ToLower() == applicant.CourierCode.Trim().ToLower())
                || await Context.TucCouriers.AnyAsync(c => c.Code.Trim().ToLower() == applicant.CourierCode.Trim().ToLower() && c.Active))
                ResponseUtility.AddMessageAndReturnResponse(response, $"Code has been assigned to another courier or applicant.");

            return !response.Messages.Any();
        }

        private async Task<BaseResponse> SendOpenforceInvitationAsync(IdentifierRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants
                .Include(a => a.CourierType)
                .Include(a => a.MasterCourier)
                .FirstOrDefaultAsync(a => a.Id == request.Id);

            if (!await ValidateApplicantAsync(response, applicant))
                return response;

            var countryCode = await Context.TblSettings.Select(s => s.CountryCode).FirstOrDefaultAsync();

            var addressLine1 = $"{applicant.AddressLine3?.Trim()} {applicant.AddressLine4?.Trim()}".Trim();
            addressLine1 = string.IsNullOrWhiteSpace(applicant.AddressLine2)
                ? addressLine1
                : $"{applicant.AddressLine2.Trim()}, {addressLine1}";

            var invitation = new OfInvitation()
            {
                activation_code = applicant.CourierType.OpenforceActivationCode,
                master_account_id = applicant.CourierTypeId == 3? applicant.MasterCourier.OpenForceNumber : null,
                first_name = applicant.FirstName,
                last_name = applicant.Surname,
                email = applicant.Email,
                mobile_phone = applicant.Mobile,
                send_notification = true,
                individual_data = new OpenforceIndividualData()
                {
                    activation_code = applicant.CourierType.OpenforceActivationCode,
                    date_of_birth = applicant.DateOfBirth.Value.ToString(countryCode == "NZ"? "dd/MM/yyyy" : "MM/dd/yyyy"),
                    tax_id = applicant.TaxNo,
                    drivers_license_number = applicant.DriversLicenceNo,
                    address_line1 = addressLine1,
                    address_city = applicant.AddressLine5,
                    address_state = applicant.AddressLine6,
                    address_zip = applicant.AddressLine7,
                    address_country = applicant.AddressLine8
                }
            };

            var result = await openforceService.SendInvitationAsync(invitation);

            response.Success = result.Success;

            if (result.Messages.Any())
                response.Messages.AddRange(result.Messages);

            return response;
        }

        public async Task<BaseResponse> ProcessOfContractorContracted(OfContractorContracted ofContractorContracted)
        {
            var idRequest = new IdentifierRequest();
            BaseResponse response = new BaseResponse(idRequest.MessageId);
            if (string.IsNullOrWhiteSpace(ofContractorContracted?.email))
                return ResponseUtility.AddMessageAndReturnResponse(response, "Applicant email is required.");

            int? id = await Context.CourierApplicants
                .Where(a => a.Email.Trim().ToLower() == ofContractorContracted.email.Trim().ToLower())
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            if (!id.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, "Applicant not found.");

            idRequest.Id = id.Value;

            return await CreateCourierAsync(idRequest, ofContractorContracted);
        }

        private async Task<BaseResponse> CreateCourierAsync(IdentifierRequest request, OfContractorContracted ofContractorContracted = null)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants
                .Include(a => a.CourierType)
                .FirstOrDefaultAsync(a => a.Id == request.Id);

            if (!await ValidateApplicantAsync(response, applicant, ofContractorContracted))
                return response;

            var settings = await Context.TblSettings
                .Select(s => new { s.DefaultCourierPercentage, s.DefaultCourierWithholdingTaxPercentage })
                .FirstOrDefaultAsync();

            var tenantTime = timeZoneService.GetTenantTime();
            applicant.Courier = new TucCourier()
            {
                CourierTypeId = applicant.CourierTypeId ?? 1,
                MasterCourierId = applicant.CourierTypeId == 3? applicant.MasterCourierId : null,
                Code = applicant.CourierCode,
                OpenForceNumber = ofContractorContracted?.id,
                UccrName = string.IsNullOrWhiteSpace(ofContractorContracted?.first_name)? applicant.FirstName : ofContractorContracted.first_name.Trim(),
                UccrSurname = string.IsNullOrWhiteSpace(ofContractorContracted?.last_name) ? applicant.Surname : ofContractorContracted?.last_name.Trim(),
                UccrDob = ofContractorContracted?.date_of_birth == null? applicant.DateOfBirth :ofContractorContracted.date_of_birth.Value,
                UccrEmail = string.IsNullOrWhiteSpace(ofContractorContracted?.email)? applicant.Email : ofContractorContracted.email.Trim(),
                UccrPassword = applicant.Password,
                PasswordResetRequired = false,
                UccrTel = string.IsNullOrWhiteSpace(ofContractorContracted?.phones?.FirstOrDefault(p => p.type == "Phone")?.number) ? applicant.Phone?.Replace("+64", "0")?.Replace(" ", string.Empty) : ofContractorContracted.phones.First(p => p.type == "Phone").number.Trim(),
                PersonalMobile = string.IsNullOrWhiteSpace(ofContractorContracted?.phones?.FirstOrDefault(p => p.type == "Mobile")?.number) ? applicant.Mobile?.Replace("+64", "0")?.Replace(" ", string.Empty) : ofContractorContracted.phones.First(p => p.type == "Mobile").number.Trim(),
                UccrAddress = applicant.Address,
                AddressLine1 = applicant.AddressLine1,
                AddressLine2 = applicant.AddressLine2,
                AddressLine3 = applicant.AddressLine3,
                AddressLine4 = applicant.AddressLine4,
                AddressLine5 = applicant.AddressLine5,
                AddressLine6 = applicant.AddressLine6,
                AddressLine7 = applicant.AddressLine7,
                AddressLine8 = applicant.AddressLine8,
                UccrDlno = string.IsNullOrWhiteSpace(ofContractorContracted?.drivers_license_number) ? applicant.DriversLicenceNo : ofContractorContracted.drivers_license_number.Trim(),
                UccrReg = applicant.VehicleRegistrationNo,
                UccrVehicle = applicant.VehicleType,
                BankRoutingNumber = applicant.BankRoutingNumber,
                UccrBankAccountNo = applicant.BankAccountNo,
                UccrGst = applicant.TaxNo,
                WithholdingTaxPercentage = applicant.GstRegistered 
                    ? 0m 
                    : (settings?.DefaultCourierWithholdingTaxPercentage ?? 0m),
                UccrPercentage = settings?.DefaultCourierPercentage ?? 0,
                UccrKinName = applicant.NextOfKin,
                UccrKinRelationship = applicant.NextOfKinRelationship,
                UccrKinTel = applicant.NextOfKinPhone,
                UccrKinAdd = applicant.NextOfKinAddress,
                RegionId = applicant.RegionId,
                SiteId = applicant.SiteId,
                CourierFleetId = applicant.CourierFleetId,
                UccrChannelId = 6, //Defaulted to Other
                Podreqd = true,
                Active = true,
                UccrStartDate = tenantTime,
                UccrInternal = false,
                UccrLate = 1,
                UccrInsuranceId = 16,
                UccrContractDate = applicant.DeclarationDate,
                UccrWebEnabled = true,
                UccrShowClientPh = true,
                DisplayWeb = true,
                Created = tenantTime,
                CreatedBy = "Courier Manager",
                LastModified = tenantTime,
                LastModifiedBy = "Courier Manager",
                JobPaperPrintOut = false,
                AutoDespatch = true,
                AfterHoursWeb = false,
                Gender = false, //Currently not recorded, default to false
                MaxPayload = 1000,
                DeviceTypeId = 5, //Currently not recorded, default to iPhone
                HeavyTransportEndorcement = false,
                VodafoneNetwork = false
            };

            //Use existing email system for sending email
            Context.TucManualMessages.Add(new TucManualMessage()
            {
                UcmmDate = tenantTime,
                UcmmAttempts = 0,
                SendToEmailAddress = applicant.Email,
                ReplyToEmailAddress = Environment.GetEnvironmentVariable("UrgentArmyEmail"),
                Subject = "Applicant Approval",
                UcmmMessage = await GenerateApplicantApprovalTemplate(applicant.Courier.Code, applicant.FirstName, applicant.Surname)
            });

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Record has been updated by another user.");
            }

            try
            {
                var result = SaltHashNewPassword(applicant.Password);
                var currentTenantId = httpContextAccessor.HttpContext.User.FindFirst("CurrentTenantID").Value;

                var courierUser = new User()
                {
                    Email = string.IsNullOrWhiteSpace(ofContractorContracted?.email)? applicant.Email : ofContractorContracted.email.Trim(),
                    Password = result.Hashed,
                    Salt = result.Salt,
                    CurrentTenantId = int.Parse(currentTenantId),
                    IsCourier = true,
                    IsLegacyHash = false
                };
                masterContext.Users.Add(courierUser);
                await masterContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error adding Courier User to Master");
                throw;
            }
            response.Success = true;
            return response;
        }
        
        public static SaltHashed SaltHashNewPassword(string password)
        {
            var random = new Random();
            var salt = random.Next(10000, 99999);
            var salted = salt.ToString();
            var result = new SaltHashed
            {
                Salt = salted,
                Hashed = HashPassword(password, salted)
            };
            return result;
        }

        public static string HashPassword(string password, string salt)
        {
            // Using the recommended constructor with explicit iteration count parameter (default is 1000)
            var k2 = new Rfc2898DeriveBytes(
                password, 
                System.Text.Encoding.UTF8.GetBytes(salt + salt),
                10000,
                HashAlgorithmName.SHA256);
        
            var hashBytes = k2.GetBytes(64); // 64 bytes = 512 bits
            var result = BitConverter.ToString(hashBytes).Replace("-", "");
            return result;
        }

        public async Task<BaseResponse> CreateDocument(ApplicantDocumentCreateRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            byte[] file = request.File == null ? null : Convert.FromBase64String(request.File.DataUrl.Substring(request.File.DataUrl.IndexOf(',') + 1));

            if (file != null && file.Length < 1)
                return ResponseUtility.AddMessageAndReturnResponse(response, "'File' must not be empty");

            if (file != null && file.Length > 10000000)
                return ResponseUtility.AddMessageAndReturnResponse(response, "'File' size exceeds 10MB");

            var tenantTime = timeZoneService.GetTenantTime();
            Context.Add(new CourierApplicantDocument()
            {
                Created = tenantTime,
                Name = request.Name,
                Instructions = request.Instructions,
                Mandatory = request.Mandatory,
                FileName = request.File?.FileName.Trim(),
                Type = request.File?.DataUrl.Substring(request.File.DataUrl.IndexOf(':') + 1, request.File.DataUrl.IndexOf(';') - request.File.DataUrl.IndexOf(':') - 1),
                Length = file?.LongLength,
                Data = file,
                Active = request.Active,
            });

            await Context.SaveChangesAsync();

            response.Success = true;

            return response;
        }

        public async Task<ApplicantDocumentsResponse> GetDocuments(Guid messageId)
        {
            return new ApplicantDocumentsResponse(messageId)
            {
                Documents = await Context.CourierApplicantDocuments
                    .Select(d => new ApplicantDocumentDto()
                    {
                        Id = d.Id,
                        Created = d.Created,
                        Name = d.Name,
                        Instructions = d.Instructions,
                        Mandatory = d.Mandatory,
                        Active = d.Active,
                        FileName = d.FileName,
                        Type = d.Type,
                        Length = d.Length
                    })
                    .ToListAsync(),
                Success = true
            };
        }

        public async Task<ApplicantDocumentResponse> GetDocument(IdentifierRequest request)
        {
            ApplicantDocumentResponse response = new ApplicantDocumentResponse(request.MessageId);

            ApplicantDocumentDto document = await Context.CourierApplicantDocuments
                .Select(d => new ApplicantDocumentDto()
                {
                    Id = d.Id,
                    Created = d.Created,
                    Name = d.Name,
                    Instructions = d.Instructions,
                    Mandatory = d.Mandatory,
                    Active = d.Active,
                    FileName = d.FileName,
                    Type = d.Type,
                    Length = d.Length
                })
                .SingleOrDefaultAsync(d => d.Id == request.Id);

            if (document is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Document '{request.Id}' not found.");

            response.Document = document;

            response.Success = true;

            return response;
        }

        public async Task<FileResponse> GetDocumentFile(IdentifierRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierApplicantDocument document = await Context.CourierApplicantDocuments.SingleOrDefaultAsync(d => d.Id == request.Id);

            if (document is null || string.IsNullOrWhiteSpace(document.FileName) || string.IsNullOrWhiteSpace(document.Type) || document.Data is null || document.Data.Length <= 0)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Document '{request.Id}' not found.");

            response.FileName = document.FileName;
            response.FileType = document.Type;
            response.File = document.Data;

            response.Success = true;

            return response;
        }

        public async Task<ApplicantDocumentsResponse> GetDocumentsActive(Guid messageId)
        {
            return new ApplicantDocumentsResponse(messageId)
            {
                Documents = await Context.CourierApplicantDocuments
                    .Where(d => d.Active)
                    .Select(d => new ApplicantDocumentDto()
                    {
                        Id = d.Id,
                        Created = d.Created,
                        Name = d.Name,
                        Instructions = d.Instructions,
                        Mandatory = d.Mandatory,
                        Active = d.Active,
                        FileName = d.FileName,
                        Type = d.Type,
                        Length = d.Length
                    })
                    .ToListAsync(),
                Success = true
            };
        }

        public async Task<ApplicantDocumentsResponse> GetDocumentsByApplicant(IdentifierRequest request)
        {
            ApplicantDocumentsResponse response = new ApplicantDocumentsResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants.SingleOrDefaultAsync(a => a.Id == request.Id);

            if (applicant is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' not found.");

            response.Documents = await Context.CourierApplicantDocuments
                .Where(d => d.CourierApplicantUploads.Any(u => u.ApplicantId == applicant.Id && u.DocumentId == d.Id)
                            || (d.Active && !applicant.DeclarationDate.HasValue))
                .Select(d => new ApplicantDocumentDto()
                {
                    Id = d.Id,
                    Created = d.Created,
                    Name = d.Name,
                    Instructions = d.Instructions,
                    Mandatory = d.Mandatory,
                    Active = d.Active,
                    FileName = d.FileName,
                    Type = d.Type,
                    Length = d.Length
                })
                .ToListAsync();

            response.Success = true;

            return response;
        }

        public async Task<ApplicantDocumentResponse> PostDocument(ApplicantDocumentUpdateRequest request)
        {
            ApplicantDocumentResponse response = new ApplicantDocumentResponse(request.MessageId);

            CourierApplicantDocument document = await Context.CourierApplicantDocuments
                .SingleOrDefaultAsync(d => d.Id == request.Id);

            if (document is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Document '{request.Id}' not found.");

            document.Instructions = string.IsNullOrWhiteSpace(request.Instructions) ? null : request.Instructions.Trim();
            document.Mandatory = request.Mandatory;
            document.Active = request.Active;

            await Context.SaveChangesAsync();

            response.Document = new ApplicantDocumentDto()
            {
                Id = document.Id,
                Created = document.Created,
                Name = document.Name,
                Instructions = document.Instructions,
                Mandatory = document.Mandatory,
                Active = document.Active,
                FileName = document.FileName,
                Type = document.Type,
                Length = document.Length
            };

            response.Success = true;

            return response;
        }

        public async Task<FileResponse> GetUploadFile(IdentifierRequest request)
        {
            FileResponse response = new FileResponse(request.MessageId);

            CourierApplicantUpload upload = await Context.CourierApplicantUploads.SingleOrDefaultAsync(u => u.Id == request.Id);

            if (upload is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Upload '{request.Id}' not found.");

            response.FileName = upload.FileName;
            response.FileType = upload.Type;
            response.File = upload.Data;

            response.Success = true;

            return response;
        }

        public async Task<BaseResponse> PostPasswordReset(ApplicantPasswordResetRequest request)
        {
            BaseResponse response = new BaseResponse(request.MessageId);

            CourierApplicant applicant = await Context.CourierApplicants.FirstOrDefaultAsync(a => a.Id == request.Id);

            if (applicant is null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' not found.");

            if (applicant.CourierId.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Applicant '{request.Id}' has already been approved.");

            //Trim just incase internal user accidentally puts in spaces
            applicant.Password = request.NewPassword.Trim();

            //Use existing email system for sending email
            var tenantTime = timeZoneService.GetTenantTime();
            Context.TucManualMessages.Add(new TucManualMessage()
            {
                UcmmDate = tenantTime,
                UcmmAttempts = 0,
                SendToEmailAddress = applicant.Email,
                ReplyToEmailAddress = Environment.GetEnvironmentVariable("UrgentArmyEmail"),
                Subject = "Urgent Couriers - Applicant Password Reset",
                UcmmMessage = await GenerateApplicantPasswordResetTemplate(applicant.Password)
            });

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Record has been updated by another user.");
            }

            response.Success = true;

            return response;
        }

        #region "Email Templates"

        private async Task<string> GenerateApplicantDeleteTemplate(string reason, bool reapply)
        {
            //Prepare email message
            string emailMessage;

            using (StreamReader reader = new StreamReader(Path.Combine(hostingEnvironment.ContentRootPath,
                       "EmailTemplates", "Applicants", "Delete.html")))
            {
                emailMessage = await reader.ReadToEndAsync();
            }

            string reasonValue = reason.Trim();

            if (reapply)
                reasonValue += Environment.NewLine + Environment.NewLine + "You are welcome to re-apply if you are still interested in joining with Urgent Army.";

            //Replace special tags in email template
            emailMessage = emailMessage.Replace("{{reason}}", HttpUtility.HtmlEncode(reasonValue));

            return emailMessage;
        }

        private async Task<string> GenerateApplicantRejectTemplate(string reason)
        {
            //Prepare email message
            string emailMessage;
            
            using (StreamReader reader = new StreamReader(Path.Combine(hostingEnvironment.ContentRootPath,
                       "EmailTemplates", "Applicants", "Reject.html")))
            {
                emailMessage = await reader.ReadToEndAsync();
            }

            //Replace special tags in email template
            emailMessage = emailMessage.Replace("{{reason}}", HttpUtility.HtmlEncode(reason));

            return emailMessage;
        }

        private async Task<string> GenerateApplicantApprovalTemplate(string courierCode, string firstName, string surname)
        {
            //Prepare email message
            string emailMessage;
            
            using (StreamReader reader = new StreamReader(Path.Combine(hostingEnvironment.ContentRootPath,
                       "EmailTemplates", "Applicants", "Approved.html")))
            {
                emailMessage = await reader.ReadToEndAsync();
            }

            //Replace special tags in email template
            emailMessage = emailMessage.Replace("{{courierCode}}", HttpUtility.HtmlEncode(courierCode));
            emailMessage = emailMessage.Replace("{{firstName}}", HttpUtility.HtmlEncode(firstName));
            emailMessage = emailMessage.Replace("{{surname}}", HttpUtility.HtmlEncode(surname));

            return emailMessage;
        }

        private async Task<string> GenerateApplicantPasswordResetTemplate(string password)
        {
            //Prepare email message
            string emailMessage;

            using (StreamReader reader = new StreamReader(Path.Combine(hostingEnvironment.ContentRootPath,
                       "EmailTemplates", "Applicants", "PasswordReset.html")))
            {
                emailMessage = await reader.ReadToEndAsync();
            }

            //Replace special tags in email template
            emailMessage = emailMessage.Replace("{{password}}", HttpUtility.HtmlEncode(password));

            return emailMessage;
        }

        #endregion

    }
}
