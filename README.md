# DFRNT Courier Portal

Standalone courier management portal extracted from the NP-Agent-Management app. Two user roles:

1. **Admin Portal** — Courier management, compliance, recruitment, scheduling, fleet, invoicing, settings
2. **Courier Portal** — Self-service for couriers: dashboard, runs, schedule, invoicing, reports

## Structure

```
src/
├── pages/
│   ├── np/           — Admin pages (30 pages)
│   │   ├── Dashboard, AddCourier, CourierSetup, CourierImport
│   │   ├── ComplianceDashboard, ComplianceHub, ComplianceProfiles
│   │   ├── ContractSettings, DocumentTypeSettings
│   │   ├── FleetManagement, FleetOverview
│   │   ├── Operations, Scheduling
│   │   ├── RecruitmentPipeline, RecruitmentAdvertising, RecruitmentStageSettings
│   │   ├── QuizBuilder, QuizPlayer
│   │   ├── OpenforceActivity
│   │   ├── Reports, Settings, Users, UserImport
│   │   └── ApplicantPortal, ApplicantDetail, PortalUrl, RegistrationSettings
│   ├── portal/       — Courier self-service (6 pages)
│   │   ├── Dashboard, Runs, Schedule
│   │   ├── Contractors, Invoicing, Reports
│   └── courier/
│       └── CourierTraining
├── components/       — 20+ reusable components
├── services/         — API service layer (currently mock data)
├── hooks/            — React hooks
├── types/            — TypeScript interfaces
└── context/          — React context providers
```

## Current Status: Frontend Prototype

All pages are fully built with polished UI but use **mock data only**. No API calls are made.

## GitLab Backend Mapping

The following existing GitLab repos contain the production backend endpoints that this frontend needs to connect to:

### `couriermanager` (GitLab: git.customd.com/urgent-couriers/couriermanager)

Admin-facing API — 16 controllers, 18 services, full EF Core entities.

| Controller | Endpoints | Maps To Frontend Page |
|---|---|---|
| `CouriersController` | CRUD, search, availability, hours, link | Dashboard, AddCourier, CourierSetup |
| `ApplicantsController` | CRUD, approve/reject, documents, uploads | RecruitmentPipeline, ApplicantDetail, ApplicantPortal |
| `FleetsController` | Fleet list, vehicles by fleet | FleetManagement, FleetOverview |
| `SchedulesController` | CRUD schedules, time slots, copy, notifications | Scheduling |
| `InvoicesController` | Invoice batches, transactions, uninvoiced jobs, search | Reports (financial) |
| `ContractsController` | Create/manage contracts | ContractSettings |
| `DeductionsController` | Deductions, recurring deductions, types | (part of courier management) |
| `InfringementsController` | CRUD infringements, categories, links | ComplianceDashboard |
| `MessagesController` | Courier messaging, recent messages | CourierMessenger component |
| `ReportsController` | No-shows, sustainability | Reports |
| `OpenforceController` | Openforce integration, settlements, contractors | OpenforceActivity |
| `SettingsController` | Tenant settings, Openforce settings | Settings |
| `LocationsController` | Location list | (used across multiple pages) |
| `VehiclesController` | Vehicle types | FleetOverview |
| `AuthController` | Portal access keys, validation | Login/Auth |
| `ValuesController` | Lookup values | (used across multiple pages) |

**Key Services:**
- `ApplicantsService` — full recruitment pipeline logic
- `CouriersService` — courier CRUD + availability matrix
- `ScheduleService` — complex scheduling with time slots + vehicle assignments
- `InvoiceService` + `InvoiceBatchService` — courier pay processing
- `OpenforceService` — 1099/contractor payment integration
- `DeductionService` — courier deductions (one-time + recurring)
- `InfringementsService` — compliance tracking

**EF Core Entities (DespatchContext):**
- `TucCourier`, `TucCourierFleet`, `CourierType`, `VehicleType`
- `CourierApplicant`, `CourierApplicantDocument`, `CourierApplicantUpload`
- `CourierSchedule`, `CourierScheduleResponse`, `CourierScheduleTimeSlot`
- `CourierInvoice`, `CourierInvoiceLine`, `CourierInvoiceBatch`
- `CourierDeduction`, `CourierDeductionRecurring`, `CourierDeductionType`
- `CourierContract`, `CourierAvailability`
- `Infringement`, `InfringementCategory`, `InfringementCategoryLink`
- `TblJob`, `TblSetting`, `TucClient`, `TblBulkRegion`

### `courierportal` (GitLab: git.customd.com/urgent-couriers/courierportal)

Courier-facing API — 13 controllers, 12 services. This is what couriers log into.

| Controller | Endpoints | Maps To Frontend Page |
|---|---|---|
| `CouriersController` | Profile, availability, hours, fleet, documents | portal/Dashboard |
| `RunsController` | Assigned runs, run items | portal/Runs |
| `SchedulesController` | View/respond to schedules | portal/Schedule |
| `InvoicesController` | View invoices, download PDFs | portal/Invoicing |
| `ReportsController` | Courier performance reports | portal/Reports |
| `ContractsController` | View/sign contracts | (contract acceptance) |
| `ApplicantsController` | Registration, profile updates, document uploads | ApplicantPortal |
| `AuthController` | JWT login, refresh, access keys | Login |
| `LocationsController` | Location list | (shared) |
| `VehiclesController` | Vehicle types | (shared) |
| `RecaptchaController` | Registration captcha | ApplicantPortal |

**Key Services:**
- `CourierService` — courier profile + availability
- `RunService` — run/job assignment viewing
- `InvoiceService` — invoice viewing + PDF download
- `ScheduleService` — schedule viewing + response
- `ApplicantService` — self-registration + document upload
- `AuthService` — JWT token management

**EF Core Entities (DespatchContext):**
- Same base entities as CourierManager (shared DB)
- `RunItem` — job/run assignments for courier view

## Wiring To Production

To make this production-ready:

1. **Replace mock services** — Each `src/services/np_*.ts` file returns hardcoded data. Replace with axios calls to the existing GitLab API endpoints.
2. **Auth** — Implement Hub cookie auth (admin) and JWT (courier portal) matching existing GitLab patterns.
3. **API base URL** — Configure `VITE_API_URL` to point to the deployed CourierManager API and `VITE_PORTAL_API_URL` for CourierPortal API.
4. **No new backend needed** — The GitLab repos already have the full API. This frontend just needs to call those endpoints.

### Service → API Mapping

| Frontend Service | GitLab API |
|---|---|
| `np_courierService.ts` | `CourierManager/API/Controllers/CouriersController.cs` |
| `np_complianceService.ts` | `CourierManager/API/Controllers/InfringementsController.cs` |
| `np_dashboardService.ts` | `CourierManager/API/Controllers/CouriersController.cs` (aggregate) |
| `np_fleetService.ts` | `CourierManager/API/Controllers/FleetsController.cs` |
| `np_recruitmentService.ts` | `CourierManager/API/Controllers/ApplicantsController.cs` |
| `np_schedulingMockData.ts` | `CourierManager/API/Controllers/SchedulesController.cs` |
| `np_openforceService.ts` | `CourierManager/API/Controllers/OpenforceController.cs` |
| `np_contractService.ts` | `CourierManager/API/Controllers/ContractsController.cs` |
| `np_reportService.ts` | `CourierManager/API/Controllers/ReportsController.cs` |
| `np_settingsService.ts` | `CourierManager/API/Controllers/SettingsController.cs` |
| `np_userService.ts` | Hub user management (separate) |
| `portal_invoiceService.ts` | `CourierPortal/API/Controllers/InvoicesController.cs` |
| `portal_mockData.ts` | `CourierPortal/API/Controllers/RunsController.cs` etc. |

## New Features (Not In GitLab)

These frontend features don't have GitLab backend equivalents yet — they'll need new API endpoints:

- **Compliance Profiles** — configurable document requirement templates
- **Quiz Builder / Player** — training quiz creation and completion
- **Recruitment Advertising** — job posting management
- **Registration Settings** — configurable applicant portal fields
- **Document Type Settings** — configurable document categories
- **Driver Approval** (tenant-side) — multi-tenant courier approval workflow
- **Courier Messenger** — real-time messaging UI (GitLab has basic `MessagesController`)

## Tech Stack

- React 19 + TypeScript
- Vite 6
- Tailwind CSS 3.4
- React Router 7
- Axios (configured, not called)
- Lucide React icons
- Recharts (dashboard charts)
