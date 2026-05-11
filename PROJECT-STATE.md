# Project State — SaaS for SME UK Accountancy Practices

**Last working session:** 2026-05-11
**Status:** Paused after Task 10. Executing Sub-plan 1a (Foundation Core) via subagent-driven-development. **10 of 40 tasks complete** on branch `feature/foundation-core`. Next: **Task 11** (cross-tenant isolation integration test — first test that actually exercises the query filter via a temporary `TenantTestRow` entity).

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
| 8 | `48a8aa4` | Initial empty EF migration |
| 9 | `8530c41` | IFirmContext abstraction + HttpContext-backed accessor in Web |
| 10 | `ed9b1fe` | DbContext applies ITenantScopedEntity global query filter (currently inert — no entities implement the marker yet) |

**Plan deviations to be aware of** (already documented in plan file):
- Task 2: test-tool package versions bumped to SDK-template baseline; `coverlet.collector` 6.0.4 added (test-tooling only)
- Task 3: `SEQ_FIRSTRUN_NOAUTHENTICATION: "true"` required for Seq 2025.2+ local dev
- Task 4: `CA1000` suppressed in-file on `Result<T>`; `CA1707` suppressed in test csprojs (xunit naming)
- Task 5: `System.Security.Cryptography.Xml 10.0.7` pinned to mitigate CVE in EF Design's transitive deps
- Task 6: 5th package required for `AddDbContextCheck<T>` (`Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`); EF Core bumped to 10.0.7
- Task 7: `CA1711` suppression added (xunit `[CollectionDefinition]` marker-type naming). Test goes GREEN immediately not RED — `Migrate()` no-op + `CanConnectAsync()` only. Plan updated to reflect this.

**Execution metrics:** 10 implementer dispatches + 7 reviewer dispatches = 17 subagent dispatches. Avg ~1.7 reviewers per task (some combined, some skipped for pure tooling output like Task 8).

**To resume tomorrow:** Read this file, check out `feature/foundation-core`, ensure Docker is running (`docker compose -f docker/docker-compose.yml up -d`), then dispatch the implementer for Task 11. The Task 11 spec is in `docs/superpowers/plans/2026-05-11-foundation-core.md` under "### Task 11".

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
