using CourierPortal.Api;
using CourierPortal.Core;
using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Domain.Master;

// Admin DTOs
using CourierPortal.Core.DTOs.Admin.Applicants;
using CourierPortal.Core.DTOs.Admin.Auth;
using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.DTOs.Admin.Contracts;
using CourierPortal.Core.DTOs.Admin.Infringements;
using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.DTOs.Admin.Messages;
using CourierPortal.Core.DTOs.Admin.Schedules;

// Portal DTOs
using CourierPortal.Core.DTOs.Portal.Applications;
using CourierPortal.Core.DTOs.Portal.Auth;
using CourierPortal.Core.DTOs.Portal.Couriers;
using CourierPortal.Core.DTOs.Portal.Invoices;
using CourierPortal.Core.DTOs.Portal.Runs;

// Admin Services
using CourierPortal.Core.Services.Admin;

// Portal Services
using CourierPortal.Core.Services.Portal;

// Infrastructure
using CourierPortal.Infrastructure.Models;
using CourierPortal.Infrastructure.Repositories;
using CourierPortal.Infrastructure.Services;

using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// ── Health checks ──
builder.Services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>("sql_server_health_check");
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// ── Data Protection ──
if (builder.Environment.IsDevelopment())
{
    var keyDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DeliverDifferent", "DataProtection-Keys");

    if (!Directory.Exists(keyDirectory))
        Directory.CreateDirectory(keyDirectory);

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
        .SetApplicationName("DeliverDifferent");
}
else
{
    builder.Services.AddDataProtection()
        .PersistKeysToAWSSystemsManager("/Hub/DataProtection")
        .SetApplicationName("DeliverDifferent");
}

// ── Logging ──
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

// ── MVC + HTTP ──
builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);

// ── Connection Strings ──
builder.Services.AddSingleton<IConnectionStringManager, ConnectionStringManager>();

var masterConnectionString = Environment.GetEnvironmentVariable("MasterSQLConnection") ?? "";
if (string.IsNullOrEmpty(masterConnectionString))
    throw new InvalidOperationException("Missing env var 'MasterSQLConnection'.");

builder.Services.AddHealthChecks().AddSqlServer(masterConnectionString);
builder.Services.AddDbContext<MasterContext>(x =>
{
    x.UseSqlServer(masterConnectionString);
#if DEBUG
    x.UseLoggerFactory(LoggerFactory.Create(c => c.AddDebug()));
#endif
});

// ── DespatchContext (multi-tenant) ──
builder.Services.AddDbContextFactory<DespatchContext>(options =>
    options.UseSqlServer(
        "Server=(localdb)\\mssqllocaldb;Database=dummy;Trusted_Connection=True;"
    ), ServiceLifetime.Transient);
builder.Services.AddScoped<IDbContextFactory<DespatchContext>, DynamicDespatchDbContextFactory>();

// ── Domain / Cookie / Session ──
var domain = Environment.GetEnvironmentVariable("Domain") ?? "";
if (string.IsNullOrEmpty(domain))
    throw new InvalidOperationException("Missing env var 'Domain'.");

var redisConfig = Environment.GetEnvironmentVariable("RedisConfig");
if (string.IsNullOrEmpty(redisConfig))
    throw new InvalidOperationException("Missing env var 'RedisConfig'.");

var redisConfigurationOptions = ConfigurationOptions.Parse(redisConfig);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConfigurationOptions));
builder.Services.AddStackExchangeRedisCache(cfg =>
    cfg.ConfigurationOptions = redisConfigurationOptions);

builder.Services.AddRazorPages();

// ── Authentication ──
builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application", options =>
    {
        options.Cookie.Name = ".AspNet.SharedCookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Forbidden/";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.HttpContext.Response.Redirect(
                    Environment.GetEnvironmentVariable("PublicPath") ?? "");
                return Task.CompletedTask;
            }
        };
        options.Cookie.HttpOnly = true;
        options.Cookie.Domain = domain;
    })
    .AddJwtBearer("JwtBearer", config =>
    {
        config.RequireHttpsMetadata = false;
        config.SaveToken = true;
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTSecretKey") ?? "")),
            ValidateIssuer = true,
            ValidIssuer = Environment.GetEnvironmentVariable("Issuer"),
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("Audience")
        };
    });

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "hub_session";
    options.IdleTimeout = TimeSpan.FromMinutes(60 * 24);
});

// ── Authorization Policies (Portal) ──
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Courier", p => p.RequireClaim("AccountType", AccountType.Courier.ToString()))
    .AddPolicy("Applicant", p => p.RequireClaim("AccountType", AccountType.Applicant.ToString()));

// ── FluentValidation ──
builder.Services.AddFluentValidationAutoValidation();

// ═══════════════════════════════════════════════════════════
// ── Admin Services (from CourierManager) ──
// ═══════════════════════════════════════════════════════════
builder.Services.AddSingleton<AdminAuthService>();
builder.Services.AddScoped<AdminTimeZoneService>();
builder.Services.AddScoped<AdminApplicantsService>();
builder.Services.AddScoped<AdminContractsService>();
builder.Services.AddScoped<AdminCourierService>();
builder.Services.AddScoped<InfringementsService>();
builder.Services.AddScoped<InvoiceLineJobRepository>();
builder.Services.AddScoped<AdminInvoiceService>();
builder.Services.AddScoped<InvoiceBatchService>();
builder.Services.AddScoped<AdminLocationService>();
builder.Services.AddScoped<AdminVehicleService>();
builder.Services.AddScoped<FleetService>();
builder.Services.AddScoped<AdminScheduleService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<AdminReportsService>();
builder.Services.AddScoped<OpenforceService>();
builder.Services.AddScoped<DeductionService>();
builder.Services.AddScoped<SettingsService>();

// ── Admin Validators ──
builder.Services.AddTransient<IValidator<AddRemoveInvoiceBatchRequest>, CourierPortal.Core.Validators.Admin.AddRemoveInvoiceBatchRequestValidator>();
builder.Services.AddTransient<IValidator<ApplicantDocumentCreateRequest>, CourierPortal.Core.Validators.Admin.ApplicantDocumentCreateRequestValidator>();
builder.Services.AddTransient<IValidator<ApplicantDeleteRequest>, CourierPortal.Core.Validators.Admin.ApplicantDeleteRequestValidator>();
builder.Services.AddTransient<IValidator<ApplicantRejectRequest>, CourierPortal.Core.Validators.Admin.ApplicantRejectRequestValidator>();
builder.Services.AddTransient<IValidator<ApplicantApproveRequest>, CourierPortal.Core.Validators.Admin.ApplicantApproveRequestValidator>();
builder.Services.AddTransient<IValidator<ApplicantPasswordResetRequest>, CourierPortal.Core.Validators.Admin.ApplicantPasswordResetRequestValidator>();
builder.Services.AddTransient<IValidator<ContractCreateRequest>, CourierPortal.Core.Validators.Admin.ContractCreateRequestValidator>();
builder.Services.AddTransient<IValidator<CourierPortalAccessValidationRequest>, CourierPortal.Core.Validators.Admin.CourierPortalAccessValidationRequestValidator>();
builder.Services.AddTransient<IValidator<CouriersByResponseStatusRequest>, CourierPortal.Core.Validators.Admin.CouriersByResponseStatusRequestValidator>();
builder.Services.AddTransient<IValidator<CreateMessageDto>, CourierPortal.Core.Validators.Admin.CreateMessageDtoValidator>();
builder.Services.AddTransient<IValidator<CreateMessagesRequest>, CourierPortal.Core.Validators.Admin.CreateMessagesRequestValidator>();
builder.Services.AddTransient<IValidator<CourierMessagesRequest>, CourierPortal.Core.Validators.Admin.CourierMessagesRequestValidator>();
builder.Services.AddTransient<IValidator<InfringementCreateRequest>, CourierPortal.Core.Validators.Admin.InfringementCreateRequestValidator>();
builder.Services.AddTransient<IValidator<InfringementCancelRequest>, CourierPortal.Core.Validators.Admin.InfringementCancelRequestValidator>();
builder.Services.AddTransient<IValidator<InfringementCategoryCreateRequest>, CourierPortal.Core.Validators.Admin.InfringementCategoryCreateRequestValidator>();
builder.Services.AddTransient<IValidator<InfringementCategoryUpdateRequest>, CourierPortal.Core.Validators.Admin.InfringementCategoryUpdateRequestValidator>();
builder.Services.AddTransient<IValidator<InfringementCategoryLinkCreateRequest>, CourierPortal.Core.Validators.Admin.InfringementCategoryLinkCreateRequestValidator>();
builder.Services.AddTransient<IValidator<InfringementCategoryLinkUpdateRequest>, CourierPortal.Core.Validators.Admin.InfringementCategoryLinkUpdateRequestValidator>();
builder.Services.AddTransient<IValidator<IdentifierRequest>, CourierPortal.Core.Validators.Admin.IdentifierRequestValidator>();
builder.Services.AddTransient<IValidator<InvoiceBatchRequest>, CourierPortal.Core.Validators.Admin.InvoiceBatchRequestValidator>();
builder.Services.AddTransient<IValidator<NotificationsRequest>, CourierPortal.Core.Validators.Admin.NotificationsRequestValidator>();
builder.Services.AddTransient<IValidator<ScheduleResponseStatusUpdateRequest>, CourierPortal.Core.Validators.Admin.ScheduleResponseStatusUpdateRequestValidator>();
builder.Services.AddTransient<IValidator<SchedulesCreateRequest>, CourierPortal.Core.Validators.Admin.SchedulesCreateRequestValidator>();
builder.Services.AddTransient<IValidator<TimeSlotCreateRequest>, CourierPortal.Core.Validators.Admin.TimeSlotCreateRequestValidator>();
builder.Services.AddTransient<IValidator<TimeSlotUpdateRequest>, CourierPortal.Core.Validators.Admin.TimeSlotUpdateRequestValidator>();
builder.Services.AddTransient<IValidator<ScheduleCopyRequest>, CourierPortal.Core.Validators.Admin.ScheduleCopyRequestValidator>();
builder.Services.AddTransient<IValidator<SearchRequest>, CourierPortal.Core.Validators.Admin.SearchRequestValidator>();

// ═══════════════════════════════════════════════════════════
// ── Portal Services (from CourierPortal) ──
// ═══════════════════════════════════════════════════════════
builder.Services.AddScoped<PortalTimeZoneService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PortalAuthService>();
builder.Services.AddScoped<PortalCourierService>();
builder.Services.AddScoped<RunRepository>();
builder.Services.AddScoped<PortalRunService>();
builder.Services.AddScoped<PortalInvoiceService>();
builder.Services.AddScoped<PortalScheduleService>();
builder.Services.AddScoped<PortalApplicantService>();
builder.Services.AddScoped<PortalContractService>();
builder.Services.AddScoped<PortalLocationService>();
builder.Services.AddScoped<PortalVehicleService>();
builder.Services.AddScoped<PortalReportService>();

// ── Portal Validators ──
builder.Services.AddTransient<IValidator<TokenRequest>, CourierPortal.Core.Validators.Portal.TokenRequestValidator>();
builder.Services.AddTransient<IValidator<TokenRefreshRequest>, CourierPortal.Core.Validators.Portal.TokenRefreshRequestValidator>();
builder.Services.AddTransient<IValidator<TokenAccessKeyRequest>, CourierPortal.Core.Validators.Portal.TokenAccessKeyRequestValidator>();
builder.Services.AddTransient<IValidator<ChangePasswordRequest>, CourierPortal.Core.Validators.Portal.ChangePasswordRequestValidator>();
builder.Services.AddTransient<IValidator<CourierUpdateRequest>, CourierPortal.Core.Validators.Portal.CourierUpdateRequestValidator>();
builder.Services.AddTransient<IValidator<InvoiceCreateRequest>, CourierPortal.Core.Validators.Portal.InvoiceCreateRequestValidator>();
builder.Services.AddTransient<IValidator<CourierPortal.Core.DTOs.Portal.Invoices.InvoiceRequest>, CourierPortal.Core.Validators.Portal.InvoiceRequestValidator>();
builder.Services.AddTransient<IValidator<RunRequest>, CourierPortal.Core.Validators.Portal.RunRequestValidator>();
builder.Services.AddTransient<IValidator<AvailableRunRequest>, CourierPortal.Core.Validators.Portal.AvailableRunRequestValidator>();
builder.Services.AddTransient<IValidator<RegisterRequest>, CourierPortal.Core.Validators.Portal.RegisterRequestValidator>();
builder.Services.AddTransient<IValidator<EmailVerificationRequest>, CourierPortal.Core.Validators.Portal.EmailVerificationRequestValidator>();
builder.Services.AddTransient<IValidator<ProfileUpdateRequest>, CourierPortal.Core.Validators.Portal.ProfileUpdateRequestValidator>();
builder.Services.AddTransient<IValidator<UploadRequest>, CourierPortal.Core.Validators.Portal.UploadRequestValidator>();
builder.Services.AddTransient<IValidator<DeclarationUpdateRequest>, CourierPortal.Core.Validators.Portal.DeclarationUpdateRequestValidator>();
builder.Services.AddTransient<IValidator<EnquiryRequest>, CourierPortal.Core.Validators.Portal.EnquiryRequestValidator>();

// ═══════════════════════════════════════════════════════════
var app = builder.Build();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description
            }),
            Duration = report.TotalDuration
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

var provider = new FileExtensionContentTypeProvider
{
    Mappings = { [".tpl"] = "text/plain" }
};
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
