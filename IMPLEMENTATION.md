# Courier Portal — Implementation Guide

> **Scope: Courier self-service portal + Applicant recruitment app only.**
> NP Admin features (fleet, compliance, scheduling, recruitment pipeline, training, settings) have been moved to Steve-v2.0-NP-Redesign.
> Last updated: 2025-04-08

## 1. Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 8.0+ |
| Node.js | 18+ (for frontend) |
| SQL Server | 2019+ (or Azure SQL) |
| Redis | 6+ (for session/caching) |
| AWS CLI | Configured (for S3 file storage + Data Protection in prod) |

### NuGet Packages Required
- `FluentValidation.AspNetCore` — request validation
- `Serilog` + `Serilog.Sinks.Console` — structured logging
- `StackExchange.Redis` — distributed cache
- `Microsoft.EntityFrameworkCore.SqlServer` — database
- `Newtonsoft.Json` — JSON serialization (legacy)

## 2. Step-by-Step Setup

### 2.1 Clone & Restore

```bash
cd /tmp/courier-portal
dotnet restore api/src/CourierPortal.Api/CourierPortal.Api.csproj
cd src && npm install && cd ..
```

### 2.2 Database Setup

The application uses **two EF Core contexts**:

| Context | Purpose | Connection |
|---|---|---|
| `MasterContext` | Tenant directory, multi-tenant routing | `MasterSQLConnection` env var |
| `DespatchContext` | Per-tenant operational data (couriers, schedules, jobs) | Dynamic via `DynamicDespatchDbContextFactory` |

> **Note:** The `CourierPortalContext` (NP Redesign entities) has been removed. Compliance, quizzes, training, recruitment, and NP settings are now in Steve-v2.0-NP-Redesign.

### 2.3 Environment Variables

| Variable | Required | Description |
|---|---|---|
| `MasterSQLConnection` | ✅ | Master DB connection string |
| `Domain` | ✅ | Cookie domain (e.g. `.deliverdifferent.com`) |
| `RedisConfig` | ✅ | Redis connection string |
| `JWTSecretKey` | ✅ | JWT signing key for bearer auth |
| `Issuer` | ✅ | JWT issuer claim |
| `Audience` | ✅ | JWT audience claim |
| `PublicPath` | ✅ | Login redirect URL |
| `S3_BUCKET_NAME` | ⚠️ | S3 bucket for file uploads (fallback to local) |
| `AccountsApi__BaseUrl` | ⚠️ | Accounts API base URL |

### 2.4 Run

```bash
cd api/src/CourierPortal.Api
dotnet run
```

Frontend dev server:
```bash
cd src
npm run dev
```

## 3. Architecture

### 3.1 Controllers

| Controller | Data Source | Notes |
|---|---|---|
| **Portal/InvoicesController** | **Accounts API** (HttpClient) | Calls Accounts for invoice CRUD |
| **Portal/RunsController** | DespatchContext (direct DB) | Uses `RunUtility` for status checks |
| **Portal/ReportsController** | DespatchContext + **Accounts API** | Hybrid |
| **Portal/CouriersController** | DespatchContext | Direct TucCourier queries |
| **Portal/SchedulesController** | DespatchContext | CourierSchedule queries |
| **Portal/AuthController** | DespatchContext | JWT token issuance |
| **Portal/ApplicantsController** | DespatchContext | Applicant management |
| **Applicant/ApplicantController** | DespatchContext | Self-service registration |
| **Applicant/UsersController** | DespatchContext | Admin user management for recruitment |
| **DocumentScanController** | AWS Textract | AI document scanning |
| **PortalStepsController** | DespatchContext | Applicant flow step config |
| **QuizController** | DespatchContext | Applicant quiz |
| **PortalController** | DespatchContext | Portal configuration |

### 3.2 Auth Setup

Two authentication schemes operate simultaneously:

1. **Hub Cookie Auth** (`Identity.Application`): Shared `.AspNet.SharedCookie` across Hub/Portal apps. Used by admin pages (FlowBuilder, QuizBuilder, etc.).

2. **JWT Bearer** (`JwtBearer`): Used by courier self-service portal (mobile app / SPA). Signing key from `JWTSecretKey` env var.

Authorization policies:
- `Courier` — requires `AccountType=Courier` claim (JWT)
- `Applicant` — requires `AccountType=Applicant` claim (JWT)
- `NpAccess` — any authenticated user (Cookie or JWT)

## 4. Service Implementation Map

### Portal Services (courier self-service)

| Service | Context | Notes |
|---|---|---|
| `PortalInvoiceService` | DespatchContext + **HttpClient** | Calls Accounts API for invoice CRUD |
| `PortalRunService` | DespatchContext | Uses `RunUtility` |
| `PortalCourierService` | DespatchContext | Courier profile/settings |
| `PortalScheduleService` | DespatchContext | Courier schedule views |
| `PortalAuthService` | DespatchContext | JWT token issuance |
| `PortalApplicantService` | DespatchContext | Applicant registration |
| `PortalContractService` | DespatchContext | Contract management |
| `PortalLocationService` | DespatchContext | Location lookups |
| `PortalVehicleService` | DespatchContext | Vehicle type lookups |
| `PortalReportService` | DespatchContext | Courier reports |
| `PortalTimeZoneService` | — | Time zone utilities |

### Applicant/Quiz Services

| Service | Context | Notes |
|---|---|---|
| `PortalService` | DespatchContext | Portal step configuration |
| `QuizService` | DespatchContext | Quiz management for applicant flow |

### Infrastructure Services

| Service | Notes |
|---|---|
| `EmailService` | SMTP email sending |
| `FileStorageService` | S3 + local fallback file storage |
| `PasswordUtility` | Password hashing |

## 5. Applicant App (Recruitment Portal)

The applicant-facing recruitment flow from `dfrnt-recruitment` (apply.urgent.co.nz).

### Pages
| Page | Path | Purpose |
|---|---|---|
| `ApplicantPortal.tsx` | `src/pages/applicant/` | Multi-step application flow |
| `AdminSettings.tsx` | `src/pages/np/` | Recruitment admin configuration |
| `DocumentManagement.tsx` | `src/pages/np/` | Document type CRUD with verification UI |
| `FlowBuilder.tsx` | `src/pages/np/` | Drag-and-drop applicant step configuration |
| `QuizBuilder.tsx` | `src/pages/np/` | Training quiz creation |
| `SetupPassword.tsx` | `src/pages/np/` | Admin password setup |

### Step Components (`src/components/steps/`)
- `DetailsStep` — Personal info + contact details
- `DriverLicenseStep` — License upload + AI scan (Textract)
- `VehicleStep` — Vehicle details + photos
- `DocumentUploadStep` — Required document uploads
- `GenericDocumentStep` — Configurable document type upload
- `QuizStep` — Training quiz with auto-grading
- `ReviewStep` — Final review before submission
- `AiScanResult` — Textract OCR results display

### Key Features
- Configurable multi-step flow (admin defines which steps via FlowBuilder)
- AI document scanning via AWS Textract (DocumentScanController)
- S3 file storage with local fallback (FileStorageService)
- Quiz system with auto-grading and pass/fail
- Email verification flow
- Applicant status tracking

## 6. What Was Removed (moved to Steve-v2.0-NP-Redesign)

### Controllers removed:
AgentController, AgentImportController, AgentVehicleRateController, ComplianceAutomationController, ComplianceController, ComplianceDashboardController, ContractController, CourierDocumentController, CourierImportController, DocumentTypeController, DriverApprovalsController, FleetController, InfringementController, MarketplaceController, MessengerController, NpCourierController, NpDashboardController, NpReportController, NpSettingsController, NpUserController, RecruitmentController, RecruitmentSettingsController, RegistrationFieldsController, SchedulingController, TrainingController, UserImportController

### Services removed:
FleetService, SchedulingService, TrainingService, NpSettingsService, ComplianceService, DocumentTypeService, RecruitmentService, RegistrationFieldService, InfringementService, DriverApprovalService, MessengerService, AgentImportService, CourierImportService, UserImportService, ComplianceAutomationService, Validators

### Middleware removed:
NpAuthorizationMiddleware, NpTierMiddleware

### Entities removed:
Agent, AgentCourierRate, AgentOnboarding, AgentVehicleRate, ComplianceAutomationConfig, ComplianceNotification, ComplianceProfile, ComplianceProfileRequirement, CourierDocument, CourierDocumentAudit, CourierDocumentType, CourierMessage, Conversation, ConversationMessage, DriverApproval, Infringement, InfringementCategory, InfringementCategoryLink, MarketplacePosting, MarketplaceQuote, NpCourier, NpFeatureConfig, NpUser, ProspectAgent, RecruitmentNote, RecruitmentStage, RecruitmentStageConfig, RegistrationField, RegistrationFieldOption, TenantSettings, TrainingItem, TucCourierFleet

### Frontend pages removed:
Dashboard, FleetOverview, FleetManagement, CourierSetup, AddCourier, CourierImport, ComplianceDashboard, ComplianceHub, ComplianceProfiles, Scheduling, Operations, Reports, Users, UserImport, Settings, RecruitmentPipeline, RecruitmentAdvertising, RecruitmentStageSettings, ApplicantDetail, ApplicantPortal (np version), ContractSettings, DocumentTypeSettings, RegistrationSettings, DriverComplianceTab, CourierPortalLinks, PortalUrl, QuizPlayer, OpenforceActivity

### Frontend services removed:
All `np_*.ts` services

## 7. Known Issues / TODOs

1. **PortalReportService** — May still reference Accounts-domain data. Needs audit.
2. **InvoiceLineJobRepository** — References stored procedures that may now live in Accounts DB. Verify or remove.
3. **OpenForce entity properties** — `TblSetting` and `TucCourier` still have OpenForce-related properties (map to existing DB columns, don't remove).
