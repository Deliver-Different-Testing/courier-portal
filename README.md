# DFRNT Courier Portal

Full courier management platform — admin portal, courier self-service (mobile + desktop), and applicant recruitment.

**Developer:** Loc  
**Backend:** .NET 8, EF Core, SQL Server (Despatch DB + CourierPortal DB)  
**Frontend:** React 18 + TypeScript, Vite, Tailwind CSS  

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

## Key Files for Development

| What | Where |
|---|---|
| **Handover guide** | `IMPLEMENTATION.md` — step-by-step wiring for every page |
| **Backend audit** | `AUDIT.md` — controller/entity overlap analysis across 4 repos |
| **Routing** | `src/App.tsx` — all routes |
| **Sidebar nav** | `src/components/Layout/Sidebar.tsx` — admin navigation |
| **Invoice service** | `api/src/CourierPortal.Core/Services/Portal/InvoiceService.cs` |
| **Document scanning** | `api/src/CourierPortal.Api/Controllers/DocumentScanController.cs` |
| **Tenant config** | `src/lib/tenants.ts` — tenant branding/config |

---

## TypeScript Status

✅ Zero errors (`npx tsc --noEmit` passes clean)
