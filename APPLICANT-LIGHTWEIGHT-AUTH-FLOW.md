# Applicant Lightweight Auth Flow

## Purpose

This document describes a **lightweight authentication approach for applicants** in the Courier Portal.

It is intentionally a **discussion document**, not a final implementation spec. The goal is to keep the applicant experience fast and low-friction while still giving us enough identity/security to:

- save progress
- let applicants resume later
- protect uploaded documents and personal details
- avoid building a heavy “full account system” too early

---

## Current State

### Business / platform context

Courier Portal **does exist as a tile on Hub** and applicants have historically been stored in Courier Portal data.

That means two things are true at once:

1. Courier Portal is part of the wider Hub ecosystem.
2. Applicant records themselves are still Courier Portal / tenant data, not just generic Hub users.

So this document is **not** proposing that applicants become standalone global users with no tenant association. The opposite is true: applicants must belong to a tenant.

### Frontend

The current applicant portal (`/apply` and `/apply/:tenantSlug`) is effectively **public and anonymous** at entry.

Current behaviour:
- applicant starts without login
- progress is saved to **localStorage** only
- resume works only on the same browser/device
- document uploads and form state in the current React build are mostly UX-driven
- there is no real applicant session in the frontend flow yet
- the route structure already suggests tenant-aware entry via `/apply/:tenantSlug`

### Backend

The backend already has the beginnings of a real applicant auth model:

- `POST /api/portal/applicants/register`
- `POST /api/portal/applicants/emailverification`
- `POST /api/portal/auth/token`
- `POST /api/portal/auth/refresh`
- applicant JWT policy: `AccountType = Applicant`
- applicant claim identity uses `CurrentId`

Important existing backend rules:
- applicant is stored in `CourierApplicants`
- applicant has `Email`, `Password`, `EmailVerificationCode`, `EmailVerified`
- applicant cannot log in until email is verified
- once logged in, applicant can access protected endpoints using JWT bearer token

### Current architectural mismatch

The frontend already points toward **tenant-from-URL** onboarding, but parts of the backend still appear to expect **Hub-style tenant context** from claims/runtime wiring.

So the real issue is not:

> "Applicants must be Hub users first."

The real issue is closer to:

> "Applicants belong to a tenant, but the public applicant flow should resolve that tenant from the application URL rather than depending on Hub tile context at runtime."

So the backend already supports a more real applicant auth flow than the frontend is using, but the tenant-resolution model still needs to be aligned with the public application journey.

---

## Problem We’re Solving

We need a flow that is:

### Light for the applicant
- no big signup wall before they even start
- no confusing “create account / verify / login / continue” sequence
- ideally feels like “start application, verify email, keep going”

### Useful for the business
- applicant data is tied to a real identity early
- progress is resumable across devices
- documents are linked to the right person
- staff can see partial/in-progress applicants
- duplicate applicants are reduced

### Practical for engineering
- reuses the existing `CourierApplicants` + JWT backend where possible
- doesn’t require a full password-reset/account-management product in phase 1
- keeps room for future improvements like magic links or OTP-only auth

---

## Recommended Direction

Use a **tenant-from-URL + progressive auth flow**:

1. applicant lands on the public `/apply/:tenantSlug` page
2. that URL identifies which tenant the applicant belongs to
3. first screen collects **email + basic details + password**
4. backend creates applicant record in that tenant and sends **6-digit verification code**
5. applicant enters verification code inline
6. once verified, frontend immediately logs them in and stores JWT
7. JWT should carry tenant context for future requests
8. remaining application steps become an authenticated applicant session in that tenant
9. progress is saved server-side, not just localStorage

This keeps the experience lightweight because:
- there is still **no separate login page at the start**
- verification happens inside the flow
- auth is introduced only once, early, in a simple way
- tenant association comes naturally from the application URL instead of requiring prior Hub user setup

---

## Proposed Applicant Journey

## 1. Entry

Applicant opens:
- `/apply`
- or preferably `/apply/:tenantSlug`

They see:
- tenant branding
- role selection if needed
- short CTA
- “Start application”

No login required at this point.

### Tenant association

This is the key architectural point:

- the applicant must still belong to a tenant
- that tenant should be identified from the **application page URL**
- the public applicant flow should not require the applicant to already exist as a Hub identity just to determine tenant

So for applicant onboarding, the desired sequence is:

1. URL slug identifies tenant
2. backend resolves slug to tenant
3. applicant record is created in that tenant's Courier Portal data
4. applicant auth/session continues inside that tenant context

This is different from internal staff launching a Hub tile, and that is OK.

---

## 2. Lightweight Registration Step

First real step asks for:
- first name
- last name
- email
- mobile
- vehicle type (optional here if wanted)
- password

Why password this early?
- backend already supports applicant password login
- it avoids needing a separate account-creation step later
- it gives us a fallback login method if they leave before finishing

### Backend call
`POST /api/portal/applicants/register`

Expected effect:
- resolve tenant from the application URL slug or equivalent request context
- create `CourierApplicants` row in that tenant
- save password
- generate `EmailVerificationCode`
- queue/send verification email

### UX notes
If email already exists:
- do **not** hard-fail in a confusing way
- show something like:
  - “It looks like you’ve already started an application with this email.”
  - “Log in to continue or request a new verification code.”

---

## 3. Inline Email Verification

Immediately after registration, show a verification screen:
- “We’ve sent a 6-digit code to your email”
- input for code
- resend option
- change email option

### Backend call
`POST /api/portal/applicants/emailverification`

Payload concept:
- email
- verificationCode

### Successful result
On success:
- applicant is marked `EmailVerified = true`
- frontend should immediately call token login

### Follow-up login call
`POST /api/portal/auth/token`

Using:
- username = email
- password = password entered during registration

Result:
- JWT token
- refresh token
- `AccountType = Applicant`

From here onward, the applicant has a proper authenticated session.

That session should also carry tenant context so future applicant requests do not need to keep re-inferring tenant from the public URL alone.

---

## 4. Authenticated Application Flow

After verification/login, the rest of the portal should behave like a normal signed-in applicant session.

Protected actions should use JWT:
- get applicant profile
- update applicant profile
- fetch required docs
- upload docs
- delete uploads
- read contract/location data where needed
- submit declaration

Relevant existing endpoints already fit this model:
- `GET /api/portal/applicants`
- `POST /api/portal/applicants`
- `GET /api/portal/applicants/documents`
- `POST /api/portal/applicants/uploads`
- `DELETE /api/portal/applicants/uploads/{id}`
- `POST /api/portal/applicants/declaration`

This is the big win: we can stop relying on browser-only draft storage and move the real source of truth to the database.

---

## 5. Resume Later

For resume-later, keep it simple.

### Option A — initial phase
Use normal login:
- applicant returns to `/apply/:tenantSlug`
- clicks “Continue existing application”
- enters email + password
- frontend calls `POST /api/portal/auth/token`
- app restores progress from backend profile/doc state

### Option B — later enhancement
Add passwordless resume:
- “Email me a sign-in link”
- or “Send me a one-time code”

This would be better UX long term, but not required for phase 1 because the current backend already supports password auth cleanly.

---

## Tenant Resolution Model

The clean model is:

### Before verification / login
Tenant comes from:
- application URL slug
- or equivalent public route context

### After verification / login
Tenant comes from:
- applicant JWT claim(s)
- with the applicant record already tied to the correct tenant DB/data set

This gives the right split:
- **public discovery and registration** use URL-based tenant resolution
- **authenticated applicant actions** use token-based tenant resolution

### Why this matters

Applicants are usually arriving from:
- recruitment ads
- QR codes
- careers pages
- direct onboarding links

They are **not** typically internal staff navigating from Hub first.

So Hub can still remain relevant for:
- internal staff access
- tile navigation
- courier/staff SSO where useful

But public applicant onboarding should be able to identify the tenant directly from the recruitment/application link.

---

## Why This Is the Right Level of Lightweight

This is lighter than a traditional account system because:
- no separate registration page
- no forced login before the applicant even starts
- verification is inline
- session begins naturally as part of the application

But it is stronger than a pure anonymous flow because:
- data survives device/browser changes
- uploads are protected
- resume later is real
- staff can manage applicants before completion

It sits in the middle, which is probably the correct balance for recruitment.

---

## What We Should Avoid

## 1. Fully anonymous application until final submit

Why not:
- hard to resume on another device
- harder to protect uploaded documents
- duplicate cleanup becomes messy
- backend applicant workflow becomes disconnected from the frontend

## 2. Heavy enterprise auth before application starts

Why not:
- too much friction
- feels like creating an account for no reason
- likely hurts conversion

## 3. Forcing applicant to leave the flow and go to a separate login page after verification

Why not:
- breaks momentum
- feels clunky
- unnecessary when we can auto-log them in immediately after code verification

---

## Suggested UX Copy

### Registration step
**Heading:** Start your application  
**Body:** Enter your details to save progress and continue your application anytime.

### Verification step
**Heading:** Check your email  
**Body:** We’ve sent a 6-digit code to `you@example.com`.

### Resume step
**Heading:** Continue your application  
**Body:** Sign in to pick up where you left off.

This framing makes auth feel like a helpful save/resume feature, not admin overhead.

---

## Data / Security Notes

## Minimum requirements
- passwords must not remain plain text long term
- JWT expiry remains short-lived
- refresh token flow can stay as-is initially, but should eventually be properly persisted/validated
- uploaded documents must stay behind authenticated applicant access

## Important observation from current code
Right now the applicant password appears to be compared directly in code, which strongly suggests we are still effectively storing raw passwords or equivalent reversible values.

That is fine for discussion purposes, but **not fine as the end state**.

Before production hardening, applicant auth should move to:
- hashed passwords
- proper reset flow
- proper refresh token persistence/rotation

That said, those are **hardening tasks**, not blockers to agreeing the flow itself.

---

## Recommended Phase Approach

## Phase 1 — lightweight, pragmatic
- keep `/apply/:tenantSlug` public at entry
- register applicant early in flow
- verify email inline with 6-digit code
- auto-login after successful verification
- use existing applicant JWT endpoints
- move progress persistence to backend-backed applicant record
- keep “resume later” as email + password login

## Phase 2 — UX polish
- resend verification code
- better handling for existing applicant email
- cleaner “continue application” entry point
- partial progress indicators in admin

## Phase 3 — auth hardening / polish
- password hashing if not already in place
- forgot password / reset password for applicants
- magic link or OTP-only resume flow
- proper refresh token storage + invalidation

---

## Proposed API Shape for Frontend Thinking

Not a final contract, but this is the clean mental model.

### Start application
Preferred mental model:
`POST /api/public/applicants/{tenantSlug}/register`

Or, if the existing controller shape is retained:
`POST /api/portal/applicants/register`
with tenant slug supplied explicitly in route/body/header and resolved before the applicant write.

Request:
- tenantSlug
- firstName
- surname
- email
- mobile
- password
- vehicleType
- location

Response:
- success
- emailSent = true
- maybe `applicantCreated = true`
- maybe resolved tenant metadata if helpful

### Verify email
`POST /api/portal/applicants/emailverification`

Request:
- email
- verificationCode

Response:
- success

### Get token after verify
`POST /api/portal/auth/token`

Request:
- username = email
- password

Response:
- token
- refreshToken
- accountType = Applicant
- tenant context in token claims (recommended)

### Continue application
Authenticated bearer session calls:
- `GET /api/portal/applicants`
- `POST /api/portal/applicants`
- `GET /api/portal/applicants/documents`
- `POST /api/portal/applicants/uploads`
- `POST /api/portal/applicants/declaration`

---

## Final Recommendation

For applicants, the best lightweight auth flow is:

- **public entry from a tenant-specific application URL**
- **tenant resolved from URL at the start of the flow**
- **early email + password capture**
- **inline code verification**
- **immediate JWT login after verification**
- **tenant carried forward in the authenticated session**
- **authenticated progress for the rest of the application**

In plain English:

> Let them arrive on the right tenant's application page, identify that tenant from the URL, verify identity once, then quietly turn the rest of the application into a normal signed-in session.

That gives us the lowest friction path without ending up with a fragile anonymous draft system or forcing applicants to be pre-created as Hub users.

---

## Open Questions

These need product/implementation decisions before implementation:

1. Do we want password at step 1, or do we want OTP-only to start?
2. Should existing applicants be prompted to log in, or should we support “resend verification / resume by code” immediately?
3. Do we want admin visibility of partial applicants from the moment registration starts?
4. Should verification be mandatory before any document upload, or only before final submission?
5. Is phase 1 allowed to use current password handling temporarily, or do we require password hashing before launch?
6. Where exactly should tenant slug be resolved on the backend — route, header, request body, or host/subdomain?
7. Should applicant JWTs include `TenantId`, `TenantSlug`, or both?
8. Which pieces of the current Hub-style tenant bootstrap need to be bypassed or replaced for public applicant endpoints?

---

## My Recommendation on Those Open Questions

- **Password at step 1:** yes, for phase 1
- **Existing applicant handling:** prompt to continue existing application
- **Admin visibility of partials:** yes
- **Verification before uploads:** yes
- **Password hashing before launch:** yes, before production launch
- **Tenant resolution source:** route-based `tenantSlug` is the cleanest default
- **JWT tenant claims:** include both `TenantId` and `TenantSlug` if possible
- **Hub-style bootstrap for applicant endpoints:** bypass it for public applicant entry, then switch to JWT tenant context after auth

That keeps phase 1 simple without painting us into a corner.
