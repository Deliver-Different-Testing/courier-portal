using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CourierPortal.Core.DTOs.Portal.Common;
using CourierPortal.Core.DTOs.Portal.Reports;
using CourierPortal.Core.Utilities;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalReportService(PortalTimeZoneService timeZoneService, IDbContextFactory<DespatchContext> contextFactory) : PortalBaseService(contextFactory)
    {
        private static readonly ReportDto[] _reports 
            = {
                new ReportDto()
                    {
                        Id = 1,
                        Name = "Jobs by Courier by Day - Detail",
                        FilterDate = "Mandatory",
                        FilterYearMonth = null
                    }
            };

        public async Task<BaseResponse> GetSettings(Guid messageId, int courierId)
        {
            BaseResponse response = new BaseResponse(messageId);

            TucCourier courier = await Context.TucCouriers
                .SingleOrDefaultAsync(c=> c.UccrId == courierId);

            if (courier is null)
            {
                response.Messages.Add(new MessageDto() { Message = $"Courier {courierId} not found." });
                return response;
            }

            //var user = await Context.TblUsers.FirstOrDefaultAsync(u => u.CourierId == courierId);
            //var userGroupId = user?.UserGroupId ?? 28; //Default to UserGroupId 28 Urgent Army is no user

            response.Data = new ReportSettingsDto
            {
                Reports = _reports
                //Reports = await (from r in Context.TblReports
                //                 join ug in Context.TblUserGroupReports on r.ReportId equals ug.ReportId
                //                 where r.Active && ug.UserGroupId == userGroupId
                //                 orderby r.Name, r.ReportId
                //                 select new ReportDto()
                //                 {
                //                     Id = r.ReportId,
                //                     Name = $"{r.Name}{(string.IsNullOrWhiteSpace(r.Style) || r.Style == "Default" ? string.Empty : " - " + r.Style)}",
                //                     FilterDate = r.FilterDate,
                //                     FilterYearMonth = r.FilterYearMonth
                //                 }).ToListAsync()
                //Periods = !courier.UccrInternal
                //    ? null
                //    : await Context.TblBuyerTaxInvoiceControls
                //        .OrderByDescending(x => x.Period)
                //        .Select(x => x.Period)
                //        .Where(x => x.CompareTo("202104") > 0)
                //        .ToListAsync()
            };

            response.Success = true;

            return response;
        }

        //public async Task<FileResponse> GetReport(Guid messageId, int courierId, ReportParametersDto p)
        //{
        //    FileResponse response = new FileResponse(messageId);

        //    if (p.StartDate?.Kind == DateTimeKind.Utc)
        //        p.StartDate = timeZoneService.ConvertUtcToTenantTime(p.StartDate.Value);

        //    if (p.EndDate?.Kind == DateTimeKind.Utc)
        //        p.EndDate = timeZoneService.ConvertUtcToTenantTime(p.EndDate.Value);

        //    var user = await Context.TblUsers.FirstOrDefaultAsync(u => u.CourierId == courierId);
        //    var userGroupId = user?.UserGroupId ?? 28; //Default to UserGroupId 28 Urgent Army is no user

        //    var rpt = await Context.TblReports
        //        .FirstOrDefaultAsync(r => r.ReportId == p.ReportId && r.Active && r.TblUserGroupReports.Any(ug => ug.UserGroupId == userGroupId));

        //    if (rpt == null)
        //        return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid or inactive report.");

        //    string reportParams = rpt.ReportName;
        //    reportParams += $"&rs:Format={WebUtility.UrlEncode(p.Format.ToString())}";
        //    reportParams += $"&CourierID={courierId}";

        //    if (rpt.FilterYearMonth == "Mandatory" && !p.YearMonth.HasValue)
        //        return ResponseUtility.AddMessageAndReturnResponse(response, $"YearMonth is required.");

        //    if (rpt.FilterDate == "Mandatory" && (!p.StartDate.HasValue || !p.EndDate.HasValue || p.StartDate.Value.Date > p.EndDate.Value.Date))
        //        return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid start and end date.");

        //    if (p.YearMonth.HasValue && (rpt.FilterYearMonth == "Mandatory" || rpt.FilterYearMonth == "Optional"))
        //        reportParams += $"&LongPeriod={p.YearMonth}";

        //    if (p.StartDate.HasValue && p.EndDate.HasValue && (rpt.FilterDate == "Mandatory" || rpt.FilterDate == "Optional"))
        //        reportParams += $"&StartDate={WebUtility.UrlEncode(p.StartDate.Value.Date.ToString("yyyy-MM-dd"))}&EndDate={WebUtility.UrlEncode(p.EndDate.Value.Date.ToString("yyyy-MM-dd"))}";

        //    //switch (p.ReportId)
        //    //{
        //    //    case 1:
        //    //        if (!p.YearMonth.HasValue)
        //    //            return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid YearMonth.");

        //    //        fileName = "BuyerCreatedTaxInvoice.pdf";
        //    //        fileType = "application/pdf";
        //    //        reportPath = $"BuyerCreatedTaxInvoice2&rs:Format=PDF&CourierID={courierId}&LongPeriod={p.YearMonth}";
        //    //        break;
        //    //    case 2:
        //    //        fileName = "JobDetailsByCourier.pdf";
        //    //        fileType = "application/pdf";
        //    //        reportPath = $"JobDetailsByCourier&rs:Format=PDF&CourierID={courierId}&StartDate={WebUtility.UrlEncode(p.StartDate.Value.Date.ToString("yyyy-MM-dd"))}&EndDate={WebUtility.UrlEncode(p.EndDate.Value.Date.ToString("yyyy-MM-dd"))}";
        //    //        break;
        //    //    default: 
        //    //        return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid ReportId.");
        //    //}

        //    switch (p.Format)
        //    {
        //        case ReportParametersDto.ReportFormat.CSV:
        //            response.FileType = "text/csv";
        //            break;
        //        case ReportParametersDto.ReportFormat.EXCEL:
        //            response.FileType = "application/vnd.ms-excel";
        //            break;
        //        default:
        //            response.FileType = "application/pdf";
        //            break;
        //    }

        //    response.FileName = rpt.Name;

        //    var reportBase = Environment.GetEnvironmentVariable("ReportBase");
        //    response.File = await GetReportBytes($"{reportBase}{reportParams}");

        //    response.Success = true;

        //    return response;
        //}

        //private async Task<byte[]> GetReportBytes(string path)
        //{
        //    using (var client = httpClientFactory.CreateClient("ReportServer"))
        //    {
        //        using (var reportStream = await client.GetStreamAsync(path))
        //        {
        //            using (var memoryStream = new MemoryStream())
        //            {
        //                await reportStream.CopyToAsync(memoryStream);
        //                return memoryStream.ToArray();
        //            }
        //        }
        //    }
        //}



        public async Task<FileResponse> GetReportAsync(Guid messageId, int courierId, ReportParametersDto p)
        {
            FileResponse response = new FileResponse(messageId);

            if (p.StartDate?.Kind == DateTimeKind.Utc)
                p.StartDate = timeZoneService.ConvertUtcToTenantTime(p.StartDate.Value);

            if (p.EndDate?.Kind == DateTimeKind.Utc)
                p.EndDate = timeZoneService.ConvertUtcToTenantTime(p.EndDate.Value);

            var rpt = _reports.FirstOrDefault(r => r.Id == p.ReportId);

            if (rpt == null)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Report selection is required.");

            string reportName;

            switch (rpt.Id)
            {
                case 1:
                    reportName = "JobsByCourierByDayDetail";
                    break;
                default:
                    return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid report.");
            }

            string reportParams = $"{Environment.GetEnvironmentVariable("ReportBase")}{WebUtility.UrlEncode(reportName)}";
            reportParams +=   $"&rs:Format={WebUtility.UrlEncode(string.IsNullOrWhiteSpace(p.Format)? "PDF" : p.Format.Trim())}";
            reportParams += $"&CourierID={courierId}";

            if (rpt.FilterYearMonth == "Mandatory" && !p.YearMonth.HasValue)
                return ResponseUtility.AddMessageAndReturnResponse(response, $"YearMonth is required.");

            if (rpt.FilterDate == "Mandatory" && (!p.StartDate.HasValue || !p.EndDate.HasValue || p.StartDate.Value.Date > p.EndDate.Value.Date))
                return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid start and end date.");

            if (p.YearMonth.HasValue && (rpt.FilterYearMonth == "Mandatory" || rpt.FilterYearMonth == "Optional"))
                reportParams += $"&LongPeriod={p.YearMonth}";

            if (p.StartDate.HasValue && p.EndDate.HasValue && (rpt.FilterDate == "Mandatory" || rpt.FilterDate == "Optional"))
                reportParams += $"&StartDate={WebUtility.UrlEncode(p.StartDate.Value.Date.ToString("yyyy-MM-dd"))}&EndDate={WebUtility.UrlEncode(p.EndDate.Value.Date.ToString("yyyy-MM-dd"))}";

            //switch (p.ReportId)
            //{
            //    case 1:
            //        if (!p.YearMonth.HasValue)
            //            return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid YearMonth.");

            //        fileName = "BuyerCreatedTaxInvoice.pdf";
            //        fileType = "application/pdf";
            //        reportPath = $"BuyerCreatedTaxInvoice2&rs:Format=PDF&CourierID={courierId}&LongPeriod={p.YearMonth}";
            //        break;
            //    case 2:
            //        fileName = "JobDetailsByCourier.pdf";
            //        fileType = "application/pdf";
            //        reportPath = $"JobDetailsByCourier&rs:Format=PDF&CourierID={courierId}&StartDate={WebUtility.UrlEncode(p.StartDate.Value.Date.ToString("yyyy-MM-dd"))}&EndDate={WebUtility.UrlEncode(p.EndDate.Value.Date.ToString("yyyy-MM-dd"))}";
            //        break;
            //    default: 
            //        return ResponseUtility.AddMessageAndReturnResponse(response, $"Invalid ReportId.");
            //}

            switch (p.Format?.ToUpper())
            {
                case "CSV":
                    response.FileType = "text/csv";
                    break;
                case "EXCEL":
                    response.FileType = "application/vnd.ms-excel";
                    break;
                default:
                    response.FileType = "application/pdf";
                    break;
            }

            response.FileName = rpt.Name;

            response.File = await GetReportBytesAsync(reportParams);

            response.Success = true;

            return response;
        }

        static List<X509Certificate2> LoadCertificatesFromPem(string pemString)
        {
            var certificates = new List<X509Certificate2>();
            try
            {
                // Normalize line endings
                pemString = pemString.Replace("\r\n", "\n");

                const string BEGIN_CERT = "-----BEGIN CERTIFICATE-----";
                const string END_CERT = "-----END CERTIFICATE-----";

                int currentIndex = 0;
                while (true)
                {
                    var certStart = pemString.IndexOf(BEGIN_CERT, currentIndex);
                    if (certStart < 0) break; // No more certificates

                    var certEnd = pemString.IndexOf(END_CERT, certStart);
                    if (certEnd < 0) break; // Malformed PEM

                    // Get content after BEGIN marker and before END marker
                    certStart += BEGIN_CERT.Length;
                    var certContent = pemString.Substring(certStart, certEnd - certStart)
                        .Replace("\n", "")
                        .Replace(" ", "")
                        .Trim();

                    var certBytes = Convert.FromBase64String(certContent);
                    var cert = new X509Certificate2(certBytes);
                    certificates.Add(cert);

                    Log.Information("Loaded certificate: Subject={subject}, Issuer={issuer}",
                        cert.Subject, cert.Issuer);

                    currentIndex = certEnd + END_CERT.Length;
                }

                if (certificates.Count == 0)
                {
                    throw new InvalidOperationException("No valid certificates found in PEM data");
                }

                Log.Information("Successfully loaded {count} certificates", certificates.Count);
                return certificates;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load certificates from PEM. Found {count} valid certificates before error",
                    certificates.Count);
                throw;
            }
        }

        private async Task<byte[]> GetReportBytesAsync(string path)
        {
            try
            {
                var handler = new HttpClientHandler();


                handler.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("ReportUsername"), Environment.GetEnvironmentVariable("ReportPassword"), Environment.GetEnvironmentVariable("ReportDomain"));
                // Enable NTLM authentication
                handler.UseDefaultCredentials = false;
                handler.PreAuthenticate = true;
                var certPem = Environment.GetEnvironmentVariable("SSL_CERTIFICATE");
                if (string.IsNullOrEmpty(certPem))
                {
                    Log.Warning("SSL_CERTIFICATE environment variable is not set");
                    // Fallback to bypass for staging only
                    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                }
                else
                {
                    var certificates = LoadCertificatesFromPem(certPem);
                    Log.Information("Loaded {count} certificates from PEM file", certificates.Count);
                    // Add all certificates to the handler's root CA store
                    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                    {
                        // Create a new chain with our certificates
                        using (var chainBuilder = new X509Chain())
                        {
                            chainBuilder.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                            chainBuilder.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                            // Add our custom certificates to the chain
                            foreach (var customCert in certificates)
                            {
                                chainBuilder.ChainPolicy.ExtraStore.Add(customCert);
                            }
                            bool isValid = chainBuilder.Build((X509Certificate2)cert);
                            if (!isValid)
                            {
                                Log.Warning("Certificate chain validation failed: {errors}",
                      string.Join(", ", chainBuilder.ChainElements.Cast<X509ChainElement>()
                        .SelectMany(ce => ce.ChainElementStatus)
                        .Select(s => s.StatusInformation)));
                            }
                            return isValid;
                        }
                    };
                    Log.Information("SSL certificate loaded successfully from environment variable");
                }

                Log.Information($"Full URL being requested: {path}");
                using (var httpClient = new HttpClient(handler))
                {
                    return await httpClient.GetByteArrayAsync(path);
                }

                //var httpClient = new HttpClient(handler);
                //    // Make the request with the manually constructed URL
                //    //var uri = new Uri(fullUrl);
                //Log.Information($"Full URL being requested: {path}");
                //return new FileStreamResult(await httpClient.GetStreamAsync(path), "application/pdf");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving report: {message}", ex.Message);
                throw new ApplicationException("Error retrieving report");
            }
        }

    }
}
