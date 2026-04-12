# DFRNT Courier Portal

Full courier management platform — admin portal, courier self-service (mobile + desktop), and applicant recruitment.

**Developer:** Loc  
**Backend:** .NET 8, EF Core, SQL Server (Despatch DB + CourierPortal DB) — 288 C# files, 39 controllers  
**Frontend:** React 18 + TypeScript, Vite, Tailwind CSS — 146 files, 55 pages, 0 TypeScript errors  

---

## What This App Does

### For NP Admins (desktop)
- **Fleet management** — courier CRUD, fleet assignment, import
- **Recruitment** — pipeline, applicant detail, stage settings, advertising, portal URL
- **Compliance** — dashboard, profiles, document types, driver approval
- **Scheduling** — shift/roster management
- **Training** — quiz builder, flow builder, document management
- **Messenger** — NP ↔ courier messaging
- **Settings** — registration fields, contracts, Openforce recruitment status, tenant config
- **Users** — NP team management, import

### For Couriers (mobile-first + desktop)
- **Dashboard** — compliance alerts, earnings summary, quick actions
- **My Runs** — view assigned runs, job details
- **Schedule** — view/mark availability
- **Invoicing** — create invoices from uninvoiced runs, or view buyer-created tax invoices (controlled by fleet setting)
- **Documents** — upload/manage compliance documents with AI scan-to-fill
- **Training** — complete assigned quizzes
- **Contractors** — manage subcontractors
- **Settings** — profile, vehicle, contact info

### For Applicants (mobile, public-facing)
- **Multi-step application** — details, driver license scan, vehicle info, document upload, quiz, review
- **AI document scanning** — Claude Vision extracts fields from license/rego/insurance photos
- **Tenant-branded** — each NP gets their own branded portal URL

---

## Project Structure

```
courier-portal/
├── api/                              # .NET 8 Backend (199 C# files)
│   └── src/
│       ├── CourierPortal.Api/        # Controllers + Middleware
│       │   ├── Controllers/
│       │   │   ├── Portal/           # Courier self-service (auth, runs, invoices, schedules, etc.)
│       │   │   ├── Applicant/        # Public applicant registration
│       │   │   ├── DocumentScanController.cs   # AI doc scanning (Anthropic Claude Vision)
│       │   │   ├── PortalStepsController.cs    # Applicant flow config
│       │   │   ├── QuizController.cs           # Quiz CRUD + attempts
│       │   │   └── PortalController.cs         # Portal config
│       │   └── Middleware/
│       ├── CourierPortal.Core/       # Domain, DTOs, Services, Interfaces
│       │   ├── Domain/
│       │   │   ├── Entities/         # All EF entities
│       │   │   ├── CourierPortalContext.cs    # New features DB
│       │   │   └── DespatchContext.cs         # Legacy Despatch DB
│       │   ├── DTOs/
│       │   ├── Services/Portal/      # Portal service implementations
│       │   ├── Interfaces/
│       │   ├── Utilities/
│       │   └── Validators/
│       └── CourierPortal.Infrastructure/   # Email, file storage, repos
│
├── src/                              # React 18 Frontend (149 files)
│   ├── App.tsx                       # Full routing (admin + mobile + portal + applicant)
│   ├── pages/
│   │   ├── np/                       # NP Admin pages (29 pages)
│   │   ├── courier/                  # Courier mobile app (10 pages, bottom-nav)
│   │   ├── portal/                   # Courier desktop portal (6 pages)
│   │   ├── applicant/                # Public applicant wizard
│   │   └── settings/                 # Tenant config
│   ├── components/
│   │   ├── Layout/                   # Admin layout (sidebar, topbar)
│   │   ├── portal/                   # Courier portal layout
│   │   ├── common/                   # Shared UI (ScanToFill, StatCard, Modal, etc.)
│   │   ├── steps/                    # Applicant wizard steps
│   │   └── import/                   # Bulk import components
│   ├── services/                     # API service layer (30 files)
│   ├── hooks/                        # React hooks (10 files)
│   ├── context/                      # RoleContext, TenantConfigContext
│   ├── lib/                          # Utilities, tenant config
│   └── types/                        # TypeScript types
│
├── IMPLEMENTATION.md                 # Step-by-step handover doc for Loc
├── AUDIT.md                          # Backend rebase audit (controller/entity analysis)
└── package.json
```

---

## Quick Start

```bash
# Frontend
npm install
npm run dev          # → http://localhost:5173

# Backend
cd api/src/CourierPortal.Api
dotnet restore
dotnet run           # → http://localhost:5000
```

---

## Environment Variables

| Variable | Required | Description |
|---|---|---|
| `MasterSQLConnection` | ✅ | Master Controller DB connection string |
| `Domain` | ✅ | Cookie domain (e.g. `.deliverdifferent.com`) |
| `RedisConfig` | ✅ | Redis connection string |
| `JWTSecretKey` | ✅ | JWT signing key for courier auth |
| `Issuer` | ✅ | JWT issuer |
| `Audience` | ✅ | JWT audience |
| `PublicPath` | ✅ | Login redirect URL |
| `ANTHROPIC_API_KEY` | Optional | For AI document scanning (Claude Vision) |
| `ReportBase` | Optional | SSRS report server base URL |
| `ReportUsername` | Optional | SSRS NTLM username |
| `ReportPassword` | Optional | SSRS NTLM password |

---

## Invoicing — Two Modes

Controlled by `tucCourier.uccrInternal` (set per fleet):

| `uccrInternal` | Mode | Courier experience |
|---|---|---|
| `false` | **Courier-generated** | Courier selects uninvoiced runs → creates own tax invoice |
| `true` | **Buyer-created (BCTI)** | Company generates invoice via Accounts app. Courier views read-only PDFs by period |

See `IMPLEMENTATION.md` → Step 12 for full details.

---

## Relationship with Other DFRNT Apps

### dfrnt-agents-partners (Garry's repo)
Handles agent discovery, agent CRUD, marketplace, NP tier management, vehicle rates, data masking, ECA/CLDA associations. **Separate app, same DB per tenant, different EF contexts.** Will merge into one Admin Manager tile long-term.

| App | DbContext | Owns |
|---|---|---|
| courier-portal | `CourierPortalContext` + `DespatchContext` | Couriers, recruitment, scheduling, compliance, training, invoicing (creation), messaging |
| dfrnt-agents-partners | `AgentsDbContext` | Agents, marketplace, NP tiers, vehicle rates |

**Do not create cross-repo dependencies.** Each app is independently deployable.

### Accounts App (accounts.dfrnt.com)
Handles invoice *processing* (batching, settlements, payments, deductions, statements). Courier-portal creates invoices; Accounts processes them. The `CourierInvoice` entity is shared.

### dfrnt-recruitment (Railway — apply.urgent.co.nz)
Currently deployed recruitment app (PostgreSQL). Courier-portal has more complete versions of all its controllers. Migration path: dfrnt-recruitment serves the public applicant portal until courier-portal's applicant endpoints are fully wired, then retires.

---

## Frontend API Status

All frontend services now make real API calls. Mock data files have been deleted.

| Layer | Status |
|---|---|
| **Portal services** (courier self-service) | ✅ Wired to real `api/portal/` controllers |
| **Invoice service** | ✅ Full CRUD — create, view, uninvoiced, past |
| **Quiz service** | ✅ Wired to real QuizController (was localStorage) |
| **Document scan** | ✅ Wired to real DocumentScanController (Claude Vision) |
| **Step API** (applicant flow) | ✅ Wired to real PortalSteps + Applicant controllers |
| **NP Admin services** | ⚠️ Wired to real API endpoints — **Loc needs to register NP services in Program.cs** |

### What Loc needs to do in Program.cs

The NP Redesign controllers and services have been copied in but are **not yet registered** in `Program.cs`. Loc needs to add DI registrations for:
- NpDashboard, NpCourier, NpUser, NpSettings, NpReport services
- Compliance, Fleet, Scheduling, Recruitment, Contract, Training, Messenger services
- CourierImport, UserImport services
- AgentsDbContext (for agent/marketplace tables)

Each frontend service file has `@backend-needed` JSDoc comments indicating which controller/endpoint it calls.

## Key Files for Development

| What | Where |
|---|---|
| **Handover guide** | `IMPLEMENTATION.md` — step-by-step wiring for every page |
| **Handover audit** | `HANDOVER-AUDIT.md` — gap analysis: mock data, missing endpoints, broken routes |
| **Backend audit** | `AUDIT.md` — controller/entity overlap analysis across 4 repos |
| **Routing** | `src/App.tsx` — all routes (52 routes) |
| **Sidebar nav** | `src/components/Layout/Sidebar.tsx` — admin navigation |
| **Invoice service** | `api/src/CourierPortal.Core/Services/Portal/InvoiceService.cs` |
| **Document scanning** | `api/src/CourierPortal.Api/Controllers/DocumentScanController.cs` |
| **NP Controllers** | `api/src/CourierPortal.Api/Controllers/Np*.cs`, `Compliance*.cs`, `Fleet*.cs`, etc. |
| **Tenant config** | `src/lib/tenants.ts` — tenant branding/config |

---

## TypeScript Status

✅ Zero errors (`npx tsc --noEmit` passes clean)  
✅ All mock data files deleted (np_mockData, np_schedulingMockData, portal_mockData, portal_devData)
