# Courier Portal — Implementation Guide

**For:** Loc (Developer)  
**Date:** 2026-04-12 (updated 2026-04-13)  
**Status:** Frontend complete, backend wiring required

> **🆕 Apr 13 Updates:**  
> - 15 missing entity classes added, `CourierApplicant` fixed to match production schema  
> - See **BACKEND-PRIORITY-CHECKLIST.md** for the prioritised build/wiring order  
> - See **PROPERTY-MAPPING.md** for NP service property name → production schema mappings  
> - Expected build errors reduced from ~241 to <20 after pulling latest

---

## What Has Been Done (Don't Re-Do This)

The `courier-portal` repo has been set up as the **authoritative app** for courier recruitment, compliance, fleet management, scheduling, and courier self-service. Here's what's already in place:

### Backend (`api/`)
- **288 C# files, 39 controllers** — includes both the original Portal/Applicant controllers AND the NP Redesign controllers (copied from NP-Agent-Management, namespaced to `CourierPortal.*`).
- **Portal controllers** (in `Controllers/Portal/`) — Auth, Couriers, Invoices, Runs, Schedules, Reports, Contracts, Locations, Vehicles, Applicants, Recaptcha. These are fully implemented with service registrations in Program.cs. ✅
- **NP Redesign controllers** (in `Controllers/`) — NpDashboard, NpCourier, NpUser, NpSettings, NpReport, Fleet, Compliance, Scheduling, Recruitment, RecruitmentSettings, Training, Messenger, DocumentType, CourierDocument, CourierImport, UserImport, ComplianceAutomation, Openforce. These have been copied in but their **services are NOT yet registered in Program.cs**. 🔧 Loc's main job.
- See `AUDIT.md` for full controller/entity overlap analysis.
- **Known TODOs you must complete:**
  - `Program.cs` → **Register all NP Redesign services.** Each NP controller needs its service implementation registered. The service classes are in `CourierPortal.Infrastructure/Services/` and interfaces in `CourierPortal.Core/Interfaces/`. Add scoped registrations for each.
  - `Portal/InvoiceService` → **KEEP as-is.** Couriers generate invoices through the portal by selecting uninvoiced runs. This reads from DespatchContext (CourierInvoice, CourierInvoiceLine tables) and must stay in courier-portal. The Accounts app handles invoice *processing/settlement* after creation — but invoice *generation* by the courier happens here.
  - `Portal/RunService` → Keep. Uses `InvoiceUtility.IsCompleted` / `CanInvoice` to determine which runs are invoiceable.
  - **Accounts boundary clarification:** Courier-portal owns invoice *creation* (courier selects runs → generates invoice). Accounts owns invoice *processing* (batching, settlements, payments, deductions, statements). The `Portal/InvoicesController` endpoints (Recent, Past, Uninvoiced, Create) all stay in courier-portal.
  - **Openforce boundary:** Courier-portal owns recruitment/onboarding status only. Payments/settlements/1099 stay in Accounts.

### Frontend (`src/`)
- **146 files, 55 pages, 0 TypeScript errors** — all pages complete and compiling clean.
- **All mock data files deleted.** No more `np_mockData.ts`, `portal_mockData.ts`, `np_schedulingMockData.ts`, or `portal_devData.ts`.
- **All frontend services now make real API calls** — portal services call `api/portal/*` controllers (which are registered), NP Admin services call the NP controller routes (which exist but need Program.cs registration).
- Each NP service file has `@backend-needed` JSDoc comments indicating which controller it calls.

**Page inventory:**

| Area | Path | Count | Status |
|------|------|--------|--------|
| Applicant portal (mobile) | `src/pages/applicant/` | 1 | UI complete — needs API wiring |
| Courier mobile app | `src/pages/courier/` | 11 | UI complete — needs API wiring |
| NP admin pages | `src/pages/np/` | 31 | UI complete — needs API wiring |
| Portal (courier self-service desktop) | `src/pages/portal/` | 6 | UI complete — needs API wiring |
| Shared layouts | `src/components/Layout/` | 4 | Complete |
| Portal layout | `src/components/portal/` | 1 | Complete |
| Common components | `src/components/common/` | 10+ | Complete |
| Import components | `src/components/import/` | 6 | Complete |
| Steps (applicant wizard) | `src/components/steps/` | 8 | Complete |
| Services (mock) | `src/services/` | 20+ | Mock — needs real API calls |
| Hooks | `src/hooks/` | 10 | Mock — needs real API calls |

---

## Project Boundary: courier-portal vs dfrnt-agents-partners

**This is critical.** There are two separate admin apps:

### courier-portal (this repo) — YOUR scope
- Courier recruitment pipeline
- Applicant portal (public-facing mobile wizard)
- Compliance management (document types, profiles, driver approval)
- Fleet CRUD + courier assignment to fleets
- Scheduling (courier shift management)
- Training (flow builder, quiz builder)
- Document management
- Messenger (NP ↔ courier messaging)
- Courier self-service portal (runs, schedule, invoices, contractors, training)
- Registration settings, contract settings, advertising
- Openforce — **recruitment/onboarding status only** (not payments/settlements/1099 — those are in Accounts)
- NP user management

### dfrnt-agents-partners (Garry's repo) — NOT your scope
- Agent discovery (Auto-Mate AI)
- Agent CRUD and onboarding
- Agent marketplace
- NP tier management
- Vehicle rates
- Data masking
- ECA/CLDA association stats

### Shared database — separate contexts
Both apps connect to the same SQL Server database per tenant, but use **separate EF contexts**:

| App | DbContext | Tables |
|-----|-----------|--------|
| courier-portal | `CourierPortalContext` + `DespatchContext` | TucCourier, TucCourierFleet, Schedule, Applicant, ComplianceProfile, DocumentType, Quiz, etc. |
| dfrnt-agents-partners | `AgentsDbContext` | Agent, Marketplace, ProspectAgent, NpTier, etc. |

**Never write to the Agent/Marketplace tables from courier-portal, and vice versa.**

### Long-term plan
Both apps will merge into one "Admin Manager" Hub tile. Short-term they are separate tiles. Build courier-portal cleanly within its own domain and don't create cross-repo dependencies.

---

## dfrnt-recruitment (Railway App) — Migration Path

**Repo:** `/data/.openclaw/workspace/dfrnt-recruitment/`  
**Deployed at:** `apply.urgent.co.nz` (Railway, PostgreSQL, .NET)  
**Controllers:** Applicant, Auth, Document, DocumentScan, PortalSteps, Quiz, Recruitment, Settings, Users (9 total)

### Current state
`dfrnt-recruitment` is the **live public applicant portal**. It serves the `ApplicantPortal.tsx` flow that new couriers use to apply.

### courier-portal's relationship
The courier-portal backend already has **equivalent or more complete** versions of every dfrnt-recruitment controller:

| dfrnt-recruitment | courier-portal equivalent |
|---|---|
| `ApplicantController` | `Portal/ApplicantsController` + `Controllers/ApplicantsController` |
| `AuthController` | `Portal/AuthController` |
| `DocumentController` | `DocumentTypesController` |
| `DocumentScanController` | `DocumentScanController` |
| `PortalStepsController` | `PortalStepsController` |
| `QuizController` | `QuizController` |
| `RecruitmentController` | `RecruitmentController` |
| `SettingsController` | `RegistrationFieldsController` + `SettingsController` |
| `UsersController` | `UsersController` |

### Migration path
1. **Phase 1 (now):** Wire courier-portal's applicant portal endpoints (see wiring steps below). Keep dfrnt-recruitment live.
2. **Phase 2:** Once courier-portal's `/apply` routes are tested, update DNS to point to courier-portal.
3. **Phase 3:** dfrnt-recruitment is retired or becomes a thin redirect.

**Database:** dfrnt-recruitment uses PostgreSQL. courier-portal uses SQL Server. When migrating, you'll need a one-time data migration for any live applicant/quiz/document data.

---

## Entity / Table Reference

From `AUDIT.md` and the backend codebase:

| Entity | Table | Context | Owned By |
|--------|-------|---------|----------|
| TucCourier | `TucCourier` | DespatchContext | courier-portal |
| TucCourierFleet | `TucCourierFleet` | DespatchContext | courier-portal |
| TucSchedule | `TucSchedule` | DespatchContext | courier-portal |
| Applicant | `Applicants` | CourierPortalContext | courier-portal |
| ComplianceProfile | `ComplianceProfiles` | CourierPortalContext | courier-portal |
| ComplianceRequirement | `ComplianceRequirements` | CourierPortalContext | courier-portal |
| DocumentType | `DocumentTypes` | CourierPortalContext | courier-portal |
| CourierDocument | `CourierDocuments` | CourierPortalContext | courier-portal |
| DriverApproval | `DriverApprovals` | CourierPortalContext | courier-portal |
| Quiz | `Quizzes` | CourierPortalContext | courier-portal |
| QuizQuestion | `QuizQuestions` | CourierPortalContext | courier-portal |
| QuizAttempt | `QuizAttempts` | CourierPortalContext | courier-portal |
| PortalStep | `PortalSteps` | CourierPortalContext | courier-portal |
| RegistrationField | `RegistrationFields` | CourierPortalContext | courier-portal |
| Contract | `Contracts` | CourierPortalContext | courier-portal |
| MessengerConversation | `MessengerConversations` | CourierPortalContext | courier-portal |
| MessengerMessage | `MessengerMessages` | CourierPortalContext | courier-portal |
| RecruitmentStage | `RecruitmentStages` | CourierPortalContext | courier-portal |
| RecruitmentJob | `RecruitmentJobs` | CourierPortalContext | courier-portal |
| User | `Users` | CourierPortalContext | courier-portal |
| NpCourier | `NpCouriers` | CourierPortalContext | courier-portal |

**Accounts app owns** (do not duplicate): CourierInvoiceBatch (batch processing/settlements), CourierDeduction, Fleet listing (GetAll only — courier-portal has full Fleet CRUD), Location, Payment, DirectDebit, Statement. **Note:** CourierInvoice entity is shared — courier-portal creates invoices, Accounts processes/settles them.

---

## How the Frontend Services Work (Currently)

All `src/services/` files are **mock services** — they return fake data. The pattern is:

```typescript
// Example: np_courierService.ts
export const courierService = {
  getAll: () => Promise.resolve(mockCouriers),
  getById: (id: number) => Promise.resolve(mockCouriers.find(c => c.id === id)),
  // ...
};
```

Each page imports from the service and calls it. To wire a page to the real backend, you replace the mock implementation with real Axios calls.

There are two API base configs:
- `src/lib/api.ts` — base Axios instance for NP admin API calls (`/api/`)
- `src/services/portal_api.ts` — portal Axios instance for courier self-service (`/api/portal/`)

---

## Step-by-Step Wiring Guide

Work through these in order. Each section has: **what page uses it**, **what API endpoint exists**, **what mock to replace**.

---

### STEP 1: Authentication

**Pages:** `Login.tsx`, `CourierLogin.tsx`  
**Backend endpoints:**
- `POST /api/admin/auth/login` — NP admin login
- `POST /api/portal/auth/token` — Courier login by email
- `POST /api/portal/auth/access-key` — Courier login by access key

**Service file to update:** `src/lib/api.ts` — add auth interceptors and token storage.

**What to do:**
1. Implement real `POST /api/admin/auth/login` call in `Login.tsx`
2. Store JWT token in `localStorage` or `sessionStorage`
3. Add Axios request interceptor to attach Bearer token
4. Implement `POST /api/portal/auth/token` in `CourierLogin.tsx`
5. Add 401 interceptor that redirects to login

---

### STEP 2: Role Context & Tenant Config

**Files:** `src/context/RoleContext.tsx`, `src/context/TenantConfigContext.tsx`  
**Backend endpoints:**
- `GET /api/admin/auth/me` — returns user profile + role
- `GET /api/admin/settings` — returns tenant settings/feature flags

**What to do:**
1. On login success, call `GET /api/admin/auth/me` and store role in `RoleContext`
2. Call `GET /api/admin/settings` to populate `TenantConfigContext` (controls which sidebar sections are visible)
3. Currently `TenantConfigContext` defaults all flags to `true` — wire this to real API

---

### STEP 3: Fleet / Courier List

**Pages:** `FleetOverview.tsx`, `AddCourier.tsx`, `CourierSetup.tsx`, `FleetManagement.tsx`, `CourierImport.tsx`  
**Backend endpoints:**
- `GET /api/admin/couriers` — list couriers
- `GET /api/admin/couriers/{id}` — get courier by ID
- `GET /api/admin/couriers/masters` — get courier masters (for dropdowns)
- `POST /api/admin/couriers` — create courier
- `PUT /api/admin/couriers/{id}` — update courier
- `GET /api/admin/fleets` — list fleets (TucCourierFleet)
- `GET /api/admin/fleets/depots` — list depots
- `POST /api/admin/couriers/import` — bulk import

**Service to update:** `src/services/np_courierService.ts`, `src/services/np_fleetService.ts`

**What to do:**
1. Replace all `mockCouriers` references with `api.get('/admin/couriers')`
2. Wire `getMasters` to `GET /api/admin/couriers/masters`
3. Wire fleet CRUD to `/api/admin/fleets`
4. Wire import to `POST /api/admin/couriers/import` with file upload

---

### STEP 4: Recruitment Pipeline

**Pages:** `RecruitmentPipeline.tsx`, `ApplicantDetail.tsx`, `RecruitmentStageSettings.tsx`, `RecruitmentAdvertising.tsx`, `PortalUrl.tsx`  
**Backend endpoints:**
- `GET /api/admin/applicants` — list applicants with status filter
- `GET /api/admin/applicants/{id}` — get applicant detail
- `PUT /api/admin/applicants/{id}/status` — update status
- `POST /api/admin/applicants/{id}/advance` — advance to next stage
- `POST /api/admin/applicants/{id}/approve` — approve
- `POST /api/admin/applicants/{id}/reject` — reject
- `GET /api/recruitment/stages` — list stages
- `PUT /api/recruitment/stages/{id}` — update stage
- `POST /api/recruitment` — create job posting
- `GET /api/recruitment/jobs` — list job postings

**Service to update:** `src/services/np_recruitmentService.ts`, `src/services/np_recruitmentSettingsService.ts`

**Wire `advanceStage`, `approveApplicant`, `rejectApplicant`** — these are methods the pages call but the stub service doesn't have. The backend has these as `PUT /api/admin/applicants/{id}/status`.

---

### STEP 5: Compliance

**Pages:** `ComplianceHub.tsx`, `ComplianceProfiles.tsx`, `DriverComplianceTab.tsx`, `DocumentTypeSettings.tsx`  
**Backend endpoints:**
- `GET /api/compliance-profiles` — list compliance profiles
- `POST /api/compliance-profiles` — create profile
- `PUT /api/compliance-profiles/{id}` — update
- `GET /api/driver-approvals` — list pending approvals
- `POST /api/driver-approvals/{id}/approve` — approve driver
- `POST /api/driver-approvals/{id}/reject` — reject driver
- `GET /api/document-types` — list document types
- `POST /api/document-types` — create document type
- `PUT /api/document-types/{id}` — update
- `DELETE /api/document-types/{id}` — delete

**Service to update:** `src/services/np_complianceService.ts`, `src/services/np_complianceProfileService.ts`, `src/services/np_driverApprovalService.ts`, `src/services/np_documentService.ts`

---

### STEP 6: Scheduling

**Pages:** `Scheduling.tsx`  
**Backend endpoints:**
- `GET /api/admin/schedules` — list schedules
- `GET /api/admin/schedules/summary` — summary by date
- `POST /api/admin/schedules` — create schedule
- `DELETE /api/admin/schedules/{id}` — delete

**Service to update:** `src/services/np_schedulingMockData.ts` → replace with `np_schedulingService.ts` calling real endpoints

---

### STEP 7: Training (Quizzes & Flow Builder)

**Pages:** `QuizBuilder.tsx`, `FlowBuilder.tsx`, `DocumentTypeSettings.tsx` (quiz-linked)  
**Backend endpoints:**
- `GET /api/quizzes` — list quizzes
- `POST /api/quizzes` — create
- `PUT /api/quizzes/{id}` — update
- `DELETE /api/quizzes/{id}` — delete
- `POST /api/quizzes/{id}/questions` — add question
- `GET /api/portal-steps` — list portal steps
- `POST /api/portal-steps` — create step
- `PUT /api/portal-steps/reorder` — reorder steps

**Service to update:** `src/services/np_quizService.ts`

---

### STEP 8: Documents & Document Scanning

**Pages:** `DocumentManagement.tsx`, `CourierDocuments.tsx` (mobile), applicant wizard steps  
**Backend endpoints:**
- `GET /api/document-types` — list types
- `POST /api/documentscan` — AI document scanning (uses Anthropic)
- `POST /api/portal/documents` — upload courier document

**Service to update:** `src/services/np_documentService.ts`

**Document Scanning Architecture:**
- `ScanToFill.tsx` (in `src/components/common/`) — mobile camera/upload UI with confidence badges. Currently uses **mock extraction data**. Wire to real `POST /api/documentscan/scan` endpoint.
- `AiScanResult.tsx` (in `src/components/steps/`) — displays extracted fields with confidence indicators (high/medium/low).
- Backend `DocumentScanController.cs` — already in the courier-portal backend. Sends document images to Anthropic Claude Vision API, extracts structured fields (driver license, vehicle rego, insurance, WOF, TSL). Requires `ANTHROPIC_API_KEY` environment variable.
- The `dfrnt-recruitment` Railway app has an identical `DocumentScanController` — that's the working reference. Same code, same prompts.
- **Key wiring task:** Replace the `MOCK_EXTRACTIONS` object in `ScanToFill.tsx` with a real `fetch('/api/documentscan/scan', { method: 'POST', body: formData })` call. The response shape is `{ success: bool, fields: { [key]: { value, confidence } }, fileName, fileSize }`.

---

### STEP 9: Users (NP Team Management)

**Pages:** `Users.tsx`, `UserImport.tsx`  
**Backend endpoints:**
- `GET /api/admin/users` — list users
- `POST /api/admin/users` — invite user
- `DELETE /api/admin/users/{id}` — remove user
- `POST /api/admin/users/import` — bulk import

**Service to update:** `src/services/np_userService.ts`, `src/services/np_userImportService.ts`

---

### STEP 10: Reports

**Pages:** `Reports.tsx` (admin), `PortalReports.tsx` (courier self-service)  
**Backend endpoints:**
- `GET /api/admin/reports/noshows` — no-shows report
- `GET /api/admin/reports` — operational reports
- `GET /api/portal/reports` — courier's own earnings/reports (calls Accounts API internally)

**Service to update:** `src/services/np_reportService.ts`

---

### STEP 11: Applicant Portal (Public)

**Pages:** `src/pages/applicant/ApplicantPortal.tsx` (1,027 LOC — mobile wizard)  
**Backend endpoints (Portal/):**
- `POST /api/portal/applicants/register` — submit application
- `POST /api/portal/applicants/verify-email` — verify email
- `GET /api/portal-steps` — get portal step config for tenant slug
- `GET /api/registration-fields` — get registration fields for tenant
- `POST /api/portal/applicants/{id}/documents` — upload document
- `POST /api/documentscan` — AI scan

**Service to update:** `src/lib/recruitmentApi.ts`

**Important:** The `tenantSlug` from the URL (`/apply/:tenantSlug`) is used to fetch the correct portal configuration. Pass it as a query param or header.

---

### STEP 12: Courier Mobile App

**Pages:** `src/pages/courier/` (10 pages)  
**Backend endpoints (Portal/):**
- `GET /api/portal/couriers/me` — get own profile
- `GET /api/portal/runs` — get own runs
- `GET /api/portal/schedules` — get availability
- `POST /api/portal/schedules` — mark availability
- `GET /api/portal/invoices/recent` — courier's recent invoices
- `GET /api/portal/invoices/past` — past invoices list
- `GET /api/portal/invoices/{invoiceNo}` — view single invoice
- `GET /api/portal/invoices/uninvoiced` — uninvoiced runs available for invoicing
- `POST /api/portal/invoices` — courier creates/submits invoice from selected runs
- `GET /api/portal/contracts` — get own contract file
- `GET /api/portal/vehicles` — get vehicle types
- `GET /api/portal/reports/settings` — get available reports (incl. buyer-created invoice periods)

**Service to update:** Create `src/services/portal_courierService.ts` calls (file exists with placeholders).

#### Invoicing Modes — Controlled by Fleet Setting

Couriers see **different invoicing experiences** depending on their fleet's `UccrInternal` flag on `TucCourier`:

| `UccrInternal` | Mode | What courier sees |
|---|---|---|
| `false` | **Courier-generated invoices** | Courier selects uninvoiced runs → creates/submits their own tax invoice. Uses `InvoiceService.Create()`. This is the standard contractor model. |
| `true` | **Buyer-created tax invoices (BCTI)** | The company (via Accounts app) generates invoices on behalf of the courier. Courier can only **view** these invoices as read-only PDFs. `ReportService.GetSettings()` returns available BCTI periods from `tblBuyerTaxInvoiceControl`. Courier selects a period → downloads the PDF via `ReportService.GetReport()`. |

**Backend code status:**
- `InvoiceService` — filters with `!c.UccrInternal` (only non-internal couriers can create invoices). ✅ Already correct.
- `ReportService.GetSettings()` — has commented-out code to return BCTI periods when `courier.UccrInternal == true` from `TblBuyerTaxInvoiceControls`. 🔧 **Loc needs to uncomment and wire this.**
- `ReportService.GetReport()` — has commented-out `BuyerCreatedTaxInvoice2` report path. 🔧 **Loc needs to uncomment and wire to SSRS or replace with direct DB query.**

**Frontend wiring needed:**
- `CourierInvoicing.tsx` (mobile) and `Invoicing.tsx` (portal) — both already have a read-only "view buyer-created invoices" section. Wire the mode toggle based on courier profile (`UccrInternal` flag from `GET /api/portal/couriers/me`).
- When `UccrInternal=true`: hide "Create Invoice" tab, show period picker → PDF download.
- When `UccrInternal=false`: show "Create Invoice" tab (current behaviour).

**Key tables:**
- `tucCourier.uccrInternal` — the flag (boolean) 
- `tucCourier.CourierFleetId` — FK to fleet. Fleet determines whether couriers are internal.
- `tblBuyerTaxInvoiceControl` — has `Period` column (e.g. `"202603"`) — controls which periods have BCTI PDFs available.

---

### STEP 13: Courier Portal (Desktop self-service)

**Pages:** `src/pages/portal/` (6 pages)  
**Backend endpoints:** Same as Courier Mobile App (Step 12) — these are the desktop equivalents. Same invoicing mode logic applies (BCTI vs courier-generated based on `UccrInternal`).

---

### STEP 14: Settings Pages

**Pages:** `Settings.tsx`, `RegistrationSettings.tsx`, `ContractSettings.tsx`, `CourierPortalLinks.tsx`  
**Backend endpoints:**
- `GET /api/admin/settings` — get settings
- `PUT /api/admin/settings` — update settings
- `GET /api/registration-fields` — get registration fields
- `POST /api/registration-fields` — create field
- `PUT /api/registration-fields/{id}` — update field
- `GET /api/admin/contracts` — list contract templates
- `POST /api/admin/contracts` — create contract
- `GET /api/admin/couriers/portal-links` — get portal link config

**Service to update:** `src/services/np_settingsService.ts`, `src/services/np_contractService.ts`, `src/services/np_recruitmentSettingsService.ts`

---

### STEP 15: Openforce Activity (Recruitment Only)

**Page:** `OpenforceActivity.tsx`  
**Scope:** This page shows **courier onboarding/recruitment status** within Openforce — i.e., is this courier registered as a 1099 contractor? What's their onboarding status?

**Courier-portal owns:**
- Contractor recruitment/onboarding status (is this courier set up in Openforce?)
- Recruitment activity view (who's been onboarded, who's pending)

**Accounts app owns (NOT your scope):**
- Openforce payment processing, settlements, 1099 tax documents
- All financial Openforce API calls (payments, earnings, tax filing)

**Backend endpoints (courier-portal scope only):**
- `GET /api/openforce/recruitment-status` — contractor onboarding status
- `GET /api/openforce/contractors` — list of contractors with recruitment status

**Note:** If the existing `OpenforceController` in the backend has financial endpoints (settings, 1099 data), those should be removed or marked as proxy-to-Accounts. Only the recruitment/onboarding status endpoints belong in courier-portal. See AUDIT.md Section 1 for the full Openforce ownership split.

---

### STEP 16: Dashboard

**Page:** `Dashboard.tsx`  
**Backend endpoint:**
- `GET /api/admin/dashboard` — returns `{ totalCouriers, activeCouriers, pendingApplicants, complianceRate, jobsToday, completed, revenueThisWeek, ... }`

**Service to update:** `src/services/np_dashboardService.ts`

The mock returns a subset of fields. The real endpoint returns more. Check what `DashboardController` returns and update the TypeScript type in `src/types/index.ts`.

---

## Running the Dev Environment

```bash
# Frontend
cd courier-portal
npm install
npm run dev     # Vite dev server at http://localhost:5173

# Backend
cd courier-portal/api
dotnet restore
dotnet run      # API at http://localhost:5000
```

The Vite config should proxy `/api` to `localhost:5000`. If not, add to `vite.config.ts`:

```typescript
server: {
  proxy: {
    '/api': 'http://localhost:5000'
  }
}
```

---

## TypeScript Notes

- All files pass `tsc --noEmit` — verified on 2026-04-12
- All mock services use loose `any` types in places. As you wire real API calls, add proper TypeScript types from the backend DTOs.
- `src/types/index.ts` has the shared type definitions. When backend DTOs differ from what the frontend expects, update `types/index.ts` as the source of truth.

---

## Key Files to Know

| File | Purpose |
|------|---------|
| `src/App.tsx` | Full route tree — all pages mapped |
| `src/context/RoleContext.tsx` | Current user role (np, dfadmin, courier) |
| `src/context/TenantConfigContext.tsx` | Feature flags from tenant settings |
| `src/components/Layout/Sidebar.tsx` | Full sidebar nav (role-aware) |
| `src/lib/api.ts` | Axios base instance — add auth interceptors here |
| `src/services/portal_api.ts` | Axios instance for portal (courier-facing) endpoints |
| `src/lib/tenants.ts` | Tenant config for mobile/applicant portal (brand colours, etc.) |
| `AUDIT.md` | Backend architecture decisions and controller inventory |

---

## What Loc Does NOT Need to Build

- ✅ All React component UI — already done
- ✅ All routing — App.tsx is complete
- ✅ Sidebar navigation — complete
- ✅ Mobile courier app layout (bottom nav) — complete
- ✅ Applicant portal multi-step wizard UI — complete
- ✅ Backend C# controllers — already rebased and correct
- ✅ TypeScript compilation — 0 errors

---

## What Loc DOES Need to Build

- 🔧 Replace mock services with real API calls (Steps 1–16 above)
- 🔧 Register NP Redesign service implementations in `Program.cs` (see AUDIT.md TODOs)
- ✅ `Portal/InvoiceService` — stays as-is (courier creates invoices from uninvoiced runs via DespatchContext)
- 🔧 Fix `Portal/RunService` to extract InvoiceUtility helpers
- 🔧 Auth flow end-to-end (JWT storage + interceptors)
- 🔧 One-time data migration from dfrnt-recruitment PostgreSQL to SQL Server (when ready to retire it)
- 🔧 Configure tenant slug mapping for the applicant portal (which tenantSlug → which tenant DB)
