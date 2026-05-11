# Project State — SaaS for SME UK Accountancy Practices

**Last working session:** 2026-05-11
**Status:** Paused after Task 19 implementation (reviews skipped per user pause). Executing Sub-plan 1a (Foundation Core) via subagent-driven-development. **19 of 40 tasks complete** on branch `feature/foundation-core`. Next: **Task 19 reviews + follow-ups** (spec compliance review, code quality review for snake_case adoption + consolidated migration + ApiFactory fix), then Task 20 (Repository abstractions + EF implementations).

**Tasks completed (commits on `feature/foundation-core`, in order):**
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
| 10 | `ed9b1fe` | DbContext applies ITenantScopedEntity global query filter (filter expression later fixed in Task 11) |
| 11 | `03dd4cc` | Cross-tenant isolation integration test via TenantTestRow; rewrote Task 10 filter to use FirmId-on-both-sides (Npgsql translatable) |
| – | `5731cbd` | docs(state): pause after Task 10 record |
| 12 | `82366cb` | AuditEvent entity + EF configuration + migration (later regenerated in Task 19) |
| 13 | `e72c89b` | IAuditWriter + EfAuditWriter; append-only enforced in SaveChanges; FrameworkReference Microsoft.AspNetCore.App superseded transitively the System.Security.Cryptography.Xml CVE pin |
| 13a | `6618439` | Fix(audit): guard moved to 2-arg SaveChanges leaf overloads; dead CPM pins dropped |
| 14 | `3113cfa` | MediatR AuditingBehavior pipeline (no audited commands exist yet) |
| 15 | `fd7862a` | EmailAddress value object with validation + lowercase normalization |
| 16 | `2c1bf91` | Role, UserStatus, FirmStatus enums |
| 17 | `1450c73` | Firm aggregate with Register/Activate + FirmRegistered event |
| 18 | `7fd14bc` | User aggregate with Register/Activate/EnrollTotp + RecordSuccessful/FailedSignIn + UserRegistered event |
| 19 | `0d87bed` | EF mappings for Firm + User; adopted snake_case via EFCore.NamingConventions 10.0.1; **regenerated to single consolidated Initial migration**; .editorconfig CA1861 suppression for Migrations folder; **fixed ApiFactory test-fixture bug** (tests had been hitting dev DB not testcontainer) |

**Plan deviations to be aware of** (already documented in plan file):
- Task 2: test-tool package versions bumped to SDK-template baseline; `coverlet.collector` 6.0.4 added (test-tooling only)
- Task 3: `SEQ_FIRSTRUN_NOAUTHENTICATION: "true"` required for Seq 2025.2+ local dev
- Task 4: `CA1000` suppressed in-file on `Result<T>`; `CA1707` suppressed in test csprojs (xunit naming)
- Task 5: `System.Security.Cryptography.Xml 10.0.7` pinned to mitigate CVE — pin removed in Task 13 (FrameworkReference supersedes)
- Task 6: 5th package required for `AddDbContextCheck<T>`; EF Core bumped to 10.0.7
- Task 7: `CA1711` suppression added (xunit `[CollectionDefinition]` marker-type naming); test goes GREEN immediately not RED
- Task 10: Original query filter `e.FirmId.Value == CurrentFirmIdRaw` was Npgsql-untranslatable; Task 11 rewrote to `e.FirmId == CurrentFirmId.Value`
- Task 11: CA1861 hoist in scaffolded migration (recurring; resolved globally in Task 19 via .editorconfig)
- Task 12: FluentAssertions added via CPM versionless ref (plan's `dotnet add package` was wrong); CA1861 hoist again
- Task 13: AuditAction.Unknown rejection added; FrameworkReference Microsoft.AspNetCore.App adopted; 3 PackageReferences pruned (NU1510); CVE pin no longer needed; guard moved to 2-arg SaveChanges leaf overloads in follow-up commit `6618439`
- Task 14: CA1725 rename `ct` → `cancellationToken` to match MediatR 12.x interface base parameter
- Task 17: en-dash in error message replaced with ASCII hyphen
- Task 19 (largest deviation): adopted snake_case naming convention project-wide; regenerated all migrations as one consolidated `Initial`; added `.editorconfig` glob suppression for CA1861 in Migrations folder; **fixed latent ApiFactory bug** where tests had been hitting the dev DB rather than the Testcontainer because `ConfigureAppConfiguration` was being overridden by SUT's `appsettings.json` (switched to `UseSetting`); moved dev connection string to `appsettings.Development.json`. Dev DB has stale migration history — anyone running `dotnet ef database update` against it needs to drop the schema first.

**Open follow-up tasks (tracker IDs):**
- #14: Add test data cleanup to tenant integration tests (Respawn / `ExecuteDeleteAsync IgnoreQueryFilters` / transaction rollback) before more tenant-scoped tests pile on. Pending.
- #15: CA1861 .editorconfig suppression — **done as part of Task 19**.
- #16: Decide column naming convention — **done as part of Task 19** (snake_case adopted).
- #17: Add `(firm_id, actor_user_id, occurred_at)` index + AuditAction docs. Pending.
- **NEW (from Task 19):** Reviews of commit `0d87bed` not yet run. Spec compliance + code quality review for: snake_case adoption, consolidated Initial migration shape, ApiFactory `UseSetting` fix, dropped HasColumnName calls. Pending.

**Execution metrics through Task 19:** ~24 implementer dispatches + ~14 reviewer dispatches + 1 fix dispatch = ~39 subagent dispatches. Avg ~2.1 subagents per task. Tasks 8, 10, 15, 16, 17, 18 skipped formal reviews (small/verbatim/pure-tooling). Task 19 reviews still owed.

**Current state at pause:**
- Branch: `feature/foundation-core`
- Last commit: `0d87bed` (Task 19 — snake_case + Firm/User EF + ApiFactory fix)
- Tasks complete: 19 of 40 (47.5%)
- Working tree: clean
- Docker: postgres + seq running (postgres healthy as of last check)
- Build: 0 warnings, 0 errors
- Tests: 31 passing (6 SharedKernel + 20 PracticeOperations.UnitTests + 5 PracticeOperations.IntegrationTests)
- Vulnerability scan: clean
- Dev DB note: contains stale PascalCase-era migration rows; drop `practice_operations` schema if running `dotnet ef database update`

**To resume tomorrow:**
1. Read this file
2. Check out `feature/foundation-core`
3. Verify Docker: `docker compose -f docker/docker-compose.yml ps`
4. **Decide first**: run Task 19 spec/quality reviews now (paused at implementer DONE_WITH_CONCERNS), or trust the implementer report and proceed. Task 19 had the biggest deviation set so far; reviews would be valuable. Especially worth reviewing: the migration shape (snake_case columns, jsonb, composite indexes, unique constraints), the ApiFactory `UseSetting` fix correctness, and the consolidated `Initial` migration not silently dropping any constraints from the prior 3 migrations.
5. Then dispatch Task 20 (Repository abstractions + EF implementations). The Task 20 spec is in `docs/superpowers/plans/2026-05-11-foundation-core.md` near line 2687.

---

## What's done

| Artifact | Location | Notes |
|---|---|---|
| Domain research | `accountancy-practice-research.md` | 22-area working model of an SMB UK practice + ecosystem context + candidate domain model. ~165 KB / ~2,400 lines. |
| Functional requirements spec | `docs/superpowers/specs/2026-05-10-saas-functional-requirements-design.md` | 367 numbered FRs across all 22 areas with cross-cutting requirements (Section 2), per-area template, glossary, 22 open questions. ~133 KB / ~1,290 lines. |
| Implementation plan — Sub-plan 1a Foundation Core | `docs/superpowers/plans/2026-05-11-foundation-core.md` | 40 TDD tasks across 12 phases. ~174 KB / 4,189 lines. Not yet executed. |

## Decisions locked in earlier sessions

- **Product positioning:** All-in-one practice-management platform (Karbon / TaxDome / BrightManager competitive set)
- **Target firm:** UK practices, **5–20 staff**
- **Build-vs-integrate stance:** **Orchestrator** — integrate Xero, BrightPay, TaxCalc/IRIS, AML/KYC providers, GoCardless, Companies House, HMRC. Do **not** build a ledger or any tax computation engine.
- **Differentiator:** **AI-native from day one** (agentic / ambient AI inside every workflow, not bolted on)
- **Phasing:** 6 MVP / 5 Phase 2 / 8 Phase 3 / 4 Backlog (see spec §3.1)
- **MVP areas:** B1 Onboarding, B2 Workflow, B3 Billing, B4 Documents/Portal, B5 Comms/Queries, A1 Bookkeeping orchestration
- **Tech stack:** **.NET 10 minimum** backend + **React + TypeScript** frontend
- **Sub-plan decomposition (9 sub-plans, agreed; Foundation split 2026-05-11):**
  1a. Foundation Core — solution scaffold, DB, tenant isolation, audit log, email/password+TOTP auth, role model, observability, React shell, CI/CD  ← **being planned now**
  1b. Foundation Extended — M365 + Google SSO, integration-framework skeleton, data-class tagging + retention framework
  2. Spine — EngagementAndCompliance core (Engagement + Job + RecurringJobSchedule + Task)
  3. Client domain & B1 onboarding (incl. AML)
  4. Document domain & B4 portal (incl. client-portal magic-link + WebAuthn auth)
  5. Comms & B5
  6. Billing & B3
  7. A1 Bookkeeping orchestration (Xero/QBO ACL)
  8. AI seam (cross-cutting)

## Architecture decision (locked 2026-05-11)

**Selective DDD + 4 bounded contexts + vertical slices** (modular monolith).

| Pattern | Apply where |
|---|---|
| Bounded contexts | **4 at MVP** (reduced from 6 after stress-test): PracticeOperations, ClientRelationship, EngagementAndCompliance, BillingAndCash. Split EngagementAndCompliance and extract AdvisoryServices later if seams emerge. |
| Ubiquitous language | Everywhere (UK accountancy terms in code) |
| Anti-corruption layers | Every external integration |
| Rich aggregates | Core complex domain only — `Client`, `Engagement`, `Job`, `Filing`, `RiskAssessment`, `WipBalance` |
| Anaemic CRUD | Supporting subdomains — `Document` storage, `IntegrationConnection`, branding/admin |
| Domain events | In-process via MediatR |
| CQRS | Selective — separate read projections for dashboards/reporting |
| Event sourcing | **Not at MVP**; reassess for `Filing` / `Document` as v2 strategic decision |
| Implementation idiom | **Vertical slices** inside each context (feature folders) rather than horizontal service layers |
| Clean Architecture layers | Inside each context (Domain → Application → Infrastructure → API) |

**Reasoning captured here so it isn't relitigated:** the original 6-context proposal split `EngagementWorkflow` from `ComplianceServices`, but a Job and a Filing share lifecycle — the seam is chatty. Merging them into `EngagementAndCompliance` is honest at MVP; we split later if/when the boundary becomes natural in code. `AdvisoryServices` was thin at MVP and folds into Engagement until it grows.

## To resume tomorrow

1. Re-ask the DDD stance question (or pick up where they left off if they've decided).
2. Write the **Foundation sub-plan** as `docs/superpowers/plans/2026-05-11-foundation.md` (or whatever date) following the writing-plans skill format:
   - Plan header with Goal / Architecture / Tech Stack
   - File structure first
   - Bite-sized TDD tasks (2–5 minutes each)
   - Real code in every step (no placeholders)
   - Testing commands + expected output
   - Frequent commits
3. Foundation scope is spec Section 2 cross-cutting (XR-2.1, 2.2, 2.3, 2.5 skeleton, 2.6) — **excluding** AI governance (XR-2.4) which goes in Sub-plan 8.
4. After plan is written, offer execution choice: **subagent-driven** (recommended) vs **inline execution**.

## Foundation scope at a glance

What's in Sub-plan 1:

- .NET 10 solution scaffold with Clean Architecture per bounded context
- Multi-tenant data isolation (`FirmId` row-level partitioning)
- Tenant lifecycle (signup → trial → paid; offboarding + 30-day retention)
- Authentication: Microsoft 365 + Google Workspace SSO + email/password+TOTP fallback
- MFA mandatory for staff; passwordless magic-link + WebAuthn for client portal
- Role model from spec §2.2 (FirmOwner, Partner, Manager, FeeEarner, PracticeAdmin, MLRO overlay, DPO overlay; ClientPrimary, ClientStaff, ClientReadOnly)
- Append-only immutable audit log
- Data-class tagging + retention policy framework
- Integration-framework skeleton (port pattern, OAuth wrappers, token rotation, health monitoring)
- React + TypeScript frontend shell with route guards, role-aware UI, basic admin shell
- PostgreSQL + EF Core
- Cyber Essentials Plus baseline (MFA, encryption at rest/in transit, audit log, backup)
- Observability: structured logging (Serilog), distributed tracing (OpenTelemetry), per-tenant metrics
- CI/CD scaffold; Azure deployment target

What's NOT in Sub-plan 1 (covered in later sub-plans):

- Any business domain (Engagement, Job, Filing, Client) — those come in Sub-plans 2 and 3
- Document storage (Sub-plan 4)
- AI gateway and governance (Sub-plan 8)
- Any external regulatory integration (Xero/HMRC/AML — come in their respective sub-plans)
- Branding / theming (light support only at MVP foundation; richer in B8 Phase 2)

---

*Continue from this file when resuming. The brainstorming and writing-plans skills are still in-flight in their narrative, but the next concrete action — re-ask DDD, write Foundation plan — is fully captured here so you can pick up cold.*
