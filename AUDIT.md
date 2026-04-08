# Courier Portal Duplication Audit

**Date:** 2026-04-08  
**Purpose:** Identify all duplicated functionality between courier-portal, the Accounts app, and the Recruitment app (dfrnt-recruitment) to determine what should be removed from courier-portal.

---

## Architecture Decision (Reference)

| App | Owns |
|-----|------|
| **Accounts** (accounts.dfrnt.com) | Courier invoicing, settlements, deductions, Openforce/1099, fleet listing, contractor management, driver earnings, locations, payments, direct debits, statements |
| **Courier Portal** (courier-portal) | Recruitment/applicants, scheduling, compliance, messaging, document management, quiz/training, courier self-service portal, registration settings, driver approval |
| **Recruitment** (apply.urgent.co.nz / dfrnt-recruitment) | Applicant portal flow, document scanning, quiz system, portal steps |

---

## 1. Controller Overlap Matrix

### Admin Controllers (`/api/admin/...`)

| Controller | Route | Function | In Accounts? | Recommendation |
|---|---|---|---|---|
| **ApplicantsController** | `api/admin/applicants` | Search, get, manage applicants | ❌ No | ✅ **KEEP** — recruitment domain |
| **AuthController** | `api/admin/auth` | Portal access validation, access keys | ❌ No (Accounts has its own auth) | ✅ **KEEP** — portal-specific auth |
| **ContractsController** | `api/admin/contracts` | CRUD courier contracts | ❌ No direct equivalent | ✅ **KEEP** — courier compliance domain |
| **CouriersController** | `api/admin/couriers` | Get courier, types, masters; injects InvoiceService & DeductionService | ⚠️ **ContractorsController** in Accounts covers courier/contractor management | 🔶 **SHARED** — courier profile stays, but invoice/deduction endpoints should call Accounts API |
| **DeductionsController** | `api/admin/deductions` | Get, search, create deductions (one-off & recurring) | ✅ **DeductionsController** in Accounts — identical functionality | ❌ **REMOVE** — fully duplicated in Accounts |
| **FleetsController** | `api/admin/fleets` | GetAll fleets | ✅ **FleetsController** in Accounts — identical GetAll | ❌ **REMOVE** — fully duplicated in Accounts |
| **InfringementsController** | `api/admin/infringements` | Create, list recent, cancel infringements | ❌ No | ✅ **KEEP** — compliance domain |
| **InvoicesController** | `api/admin/invoices` | Get, search, on-hold, batches — full courier invoicing | ✅ **ContractorInvoicesController** in Accounts — identical functionality | ❌ **REMOVE** — fully duplicated in Accounts |
| **LocationsController** | `api/admin/locations` | GetAll, enable/disable locations | ✅ **LocationsController** in Accounts | ❌ **REMOVE** — duplicated in Accounts |
| **MessagesController** | `api/admin/messages` | Get recent, courier messages, send messages | ❌ No | ✅ **KEEP** — messaging domain |
| **OpenforceController** | `api/admin/openforce` | Settings, contractors, 1099 management | ✅ **CourierOpenforceController** in Accounts — identical | ❌ **REMOVE** — fully duplicated in Accounts |
| **ReportsController** | `api/admin/reports` | NoShows, operational reports | ⚠️ Accounts has ReportService but for financial reports | 🔍 **REVIEW** — keep if operational (no-shows, scheduling reports); remove if financial |
| **SchedulesController** | `api/admin/schedules` | Summaries by date, create, delete schedules | ❌ No | ✅ **KEEP** — scheduling domain |
| **SettingsController** | `api/admin/settings` | Get tenant settings | ⚠️ Accounts has SettingsController but for accounting settings | ✅ **KEEP** — portal-specific settings |
| **ValuesController** | `api/admin/values` | Health-check / test endpoint | N/A | ❌ **REMOVE** — boilerplate, not needed |
| **VehiclesController** | `api/admin/vehicles` | Get vehicle types | ❌ No direct equivalent | ✅ **KEEP** — used by scheduling/compliance |

### Portal Controllers (`/api/portal/...`) — Courier self-service

| Controller | Route | Function | In Accounts? | Recommendation |
|---|---|---|---|---|
| **ApplicantsController** | `api/portal/applicants` | Register, email verification, applicant self-service | ❌ No | ✅ **KEEP** — recruitment portal |
| **AuthController** | `api/portal/auth` | Token, AccessKey login for couriers | ❌ No (portal-specific) | ✅ **KEEP** |
| **BaseController** | N/A | Shared base class | N/A | ✅ **KEEP** |
| **ContractsController** | `api/portal/contracts` | Get current contract file | ❌ No | ✅ **KEEP** — courier self-service |
| **CouriersController** | `api/portal/couriers` | Courier view/update own profile | ❌ No | ✅ **KEEP** — courier self-service |
| **InvoicesController** | `api/portal/invoices` | Courier views their own recent/past invoices | ⚠️ No portal-facing equivalent in Accounts | 🔶 **SHARED** — keep the portal endpoint but back it with Accounts API data |
| **LocationsController** | `api/portal/locations` | Get all locations (for applicant forms) | ✅ Accounts has this | 🔶 **SHARED** — could call Accounts API, or keep if minimal |
| **RecaptchaController** | `api/portal/recaptcha` | Google recaptcha site key | ❌ No | ✅ **KEEP** |
| **ReportsController** | `api/portal/reports` | Courier's own earnings/reports | ⚠️ Accounts has DriverEarningsService | 🔶 **SHARED** — data should come from Accounts API |
| **RunsController** | `api/portal/runs` | Courier views their scheduled runs | ❌ No | ✅ **KEEP** — scheduling domain |
| **SchedulesController** | `api/portal/schedules` | Courier views/marks availability | ❌ No | ✅ **KEEP** — scheduling domain |
| **ValuesController** | `api/portal/values` | Health-check / test endpoint | N/A | ❌ **REMOVE** — boilerplate |
| **VehiclesController** | `api/portal/vehicles` | Get vehicle types | ❌ No | ✅ **KEEP** |

### Root Controllers (`/api/...`) — New features

| Controller | Route | Function | In Accounts? | In Recruitment? | Recommendation |
|---|---|---|---|---|---|
| **ComplianceController** | `api/compliance-profiles` | CRUD compliance profiles & requirements | ❌ | ❌ | ✅ **KEEP** — new feature, courier-portal domain |
| **DocumentScanController** | `api/documentscan` | AI document scanning (Anthropic) | ❌ | ✅ Same concept | 🔶 **REVIEW** — exists in dfrnt-recruitment too; decide canonical home |
| **DocumentTypesController** | `api/document-types` | CRUD document types | ❌ | ✅ DocumentController | 🔶 **REVIEW** — exists in dfrnt-recruitment; decide canonical home |
| **DriverApprovalsController** | `api/driver-approvals` | Pending approvals, review workflow | ❌ | ❌ | ✅ **KEEP** — new feature, courier-portal domain |
| **MessengerController** | `api/messenger` | Conversations, messages, real-time messaging | ❌ | ❌ | ✅ **KEEP** — new feature, courier-portal domain |
| **PortalStepsController** | `api/portal-steps` | CRUD portal steps, reorder | ❌ | ✅ PortalStepsController | 🔶 **REVIEW** — duplicated with dfrnt-recruitment |
| **QuizController** | `api/quizzes` | CRUD quizzes, questions, attempts | ❌ | ✅ QuizController | 🔶 **REVIEW** — duplicated with dfrnt-recruitment |
| **RecruitmentController** | `api/recruitment` | Job postings, applications | ❌ | ✅ RecruitmentController | 🔶 **REVIEW** — duplicated with dfrnt-recruitment |
| **RegistrationFieldsController** | `api/registration-fields` | CRUD registration fields & options | ❌ | ⚠️ SettingsController handles some | ✅ **KEEP** — more complete than recruitment version |

---

## 2. Service Overlap

### Admin Services — REMOVE (duplicated in Accounts)

| Courier Portal Service | Accounts Equivalent | Action |
|---|---|---|
| `DeductionService` | `DeductionService` | ❌ **REMOVE** |
| `FleetService` | `FleetService` | ❌ **REMOVE** |
| `InvoiceService` | `ContractorInvoiceService` | ❌ **REMOVE** |
| `InvoiceBatchService` | `ContractorInvoiceBatchService` | ❌ **REMOVE** |
| `OpenforceService` | `OpenforceService` | ❌ **REMOVE** |
| `LocationService` (Admin) | `DespatchService` (handles locations) | ❌ **REMOVE** |

### Admin Services — KEEP (courier-portal domain)

| Service | Purpose |
|---|---|
| `ApplicantsService` | Applicant management |
| `AuthService` (Admin) | Portal access validation |
| `ContractsService` | Courier contracts |
| `CouriersService` | Courier profiles (⚠️ remove invoice/deduction dependencies) |
| `InfringementsService` | Compliance infringements |
| `MessageService` | Messaging system |
| `ReportsService` | Operational reports (no-shows etc.) |
| `ScheduleService` | Schedule management |
| `SettingsService` | Portal settings |
| `VehicleService` | Vehicle types |

### Portal Services — KEEP (courier self-service)

| Service | Purpose | Note |
|---|---|---|
| `ApplicantService` | Registration, self-service | ✅ KEEP |
| `AuthService` (Portal) | Courier JWT login | ✅ KEEP |
| `ContractService` | View contracts | ✅ KEEP |
| `CourierService` | View/update own profile | ✅ KEEP |
| `InvoiceService` | View own invoices | 🔶 KEEP endpoint, but should proxy to Accounts API |
| `LocationService` | Get locations for forms | 🔶 Could proxy to Accounts |
| `ReportService` | Courier earnings report | 🔶 Should proxy to Accounts (DriverEarningsService) |
| `RunService` | View assigned runs | ✅ KEEP |
| `ScheduleService` | View/mark availability | ✅ KEEP |

### Root Services — KEEP (new features)

| Service | Purpose |
|---|---|
| `ComplianceService` | Compliance profiles |
| `DocumentTypeService` | Document type management |
| `DriverApprovalService` | Driver approval workflow |
| `MessengerService` | Messaging conversations |
| `QuizService` | Quiz management (⚠️ overlaps recruitment) |
| `RecruitmentService` | Job postings (⚠️ overlaps recruitment) |
| `RegistrationFieldService` | Registration field config |

---

## 3. Entity Overlap

### Entities duplicated in BOTH courier-portal AND Accounts

| Entity | Courier Portal Path | Accounts Path | Action |
|---|---|---|---|
| `CourierDeduction` | `Domain/Entities/CourierDeduction.cs` | `Core/Domain/Despatch/CourierDeduction.cs` | ❌ **REMOVE from courier-portal** |
| `CourierDeductionLine` | `Domain/Entities/CourierDeductionLine.cs` | `Core/Domain/Despatch/CourierDeductionLine.cs` | ❌ **REMOVE** |
| `CourierDeductionRecurring` | `Domain/Entities/CourierDeductionRecurring.cs` | `Core/Domain/Despatch/CourierDeductionRecurring.cs` | ❌ **REMOVE** |
| `CourierDeductionRecurringLine` | `Domain/Entities/CourierDeductionRecurringLine.cs` | `Core/Domain/Despatch/CourierDeductionRecurringLine.cs` | ❌ **REMOVE** |
| `CourierDeductionType` | `Domain/Entities/CourierDeductionType.cs` | `Core/Domain/Despatch/CourierDeductionType.cs` | ❌ **REMOVE** |
| `CourierInvoice` | `Domain/Entities/CourierInvoice.cs` | `Core/Domain/Despatch/CourierInvoice.cs` | ❌ **REMOVE** |
| `CourierInvoiceBatch` | `Domain/Entities/CourierInvoiceBatch.cs` | `Core/Domain/Despatch/CourierInvoiceBatch.cs` | ❌ **REMOVE** |
| `CourierInvoiceBatchItem` | `Domain/Entities/CourierInvoiceBatchItem.cs` | `Core/Domain/Despatch/CourierInvoiceBatchItem.cs` | ❌ **REMOVE** |
| `CourierInvoiceBatchStatus` | `Domain/Entities/CourierInvoiceBatchStatus.cs` | `Core/Domain/Despatch/CourierInvoiceBatchStatus.cs` | ❌ **REMOVE** |
| `CourierInvoiceLine` | `Domain/Entities/CourierInvoiceLine.cs` | `Core/Domain/Despatch/CourierInvoiceLine.cs` | ❌ **REMOVE** |
| `CourierInvoiceLineJob` | `Domain/Entities/CourierInvoiceLineJob.cs` | `Core/Domain/Despatch/CourierInvoiceLineJob.cs` | ❌ **REMOVE** |
| `CourierInvoiceLineJobMaster` | `Domain/Entities/CourierInvoiceLineJobMaster.cs` | `Core/Domain/Despatch/CourierInvoiceLineJobMaster.cs` | ❌ **REMOVE** |
| `CourierType` | `Domain/Entities/CourierType.cs` | `Core/Domain/Despatch/CourierType.cs` | ❌ **REMOVE** |
| `TucCourier` | `Domain/Entities/TucCourier.cs` | `Core/Domain/Despatch/TucCourier.cs` | 🔶 **SHARED** — portal needs courier read access; consider read-only view or API call |
| `TucCourierFleet` | `Domain/Entities/TucCourierFleet.cs` | `Core/Domain/Despatch/TucCourierFleet.cs` | ❌ **REMOVE** |
| `TucClient` | `Domain/Entities/TucClient.cs` | `Core/Domain/Despatch/TucClient.cs` | ❌ **REMOVE** |
| `TblJob` | `Domain/Entities/TblJob.cs` | `Core/Domain/Despatch/TblJob.cs` | ❌ **REMOVE** |
| `TblBulkRegion` | `Domain/Entities/TblBulkRegion.cs` | `Core/Domain/Despatch/TblBulkRegion.cs` | ❌ **REMOVE** |
| `TblReport` | `Domain/Entities/TblReport.cs` | `Core/Domain/Despatch/TblReport.cs` | ❌ **REMOVE** |
| `TblSetting` | `Domain/Entities/TblSetting.cs` | `Core/Domain/Despatch/TblSetting.cs` | 🔶 **SHARED** — may need for portal settings; review |
| `PaymentTerm` | `Domain/Entities/PaymentTerm.cs` | `Core/Domain/Despatch/PaymentTerm.cs` | ❌ **REMOVE** |
| `VehicleType` | `Domain/Entities/VehicleType.cs` | (not found but likely shared) | 🔍 **REVIEW** |
| `TblBuyerTaxInvoiceControl` | `Domain/Entities/TblBuyerTaxInvoiceControl.cs` | (invoicing domain) | ❌ **REMOVE** — invoicing entity |

### Banking/Payment domain files (REMOVE — belongs in Accounts)

| File | Action |
|---|---|
| `Domain/Anz/AnzDomestic.cs` | ❌ **REMOVE** |
| `Domain/Anz/AnzDomesticLine.cs` | ❌ **REMOVE** |
| `Domain/Bnz/BnzPayroll.cs` | ❌ **REMOVE** |
| `Domain/Bnz/BnzPayrollTransaction.cs` | ❌ **REMOVE** |
| `Domain/Ird/PaydayFiling/*` (6 files) | ❌ **REMOVE** — IRD/tax domain belongs in Accounts |

### Entities unique to courier-portal (KEEP)

| Entity | Purpose |
|---|---|
| `CourierApplicant` | Applicant records |
| `CourierApplicantDocument` | Applicant document uploads |
| `CourierApplicantUpload` | Upload tracking |
| `ApplicantStepData` | Portal step completion data |
| `CourierAvailability` | Schedule availability |
| `CourierAvailabilityExclude` | Excluded dates |
| `CourierContract` | Contract management |
| `CourierSchedule` + related | Schedule entities |
| `ComplianceProfile` | Compliance profiles |
| `ComplianceProfileRequirement` | Compliance requirements |
| `Conversation` / `ConversationMessage` | Messaging |
| `DocumentType` | Document type config |
| `DriverApproval` | Driver approval workflow |
| `Infringement` + related | Infringement tracking |
| `JobPosting` / `JobApplication` | Recruitment postings |
| `PortalStep` | Portal step config |
| `Quiz` / `QuizQuestion` / `QuizOption` / `QuizAttempt` / `QuizAttemptAnswer` | Training/quiz system |
| `RecruitmentNote` / `RecruitmentStage` | Recruitment pipeline |
| `RegistrationField` / `RegistrationFieldOption` | Registration config |
| `TenantSettings` | Portal tenant settings |
| `AdminUser` | Portal admin users |
| `TucManualMessage` | Manual messaging |

---

## 4. Frontend Pages

### `/src/pages/np/` (Admin panel pages)

| Page | Function | Recommendation |
|---|---|---|
| `Dashboard.tsx` | Admin dashboard | ✅ **KEEP** |
| `AddCourier.tsx` | Add new courier | ✅ **KEEP** |
| `ApplicantDetail.tsx` | View applicant details | ✅ **KEEP** — recruitment |
| `ApplicantPortal.tsx` | Applicant portal config | ✅ **KEEP** — recruitment |
| `ComplianceDashboard.tsx` | Compliance overview | ✅ **KEEP** — compliance |
| `ComplianceHub.tsx` | Compliance management | ✅ **KEEP** — compliance |
| `ComplianceProfiles.tsx` | Manage compliance profiles | ✅ **KEEP** — compliance |
| `ContractSettings.tsx` | Contract template settings | ✅ **KEEP** |
| `CourierImport.tsx` | Bulk import couriers | ✅ **KEEP** |
| `CourierPortalLinks.tsx` | Portal link management | ✅ **KEEP** |
| `CourierSetup.tsx` | Courier onboarding setup | ✅ **KEEP** |
| `DocumentTypeSettings.tsx` | Document type configuration | ✅ **KEEP** |
| `DriverComplianceTab.tsx` | Driver compliance details | ✅ **KEEP** — compliance |
| `FleetManagement.tsx` | Fleet management | ❌ **REMOVE** — fleet listing is in Accounts |
| `FleetOverview.tsx` | Fleet overview/dashboard | ❌ **REMOVE** — fleet is in Accounts |
| `FlowBuilder.tsx` | Portal flow builder | ✅ **KEEP** — recruitment setup |
| `OpenforceActivity.tsx` | Openforce activity view | ❌ **REMOVE** — Openforce is in Accounts |
| `Operations.tsx` | Operations dashboard | 🔍 **REVIEW** — may overlap with Accounts if it shows financial data |
| `PortalUrl.tsx` | Portal URL configuration | ✅ **KEEP** |
| `QuizBuilder.tsx` | Create/edit quizzes | ✅ **KEEP** — training domain |
| `QuizPlayer.tsx` | Take quizzes | ✅ **KEEP** — training domain |
| `RecruitmentAdvertising.tsx` | Job advertising | ✅ **KEEP** — recruitment |
| `RecruitmentPipeline.tsx` | Recruitment pipeline view | ✅ **KEEP** — recruitment |
| `RecruitmentStageSettings.tsx` | Configure recruitment stages | ✅ **KEEP** — recruitment |
| `RegistrationSettings.tsx` | Registration form config | ✅ **KEEP** |
| `Reports.tsx` | Reports page | 🔍 **REVIEW** — keep if operational; remove if financial |
| `Scheduling.tsx` | Schedule management | ✅ **KEEP** |
| `Settings.tsx` | Portal settings | ✅ **KEEP** |
| `SetupPassword.tsx` | Password setup | ✅ **KEEP** |
| `UserImport.tsx` | Bulk user import | ✅ **KEEP** |
| `Users.tsx` | User management | ✅ **KEEP** |

### `/src/pages/courier/` (Courier self-service)

| Page | Function | Recommendation |
|---|---|---|
| `CourierTraining.tsx` | Courier training/quiz | ✅ **KEEP** |

### Summary: Pages to REMOVE

| File | Reason |
|---|---|
| `src/pages/np/FleetManagement.tsx` | Fleet managed by Accounts |
| `src/pages/np/FleetOverview.tsx` | Fleet managed by Accounts |
| `src/pages/np/OpenforceActivity.tsx` | Openforce managed by Accounts |

---

## 5. Recruitment App (dfrnt-recruitment) Overlap

### Controller-level comparison

| Feature | Courier Portal | dfrnt-recruitment | Which is more complete? | Recommendation |
|---|---|---|---|---|
| **Applicant CRUD** | `Admin/ApplicantsController` + `Portal/ApplicantsController` | `ApplicantController` | **Courier Portal** — has both admin and self-service flows, plus legacy integration | Courier Portal is canonical |
| **Document scanning** | `DocumentScanController` | `DocumentScanController` | **Equivalent** — both use Anthropic AI | Keep courier-portal version; retirement candidate for recruitment app |
| **Document types** | `DocumentTypesController` | `DocumentController` | **Courier Portal** — cleaner CRUD interface | Keep courier-portal |
| **Portal steps** | `PortalStepsController` | `PortalStepsController` | **Equivalent** — nearly identical code | Keep courier-portal version |
| **Quizzes** | `QuizController` | `QuizController` | **Courier Portal** — has quiz attempts, answers, full lifecycle | Keep courier-portal |
| **Recruitment** | `RecruitmentController` | `RecruitmentController` | **Courier Portal** — has job postings + applications | Keep courier-portal |
| **Settings** | `SettingsController` | `SettingsController` | **Different scope** — courier-portal settings are broader | Keep both (different purpose) |
| **Auth** | `Portal/AuthController` | `AuthController` | **Courier Portal** — has JWT + AccessKey flows | Keep courier-portal |

### Entity-level comparison

| Entity | Courier Portal | dfrnt-recruitment | Notes |
|---|---|---|---|
| Applicant | `CourierApplicant` | `Applicant` | Different schema — courier portal has richer legacy fields |
| Documents | `CourierApplicantDocument` | `ApplicantDocument` | Similar concept, different structure |
| Step data | `ApplicantStepData` | `ApplicantStepData` | Nearly identical |
| Portal steps | `PortalStep` | `PortalStep` | Nearly identical |
| Quiz | `Quiz` + `QuizQuestion` + `QuizOption` | `Quiz` + `QuizQuestion` + `QuizAnswer` | Similar; courier-portal has `QuizAttempt` + `QuizAttemptAnswer` |
| Recruitment | `RecruitmentNote` + `RecruitmentStage` + `JobPosting` + `JobApplication` | `RecruitmentNote` + `RecruitmentStage` | Courier-portal has more entities |
| Document types | `DocumentType` | `DocumentType` | Identical concept |
| Admin users | `AdminUser` | `AdminUser` | Both exist |
| Tenant settings | `TenantSettings` | `TenantSettings` | Both exist |

### Verdict: **Courier Portal is the canonical version**

The dfrnt-recruitment app was a clean-room rebuild for the applicant-facing portal (apply.urgent.co.nz). The courier-portal version is more complete with:
- Full admin + portal (self-service) flows
- Legacy Despatch DB integration
- Richer entity relationships
- Job postings and applications

**Recommendation:** The dfrnt-recruitment app should be considered a **subset** used only for the public-facing applicant portal. Long-term, consolidate into courier-portal as the single source of truth for recruitment, with dfrnt-recruitment either being retired or kept as a thin frontend-only shell that calls courier-portal APIs.

---

## 6. Files to REMOVE — Complete List

### Controllers (6 files)
```
api/src/CourierPortal.Api/Controllers/Admin/DeductionsController.cs
api/src/CourierPortal.Api/Controllers/Admin/FleetsController.cs
api/src/CourierPortal.Api/Controllers/Admin/InvoicesController.cs
api/src/CourierPortal.Api/Controllers/Admin/LocationsController.cs
api/src/CourierPortal.Api/Controllers/Admin/OpenforceController.cs
api/src/CourierPortal.Api/Controllers/Admin/ValuesController.cs
api/src/CourierPortal.Api/Controllers/Portal/ValuesController.cs
```

### Services (7 files)
```
api/src/CourierPortal.Core/Services/Admin/DeductionService.cs
api/src/CourierPortal.Core/Services/Admin/FleetService.cs
api/src/CourierPortal.Core/Services/Admin/InvoiceService.cs
api/src/CourierPortal.Core/Services/Admin/InvoiceBatchService.cs
api/src/CourierPortal.Core/Services/Admin/LocationService.cs
api/src/CourierPortal.Core/Services/Admin/OpenforceService.cs
```

### Entities (25+ files)
```
api/src/CourierPortal.Core/Domain/Entities/CourierDeduction.cs
api/src/CourierPortal.Core/Domain/Entities/CourierDeductionLine.cs
api/src/CourierPortal.Core/Domain/Entities/CourierDeductionRecurring.cs
api/src/CourierPortal.Core/Domain/Entities/CourierDeductionRecurringLine.cs
api/src/CourierPortal.Core/Domain/Entities/CourierDeductionType.cs
api/src/CourierPortal.Core/Domain/Entities/CourierInvoice.cs
api/src/CourierPortal.Core/Domain/Entities/CourierInvoiceBatch.cs
api/src/CourierPortal.Core/Domain/Entities/CourierInvoiceBatchItem.cs
api/src/CourierPortal.Core/Domain/Entities/CourierInvoiceBatchStatus.cs
api/src/CourierPortal.Core/Domain/Entities/CourierInvoiceLine.cs
api/src/CourierPortal.Core/Domain/Entities/CourierInvoiceLineJob.cs
api/src/CourierPortal.Core/Domain/Entities/CourierInvoiceLineJobMaster.cs
api/src/CourierPortal.Core/Domain/Entities/CourierType.cs
api/src/CourierPortal.Core/Domain/Entities/TucCourierFleet.cs
api/src/CourierPortal.Core/Domain/Entities/TucClient.cs
api/src/CourierPortal.Core/Domain/Entities/TblJob.cs
api/src/CourierPortal.Core/Domain/Entities/TblBulkRegion.cs
api/src/CourierPortal.Core/Domain/Entities/TblReport.cs
api/src/CourierPortal.Core/Domain/Entities/PaymentTerm.cs
api/src/CourierPortal.Core/Domain/Entities/TblBuyerTaxInvoiceControl.cs
api/src/CourierPortal.Core/Domain/Anz/AnzDomestic.cs
api/src/CourierPortal.Core/Domain/Anz/AnzDomesticLine.cs
api/src/CourierPortal.Core/Domain/Bnz/BnzPayroll.cs
api/src/CourierPortal.Core/Domain/Bnz/BnzPayrollTransaction.cs
api/src/CourierPortal.Core/Domain/Ird/PaydayFiling/Ded.cs
api/src/CourierPortal.Core/Domain/Ird/PaydayFiling/Dei.cs
api/src/CourierPortal.Core/Domain/Ird/PaydayFiling/EmployeeDetails.cs
api/src/CourierPortal.Core/Domain/Ird/PaydayFiling/EmployeeDetailsLine.cs
api/src/CourierPortal.Core/Domain/Ird/PaydayFiling/EmployeeInformation.cs
api/src/CourierPortal.Core/Domain/Ird/PaydayFiling/Ted.cs
```

### Frontend (3 files)
```
src/pages/np/FleetManagement.tsx
src/pages/np/FleetOverview.tsx
src/pages/np/OpenforceActivity.tsx
```

### DTOs to review for removal
```
api/src/CourierPortal.Core/DTOs/Admin/Deductions/     (entire folder)
api/src/CourierPortal.Core/DTOs/Admin/Invoices/        (entire folder)
api/src/CourierPortal.Core/DTOs/Admin/Openforce/       (entire folder)
```

---

## 7. Gaps — Things Courier Portal Needs That Aren't Covered

| Gap | Description | Recommendation |
|---|---|---|
| **Courier self-service invoices** | Portal/InvoicesController lets couriers view their invoices, but if we remove invoice entities we need an API bridge | Create an API client service that calls Accounts API for courier invoice data |
| **Courier self-service earnings** | Portal/ReportsController generates earnings reports from local data | Create an API client that calls Accounts `DriverEarningsService` |
| **Location data for applicant forms** | Portal/LocationsController serves location data for registration | Either proxy to Accounts API or maintain a lightweight read-only cache |
| **Courier profile financial data** | Admin/CouriersController currently injects InvoiceService and DeductionService | Refactor to remove financial service dependencies; fetch via Accounts API when needed |
| **DespatchContext** | `DespatchContext.cs` is the legacy DB context used by invoice/deduction services — but may also be used by scheduling/runs | Audit DespatchContext usage; keep for scheduling/runs, remove invoice-related DbSets |

---

## 8. Architecture Diagram — Final State

```
┌─────────────────────────────────────────────────────────────┐
│                     ACCOUNTS APP                             │
│                  (accounts.dfrnt.com)                         │
│                                                              │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │  Invoicing    │ │  Settlements │ │  Deductions  │         │
│  │  (Contractor  │ │  (Batches,   │ │  (One-off,   │         │
│  │   Invoices)   │ │   Summaries) │ │   Recurring) │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │  Openforce   │ │  Fleet       │ │  Contractors │         │
│  │  (1099/NZ)   │ │  (Listing)   │ │  (Management)│         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │  Locations   │ │  Driver      │ │  Direct      │         │
│  │              │ │  Earnings    │ │  Debits      │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐ ┌──────────────┐                          │
│  │  Statements  │ │  Payments    │   ← Financial domain     │
│  └──────────────┘ └──────────────┘                          │
└──────────────────────────┬──────────────────────────────────┘
                           │ API calls (for courier invoice
                           │ views and earnings reports)
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   COURIER PORTAL                             │
│                (courier-portal app)                           │
│                                                              │
│  ADMIN SIDE (np/):                                           │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │ Recruitment  │ │  Scheduling  │ │  Compliance  │         │
│  │ (Pipeline,   │ │  (Shifts,    │ │  (Profiles,  │         │
│  │  Applicants, │ │   Summaries) │ │   Requirements│        │
│  │  Stages)     │ │              │ │   Infringements│       │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │  Messaging   │ │  Documents   │ │  Training    │         │
│  │  (Messenger, │ │  (Types,     │ │  (Quizzes,   │         │
│  │   Messages)  │ │   Scanning)  │ │   Builder)   │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │  Contracts   │ │  Driver      │ │  Registration│         │
│  │  (Templates) │ │  Approvals   │ │  (Settings,  │         │
│  └──────────────┘ └──────────────┘ │   Fields)    │         │
│                                     └──────────────┘         │
│  PORTAL SIDE (courier self-service):                         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │ Profile      │ │  Schedules   │ │  Runs        │         │
│  │ (View/Edit)  │ │  (Avail.)    │ │  (View)      │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │ Invoices*    │ │  Reports*    │ │  Contracts   │         │
│  │ (via Accts)  │ │  (via Accts) │ │  (View)      │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐                                            │
│  │ Training     │    * = proxied from Accounts API           │
│  │ (Quizzes)    │                                            │
│  └──────────────┘                                            │
└──────────────────────────┬──────────────────────────────────┘
                           │ Applicant-facing portal
                           │ (may retire or keep as thin shell)
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  RECRUITMENT APP                             │
│              (apply.urgent.co.nz)                            │
│              (dfrnt-recruitment)                              │
│                                                              │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │  Applicant   │ │  Document    │ │  Quiz        │         │
│  │  Portal      │ │  Upload/Scan │ │  Taking      │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐                                            │
│  │  Portal      │  ← Thin applicant-facing frontend          │
│  │  Steps       │    Could be retired in favor of             │
│  └──────────────┘    courier-portal's Portal/ endpoints       │
└─────────────────────────────────────────────────────────────┘
```

---

## 9. Migration Priority

### Phase 1: Remove clear duplicates (low risk)
1. Delete fleet, deduction, invoice, openforce, location admin controllers + services + entities
2. Delete banking/IRD entity files
3. Delete frontend fleet/openforce pages
4. Remove corresponding DTOs

### Phase 2: Create Accounts API bridge (medium risk)
1. Build an `AccountsApiClient` service in courier-portal
2. Refactor `Portal/InvoicesController` to proxy to Accounts
3. Refactor `Portal/ReportsController` earnings to proxy to Accounts
4. Refactor `Admin/CouriersController` to remove invoice/deduction dependencies

### Phase 3: Decide recruitment app fate (strategic decision)
1. If keeping dfrnt-recruitment as separate app → ensure it calls courier-portal APIs
2. If retiring → migrate any unique applicant-portal UI features into courier-portal
3. Ensure courier-portal's `Portal/ApplicantsController` covers all public-facing flows

### Phase 4: Clean up DespatchContext
1. Audit all DespatchContext usages
2. Remove DbSets for deleted entities
3. Keep only the tables needed for scheduling, runs, and courier profiles

---

## 10. Risk Notes

- **DespatchContext shared access:** Both courier-portal and Accounts read from the same underlying Despatch database. Removing entities from courier-portal doesn't remove the tables — it just stops courier-portal from accessing them directly. This is the desired outcome.
- **Admin/CouriersController coupling:** This controller currently injects `AdminInvoiceService` and `DeductionService`. These dependencies must be removed or replaced with API calls before deleting the invoice/deduction services.
- **TucCourier entity:** Widely used across scheduling, messaging, and applicant features. Cannot be removed even though it's duplicated in Accounts. Keep as read-only in courier-portal.
- **TblSetting entity:** Used by SettingsService for portal configuration. Review whether it overlaps with Accounts settings or is portal-specific before removing.

---

## NP Redesign Rebase — 2026-04-08

### What Was Done

This section documents the re-basing of the courier-portal backend onto the NP Redesign codebase, removing duplication with the Accounts app.

### Step 1: Duplication Analysis

**NP Redesign controllers checked against Accounts app:**
- None of the 23 NP Redesign controllers duplicate Accounts functionality
- NP Redesign has NO invoice, deduction, openforce, settlement, or location-listing controllers
- The NP FleetController has full CRUD + courier assignment (operational), while Accounts FleetsController only has GetAll (financial listing) — these are complementary, not duplicated
- All duplication with Accounts existed in the OLD Admin/ folder from GitLab, not in NP Redesign

### Step 2: NP Redesign Backend Copied Into Courier-Portal

**23 controllers copied** from `DfrntAgentsPartners.Api/Controllers/`:
- AgentController, AgentImportController, AgentVehicleRateController
- ComplianceAutomationController, ComplianceDashboardController
- ContractController, CourierDocumentController, CourierImportController
- DocumentTypeController, FleetController, MarketplaceController
- MessengerController (replaced old version), NpCourierController
- NpDashboardController, NpReportController, NpSettingsController
- NpUserController, PortalController, RecruitmentController (replaced old version)
- RecruitmentSettingsController, SchedulingController, TrainingController, UserImportController

**Models copied** (22 entities → `Domain/Entities/`):
- Agent, AgentCourierRate, AgentOnboarding, AgentVehicleRate
- ComplianceAutomationConfig, ComplianceNotification
- CourierApplicant (NP version), CourierContract, CourierDocument, CourierDocumentAudit
- CourierDocumentType, CourierMessage, CourierSchedule
- MarketplacePosting, MarketplaceQuote, NpCourier, NpFeatureConfig, NpUser
- ProspectAgent, RecruitmentStageConfig, TrainingItem, TucCourierFleet

**DTOs copied** (14 files):
- AgentDtos, AgentImportDtos, ApplicantDtos, ComplianceDtos, CourierImportDtos
- DocumentDtos, MarketplaceDtos, MaskedJobDto, MessengerDtos (replaced old)
- NpCourierDtos, NpDashboardDtos, NpUserDtos, PortalDtos, UserImportDtos

**Interfaces copied** (13):
- IAgentImportService, IComplianceDashboardService, ICourierImportService
- IDocumentServices, IFleetService, IMessengerService (replaced old)
- INpSettingsService, IPortalService, IRecruitmentService (replaced old)
- ISchedulingService, IServices, ITrainingService, IUserImportService

**Middleware copied** (2):
- NpAuthorizationMiddleware, NpTierMiddleware

**All namespaces updated** from `DfrntAgentsPartners.*` to `CourierPortal.*`

### Step 3: Accounts Duplicates Removed

**Nothing needed to be removed from NP Redesign** — the duplication was entirely in the old Admin/ folder.

### Step 4: Portal Self-Service Controllers Kept

All Portal/ controllers retained:
- Portal/ApplicantsController, Portal/AuthController, Portal/BaseController
- Portal/ContractsController, Portal/CouriersController
- Portal/InvoicesController (**marked SHARED** — needs Accounts API)
- Portal/LocationsController, Portal/RecaptchaController
- Portal/ReportsController (**marked SHARED** — needs Accounts API)
- Portal/RunsController, Portal/SchedulesController
- Portal/ValuesController, Portal/VehiclesController

### Step 5: New Feature Controllers Kept

- ComplianceController (existing, courier-portal specific)
- DocumentScanController (existing)
- DriverApprovalsController (existing)
- PortalStepsController (existing)
- QuizController (existing)
- RegistrationFieldsController (existing)
- MessengerController — NP Redesign version preferred and used
- DocumentTypeController — NP Redesign version preferred, old DocumentTypesController removed
- RecruitmentController — NP Redesign version preferred

### Step 6: Old GitLab Admin/ Folder Removed

**Deleted entirely:**
- `Controllers/Admin/` — 16 controllers (ApplicantsController, AuthController, ContractsController, CouriersController, DeductionsController, FleetsController, InfringementsController, InvoicesController, LocationsController, MessagesController, OpenforceController, ReportsController, SchedulesController, SettingsController, ValuesController, VehiclesController)
- `Services/Admin/` — 18 services (all old GitLab services)
- `DTOs/Admin/` — All admin DTOs (Applicants, Auth, Common, Contracts, Couriers, Deductions, Fleets, Infringements, Invoices, Locations, Messages, Openforce, Reports, Schedules, Vehicles)
- `Validators/Admin/` — All admin validators (30+ validator classes)
- `Utilities/` — DeductionUtility, InvoiceBatchUtility, InvoiceUtility, CourierUtility

### Step 7: Program.cs Updated

- Removed all Admin service registrations (AdminAuthService, AdminApplicantsService, etc.)
- Removed all Admin validator registrations
- Removed Admin DTO using statements
- Added TODO placeholders for NP Redesign service interface registrations
- Added NpAccess authorization policy placeholder
- Marked PortalInvoiceService and PortalReportService with TODO for Accounts API refactor

### Step 8: Old Entities Removed

**Deleted from Domain/Entities/ (handled by Accounts):**
- CourierDeduction, CourierDeductionLine, CourierDeductionRecurring, CourierDeductionRecurringLine, CourierDeductionType
- CourierInvoice, CourierInvoiceBatch, CourierInvoiceBatchItem, CourierInvoiceBatchStatus
- CourierInvoiceLine, CourierInvoiceLineJob, CourierInvoiceLineJobMaster
- TblBuyerTaxInvoiceControl

**Deleted domain folders (financial, handled by Accounts):**
- Domain/Anz/ (ANZ payment file generation)
- Domain/Bnz/ (BNZ payroll file generation)
- Domain/Ird/ (IRD payday filing)

**DespatchContext updated:** DbSets for removed entities commented out with REMOVED markers
**TucCourier updated:** Navigation properties to removed entities commented out
**TblSetting updated:** Navigation property to removed PaymentTerm relationship commented out

### Known Compilation Issues (TODO)

1. **Portal/InvoiceService** — References removed CourierInvoice entities and InvoiceUtility. Needs refactoring to call Accounts API via HttpClient.
2. **Portal/RunService** — References InvoiceUtility.IsCompleted and InvoiceUtility.CanInvoice. Extract these helper methods locally or call Accounts API.
3. **NP Redesign service implementations** — Interface registrations in Program.cs are TODO placeholders. Need concrete service implementations registered.
4. **DespatchContext entity configurations** — The OnModelCreating method still contains configuration blocks for removed entities. These should be cleaned up (they're harmless but add noise).

### Architecture After Rebase

```
courier-portal/
├── Controllers/
│   ├── [23 NP Redesign controllers] — Admin operations (fleet, scheduling, compliance, recruitment, etc.)
│   ├── Portal/ — Courier self-service (auth, runs, schedules, contracts, invoices*, reports*)
│   ├── ComplianceController — Compliance profiles
│   ├── DocumentScanController — Document scanning
│   ├── DriverApprovalsController — Driver approvals
│   ├── PortalStepsController — Portal step configuration
│   ├── QuizController — Training quizzes
│   └── RegistrationFieldsController — Registration field config
├── DTOs/
│   ├── [14 NP Redesign DTOs] — Agent, courier, compliance, recruitment, etc.
│   ├── Portal/ — Self-service DTOs (runs, schedules, invoices, etc.)
│   └── [Feature DTOs] — Quiz, compliance, driver approval, registration
├── Domain/Entities/
│   ├── [22 NP Redesign models] — Agent, NpCourier, marketplace, training, etc.
│   └── [Shared entities] — TucCourier, TucCourierFleet, schedules, applicants, etc.
├── Interfaces/
│   ├── [13 NP Redesign interfaces] — Fleet, scheduling, recruitment, etc.
│   └── [Feature interfaces] — Compliance, quiz, driver approval, etc.
└── Services/
    ├── Portal/ — Self-service services
    ├── NpRedesign/ — ComplianceAutomationService
    └── [Feature services] — Compliance, quiz, recruitment, etc.
```

**Accounts app handles:** Invoicing, settlements, deductions, openforce/1099, payments, statements, direct debits, contractor financial CRUD, fleet listing (GetAll only)
**Courier-portal handles:** Fleet CRUD + courier assignment, scheduling, compliance, recruitment, training, documents, messenger, marketplace, portal self-service
