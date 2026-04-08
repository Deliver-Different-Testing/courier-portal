using CourierPortal.Api;
using CourierPortal.Core;
using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Domain.Master;

// Portal DTOs
using CourierPortal.Core.DTOs.Portal.Applications;
using CourierPortal.Core.DTOs.Portal.Auth;
using CourierPortal.Core.DTOs.Portal.Couriers;
using CourierPortal.Core.DTOs.Portal.Invoices;
using CourierPortal.Core.DTOs.Portal.Runs;

// Portal Services
using CourierPortal.Core.Services.Portal;

// Feature Services
using CourierPortal.Core.Services;

// Interfaces
using CourierPortal.Core.Interfaces;

// Infrastructure
using CourierPortal.Infrastructure.Models;
using CourierPortal.Infrastructure.Repositories;
using CourierPortal.Infrastructure.Services;

// Middleware
using CourierPortal.Api.Middleware;

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

// ── Accounts API HttpClient ──
builder.Services.AddHttpClient("AccountsApi", client =>
{
    var baseUrl = builder.Configuration["AccountsApi:BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

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

// ── CourierPortalContext (NP Redesign entities) ──
builder.Services.AddDbContext<CourierPortalContext>(options =>
    options.UseSqlServer(
        Environment.GetEnvironmentVariable("MasterSQLConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=dummy;Trusted_Connection=True;"
    ));

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

// ── Authorization Policies ──
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Courier", p => p.RequireClaim("AccountType", AccountType.Courier.ToString()))
    .AddPolicy("Applicant", p => p.RequireClaim("AccountType", AccountType.Applicant.ToString()))
    .AddPolicy("NpAccess", p => p.RequireAuthenticatedUser());

// ── FluentValidation ──
builder.Services.AddFluentValidationAutoValidation();

// ═══════════════════════════════════════════════════════════
// ── NP Redesign Services ──
// ═══════════════════════════════════════════════════════════
builder.Services.AddScoped<IFleetService, FleetService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<INpSettingsService, NpSettingsService>();
builder.Services.AddScoped<ICourierImportService, CourierImportService>();
builder.Services.AddScoped<IAgentImportService, AgentImportService>();
builder.Services.AddScoped<IUserImportService, UserImportService>();
builder.Services.AddScoped<IPortalService, PortalService>();

// ── Feature Services (courier-portal specific) ──
builder.Services.AddScoped<ComplianceService>();
builder.Services.AddScoped<DocumentTypeService>();
builder.Services.AddScoped<DriverApprovalService>();
builder.Services.AddScoped<MessengerService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<RecruitmentService>();
builder.Services.AddScoped<RegistrationFieldService>();

// Infringements
builder.Services.AddScoped<InfringementService>();

// ═══════════════════════════════════════════════════════════
// ── Portal Services (courier self-service) ──
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

// ── File Storage (S3 + local fallback) ──
builder.Services.AddSingleton<FileStorageService>();

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

// Global error handling middleware
app.UseErrorHandling();

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
