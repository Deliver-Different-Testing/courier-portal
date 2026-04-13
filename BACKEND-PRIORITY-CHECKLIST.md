# Courier Portal — Backend Priority Checklist

**For:** Loc  
**Created:** 2026-04-13  
**Goal:** Get the backend compiling, then wire services in priority order.

---

## Phase 0: GET IT COMPILING (Do This First)

These are the root causes of the 241 errors. Fixing these will collapse most of the error list.

### 0.1 Missing Entity Classes ✅ (DONE — committed by Steve's team)

The following entity files were missing from `src/CourierPortal.Core/Domain/Entities/`. They've now been created and committed:

| Entity | File | Referenced By |
|--------|------|---------------|
| `TenantSettings` | `TenantSettings.cs` | CourierPortalContext |
| `RecruitmentStage` | `RecruitmentStage.cs` | CourierPortalContext |
| `RecruitmentNote` | `RecruitmentNote.cs` | CourierPortalContext |
| `Infringement` | `Infringement.cs` | DespatchContext |
| `InfringementCategory` | `InfringementCategory.cs` | DespatchContext |
| `InfringementCategoryLink` | `InfringementCategoryLink.cs` | DespatchContext |
| `TblBuyerTaxInvoiceControl` | `TblBuyerTaxInvoiceControl.cs` | DespatchContext |
| `CourierDeduction` | `CourierDeduction.cs` | DespatchContext |
| `CourierDeductionLine` | `CourierDeductionLine.cs` | DespatchContext |
| `CourierDeductionRecurring` | `CourierDeductionRecurring.cs` | DespatchContext |
| `CourierDeductionRecurringLine` | `CourierDeductionRecurringLine.cs` | DespatchContext |
| `CourierDeductionType` | `CourierDeductionType.cs` | DespatchContext |
| `CourierInvoiceBatch` | `CourierInvoiceBatch.cs` | DespatchContext |
| `CourierInvoiceBatchItem` | `CourierInvoiceBatchItem.cs` | DespatchContext |
| `CourierInvoiceBatchStatus` | `CourierInvoiceBatchStatus.cs` | DespatchContext |

### 0.2 Fixed `CourierApplicant.cs` ✅ (DONE — committed)

The old version had wrong property names (`LastName` instead of `Surname`, missing `Password`, `EmailVerificationCode`, `Mobile`, `ContractId`, `CourierFleetId`, etc.). Replaced with the correct version matching DespatchContext's Fluent API.

### 0.3 Added Missing Navigation Properties ✅ (DONE — committed)

Added to `TucCourier.cs`:
- `Infringements`, `CourierDeductions`, `CourierDeductionRecurrings`

Added to `CourierInvoice.cs`, `CourierInvoiceLine.cs`, `PaymentTerm.cs`:
- Matching navigation collections for DespatchContext relationships

### 0.4 Remaining Build Errors (Your Job)

After pulling the above commits, do `dotnet build` and work through any remaining errors. They'll likely be:
- Service classes referencing properties that changed name (e.g., `LastName` → `Surname` in ApplicantService)
- Missing `using` statements
- Any other entity property mismatches

**Expected remaining error count after these fixes: <20** (down from 241)

---

## Phase 1: REGISTER NP SERVICES IN Program.cs

The NP Redesign controllers exist but their services aren't registered. Add these to `Program.cs`:

```csharp
// NP Redesign Services — add after existing Portal service registrations
builder.Services.AddScoped<INpCourierService, NpCourierService>();
builder.Services.AddScoped<INpDashboardService, NpDashboardService>();
builder.Services.AddScoped<INpReportService, NpReportService>();
builder.Services.AddScoped<INpSettingsService, NpSettingsService>();
builder.Services.AddScoped<INpUserService, NpUserService>();
builder.Services.AddScoped<IFleetService, FleetService>();
builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();
builder.Services.AddScoped<RecruitmentStageService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<IComplianceDashboardService, ComplianceDashboardService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<ICourierDocumentService, CourierDocumentService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<IMessengerService, MessengerService>();
builder.Services.AddScoped<ICourierImportService, CourierImportService>();
```

Check each interface exists in `CourierPortal.Core/Interfaces/`. Some services may not have interfaces yet — either create them or register as concrete `AddScoped<ConcreteService>()`.

---

## Phase 2: WIRE FRONTEND SERVICES (Priority Order)

Work through these in order — each one unlocks a visible section of the app.

### Priority 1: Auth + Role Context (unlocks everything)
- **Files:** `src/lib/api.ts`, `Login.tsx`, `CourierLogin.tsx`
- **Backend:** `POST /api/admin/auth/login`, `GET /api/admin/auth/me`
- **Why first:** Nothing works without auth. JWT storage + Axios interceptors.

### Priority 2: Dashboard (proves the wiring works)
- **File:** `src/services/np_dashboardService.ts`
- **Backend:** `GET /api/admin/dashboard`
- **Why second:** Quick win that proves end-to-end flow works.

### Priority 3: Courier List + Fleet (core data)
- **Files:** `np_courierService.ts`, `np_fleetService.ts`
- **Backend:** `GET /api/admin/couriers`, `GET /api/admin/fleets`
- **Why:** Most other pages depend on courier/fleet data.

### Priority 4: Recruitment Pipeline (high business value)
- **Files:** `np_recruitmentService.ts`, `np_recruitmentSettingsService.ts`
- **Backend:** `GET/PUT /api/admin/applicants`, `GET /api/recruitment/stages`
- **Why:** Active business process — couriers are applying now.

### Priority 5: Compliance Hub
- **Files:** `np_complianceService.ts`, `np_complianceProfileService.ts`, `np_driverApprovalService.ts`
- **Backend:** `GET /api/compliance-profiles`, `GET /api/driver-approvals`

### Priority 6: Scheduling
- **File:** `np_schedulingService.ts` (replace `np_schedulingMockData.ts`)
- **Backend:** `GET/POST /api/admin/schedules`

### Priority 7: Courier Mobile Portal (courier-facing)
- **File:** `portal_courierService.ts`
- **Backend:** Portal endpoints (`/api/portal/couriers/me`, `/api/portal/runs`, etc.)
- **Includes:** Invoicing modes (BCTI vs courier-generated — see IMPLEMENTATION.md Step 12)

### Priority 8: Everything else
- Training/Quiz, Documents/Scanning, Users, Reports, Settings, Applicant Portal, Openforce

---

## Key Gotchas

1. **CourierApplicant is shared across THREE DbContexts.** 
   - `DespatchContext` — maps to production `CourierApplicant` table with full schema (`Surname`, `Password`, `EmailVerificationCode`, `Mobile`, `ContractId`, `CourierFleetId`, etc.)
   - `AgentsDbContext` — also references `CourierApplicant` via `DbSet<CourierApplicant> CourierApplicants`
   - The entity class has been corrected to match the production DespatchContext schema.
   - **The NP Redesign services** (`RecruitmentService.cs`, etc.) reference OLD property names like `LastName`, `City`, `State`, `BankBSB`, `PipelineStage`, `DeclarationSigned`, `CreatedDate`, `ModifiedDate`, `ApprovedAsCourierId`. **These need updating** to use the correct production property names. Key mappings:
     - `LastName` → `Surname`
     - `City/State/Postcode` → `AddressLine1-8` (production schema uses address lines, not city/state)
     - `VehiclePlate` → `VehicleRegistrationNo`
     - `BankAccountName` → (no direct equivalent — remove or use BankAccountNo)
     - `BankBSB` → `BankRoutingNumber`
     - `PipelineStage` → (new column needed in production, or use a status int)
     - `CreatedDate` → `Created`
     - `ModifiedDate` → (no direct equivalent — add if needed)
     - `ApprovedAsCourierId` → `CourierId`
     - `DeclarationSigned` → `DeclarationAgree`
     - `DeclarationSignedDate` → `DeclarationDate`
   - **Decision for Loc:** Either (a) update all NP services to use production property names, or (b) add the missing convenience properties to the entity class for AgentsDbContext compatibility. Option (a) is cleaner long-term.

2. **Invoice entities are commented out in DespatchContext.** The `DbSet` properties are commented but the Fluent API config and entity classes exist. This is intentional — courier-portal creates invoices, Accounts processes them. The entity classes need to exist for navigation properties even if the DbSets are commented.

3. **Deduction/InvoiceBatch entities also commented out.** Same reason — they exist in the DB, referenced by navigation properties, but owned by Accounts. Entity files must exist for EF compilation.

4. **`OnModelCreatingGeneratedFunctions`** — DespatchContext calls this partial method. An implementation already exists at `DespatchContext.Functions.cs`. If it causes issues, verify the file is included in the build.

5. **Portal services vs NP services.** Portal services (in `CourierPortal.Core/Services/Portal/`) are registered and working. NP services (in `CourierPortal.Infrastructure/Services/`) are NOT registered. The naming follows: `Portal*` = courier-facing, `Np*` = admin-facing.

6. **AgentsDbContext is not registered in Program.cs either.** The NP Infrastructure services all depend on `AgentsDbContext`. You'll need to register it:
   ```csharp
   builder.Services.AddDbContext<AgentsDbContext>(options =>
       options.UseSqlServer(connectionString));
   ```

---

## Database Notes

- No migrations needed for Phase 0-1. These are existing production tables.
- The entity classes match the existing SQL Server schema exactly (copied from EF Core Power Tools scaffold).
- CourierPortalContext entities (TenantSettings, RecruitmentStage, etc.) will need migrations when you're ready to create those tables — they're new tables for the portal features.

---

## Quick Reference: What's Already Done vs What's Needed

| Layer | Status |
|-------|--------|
| React UI (146 files) | ✅ Complete |
| Frontend routing | ✅ Complete |
| Frontend services (mock) | 🔧 Replace with real API calls |
| Backend controllers (39) | ✅ Controllers exist |
| Backend services (Portal) | ✅ Registered in Program.cs |
| Backend services (NP) | 🔧 Need registration in Program.cs |
| Entity classes | ✅ All created (this commit) |
| EF DbContext config | ✅ Complete |
| Auth flow | 🔧 Need JWT implementation |
| Database tables | ✅ Exist in production (no migrations needed for Despatch entities) |
