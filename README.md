# DFRNT Courier Portal

Courier self-service portal and applicant recruitment app. **Developer: Loc**

> **Scope change (Apr 2025):** NP Admin features (fleet, compliance, scheduling, recruitment pipeline, training, settings) have been moved to the [Steve-v2.0-NP-Redesign](https://github.com/dfrnt/Steve-v2.0-NP-Redesign) repo, which Garry owns. This repo now contains **only** the courier-facing portal and the applicant recruitment flow.

## User Roles

1. **Courier Portal** — Self-service for active couriers: dashboard, runs, schedule, invoicing, reports, contractors
2. **Applicant Portal** — Multi-step recruitment application: registration, document upload, quiz, declaration

## Structure

```
src/
├── pages/
│   ├── portal/       — Courier self-service (6 pages)
│   │   ├── Dashboard, Runs, Schedule
│   │   ├── Contractors, Invoicing, Reports
│   ├── applicant/    — Applicant recruitment flow
│   │   └── ApplicantPortal
│   ├── np/           — Admin tools used by applicant flow (5 pages)
│   │   ├── FlowBuilder — step config for applicant flow
│   │   ├── QuizBuilder — quiz creation for applicants
│   │   ├── AdminSettings — recruitment admin settings
│   │   ├── DocumentManagement — recruitment document config
│   │   └── SetupPassword — admin auth
│   └── courier/
│       └── CourierTraining
├── components/
│   ├── steps/        — Applicant step components
│   └── Layout/       — Admin + applicant layouts
├── services/         — Portal API services (portal_*.ts)
└── context/          — React contexts

api/
└── src/
    ├── CourierPortal.Api/
    │   ├── Controllers/
    │   │   ├── Portal/       — Courier self-service endpoints
    │   │   ├── Applicant/    — Applicant registration endpoints
    │   │   ├── DocumentScanController.cs — Document scanning (applicant)
    │   │   ├── PortalStepsController.cs  — Applicant flow steps
    │   │   ├── QuizController.cs         — Applicant quiz
    │   │   └── PortalController.cs       — Portal config
    │   └── Middleware/
    │       ├── ErrorHandlingMiddleware.cs
    │       └── HubAuthMiddleware.cs
    ├── CourierPortal.Core/
    │   ├── Domain/Entities/  — Portal + Applicant entities
    │   ├── DTOs/Portal/      — Portal DTOs (auth, runs, invoices, schedules, etc.)
    │   ├── Interfaces/       — Service interfaces
    │   └── Services/
    │       └── Portal/       — Portal service implementations
    └── CourierPortal.Infrastructure/
        ├── Services/         — Email, file storage, password utilities
        └── Repositories/     — Data access

database/                     — SQL migrations (shared with Steve-v2.0)
```

## Quick Start

```bash
# Backend
cd api/src/CourierPortal.Api
dotnet restore
dotnet run

# Frontend
cd src
npm install
npm run dev
```

## Environment Variables

| Variable | Required | Description |
|---|---|---|
| `MasterSQLConnection` | ✅ | Master DB connection string |
| `Domain` | ✅ | Cookie domain (e.g. `.deliverdifferent.com`) |
| `RedisConfig` | ✅ | Redis connection string |
| `JWTSecretKey` | ✅ | JWT signing key |
| `Issuer` | ✅ | JWT issuer |
| `Audience` | ✅ | JWT audience |
| `PublicPath` | ✅ | Login redirect URL |

## Related Repos

- **Steve-v2.0-NP-Redesign** — NP Admin (fleet, compliance, scheduling, recruitment pipeline, training, settings) — owned by Garry

---

## Future Merge Plan

This repo and [Steve-v2.0-NP-Redesign](https://github.com/Deliver-Different-Testing/Steve-v2.0-NP-Redesign) are being developed separately for parallel workstreams but **will be merged into a single unified app** in the future.

| Repo | Developer | Scope | Merge Role |
|---|---|---|---|
| **Steve-v2.0-NP-Redesign** | Garry | NP Admin — fleet, compliance, scheduling, recruitment pipeline, training, settings, marketplace | Becomes the **admin tile** in the merged app |
| **courier-portal** (this repo) | Loc | Courier self-service + applicant recruitment | Becomes the **portal tile** in the merged app |

**Merge approach:**
1. This repo's Portal/ controllers + pages merge into Steve-v2.0's `web-portal` or `web-unified/pages/portal/`
2. This repo's Applicant/ controllers + step components merge into the applicant flow
3. Shared entities (Quiz, DocumentType, PortalStep) exist in both — deduplicate keeping Steve-v2.0's version
4. `FileStorageService` + `DocumentScanController` are shared — keep one copy
5. Single `Program.cs` registers all services from both projects
6. Database migrations from both repos run on the same Despatch DB (no conflicts — different table names)

**Until merge:** This repo is independently deployable. It calls Steve-v2.0's NP API for any admin data it needs (e.g. fleet lists, compliance profiles).
