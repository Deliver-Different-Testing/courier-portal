# Entity Property Mapping: NP Redesign → Production Schema

**For:** Loc  
**Purpose:** The NP Redesign services (`src/CourierPortal.Infrastructure/Services/`) were built against simplified entity classes that don't match the production SQL Server schema. This document maps the old property names to the correct production ones.

The entity classes have been corrected to match the production DespatchContext Fluent API. The Infrastructure services need updating to use the correct property names.

---

## CourierApplicant

| NP Service Property | Production Property | Notes |
|---------------------|-------------------|-------|
| `LastName` | `Surname` | |
| `City` | _(no equivalent)_ | Use AddressLine fields or add to entity |
| `State` | _(no equivalent)_ | Use AddressLine fields |
| `Postcode` | _(no equivalent)_ | Use AddressLine fields |
| `VehicleMake` | _(no equivalent on applicant)_ | Production uses `VehicleType` (string) only |
| `VehicleModel` | _(no equivalent)_ | |
| `VehicleYear` | _(no equivalent)_ | |
| `VehiclePlate` | `VehicleRegistrationNo` | |
| `BankAccountName` | _(no equivalent)_ | Production has only `BankAccountNo` |
| `BankAccountNumber` | `BankAccountNo` | |
| `BankBSB` | `BankRoutingNumber` | |
| `NextOfKinName` | `NextOfKin` | |
| `PipelineStage` | _(no equivalent)_ | Production uses status ints/enums — may need new column |
| `DeclarationSigned` | `DeclarationAgree` | bool |
| `DeclarationSignedDate` | `DeclarationDate` | |
| `DeclarationSignatureS3Key` | `DeclarationSignatureFileName` + `DeclarationSignature` (byte[]) | Production stores signature as binary blob |
| `RejectedDate` | `RejectDate` | |
| `RejectedReason` | `RejectReason` | |
| `ApprovedAsCourierId` | `CourierId` | |
| `CreatedDate` | `Created` | |
| `ModifiedDate` | _(no equivalent)_ | Add if needed |
| `TenantId` | _(no equivalent on DespatchContext)_ | DespatchContext is per-tenant (1 DB per tenant). AgentsDbContext has TenantId column. |
| `Notes` | ✅ Same | |

**Key architectural note:** DespatchContext uses **one database per tenant** (connection string resolved at runtime). AgentsDbContext was designed with a `TenantId` column in a shared DB. The production pattern is per-tenant DB — so `TenantId` is implicit in DespatchContext. The NP services that filter by `TenantId` should be updated to rely on the connection string resolver instead.

---

## CourierContract

| NP Service Property | Production Property | Notes |
|---------------------|-------------------|-------|
| `TenantId` | _(implicit via connection)_ | Per-tenant DB |
| `Name` | _(no equivalent)_ | |
| `S3Key` | _(no equivalent)_ | Production stores file as `byte[] Data` |
| `FileName` | ✅ Same | |
| `MimeType` | `Type` | string, max 50 |
| `FileSize` | `Length` | long |
| `UploadedDate` | `Created` | |
| `UploadedBy` | _(no equivalent)_ | |
| `IsActive` | _(no equivalent)_ | |
| `Version` | _(no equivalent)_ | |
| `CreatedDate` | `Created` | |

---

## NpCourier vs TucCourier

The NP Redesign uses `NpCourier` (in `AgentsDbContext`) as a simplified courier entity. Production uses `TucCourier` (in `DespatchContext`) with Hungarian notation columns. Key mappings:

| NpCourier Property | TucCourier Column | Notes |
|--------------------|-------------------|-------|
| `CourierId` (PK) | `UccrId` (PK) | `uccrID` in DB |
| `FirstName` | `UccrName` | `uccrName` in DB |
| `SurName` | `UccrSurname` | `uccrSurname` in DB |
| `Email` | `UccrEmail` | |
| `PersonalMobile` | `PersonalMobile` | ✅ Same |
| `Phone` | `UccrTel` | |
| `Address` | `UccrAddress` | max 100 chars |
| `City/State/Postcode` | `AddressLine1-8` | No city/state on production |
| `VehicleType` | `UccrVehicle` | string max 50 |
| `VehicleMake` | `UccrVehicleMakeId` | FK to lookup table, not string |
| `VehicleModel` | `UccrVehicleModel` | |
| `VehicleYear` | `UccrVehicleYear` | |
| `RegoNo` | `UccrReg` | |
| `Status` | `Active` (bool) | Production uses bool Active, not string Status |
| `BankAccount` | `UccrBankAccountNo` | |
| `AgentId` | `CourierFleetId` | Maps to fleet, not agent |

**Decision:** The NP services that create/read NpCourier objects need to be refactored to work with TucCourier via DespatchContext, OR NpCourier needs to stay as a separate simplified entity in AgentsDbContext for new features. This is a design decision for Loc.

---

## Services That Need Property Updates

| Service File | Entity Used | Issue |
|-------------|-------------|-------|
| `RecruitmentService.cs` | `CourierApplicant` | Uses old property names throughout |
| `RecruitmentStageService.cs` | `RecruitmentStageConfig` | May be OK — separate entity |
| `ContractService.cs` | `CourierContract` | Uses `S3Key`, `UploadedDate`, `IsActive` etc. |
| `CourierImportService.cs` | `CourierApplicant` | References `LastName` |
| `CourierDocumentService.cs` | `CourierDocument` | Uses `ModifiedDate` |
| `ComplianceDashboardService.cs` | Various | Check property names |
| `AgentService.cs` | `Agent` | Uses `ModifiedDate`, `CreatedDate` — check if Agent entity is OK |

---

## Recommended Approach

**Option A (Clean but more work):** Update all NP services to use production entities (TucCourier, CourierApplicant with production schema). Remove AgentsDbContext where possible, use DespatchContext.

**Option B (Pragmatic):** Keep AgentsDbContext for NP-specific tables (Agent, NpCourier, Marketplace, etc.) but update CourierApplicant references to use DespatchContext. This lets the NP features evolve independently while sharing the courier applicant data.

**Option C (Quick fix):** Add the missing "convenience" properties to the production entity classes as computed properties or extra columns. Least disruptive but adds tech debt.
