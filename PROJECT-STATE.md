# Project State — SaaS for SME UK Accountancy Practices

**Last working session:** 2026-05-11 (extended)
**Status:** Paused after Task 28 — **Phase 8 (authz) complete**. Sub-plan 1a (Foundation Core) at **28 of 40 tasks** on branch `feature/foundation-core`. Next: **Phase 9 — Observability (Tasks 29-31: Serilog + OpenTelemetry + CorrelationId)**, then Phases 10-12.

## Tasks completed (commits on `feature/foundation-core`, in order)

| # | Commit | Description |
|---|---|---|
| 1 | `536471e` | Repo bootstrap — solution, .gitignore, .editorconfig, central package management, README |
| 2 | `a7fb188` | 8 csproj scaffold with Clean Architecture references |
| 3 | `901150c` | docker-compose.yml (postgres + seq running locally) |
| – | `d9300d6` | Plan-fix: Seq `SEQ_FIRSTRUN_NOAUTHENTICATION` requirement recorded |
| 4 | `52acf3e` | SharedKernel: FirmId, UserId, Entity/AggregateRoot/ValueObject, Clock, Result + 6 tests |
| 5 | `0f2faee` | PracticeOperations DbContext skeleton + DI registration |
| 6 | `b9db07c` | Web composition root + /health endpoint (DB-check) |
| 7 | `1b158a6` | Integration test infra: PostgresFixture, ApiFactory, HealthCheckTests |
| 8 | `48a8aa4` | Initial empty EF migration (later regenerated in Task 19) |
| 9 | `8530c41` | IFirmContext abstraction + HttpContext-backed accessor in Web |
| 10 | `ed9b1fe` | DbContext applies ITenantScopedEntity global query filter |
| 11 | `03dd4cc` | Cross-tenant isolation integration test via TenantTestRow |
| – | `5731cbd` | docs(state): pause after Task 10 record |
| 12 | `82366cb` | AuditEvent entity + EF configuration + migration |
| 13 | `e72c89b` | IAuditWriter + EfAuditWriter; append-only in SaveChanges; FrameworkReference Microsoft.AspNetCore.App |
| 13a | `6618439` | Fix(audit): guard moved to 2-arg SaveChanges leaf overloads; dead CPM pins dropped |
| 14 | `3113cfa` | MediatR AuditingBehavior pipeline |
| 15 | `fd7862a` | EmailAddress value object with validation + normalization |
| 16 | `2c1bf91` | Role, UserStatus, FirmStatus enums |
| 17 | `1450c73` | Firm aggregate with Register/Activate + FirmRegistered event |
| 18 | `7fd14bc` | User aggregate with Register/Activate/EnrollTotp + RecordSuccessful/FailedSignIn + UserRegistered event |
| 19 | `0d87bed` | EF mappings for Firm + User; snake_case via EFCore.NamingConventions 10.0.1; consolidated Initial migration; ApiFactory test-fixture bug fixed |
| – | `1b233c8` | docs(state): pause after Task 19 record |
| 19a | `2a23de8` | Task 19 review fixes: Email converter NRE → explicit throw, lambda-form HasIndex, ApiFactory unused-using drop, Firm/User persistence smoke tests (5 new tests) |
| 20 | `03051d2` | Repository abstractions + EF implementations (IFirmRepository, IUserRepository, IUnitOfWork) |
| 21 | `30392ad` | IPasswordHasher + Pbkdf2 (via FrameworkReference, no NuGet add) + smoke test |
| 21a | `4313cb3` | Fix(cpm): unify EF Core Relational at 10.0.7 across solution via `CentralPackageTransitivePinningEnabled` + explicit `PackageVersion` pin (resolves MSB3277 surfaced by Task 21's new UnitTests→Infrastructure reference) |
| 22 | `9a04de3` | RegisterFirm command/handler/validator + ValidationBehavior MediatR pipeline; validation registered before auditing |
| 22a | `891a461` | Task 22 review fixes: validator regex unified with EmailAddress.RegexPattern const; email-taken test added; DI pipeline-ordering comment |
| 23 | `001cf52` | POST /api/firms/register endpoint; introduces **ConflictException** in SharedKernel; converts duplicate-slug/email throws to typed ConflictException; **fixes latent EfUserRepository LINQ-translation bug** (`u.Email.Value == lower` unconverted; now constructs EmailAddress and uses value-converter equality) |
| 24 | `d1501cf` | ITotpService + Otp.NET implementation (KeyGeneration.GenerateRandomKey, Base32, Totp w/ VerificationWindow ±1) + 3 unit tests |
| 25 | `f716fa5` | EnrollTotpCommand + handler + /api/auth/enroll-totp endpoint (IFirmContext-guarded, RequireAuthorization) + 2 unit tests |
| 26 | `ac33b07` | IJwtIssuer + JwtIssuer (HMAC-SHA256, 60-min lifetime, claims: sub/firm_id/Role); JwtBearer scheme wired in Web/Program.cs; appsettings.json holds empty placeholders; dev Jwt config in appsettings.Development.json; ApiFactory injects test Jwt config via UseSetting (auth scheme inits eagerly); JwtBearer NuGet was needed (not in implicit FrameworkReference); 3 unit tests |
| 27 | `6afcd59` | SignInCommand + handler + /api/auth/sign-in endpoint with TOTP step-up; auto-Activate on first sign-in (MFA grace); IAuditWriter ctor inject omitted (deferred to Task 28-29); 2 integration tests |
| – | `31230cd` | docs(state): checkpoint after Task 27 |
| 28 | `a1f320e` | Authorization policies (RequireFirmOwner / RequirePartnerOrAbove / RequireManagerOrAbove / RequireStaff) + /api/admin/me (RequireStaff) + /api/admin/owner-only (RequireFirmOwner); AdminEndpointsPartial stub removed; 2 integration tests proving full bearer → role-claim → policy chain |

## Auth surface now complete

- `POST /api/firms/register` — creates Firm + owner User (`PendingVerification` status)
- `POST /api/auth/sign-in` — validates password (+ TOTP if enrolled), auto-Activates first sign-in, issues 60-min JWT bearer; 401 on bad creds/TOTP; supports two-step flow (`TotpRequired: true` if user is enrolled but no code provided)
- `POST /api/auth/enroll-totp` — `RequireAuthorization()`; generates 20-byte Base32 secret + otpauth:// URI for QR; updates user state via `User.EnrollTotp`
- All staff-side endpoints will gate via `RequireAuthorization()` + per-policy `RequireRole(...)` in Task 28

## Plan deviations to be aware of (already documented inline)

- Task 2: test-tool package versions bumped to SDK-template baseline; `coverlet.collector` 6.0.4 added
- Task 3: `SEQ_FIRSTRUN_NOAUTHENTICATION: "true"` required for Seq 2025.2+ local dev
- Task 4: `CA1000` suppressed in-file on `Result<T>`; `CA1707` suppressed in test csprojs
- Task 5: `System.Security.Cryptography.Xml 10.0.7` pinned (removed in Task 13 when FrameworkReference superseded)
- Task 6: 5th package for `AddDbContextCheck<T>`; EF Core bumped to 10.0.7
- Task 7: `CA1711` suppression for xunit collection-definition marker types
- Task 10: filter rewritten from `e.FirmId.Value == CurrentFirmIdRaw` (Npgsql-untranslatable) to `e.FirmId == CurrentFirmId.Value`
- Task 11/12: CA1861 hoist in scaffolded migrations (recurring; resolved globally in Task 19 via .editorconfig)
- Task 13: AuditAction.Unknown rejection; FrameworkReference Microsoft.AspNetCore.App; 3 PackageReferences pruned (NU1510); guard moved to 2-arg SaveChanges leaf overloads (commit `6618439`)
- Task 14: CA1725 rename `ct` → `cancellationToken` (recurring — applied to every MediatR handler since)
- Task 17: en-dash in error message replaced with ASCII hyphen
- Task 19 (largest deviation): adopted snake_case naming convention project-wide; regenerated all migrations as one consolidated `Initial`; added `.editorconfig` glob suppression for CA1861 in Migrations folder; **fixed latent ApiFactory bug** where tests had been hitting the dev DB rather than the Testcontainer (switched to `UseSetting`); moved dev connection string to `appsettings.Development.json`
- Task 19a: EmailAddress converter null-forgiveness replaced with explicit throw; lambda-form `HasIndex` for composite unique; persistence smoke tests added for both Firm and User aggregates
- Task 21: FrameworkReference path worked — `PasswordHasher<TUser>` available without explicit NuGet add (`Microsoft.Extensions.Identity.Core` is in the AspNetCore shared framework)
- Task 21a: `CentralPackageTransitivePinningEnabled=true` + explicit `Microsoft.EntityFrameworkCore.Relational` pin in Directory.Packages.props (only `10.0.0` and `10.0.1` exist on NuGet for EFCore.NamingConventions — no 10.0.7 to bump to; force unification at the transitive pin instead)
- Task 22: `EmailAddress.RegexPattern` const introduced so validator and value object stay in lockstep automatically
- Task 23 (largest deviation since 19): introduced `ConflictException` in SharedKernel and converted RegisterFirmHandler duplicate-resource throws (left the EmailAddress.Create defensive throw as InvalidOperationException — that path is now unreachable since validator catches first); endpoint catches typed exception; **also fixed latent EfUserRepository LINQ-translation bug** that had zero test coverage prior
- Task 24: smoke tests added beyond plan (3 — covers GenerateSecret base32 validity, BuildOtpAuthUri encoding, Verify round-trip with real Totp.ComputeTotp)
- Task 25: unit tests added beyond plan (2 — enroll happy path + user-not-found 400); FluentValidation `using` dropped from AuthEndpoints.cs (unused → CS8019)
- Task 26: JwtBearer **NuGet package WAS required** in Accounts.Web.csproj (NOT in the implicit framework reference from `Microsoft.NET.Sdk.Web`); dev Jwt config split to appsettings.Development.json, prod-defaults appsettings.json has empty placeholders; ApiFactory `UseSetting` for Jwt config too (auth scheme inits eagerly on first request, otherwise IDX10703); CA1305 InvariantCulture on int.Parse, CA1062 ArgumentNullException.ThrowIfNull added defensively; 3 unit tests
- Task 27: `IAuditWriter` ctor inject OMITTED (plan had it as a reserved-for-future field, would have tripped IDE0052/CA1823); SignInValidator uses lenient default `.EmailAddress()` deliberately (not strict regex — sign-in failure UX should be same 401 whether email format-bad or just wrong)

## Open follow-up tasks (tracker IDs)

- **#14:** Add test data cleanup to tenant integration tests (Respawn / `ExecuteDeleteAsync IgnoreQueryFilters` / transaction rollback). Pending. Becoming more pressing — we now have 14 integration tests, several creating Firm+User combinations with unique slugs/emails per run, but no cleanup between runs.
- **#17:** Add `(firm_id, actor_user_id, occurred_at)` audit index + AuditAction XML doc. Pending.
- **#19:** Document persistence conventions (snake_case adoption, no manual HasColumnName, schema name, migration history table, consolidated-migration rationale) — `docs/conventions/persistence.md` or README section. Pending.
- **#20:** Move dev DB password AND JWT secret to `dotnet user-secrets`. Pending. JWT secret added to the same file in Task 26 so the issue compounded.
- **Closed during this run:** #15 (CA1861 .editorconfig — Task 19), #16 (column naming — Task 19), #18 (Task 19 reviews — Task 19a), #21 (ConflictException — Task 23)

## Execution metrics through Task 27

~32 implementer dispatches + ~16 reviewer dispatches + 3 inline fix dispatches = ~51 subagent dispatches. Average ~1.9 subagents per task. Tasks 8, 10, 15, 16, 17, 18, 20, 21, 24, 25, 26, 27 skipped formal reviews (small/verbatim/well-tested-by-design). Tasks 11, 12, 13, 14, 19, 22, 23 received full review pairs and produced material fix commits.

## Current state at pause

- Branch: `feature/foundation-core`
- Last commit: `a1f320e` (Task 28 — authz policies + admin endpoints)
- Tasks complete: **28 of 40 (70%)**
- Working tree: clean
- Docker: postgres + seq still running locally (postgres healthy; required for integration tests)
- Build: 0 warnings, 0 errors
- Tests: **54 passing** (6 SharedKernel + 32 PracticeOperations.UnitTests + 16 PracticeOperations.IntegrationTests)
- Vulnerability scan: clean (last verified at Task 21a commit)
- Dev DB note: still contains stale PascalCase-era migration rows from before Task 19 regeneration; anyone running `dotnet ef database update` against dev needs to drop the `practice_operations` schema first

## To resume next session

1. Read this file.
2. Check out `feature/foundation-core`.
3. Verify Docker: `docker compose -f docker/docker-compose.yml ps`
4. Optionally back-fill reviews for Tasks 24-28 (all skipped — small/verbatim with adequate test coverage; not strictly owed). Otherwise proceed.
5. **Dispatch Task 29**: Serilog with structured properties. Spec in `docs/superpowers/plans/2026-05-11-foundation-core.md` near line 4013.
6. Phases left: Phase 9 = observability (Serilog/OpenTelemetry/CorrelationId, Tasks 29-31); Phase 10 = Vite/React/TS frontend (Tasks 32-35); Phase 11 = Docker + GitHub Actions CI/CD (Tasks 36-39); Phase 12 = end-to-end Playwright (Task 40).

---

## What's done

| Artifact | Location | Notes |
|---|---|---|
| Domain research | `accountancy-practice-research.md` | 22-area working model of an SMB UK practice + ecosystem context + candidate domain model. ~165 KB / ~2,400 lines. |
| Functional requirements spec | `docs/superpowers/specs/2026-05-10-saas-functional-requirements-design.md` | 367 numbered FRs across all 22 areas with cross-cutting requirements (Section 2), per-area template, glossary, 22 open questions. ~133 KB / ~1,290 lines. |
| Implementation plan — Sub-plan 1a Foundation Core | `docs/superpowers/plans/2026-05-11-foundation-core.md` | 40 TDD tasks across 12 phases. ~174 KB / 4,189 lines. **28 of 40 executed.** |

## Decisions locked in earlier sessions

- **Product positioning:** All-in-one practice-management platform (Karbon / TaxDome / BrightManager competitive set)
- **Target firm:** UK practices, **5–20 staff**
- **Build-vs-integrate stance:** **Orchestrator** — integrate Xero, BrightPay, TaxCalc/IRIS, AML/KYC providers, GoCardless, Companies House, HMRC. Do **not** build a ledger or any tax computation engine.
- **Differentiator:** **AI-native from day one** (agentic / ambient AI inside every workflow, not bolted on)
- **Phasing:** 6 MVP / 5 Phase 2 / 8 Phase 3 / 4 Backlog (see spec §3.1)
- **MVP areas:** B1 Onboarding, B2 Workflow, B3 Billing, B4 Documents/Portal, B5 Comms/Queries, A1 Bookkeeping orchestration
- **Tech stack:** **.NET 10 minimum** backend + **React + TypeScript** frontend
- **Sub-plan decomposition (9 sub-plans):**
  1a. Foundation Core ← **in flight, 70% complete**
  1b. Foundation Extended — M365 + Google SSO, integration-framework skeleton, data-class tagging + retention framework
  2. Spine — EngagementAndCompliance core
  3. Client domain & B1 onboarding (incl. AML)
  4. Document domain & B4 portal
  5. Comms & B5
  6. Billing & B3
  7. A1 Bookkeeping orchestration (Xero/QBO ACL)
  8. AI seam (cross-cutting)

## Architecture decision (locked 2026-05-11)

**Selective DDD + 4 bounded contexts + vertical slices** (modular monolith).

| Pattern | Apply where |
|---|---|
| Bounded contexts | **4 at MVP**: PracticeOperations, ClientRelationship, EngagementAndCompliance, BillingAndCash |
| Ubiquitous language | Everywhere (UK accountancy terms in code) |
| Anti-corruption layers | Every external integration |
| Rich aggregates | Core complex domain only — `Client`, `Engagement`, `Job`, `Filing`, `RiskAssessment`, `WipBalance` |
| Anaemic CRUD | Supporting subdomains — `Document` storage, `IntegrationConnection`, branding/admin |
| Domain events | In-process via MediatR (dispatcher in a later sub-plan; events are raised on aggregates but not yet flushed) |
| CQRS | Selective — separate read projections for dashboards/reporting |
| Event sourcing | **Not at MVP**; reassess for `Filing` / `Document` as v2 strategic decision |
| Implementation idiom | **Vertical slices** inside each context (feature folders) — e.g. `Application/Firms/Register/`, `Application/Users/SignIn/`, `Application/Users/EnrollTotp/` |
| Clean Architecture layers | Inside each context (Domain → Application → Infrastructure → API) |

## Foundation scope at a glance

What's in Sub-plan 1a:
- .NET 10 solution scaffold with Clean Architecture per bounded context ✓
- Multi-tenant data isolation (`FirmId` row-level partitioning) ✓
- Authentication: email/password + TOTP MFA + JWT bearer ✓
- Role model from spec §2.2 (FirmOwner, Partner, Manager, FeeEarner, PracticeAdmin) ✓ — policies wired in Task 28
- Append-only immutable audit log ✓
- React + TypeScript frontend shell **(pending, Tasks 32–35)**
- PostgreSQL + EF Core with snake_case ✓
- Cyber Essentials Plus baseline (MFA, audit log, encryption at rest/in transit) ✓ for auth/audit, TLS via deployment
- Observability **(pending, Tasks 29–31)** — Serilog, OpenTelemetry, CorrelationId middleware
- CI/CD scaffold **(pending, Tasks 36–39)**

Out of scope here (covered later):
- Tenant lifecycle / trial / offboarding (Sub-plan 1b)
- Microsoft 365 + Google SSO (Sub-plan 1b)
- Passwordless magic-link + WebAuthn for client portal (Sub-plan 4)
- Data-class tagging + retention framework (Sub-plan 1b)
- Integration-framework skeleton (Sub-plan 1b)
- Any business domain — Engagement, Job, Filing, Client (Sub-plans 2 and 3)
- Document storage (Sub-plan 4)
- AI gateway and governance (Sub-plan 8)
- External regulatory integrations (Xero/HMRC/AML — respective sub-plans)
- Branding / theming

---

*Continue from this file when resuming. Auth + authz foundation is now complete — register, sign-in, MFA, JWT bearer, role-gated endpoints all working end-to-end. Next concrete action is Task 29 (Serilog with structured properties) — start of Phase 9 observability.*
