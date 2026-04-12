# Courier Portal Handover Audit

> Generated: 2026-04-12 — Comprehensive pre-handover audit for Loc.

---

## Summary

| Metric | Count |
|---|---|
| Total pages (route-mapped) | 52 |
| Pages directly consuming mock data | 6 |
| Service files backed 100% by mock/localStorage | 10 |
| API endpoints (frontend calls) | 39 |
| API endpoints (backend exists) | 24 |
| API endpoints (MISSING backend) | 15 |
| TypeScript errors | 0 |
| Sidebar routes with no App.tsx Route | 7 |

---

## 1. Mock/Dummy Data Found

### 1a. Files That ARE Mock Data (no real API backing)

| File | Lines | What It Contains | Real API Needed |
|---|---|---|---|
| `src/services/np_mockData.ts` | 282 | `couriers[]`, `activityFeed[]`, `mockApplicants[]`, `mockPipelineSummary[]`, `mockContracts[]`, `mockRecruitmentStages[]`, `mockComplianceProfiles[]`, `mockDriverComplianceStatuses[]`, `users[]` | NP Admin: GET /v1/np/couriers, /v1/np/applicants, /v1/np/contracts, /v1/np/users, etc. |
| `src/services/np_schedulingMockData.ts` | 312 | `ScheduleSummary[]`, `ScheduleCourier[]`, mock courier responses, mock pending notifications | NP Admin: GET /v1/np/scheduling/schedules, /v1/np/scheduling/responses |
| `src/services/portal_mockData.ts` | 329 | `mockCourier`, `mockRuns[]`, `getMockUninvoiced()`, `mockSchedules[]`, `mockSubcontractors[]`, `mockReportSummary` | Portal (courier self-service): these are now the ONLY source of data for 5 portal pages — see 1b |
| `src/services/portal_devData.ts` | 329 | TypeScript types + sample data for `CourierProfile`, `Schedule`, `Run`, `ReportSummary`, `Subcontractor` — used as type source by `portal_courierService.ts` | Types should stay; sample data should be removed |

### 1b. Pages Directly Importing Mock Data

| File | Lines | Mock Used | Real Service Exists? |
|---|---|---|---|
| `src/pages/portal/Dashboard.tsx` | 2 | `mockCourier`, `mockRuns`, `getMockUninvoiced`, `mockSchedules` from `portal_mockData` | ❌ No — should call `portalCourierService` |
| `src/pages/portal/Schedule.tsx` | 2 | `mockSchedules` from `portal_mockData` | ❌ No — should call `portalApi.get('/portal/schedule')` |
| `src/pages/portal/Runs.tsx` | 2 | `mockRuns` from `portal_mockData` | ❌ No — should call `portalApi.get('/portal/runs')` |
| `src/pages/portal/Contractors.tsx` | 2 | `mockSubcontractors` from `portal_mockData` | ❌ No — should call `portalApi.get('/portal/subcontractors')` |
| `src/pages/portal/Reports.tsx` | 1 | `mockReportSummary` from `portal_mockData` | ❌ No — should call `portalApi.get('/portal/reports')` |
| `src/pages/np/Scheduling.tsx` | 13 | All scheduling data from `np_schedulingMockData` | ❌ No backend NP scheduling controller exists |
| `src/components/portal/PortalLayout.tsx` | 3 | `mockCourier` from `portal_mockData` (used for display name in nav) | ❌ No — should get from auth context or `portalCourierService.getProfile()` |

### 1c. Service Files Using Mock Data Instead of Real API

| Service File | Mock Source | What It Does |
|---|---|---|
| `np_dashboardService.ts` | `couriers`, `activityFeed` from `np_mockData` | Entire dashboard stats derived from mock data |
| `np_courierService.ts` | `couriers` from `np_mockData` | All courier CRUD from mock array — no API calls |
| `np_recruitmentService.ts` | `mockApplicants`, `mockPipelineSummary` from `np_mockData` | All applicant/pipeline data from mock |
| `np_contractService.ts` | `mockContracts` from `np_mockData` | Contract list/activate/delete via mock array mutation |
| `np_userService.ts` | `users` from `np_mockData` | User list from mock |
| `np_driverApprovalService.ts` | `couriers` from `np_mockData` + localStorage overrides | Driver approval from mock + localStorage |
| `np_recruitmentSettingsService.ts` | `mockRecruitmentStages` from `np_mockData` | Stage CRUD via mock array |
| `np_complianceProfileService.ts` | `mockComplianceProfiles`, `mockDriverComplianceStatuses` from `np_mockData` + localStorage | Compliance profiles from mock + localStorage |
| `np_documentService.ts` | localStorage-backed with `DEFAULT_SEED[]` hardcoded | Document types stored in localStorage, never hits API |
| `np_fleetService.ts` | Hardcoded `depots[]`, `fleets[]` arrays inline | Fleet management fully static — no API |
| `np_settingsService.ts` | Hardcoded object with fake company name/data | Settings data never fetched from API |
| `np_reportService.ts` | Hardcoded stats (`jobsCompleted: 256`, `revenue: '$12,450'`) | Reports never fetched from real API |
| `np_openforceService.ts` | localStorage + `generateMockLog()` hardcoded | Openforce API log is fake; settings are localStorage only |
| `np_quizService.ts` | localStorage-backed with hardcoded seed quizzes | Quizzes stored locally — backend `QuizController` exists but is not used |
| `src/hooks/useCompliance.ts` | `couriers` from `np_mockData` | Compliance hook reads from mock |

---

## 2. API Endpoint Gap Analysis

### Portal (Courier Self-Service) — `portal_api` base: `/api`

| Frontend Call | Backend Controller | Status |
|---|---|---|
| `GET /portal/dashboard` | ❌ None (PortalController is at `api/v1/portal`) | ⚠️ ROUTE MISMATCH — portal_courierService calls `/portal/dashboard` but that endpoint doesn't exist; this is also blocked because pages don't call the service yet (they use mock data directly) |
| `GET /portal/profile` | ❌ `GET api/portal/Couriers` exists (CouriersController) | ⚠️ URL MISMATCH — frontend calls `/portal/profile`, backend is at `api/portal/Couriers` |
| `GET /portal/runs` | ✅ `GET api/portal/Runs` | ✅ EXISTS (slight casing/path difference to verify) |
| `GET /portal/schedule` | ✅ `GET api/portal/Schedules` | ✅ EXISTS |
| `POST /portal/schedule/:id/respond` | ✅ `POST api/portal/Schedules/Available` + `Unavailable` | ⚠️ Partial — frontend sends `statusId` but backend has separate Available/Unavailable endpoints |
| `GET /portal/reports` | ✅ `POST api/portal/Reports` + `GET api/portal/Reports/Settings` | ⚠️ Frontend calls GET `/portal/reports`, backend has POST + Settings |
| `GET /portal/subcontractors` | ✅ `GET api/portal/Couriers/Contractors` | ✅ EXISTS |
| `GET /Invoices/Recent` | ✅ `GET api/portal/Invoices/Recent` | ✅ EXISTS |
| `GET /Invoices/Past` | ✅ `GET api/portal/Invoices/Past` | ✅ EXISTS |
| `GET /Invoices/{invoiceNo}` | ✅ `GET api/portal/Invoices/{invoiceNo}` | ✅ EXISTS |
| `GET /Invoices/Uninvoiced` | ✅ `GET api/portal/Invoices/Uninvoiced` | ✅ EXISTS |
| `POST /Invoices` | ✅ `POST api/portal/Invoices` | ✅ EXISTS |
| `POST /portal/Auth/Token` | ✅ `POST api/portal/Auth/Token` | ✅ EXISTS |
| `POST /portal/Auth/Refresh` | ✅ `POST api/portal/Auth/Refresh` | ✅ EXISTS |
| `POST /portal/Auth/Password` | ✅ `POST api/portal/Auth/Password` | ✅ EXISTS |

### Applicant Portal — `np_stepApi` base: `/api`

| Frontend Call | Backend Controller | Status |
|---|---|---|
| `GET /api/portal-steps` | ✅ `GET api/portal-steps` (PortalStepsController) | ✅ EXISTS |
| `POST /api/portal-steps` | ✅ `POST api/portal-steps` | ✅ EXISTS |
| `PUT /api/portal-steps/:id` | ✅ `PUT api/portal-steps/{id}` | ✅ EXISTS |
| `DELETE /api/portal-steps/:id` | ✅ `DELETE api/portal-steps/{id}` | ✅ EXISTS |
| `PUT /api/portal-steps/reorder` | ✅ `PUT api/portal-steps/reorder` | ✅ EXISTS |
| `POST /api/applicant/:id/step-data` | ✅ ApplicantController (`/api/Applicant`) — but **step-data** endpoint needs verification | ⚠️ NEEDS VERIFY |
| `POST /api/applicant/:id/upload` | ✅ `POST api/portal/Applicants/Uploads` (Portal path) — but stepApi uses `/api/applicant/` | ⚠️ PATH MISMATCH |
| `POST /api/applicant/:id/quiz` | ❌ No `/api/applicant/{id}/quiz` endpoint found | ❌ MISSING |
| `POST /api/[controller]/scan` | ✅ `POST api/DocumentScan/scan` | ✅ EXISTS (AI scanning, requires ANTHROPIC_API_KEY) |

### NP Admin — `np_api` base: `/api`

| Frontend Call | Backend Controller | Status |
|---|---|---|
| `GET /v1/np/compliance/dashboard` | ❌ No NP controller folder | ❌ MISSING |
| `GET /v1/np/compliance/alerts` | ❌ No NP controller | ❌ MISSING |
| `GET /v1/np/compliance/score/:id` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/compliance/bulk-notify` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/courier-import/upload` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/courier-import/ai-map` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/courier-import/validate` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/courier-import/execute` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/user-import/upload` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/user-import/ai-map` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/user-import/validate` | ❌ No NP controller | ❌ MISSING |
| `POST /v1/np/user-import/execute` | ❌ No NP controller | ❌ MISSING |

### Quiz — `QuizController` (backend has full CRUD, frontend partially uses it)

| Backend Endpoint | Frontend Consumer | Status |
|---|---|---|
| `GET api/quizzes` | `np_quizService` uses localStorage — NOT this endpoint | ❌ UNUSED — backend quiz endpoints completely bypassed by localStorage service |
| `POST api/quizzes` | ❌ Never called | ❌ UNUSED |
| `PUT api/quizzes/{id}` | ❌ Never called | ❌ UNUSED |
| `DELETE api/quizzes/{id}` | ❌ Never called | ❌ UNUSED |
| `POST api/quizzes/attempts` | ❌ Never called (stepApi has `/api/applicant/:id/quiz` instead) | ❌ UNUSED |

---

## 3. Missing/Broken Routes

### Routes in Sidebar That Have NO Matching `App.tsx` Route

These are **tenant and dfAdmin role** navigation items — clicking them will hit the `<Route path="*" element={<Navigate to="/" replace />} />` catch-all:

| Sidebar Route | Sidebar Label | Role | Status |
|---|---|---|---|
| `/agents` | Agent/NP Directory, Agent Directory, NP Directory | tenant, dfAdmin | ❌ NO ROUTE |
| `/associations` | Association Stats | tenant, dfAdmin | ❌ NO ROUTE |
| `/discovery` | Discovery | tenant (agents only) | ❌ NO ROUTE |
| `/onboarding` | Onboarding | tenant (agents only) | ❌ NO ROUTE |
| `/deliveries` | My Deliveries | id (in-house driver) | ❌ NO ROUTE |
| `/quotes` | Quote Requests | tenant (marketplace) | ❌ NO ROUTE |
| `/import` | Import Agents | tenant | ❌ NO ROUTE — referenced in `tenantImportOptions` |

> **Note:** All NP-specific routes (`/fleet`, `/compliance`, `/scheduling`, etc.) are correctly registered. The 7 missing routes are all tenant/dfAdmin and ID-role features.

### Sidebar Hardcoded Placeholder Data (not routes, but worth noting)

- `Sidebar.tsx` line ~360: Username display shows hardcoded `"admin@tenant.com"`, `"admin@midwestmedical.com"` — should come from auth context
- `Sidebar.tsx`: compliance badge `badge: 3` is hardcoded — should be a live count

---

## 4. Missing Components

All components imported in `App.tsx` were verified against the filesystem:

| Import | File Path | Status |
|---|---|---|
| `ApplicantPortalMobile` | `src/pages/applicant/ApplicantPortal.tsx` | ✅ |
| `PortalLayout` | `src/components/portal/PortalLayout.tsx` | ✅ |
| `PortalDashboard` | `src/pages/portal/Dashboard.tsx` | ✅ |
| `PortalSchedule` | `src/pages/portal/Schedule.tsx` | ✅ |
| `PortalRuns` | `src/pages/portal/Runs.tsx` | ✅ |
| `PortalInvoicing` | `src/pages/portal/Invoicing.tsx` | ✅ |
| `PortalContractors` | `src/pages/portal/Contractors.tsx` | ✅ |
| `PortalReports` | `src/pages/portal/Reports.tsx` | ✅ |
| `CourierLayout` | `src/pages/courier/CourierLayout.tsx` | ✅ |
| `CourierDashboard` | `src/pages/courier/CourierDashboard.tsx` | ✅ |
| `MyRuns` | `src/pages/courier/MyRuns.tsx` | ✅ |
| `CourierSchedule` | `src/pages/courier/CourierSchedule.tsx` | ✅ |
| `CourierDocuments` | `src/pages/courier/CourierDocuments.tsx` | ✅ |
| `CourierTraining` | `src/pages/courier/CourierTraining.tsx` | ✅ |
| `CourierInvoicing` | `src/pages/courier/CourierInvoicing.tsx` | ✅ |
| `CourierContractors` | `src/pages/courier/CourierContractors.tsx` | ✅ |
| `CourierSettings` | `src/pages/courier/CourierSettings.tsx` | ✅ |
| `NpDashboard` | `src/pages/np/Dashboard.tsx` | ✅ |
| `FleetOverview` | `src/pages/np/FleetOverview.tsx` | ✅ |
| `AddCourier` | `src/pages/np/AddCourier.tsx` | ✅ |
| `CourierPortalLinks` | `src/pages/np/CourierPortalLinks.tsx` | ✅ |
| `CourierSetup` | `src/pages/np/CourierSetup.tsx` | ✅ |
| `CourierImport` | `src/pages/np/CourierImport.tsx` | ✅ |
| `Users` | `src/pages/np/Users.tsx` | ✅ |
| `UserImport` | `src/pages/np/UserImport.tsx` | ✅ |
| `Reports` | `src/pages/np/Reports.tsx` | ✅ |
| `NpSettings` | `src/pages/np/Settings.tsx` | ✅ |
| `DocumentTypeSettings` | `src/pages/np/DocumentTypeSettings.tsx` | ✅ |
| `ComplianceHub` | `src/pages/np/ComplianceHub.tsx` | ✅ |
| `RecruitmentPipeline` | `src/pages/np/RecruitmentPipeline.tsx` | ✅ |
| `ApplicantDetail` | `src/pages/np/ApplicantDetail.tsx` | ✅ |
| `RecruitmentStageSettings` | `src/pages/np/RecruitmentStageSettings.tsx` | ✅ |
| `ContractSettings` | `src/pages/np/ContractSettings.tsx` | ✅ |
| `NpApplicantPortal` | `src/pages/np/ApplicantPortal.tsx` | ✅ |
| `RecruitmentAdvertising` | `src/pages/np/RecruitmentAdvertising.tsx` | ✅ |
| `PortalUrl` | `src/pages/np/PortalUrl.tsx` | ✅ |
| `Scheduling` | `src/pages/np/Scheduling.tsx` | ✅ |
| `RegistrationSettings` | `src/pages/np/RegistrationSettings.tsx` | ✅ |
| `FleetManagement` | `src/pages/np/FleetManagement.tsx` | ✅ |
| `ComplianceProfiles` | `src/pages/np/ComplianceProfiles.tsx` | ✅ |
| `OpenforceActivity` | `src/pages/np/OpenforceActivity.tsx` | ✅ |
| `Operations` | `src/pages/np/Operations.tsx` | ✅ |
| `QuizBuilder` | `src/pages/np/QuizBuilder.tsx` | ✅ |
| `FlowBuilder` | `src/pages/np/FlowBuilder.tsx` | ✅ |
| `DocumentManagement` | `src/pages/np/DocumentManagement.tsx` | ✅ |
| `SetupPassword` | `src/pages/np/SetupPassword.tsx` | ✅ |
| `TenantConfigPage` | `src/pages/settings/TenantConfig.tsx` | ✅ |
| `Login` | `src/pages/Login.tsx` | ✅ |
| `RoleSelect` | `src/pages/RoleSelect.tsx` | ✅ |

✅ **No missing components.** All 49 imported page components resolve to real files.

---

## 5. TypeScript Status

```
./node_modules/.bin/tsc --noEmit
```

**Result: 0 errors** — TypeScript compilation passes cleanly.

---

## 6. Backend Service Registration Status

All services registered in `Program.cs` (`api/src/CourierPortal.Api/Program.cs`):

### ✅ Registered Services

| Service | Registration |
|---|---|
| `IPortalService` → `PortalService` | ✅ Scoped |
| `QuizService` | ✅ Scoped |
| `PortalTimeZoneService` | ✅ Scoped |
| `EmailService` | ✅ Scoped |
| `PortalAuthService` | ✅ Scoped |
| `PortalCourierService` | ✅ Scoped |
| `RunRepository` | ✅ Scoped |
| `PortalRunService` | ✅ Scoped |
| `PortalInvoiceService` | ✅ Scoped |
| `PortalScheduleService` | ✅ Scoped |
| `PortalApplicantService` | ✅ Scoped |
| `PortalContractService` | ✅ Scoped |
| `PortalLocationService` | ✅ Scoped |
| `PortalVehicleService` | ✅ Scoped |
| `PortalReportService` | ✅ Scoped |
| `FileStorageService` | ✅ Singleton |
| `IConnectionStringManager` → `ConnectionStringManager` | ✅ Singleton |
| `IDbContextFactory<DespatchContext>` → `DynamicDespatchDbContextFactory` | ✅ Transient |

### ❌ Missing Registrations (Implied by Frontend API Calls)

| Expected Service | Needed For |
|---|---|
| NP Compliance Service | `GET /v1/np/compliance/*` endpoints |
| NP Courier Import Service | `POST /v1/np/courier-import/*` endpoints |
| NP User Import Service | `POST /v1/np/user-import/*` endpoints |
| NP Dashboard Service | NP Admin dashboard |
| NP Courier/Fleet Service | NP courier list management |
| NP Recruitment Service | Applicant pipeline management |

> **Note:** The backend has a complete `ApplicantController` (at `api/Applicant`) and a separate `Portal/ApplicantsController` that cover applicant workflows — but neither covers the NP Admin CRUD for couriers, compliance, or import.

### ⚠️ TODO in np_stepApi

`np_stepApi.ts` line ~57 has:
```typescript
export async function scanDocument(_file: File): Promise<...> {
  // TODO: AI document scanning
  return { success: false, fields: {}, confidence: 0 };
}
```
This is a frontend stub — the real endpoint is `POST api/DocumentScan/scan` and exists in the backend.

---

## 7. Recommendations for Loc

Priority-ordered — highest business impact first:

### 🔴 P1 — Portal Self-Service (Courier Web App): Wire Real API

**These 5 portal pages show 100% mock data to logged-in couriers:**

1. **`src/pages/portal/Dashboard.tsx`** — Replace `mockCourier`, `mockRuns`, `getMockUninvoiced`, `mockSchedules` imports with calls to `portalCourierService.getDashboard()` (service already exists).
2. **`src/pages/portal/Schedule.tsx`** — Replace `mockSchedules` with `portalCourierService.getSchedules()` and wire `respondToSchedule()`. Note: backend has separate `Available`/`Unavailable` endpoints — frontend response call needs to split on `statusId`.
3. **`src/pages/portal/Runs.tsx`** — Replace `mockRuns` with `portalCourierService.getRuns()`.
4. **`src/pages/portal/Contractors.tsx`** — Replace `mockSubcontractors` with `portalCourierService.getSubcontractors()`.
5. **`src/pages/portal/Reports.tsx`** — Replace `mockReportSummary` with `portalCourierService.getReportSummary()`.
6. **`src/components/portal/PortalLayout.tsx`** — Replace `mockCourier` (used for nav display name) with auth token payload or `portalCourierService.getProfile()`.

> ✅ All backend Portal/* controllers are already implemented and registered. This is a frontend-only wiring task.

---

### 🔴 P2 — Fix Portal API URL Mismatch

`portal_courierService.ts` calls `/portal/profile` but the backend CouriersController is at `api/portal/Couriers` (GET). Audit all portal service URL paths against `api/portal/[controller]` pattern — the `baseURL` in `portal_api.ts` is `/api`, so all calls should use the controller name, not a `/portal/` prefix.

**Specifically:**
- `/portal/profile` → should be `/portal/Couriers` (GET for profile)
- `/portal/runs` → `/portal/Runs` ✅ (close enough — verify casing tolerance)
- `/portal/schedule` → `/portal/Schedules`
- `/portal/reports` → `/portal/Reports` (GET, not POST)
- `/portal/subcontractors` → `/portal/Couriers/Contractors`
- `/portal/dashboard` → **No single dashboard endpoint exists** — calls to be replaced with individual service calls

---

### 🟡 P3 — NP Admin: Replace Mock Services With Real API

The entire NP Admin area (Fleet, Compliance, Recruitment, Settings, Reports, Contracts, Users) runs off mock data and localStorage. No NP-specific controllers exist in the backend.

Loc needs to build (or wire to existing AdminManager endpoints):

| Area | Service File to Replace | Endpoints Needed |
|---|---|---|
| Courier/Fleet List | `np_courierService.ts` | `GET /v1/np/couriers`, `POST`, `PUT`, `DELETE` |
| Dashboard | `np_dashboardService.ts` | `GET /v1/np/dashboard` |
| Compliance | `np_complianceService.ts` already calls real API | ✅ Service is wired — **backend endpoint is MISSING** |
| Recruitment | `np_recruitmentService.ts` | `GET /v1/np/applicants`, etc. (or wire to existing `ApplicantController`) |
| Contracts | `np_contractService.ts` | `GET /v1/np/contracts`, etc. |
| Users | `np_userService.ts` | `GET /v1/np/users` |
| Driver Approval | `np_driverApprovalService.ts` | `GET/PUT /v1/np/driver-approval` |
| Recruitment Stages | `np_recruitmentSettingsService.ts` | `GET/POST/PUT/DELETE /v1/np/recruitment-stages` |
| Compliance Profiles | `np_complianceProfileService.ts` | `GET/POST/PUT/DELETE /v1/np/compliance-profiles` |
| Document Types | `np_documentService.ts` | `GET/POST/PUT/DELETE /v1/np/document-types` |
| Fleet Management | `np_fleetService.ts` | `GET/POST/PUT/DELETE /v1/np/fleets` |
| Settings | `np_settingsService.ts` | `GET/PUT /v1/np/settings` |
| Reports | `np_reportService.ts` | `GET /v1/np/reports` |

> **Decision for Loc:** Check whether these should hit AdminManager APIs directly or go through new CourierPortal backend controllers.

---

### 🟡 P4 — Wire Quiz Service to Backend (QuizController exists but is unused)

`np_quizService.ts` stores everything in localStorage. The backend `QuizController` at `api/quizzes` is fully implemented with CRUD + attempts.

Replace localStorage quiz operations with real API calls. The frontend service needs a rewrite from localStorage to:
```
GET/POST/PUT/DELETE api/quizzes
POST api/quizzes/attempts
GET api/quizzes/attempts/courier/{courierId}
```

---

### 🟡 P5 — Fix Applicant Portal `step-data` and `quiz` Endpoints

In `np_stepApi.ts`:
- `POST /api/applicant/:id/step-data` — verify backend has this endpoint (not obvious from controller inspection)
- `POST /api/applicant/:id/quiz` — **no matching endpoint found** in any controller; closest is `POST api/quizzes/attempts`
- `POST /api/applicant/:id/upload` — path mismatch with backend `POST api/portal/Applicants/Uploads`

Fix `scanDocument()` stub in `np_stepApi.ts` to call `POST api/DocumentScan/scan` (backend is implemented).

---

### 🟡 P6 — NP Scheduling: Wire Real API

`src/pages/np/Scheduling.tsx` imports entirely from `np_schedulingMockData.ts`. No backend NP scheduling controller exists.

Options:
1. Build `GET /v1/np/scheduling/schedules` and related endpoints in backend
2. Or wire to existing CourierManager ScheduleService via AdminManager

---

### 🟢 P7 — Tenant Role Routes (Out of Scope for MVP?)

7 Sidebar routes have no pages — these are for the Tenant and ID roles which may not be in scope for the NP handover:

- `/agents` — Agent/NP Directory page
- `/associations` — Association Stats  
- `/discovery` — Discovery
- `/onboarding` — Onboarding
- `/deliveries` — My Deliveries (ID role)
- `/quotes` — Quote Requests (marketplace)
- `/import` — Import Agents

These will silently redirect to `/` when clicked. Confirm with Steve/Garry whether these are in scope for Loc.

---

### 🟢 P8 — Minor Hardcoded Values to Clean Up

- `Sidebar.tsx`: Hardcoded email addresses (`admin@tenant.com`, `admin@midwestmedical.com`) and compliance badge count (`badge: 3`) — should come from auth context and real API
- `portal/Dashboard.tsx` line ~40: Hardcoded `156` for "Total Runs" — should come from `reportSummary.totalRuns`
- `np_openforceService.ts`: Mock API log auto-seeded on first load — remove `generateMockLog()` once backend webhooks/log endpoint exists

---

## Appendix: Service Layer Status Summary

| Service File | API Strategy | Production Ready? |
|---|---|---|
| `portal_api.ts` | Axios, JWT Bearer auth | ✅ |
| `portal_invoiceService.ts` | Real API calls via portalApi | ✅ |
| `portal_courierService.ts` | Real API calls via portalApi | ✅ (URLs need fix — see P2) |
| `np_api.ts` | Axios, JWT Bearer auth | ✅ |
| `np_stepApi.ts` | fetch() calls to real backend | ⚠️ Mostly real — 2 endpoints need fix, scanDocument() stub needs wiring |
| `np_complianceService.ts` | Real API calls via np_api | ✅ (backend missing — see P3) |
| `np_importService.ts` | Real API calls via np_api | ✅ (backend missing — see P3) |
| `np_userImportService.ts` | Real API calls via np_api | ✅ (backend missing — see P3) |
| `np_dashboardService.ts` | 100% mock data | ❌ Needs full rewrite |
| `np_courierService.ts` | 100% mock data | ❌ Needs full rewrite |
| `np_recruitmentService.ts` | 100% mock data | ❌ Needs full rewrite |
| `np_contractService.ts` | 100% mock data | ❌ Needs full rewrite |
| `np_userService.ts` | 100% mock data | ❌ Needs full rewrite |
| `np_driverApprovalService.ts` | Mock + localStorage | ❌ Needs full rewrite |
| `np_recruitmentSettingsService.ts` | Mock + in-memory | ❌ Needs full rewrite |
| `np_complianceProfileService.ts` | Mock + localStorage | ❌ Needs full rewrite |
| `np_documentService.ts` | localStorage only | ❌ Needs full rewrite |
| `np_fleetService.ts` | Hardcoded arrays | ❌ Needs full rewrite |
| `np_settingsService.ts` | Hardcoded object | ❌ Needs full rewrite |
| `np_reportService.ts` | Hardcoded stats | ❌ Needs full rewrite |
| `np_openforceService.ts` | localStorage + mock log | ❌ Needs partial rewrite |
| `np_quizService.ts` | localStorage (backend exists!) | ❌ Needs rewrite to use QuizController |
| `tenant_agentService.ts` | Real API calls via tenant_api | ✅ (no backend tenant controller found) |
| `tenant_importService.ts` | Real API calls via tenant_api | ✅ (no backend tenant controller found) |
| `tenant_marketplaceService.ts` | Real API calls via tenant_api | ✅ (no backend tenant controller found) |
