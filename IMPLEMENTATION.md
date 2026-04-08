# Courier Portal — Implementation Guide

## 1. Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 8.0+ |
| Node.js | 18+ (for frontend) |
| SQL Server | 2019+ (or Azure SQL) |
| Redis | 6+ (for session/caching) |
| AWS CLI | Configured (for S3 file storage + Data Protection in prod) |

### NuGet Packages Required
- `ClosedXML` — XLSX parsing for import services
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
| `DespatchContext` | Per-tenant operational data (couriers, schedules, jobs, infringements) | Dynamic via `DynamicDespatchDbContextFactory` (resolved from `MasterContext`) |
| `CourierPortalContext` | NP Redesign entities (compliance, quizzes, training, registration, recruitment, NP settings) | `MasterSQLConnection` env var |

### 2.3 Run SQL Migrations

Execute the scripts in `database/` in order against the **tenant Despatch database**:

```
1. database/001_document_types.sql
2. database/002_compliance_profiles.sql
3. database/003_quizzes.sql
4. database/004_recruitment.sql
5. database/005_registration_fields.sql
6. database/006_driver_approvals.sql
7. database/007_messenger.sql
```

> **Note:** These scripts create tables for the `CourierPortalContext` entities. The `DespatchContext` tables (tucCourier, tucCourierFleet, Infringement, etc.) already exist in the legacy Despatch database.

### 2.4 Configure Connection Strings

Edit `api/src/CourierPortal.Api/appsettings.json` with actual values, or use environment variables:

```json
{
  "ConnectionStrings": {
    "Despatch": "Server=<host>;Database=<db>;User Id=<user>;Password=<pass>;TrustServerCertificate=true;",
    "Master": "Server=<host>;Database=DfrntMaster;User Id=<user>;Password=<pass>;TrustServerCertificate=true;"
  },
  "AccountsApi": {
    "BaseUrl": "https://accounts.<tenant>.deliverdifferent.com"
  }
}
```

### 2.5 Environment Variables

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
| `AccountsApi__BaseUrl` | ⚠️ | Accounts API base URL (overrides appsettings) |
| `SmtpServer` | ⚠️ | SMTP server for email |
| `SmtpPort` | ⚠️ | SMTP port |
| `SmtpUsername` | ⚠️ | SMTP credentials |
| `SmtpPassword` | ⚠️ | SMTP credentials |
| `UrgentArmyEmail` | ⚠️ | Sender email for enquiries |

### 2.6 Run

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

### 3.1 Controllers → Accounts API vs Direct DB

| Controller | Data Source | Notes |
|---|---|---|
| **Portal/InvoicesController** | **Accounts API** (HttpClient) | Refactored — no longer queries CourierInvoice directly |
| **Portal/RunsController** | DespatchContext (direct DB) | Uses `RunUtility` for status checks |
| **Portal/ReportsController** | DespatchContext + **Accounts API** | Hybrid — some report data from Accounts |
| **Portal/CouriersController** | DespatchContext | Direct TucCourier queries |
| **Portal/SchedulesController** | DespatchContext | CourierSchedule queries |
| **FleetController** | DespatchContext | TucCourierFleet CRUD |
| **SchedulingController** | DespatchContext | CourierSchedule + ScheduleTemplate |
| **InfringementController** | DespatchContext | Infringement CRUD (migrated from GitLab CM) |
| **TrainingController** | CourierPortalContext | TrainingItem + TrainingCompletion |
| **NpSettingsController** | CourierPortalContext | NpFeatureConfig |
| **ComplianceController** | CourierPortalContext | ComplianceProfile |
| **CourierImportController** | CourierPortalContext | NpCourier bulk import |
| **AgentImportController** | CourierPortalContext | Agent bulk import |
| **UserImportController** | CourierPortalContext | NpUser bulk import |
| **MessengerController** | CourierPortalContext | Conversations (NP Redesign) |

### 3.2 Auth Setup

Two authentication schemes operate simultaneously:

1. **Hub Cookie Auth** (`Identity.Application`): Shared `.AspNet.SharedCookie` across Hub/Portal apps. Cookie domain set via `Domain` env var. Used by all NP Redesign controllers (browser-based admin).

2. **JWT Bearer** (`JwtBearer`): Used by the courier self-service portal (mobile app / SPA). Signing key from `JWTSecretKey` env var.

Authorization policies:
- `Courier` — requires `AccountType=Courier` claim (JWT)
- `Applicant` — requires `AccountType=Applicant` claim (JWT)
- `NpAccess` — any authenticated user (Cookie or JWT)

## 4. Service Implementation Map

### NP Redesign Services (new implementations)

| Interface | Implementation | Context |
|---|---|---|
| `IFleetService` | `FleetService` | DespatchContext |
| `ISchedulingService` | `SchedulingService` | DespatchContext |
| `ITrainingService` | `TrainingService` | CourierPortalContext |
| `INpSettingsService` | `NpSettingsService` | CourierPortalContext |
| `ICourierImportService` | `CourierImportService` | CourierPortalContext |
| `IAgentImportService` | `AgentImportService` | CourierPortalContext |
| `IUserImportService` | `UserImportService` | CourierPortalContext |
| `IPortalService` | `PortalService` | DespatchContext + CourierPortalContext |

### Feature Services (concrete classes, no interface)

| Service | Context | Notes |
|---|---|---|
| `ComplianceService` | CourierPortalContext | Profile + requirement CRUD |
| `DocumentTypeService` | CourierPortalContext | Document type config |
| `DriverApprovalService` | CourierPortalContext | Approval workflow |
| `MessengerService` | CourierPortalContext | NP-to-courier messaging |
| `QuizService` | CourierPortalContext | Quiz management |
| `RecruitmentService` | CourierPortalContext | Job postings + applications |
| `RegistrationFieldService` | CourierPortalContext | Dynamic registration fields |
| `InfringementService` | DespatchContext | Migrated from GitLab CM |

### Portal Services (courier self-service)

| Service | Context | Notes |
|---|---|---|
| `PortalInvoiceService` | DespatchContext + **HttpClient** | Calls Accounts API for invoice CRUD |
| `PortalRunService` | DespatchContext | Uses `RunUtility` (extracted from removed `InvoiceUtility`) |
| `PortalCourierService` | DespatchContext | Courier profile/settings |
| `PortalScheduleService` | DespatchContext | Courier schedule views |
| `PortalAuthService` | DespatchContext | JWT token issuance |

## 5. Applicant App (Recruitment Portal)

The applicant-facing recruitment flow from `dfrnt-recruitment` (apply.urgent.co.nz) is integrated as the third section of this app.

### Pages
| Page | Path | Purpose |
|---|---|---|
| `ApplicantPortal.tsx` | `src/pages/applicant/` | Multi-step application flow (production version from Railway) |
| `AdminSettings.tsx` | `src/pages/np/` | Recruitment admin configuration |
| `DocumentManagement.tsx` | `src/pages/np/` | Document type CRUD with verification UI |
| `FlowBuilder.tsx` | `src/pages/np/` | Drag-and-drop applicant step configuration |
| `QuizBuilder.tsx` | `src/pages/np/` | Training quiz creation |
| `RecruitmentPipeline.tsx` | `src/pages/np/` | Kanban applicant pipeline |

### Step Components (`src/components/steps/`)
- `DetailsStep` — Personal info + contact details
- `DriverLicenseStep` — License upload + AI scan (Textract)
- `VehicleStep` — Vehicle details + photos
- `DocumentUploadStep` — Required document uploads
- `GenericDocumentStep` — Configurable document type upload
- `QuizStep` — Training quiz with auto-grading
- `ReviewStep` — Final review before submission
- `AiScanResult` — Textract OCR results display

### Backend (`Controllers/Applicant/`)
| Controller | Route | Purpose |
|---|---|---|
| `ApplicantController` | `/api/applicants` | Register, verify email, update profile, upload documents, check status |
| `UsersController` | `/api/users` | Admin user management for recruitment |

### API Client
- `src/lib/recruitmentApi.ts` — Applicant-facing fetch-based API client (token in localStorage)
- `src/lib/stepTypes.ts` — Step type definitions and configuration

### Key Features
- Configurable multi-step flow (admin defines which steps via FlowBuilder)
- AI document scanning via AWS Textract (DocumentScanController)
- S3 file storage with local fallback (FileStorageService)
- Quiz system with auto-grading and pass/fail
- Email verification flow
- Applicant status tracking (pipeline stages)

### Source of Truth
The **Railway-deployed `dfrnt-recruitment`** app (`Deliver-Different-Testing/dfrnt-recruitment`) is the most advanced version. The code in this repo's `src/pages/applicant/` and `Controllers/Applicant/` is copied from that production codebase.

## 6. Frontend Notes

- **`src/services/np_mockData.ts`** — Re-export shim for `np_devData.ts` (backward compat)
- **`src/services/portal_mockData.ts`** — Re-export shim for `portal_devData.ts` (backward compat)
- **`src/services/portal_courierService.ts`** — Makes real API calls (verified)
- **`src/services/portal_invoiceService.ts`** — Makes real API calls to Portal/Invoices endpoints
- **`src/services/np_schedulingMockData.ts`** — Contains `MOCK_*` constants but is NOT imported by any service file. Safe to remove when scheduling is fully wired.
- **`src/services/np_openforceService.ts`** — References OpenForce API endpoints. **OpenForce integration is handled by Accounts** — this frontend service should either be removed or redirected to call Accounts API endpoints.

## 8. Known Issues / TODOs

1. **PortalReportService** — May still reference Accounts-domain data. Needs audit to confirm it calls Accounts API for financial reports.

2. **InvoiceLineJobRepository** (`Infrastructure/Repositories/InvoiceLineJobRepository.cs`) — References stored procedures `CM_stpJobsByInvoice` and `CM_stpUninvoicedJobs` which are invoice-related. These procs may now live in the Accounts DB. Verify whether this repository is still called anywhere, or remove it.

3. **OpenForce entity properties** — `TblSetting` and `TucCourier` entities still have OpenForce-related properties (`OpenforceClientId`, `OpenForceNumber`, etc.). These map to existing DB columns and should NOT be removed, but the **OpenForce business logic** lives in Accounts.

4. **Google Sheets import** — `AgentImportService.ParseGoogleSheetAsync()` throws `NotImplementedException`. Requires Google Sheets API configuration if needed.

5. **NpCourier/NpUser/Agent/TrainingItem DbSets** — Added to `CourierPortalContext` but may need EF migrations to create the underlying tables if they don't already exist.

6. **`ScheduleTemplate` entity** — Referenced by `ISchedulingService` and registered via `db.Set<ScheduleTemplate>()`. Ensure the table exists in the Despatch database or add a migration.

7. **Compliance Automation** — `ComplianceAutomationService` exists in `Services/NpRedesign/` but is not registered in DI. Wire up when ready.

8. **`np_schedulingMockData.ts`** — Orphaned mock data file. Remove once scheduling frontend is fully connected to the API.

9. **`np_openforceService.ts`** — Frontend service for OpenForce that should either be removed or pointed at Accounts API.

## 9. Removed / Migrated Entities

The following entities were removed from courier-portal (now in Accounts app):

- `CourierInvoice`, `CourierInvoiceLine`, `CourierInvoiceLineJob`, `CourierInvoiceLineJobMaster`
- `CourierInvoiceBatch`, `CourierInvoiceBatchItem`, `CourierInvoiceBatchStatus`
- `CourierDeduction`, `CourierDeductionLine`, `CourierDeductionRecurring`, `CourierDeductionRecurringLine`
- `CourierDeductionType`

These are commented out in `DespatchContext.cs` with `// REMOVED: handled by Accounts` markers.

`PortalInvoiceService` has been refactored to call the Accounts API via `HttpClient` instead of querying these entities directly.
