# SaaS for SME Accountancy Practices — Functional Requirements

**Spec date:** 10 May 2026
**Author:** brainstorming session, derived from `accountancy-practice-research.md`
**Status:** Draft for review

This document specifies the functional requirements for a SaaS product targeted at small-and-medium UK accountancy practices. It covers the 22 functional areas identified in the working-model research (sections 1.1 and 1.2 of `accountancy-practice-research.md`). Every area follows a fixed 8-subsection template so requirements can be compared apples-to-apples across the product.

This is the **starting design**. It is intentionally complete enough to guide implementation planning but light enough to remain readable. It does not specify implementation, UI screens, or data schemas — those follow in subsequent plans.

---

## Table of Contents

1. **Product Overview** — positioning, target user, build-vs-integrate stance, AI stance, phasing, out-of-scope
2. **Cross-Cutting Requirements** — multi-tenancy, identity, audit, AI governance, integrations, NFRs
3. **Functional Requirements**
   - MVP: B1, B2, B3, B4, B5, A1
   - Phase 2: A2, A3, A7, A12, B8 (proposal/pricing)
   - Phase 3: A4, A5, A6, A8, A10, A11, B6, B10
   - Backlog: B7, B9, A9, B8 (CRM/marketing)
4. **Glossary**
5. **Open Questions**

---

## 1. Product Overview

### 1.1 Positioning

An **all-in-one practice-management platform** for **small UK accountancy practices of 5–20 staff** — the segment served today by Karbon, TaxDome, BrightManager, Senta, and Pixie. The product is **AI-native from day one**: agentic AI is built into every workflow rather than bolted on as a side feature.

### 1.2 Build-vs-integrate stance: orchestrator

The product **does not build any regulatory engine or general ledger**. It orchestrates over the established UK regulatory stack:

- **Ledger:** integrate Xero, QuickBooks Online, FreeAgent, Sage Business Cloud
- **Receipt capture:** integrate Dext, Hubdoc
- **Tax computation:** integrate TaxCalc, IRIS, Capium, Xero Tax (read-only orchestration)
- **Payroll:** integrate BrightPay
- **AML / KYC:** integrate SmartSearch, Veriphy, Credas
- **Direct Debit / billing:** integrate GoCardless, Stripe
- **Companies House:** API direct
- **HMRC:** Agent Services Account direct (MTD VAT, MTD-IT)

The product **builds** the workflow, document portal, communication, billing-orchestration, AI, onboarding, and practice-management layers that tie those external systems together.

### 1.3 AI-native stance

AI is treated as a **product layer**, not a feature. Every functional area in this spec includes an explicit `AI capabilities` sub-section. AI features are subject to the cross-cutting governance in Section 2.4: permission-aware, drafted-not-sent, fully audited, with human-in-the-loop on every client-facing or HMRC-facing action.

### 1.4 Phasing

| Phase | Definition |
|---|---|
| **MVP** | Phase 1 ship. Required for first paying customer. |
| **Phase 2** | Next ship. Released within ~6 months of MVP. |
| **Phase 3** | Required for "all-in-one" claim parity with incumbents. ~12–18 months from MVP. |
| **Backlog** | Captured for completeness; not yet roadmap-committed. |

Each requirement is tagged inline with its phase and a priority (`MUST` / `SHOULD` / `COULD`).

### 1.5 Explicit out-of-scope

This spec does **not** require any of the following. Reviewers should not expect to see FRs covering them:

- **Tax computation engines** — no in-house CT600, SA, VAT, or P11D calculation engine
- **A general ledger** — we read Xero/QBO/Sage/FreeAgent; we do not compete with them
- **Payroll engine** — orchestrate BrightPay; no gross-to-net calculation logic
- **Audit methodology / ISA-driven workflow** — A9 deferred to backlog
- **Cross-firm staff / network model** — multi-firm memberships out of MVP
- **Non-UK jurisdictions** — UK only; international is a separate product decision
- **Trust accounting, insolvency, probate** — different professional licences; deferred
- **FCA-regulated financial advice** — the product is not an IFA tool
- **Direct lending / financing features** — referrals out only; nothing embedded
- **Marketing / SEO / website builder** — backlog at most; firms use external tools

### 1.6 MVP buyer narrative

The headline value proposition for a 10-staff UK practice in the MVP slice:

> *"Onboard a new client end-to-end in under 60 minutes — AML, engagement letter, Xero connect, recurring jobs, GoCardless DD — without touching a Word document. Run your firm's deadlines, queries, time, and billing from one place. Let the AI handle the document chase, the comms drafting, and the bookkeeping anomaly-spotting."*

Phase 2 extends this to: **"do your VAT, SA / MTD-IT, and management accounts here too."** Phase 3 closes the all-in-one claim.

---

## 2. Cross-Cutting Requirements

These apply to every functional area. They are stated once here and not repeated per-section. Cross-cutting requirements are tagged `XR-2.<n>.<m>`.

### 2.1 Tenancy & isolation

- **XR-2.1.1** [MVP][MUST] The product is multi-tenant. Each accountancy firm is one tenant identified by `FirmId`. All persisted data carries a tenant identifier; database-level row security enforces isolation.
- **XR-2.1.2** [MVP][MUST] No data — including AI prompts/responses, audit logs, and aggregated metrics shown to users — crosses the tenant boundary.
- **XR-2.1.3** [MVP][MUST] Tenant lifecycle: signup → 14-day trial → paid. Admin offboarding produces a full data export (JSON + PDFs of finalised documents) within 7 days of request, with 30-day retention before erasure.
- **XR-2.1.4** [Backlog][COULD] Cross-firm staff memberships (one user, multiple firms).
- **XR-2.1.5** [MVP][MUST] Tenant configuration includes firm-level branding (logo, colour, custom domain for portal) applied across all client-facing surfaces.

### 2.2 Identity & roles

**Authentication**
- **XR-2.2.1** [MVP][MUST] Firm-staff authentication supports SSO via Microsoft 365 and Google Workspace as primary providers, with email + password + TOTP MFA as fallback.
- **XR-2.2.2** [MVP][MUST] MFA is mandatory for all firm-staff accounts.
- **XR-2.2.3** [MVP][MUST] Client portal authentication supports passwordless magic-link sign-in plus biometric authentication on mobile (Face ID / fingerprint via WebAuthn).
- **XR-2.2.4** [MVP][SHOULD] MFA is optional but recommended for client accounts; firm admin can require MFA per client.
- **XR-2.2.5** [MVP][MUST] Session management: idle timeout 60 minutes default (firm-configurable 15–240 min); device-list visibility for users; admin-forced sign-out.

**Firm-side roles**
- **XR-2.2.6** [MVP][MUST] The product implements the following firm-side roles:

| Role | Scope |
|---|---|
| `FirmOwner` | Super-admin; one or more per firm; billing access |
| `Partner` | All firm data; sign-off authority |
| `Manager` | Books-of-clients view; approve work; manage juniors |
| `FeeEarner` (Senior / Junior / Bookkeeper) | Assigned clients/jobs only |
| `PracticeAdmin` | Workflow, billing, onboarding admin |
| `MLRO` *(designated principal)* | AML risk-rating + SAR authority — overlay |
| `DPO` | Data protection; SAR & retention tooling — overlay |

- **XR-2.2.7** [MVP][MUST] Permissions are evaluated as **role + scope**. Scope is the set of clients/jobs the user is assigned to (or all, for Partner+). Explicit deny rules apply to AML-restricted data and tipping-off-prohibited fields.

**Client-side roles**
- **XR-2.2.8** [MVP][MUST] The product implements the following client-side roles:

| Role | Scope |
|---|---|
| `ClientPrimary` | The owner-manager / director; full portal access |
| `ClientStaff` | Limited view (e.g. internal bookkeeper given upload rights) |
| `ClientReadOnly` | View only (e.g. spouse, lender review window) |

- **XR-2.2.9** [Phase 2][SHOULD] Client roles support time-bound access (e.g. lender granted 7-day review window).

### 2.3 Audit log & data retention

- **XR-2.3.1** [MVP][MUST] The system records an append-only, immutable audit log of every state change in every aggregate, including: actor, timestamp, action, before/after state hash, IP address, device fingerprint.
- **XR-2.3.2** [MVP][MUST] Audit logs are retained for a minimum of 7 years and are queryable by Firm Owner, MLRO, and DPO roles only.
- **XR-2.3.3** [MVP][MUST] Each domain entity is tagged with a data class (`PersonalData`, `AmlRecord`, `AuditWorkingPaper`, `TaxRecord`, `OperationalData`, `MarketingData`) which determines its retention policy.
- **XR-2.3.4** [MVP][MUST] Default retention periods (configurable up only): General records 6 yr; AML records 10 yr after relationship end; tax records 6 yr; audit working papers 6 yr; marketing data 2 yr.
- **XR-2.3.5** [MVP][MUST] GDPR right-to-erasure is implemented as redaction-with-tombstone, not deletion. Original data is replaced with a hash + redaction marker; the audit chain remains intact. Documented data-class map specifies which fields can/cannot be erased.
- **XR-2.3.6** [MVP][MUST] Subject Access Requests (SARs) are responded to within 30 calendar days; tooling produces a per-individual data extract on demand for the DPO.

### 2.4 AI governance & safety

- **XR-2.4.1** [MVP][MUST] AI features are permission-aware: AI agents only see data the calling user is authorised to see. There is no privileged-AI mode that bypasses scope.
- **XR-2.4.2** [MVP][MUST] AI outputs that are client-facing or regulator-facing (HMRC, Companies House) are produced as **drafts**. Human approval is required before any send/submit action. There are no autonomous client-facing actions in MVP.
- **XR-2.4.3** [MVP][MUST] Every AI-generated artifact is tagged with: model identifier, prompt template version, source document IDs, generation timestamp, the user who triggered it, and (after review) the user who approved it.
- **XR-2.4.4** [MVP][MUST] Every AI suggestion in the UI provides a "Why?" panel showing the source documents and reasoning summary, plus a "Reject + reason" path that captures the rejection reason for evaluation feedback.
- **XR-2.4.5** [MVP][MUST] No client personal data leaves the EU/UK data residency boundary. Calls to LLM providers go through a controlled gateway that either (a) performs PII redaction before egress, or (b) routes to an EU/UK-resident inference endpoint.
- **XR-2.4.6** [MVP][MUST] All AI prompts and responses are logged for 90 days for regulatory enquiry support; aggregate AI metrics (tokens, latency, accuracy) are retained for 2 years.
- **XR-2.4.7** [MVP][MUST] Every AI feature has a documented evaluation set with regression-tested accuracy thresholds; new model versions cannot be promoted without passing evaluation.
- **XR-2.4.8** [Phase 2][SHOULD] Firms can opt out of individual AI features at firm level (e.g. "do not use AI on client comms").

### 2.5 Integration framework

- **XR-2.5.1** [MVP][MUST] Every external integration is wrapped in a documented adapter (port pattern). Provider implementations are swappable without changing consuming code.
- **XR-2.5.2** [MVP][MUST] Integration tokens are stored encrypted (envelope encryption, KMS-managed). Firm admins can view, refresh, revoke tokens.
- **XR-2.5.3** [MVP][MUST] Integration health is continuously monitored. Broken bank feeds, expired tokens, rate-limit hits, and webhook failures are surfaced to the Firm Owner / Practice Admin within 1 hour of detection.
- **XR-2.5.4** [MVP][MUST] OAuth 2.0 flows are used wherever supported by the provider (Xero, QBO, MS Graph, Google, GoCardless, Stripe, HMRC ASA).
- **XR-2.5.5** [MVP][MUST] **MVP integrations live:** Xero, QuickBooks Online, Dext, SmartSearch (or one equivalent AML provider), GoCardless, Companies House API, HMRC Agent Services Account (MTD VAT + MTD-IT scope), Microsoft Graph, Google Workspace.
- **XR-2.5.6** [Phase 2][MUST] Integration set extends to: TaxCalc or one equivalent tax engine (read-only computation results), BrightPay, Stripe, FreeAgent, Sage Business Cloud, IRIS Open Suite (read-only).
- **XR-2.5.7** [Phase 3][SHOULD] Slack, Microsoft Teams, Zapier, Make, native Karbon-style email triage protocols.

### 2.6 Non-functional requirements

| ID | Category | Requirement | Phase |
|---|---|---|---|
| **XR-2.6.1** | Performance | Page load p95 < 2 s; search p95 < 500 ms; bulk operations (e.g. import 200 clients) < 5 min | MVP |
| **XR-2.6.2** | Availability | 99.9% uptime SLO with announced maintenance windows | MVP |
| **XR-2.6.3** | Scale | Designed for 200 firms × 500 clients × 10 staff at MVP; horizontally scalable architecture from day one | MVP |
| **XR-2.6.4** | Security | Cyber Essentials Plus from launch; SOC 2 Type II within 18 months; ISO 27001 within 24 months | MVP / Phase 2 |
| **XR-2.6.5** | Data | GDPR by design; UK/EU data residency only; AES-256 at rest; TLS 1.3 in transit | MVP |
| **XR-2.6.6** | Accessibility | WCAG 2.1 AA across firm and client surfaces | MVP |
| **XR-2.6.7** | Browsers | Chrome, Edge, Safari, Firefox — latest 2 major versions | MVP |
| **XR-2.6.8** | Mobile | Responsive web for staff; PWA or native iOS/Android for client portal (camera capture is critical) | MVP |
| **XR-2.6.9** | Backup | Daily snapshots, 35-day retention, point-in-time recovery to last 24 hours | MVP |
| **XR-2.6.10** | Disaster Recovery | RPO 1 hour, RTO 4 hours | MVP |
| **XR-2.6.11** | Observability | Centralised structured logs, distributed traces, real-user monitoring, per-tenant usage analytics | MVP |
| **XR-2.6.12** | Localisation | UK English only at MVP; en-GB date / currency formats | MVP |

---

## 3. Functional Requirements

### B1 — Client Onboarding & Engagement

**B1.0 Phase tag** — MVP

**B1.1 Purpose & user personas**

To convert a prospect into a fully scoped, AML-compliant, system-configured paying client without the firm touching a Word document. Personas: `Prospect` → `ClientPrimary`, `PracticeAdmin`, `Manager`, `Partner`, `MLRO`.

**B1.2 User stories**

- **US-B1.1** As a `PracticeAdmin`, I want to triage a new enquiry from the website or referral so I can decide quickly whether to progress.
- **US-B1.2** As a `PracticeAdmin`, I want a structured discovery form to capture prospect facts (entity type, turnover band, transaction volume, payroll size, services wanted) so proposals are consistent.
- **US-B1.3** As a `Manager`, I want a fee proposal generated automatically based on complexity inputs so we don't underprice or spend an hour quoting.
- **US-B1.4** As a `Prospect`, I want to receive and e-sign an engagement letter on my phone in under 2 minutes.
- **US-B1.5** As a `PracticeAdmin`, I want AML/KYC checks run automatically when the prospect uploads ID, with clear pass/fail/manual-review outcomes.
- **US-B1.6** As an `MLRO`, I want every new client risk-rated, with high-risk clients blocked from work until I sign off.
- **US-B1.7** As a `PracticeAdmin`, I want professional clearance sent to the prior accountant automatically with a templated email.
- **US-B1.8** As a `PracticeAdmin`, I want HMRC agent authorisations (ASA + 64-8 codes) requested as part of onboarding.
- **US-B1.9** As a `PracticeAdmin`, I want recurring jobs (year-end, VAT, payroll, SA, CS01) auto-scheduled the moment the engagement is signed.
- **US-B1.10** As a `ClientPrimary`, I want a single welcome screen telling me what to do next: connect Xero, set up Direct Debit, sign final docs.

**B1.3 Functional requirements**

- **FR-B1.1** [MVP][MUST] The system supports inbound enquiry intake via web form, email-to-inbox, manual entry, and integrated marketing channels (HubSpot/Mailchimp webhook in MVP).
- **FR-B1.2** [MVP][MUST] Each enquiry creates a `Prospect` record holding all interactions, documents, and activities through to conversion or abandonment.
- **FR-B1.3** [MVP][MUST] A configurable discovery form captures: entity type (sole trader / Ltd / partnership / LLP), expected turnover band, monthly transaction volume, employees, VAT status, year-end date, services wanted (pick from firm's service catalogue).
- **FR-B1.4** [MVP][MUST] The system generates a fee proposal from a firm-defined fee model that maps complexity inputs (turnover, volume, employees, services) to a monthly subscription price.
- **FR-B1.5** [MVP][MUST] Proposals support three named tiers per service (Starter / Growth / Scale) and a "Custom" override; firms edit tier definitions in B8 (Phase 2) — at MVP, tier definitions are configured by Firm Owner via admin UI.
- **FR-B1.6** [MVP][MUST] Proposals are issued as a single web link (no PDF attachment). The prospect can review services, fees, and exclusions on any device.
- **FR-B1.7** [MVP][MUST] Engagement letters are generated from a firm-managed template merged with proposal data; templates support clause libraries for service-specific scope language.
- **FR-B1.8** [MVP][MUST] Engagement letters are e-signed using the embedded e-signature provider (see Open Questions); both sides receive a copy; signed copy is stored immutably in B4.
- **FR-B1.9** [MVP][MUST] Customer Due Diligence (CDD) workflow runs for every Beneficial Owner, Officer, and Controller (BOOC) — directors, shareholders ≥25%, sole traders. Required: ID document, address proof, sanctions/PEP screen.
- **FR-B1.10** [MVP][MUST] CDD integrates with the chosen AML provider (see Section 2.5.5). Outcome states: `Pass`, `ReferToHuman`, `Fail`. Each outcome is logged with full provider response.
- **FR-B1.11** [MVP][MUST] A Money-Laundering Risk Assessment (MLR 2017 §28) runs per client, scoring on: geography, sector, transaction profile, structure complexity. Outcome rating: `Low` / `Medium` / `High`.
- **FR-B1.12** [MVP][MUST] Clients rated `High` are placed in `PendingMLROApproval` state. No work activities (job creation, document requests, agent authorisation) can begin while in this state.
- **FR-B1.13** [MVP][MUST] The MLRO can approve, reject, or escalate a high-risk client; approval requires a written rationale stored in the audit log.
- **FR-B1.14** [MVP][MUST] Professional clearance is sent to the prior accountant via templated email with a structured request: confirmation of no professional reasons, prior-year accounts, prior-year tax returns, payroll history, software access.
- **FR-B1.15** [MVP][MUST] HMRC agent authorisation requests (Agent Services Account onboarding for MTD VAT + MTD-IT; 64-8 / online agent code requests for SA, PAYE, CT, CIS) are initiated from the onboarding flow with status tracked through HMRC's reply.
- **FR-B1.16** [MVP][MUST] Companies House authentication code request is triggered when the firm acts as filing agent.
- **FR-B1.17** [MVP][MUST] Software setup is initiated as part of onboarding: Xero/QuickBooks invite, client portal credentials, GoCardless DD mandate.
- **FR-B1.18** [MVP][MUST] On engagement-letter signature **and** completion of CDD **and** MLRO sign-off (where required), the system: creates the `Client`; spawns recurring jobs based on services subscribed; activates the fee mandate; sends the welcome pack to the client primary contact.
- **FR-B1.19** [MVP][MUST] Onboarding state is visible in real time to the practice via a single onboarding dashboard showing each prospect's stage, blockers, and SLA timer.
- **FR-B1.20** [Phase 2][SHOULD] Bulk onboarding: import a list of clients from a prior PM tool, with mapping wizard for service mix, fees, recurring schedules, and bulk AML re-screening.

**B1.4 AI capabilities**

- **AI-B1.1** [MVP][MUST] AI pre-populates the discovery form by reading: prospect's website, Companies House public record, last filed accounts (where the prospect uploads them), and any prior-accountant info the prospect provides.
- **AI-B1.2** [MVP][MUST] AI generates an explainable AML risk score by analysing: entity structure complexity, sector risk, country risk for any non-UK BOOCs, public adverse-media screening. Output is a draft score with reasoning; the MLRO confirms or overrides.
- **AI-B1.3** [MVP][MUST] AI drafts the proposal narrative and the engagement letter content from the firm's templates, customised with the prospect's specific facts. The user reviews and approves before issue.
- **AI-B1.4** [Phase 2][SHOULD] AI suggests the appropriate fee tier and identifies likely upsell pathways (e.g. "looks like an R&D candidate" or "VAT registration imminent at current growth rate").
- **AI-B1.5** [Phase 2][SHOULD] AI drafts the professional clearance email, customised to the prior accountant's typical format.

**B1.5 Integrations consumed**

- AML provider (SmartSearch / Veriphy / Credas — TBD)
- Companies House API
- HMRC Agent Services Account (ASA)
- Xero / QuickBooks Online (client invite)
- E-signature provider
- GoCardless (Direct Debit mandate setup)
- Email (Microsoft Graph / Google Workspace) — outbound proposals, clearance, welcomes

**B1.6 Data entities**

References §8 of the research doc (`ClientRelationship` and `EngagementWorkflow` contexts).

- `Prospect` — aggregate root for pre-conversion state
- `Client` (created on conversion) — aggregate root with `BeneficialOwner`, `Contact`, `RiskAssessment`, `OngoingMonitoringSchedule`, `IdVerification`
- `Engagement` — aggregate root with `EngagementLetter`, `Scope`, `FeeAgreement`, `LiabilityCap`
- `RecurringJobSchedule` — generated from Engagement on signature
- `FeeMandate` — links to GoCardless mandate ID and billing schedule

**B1.7 Acceptance criteria**

- **AC-B1.1** An onboarding completes end-to-end (enquiry → live client with first scheduled job) in **under 60 minutes** when no exceptions arise.
- **AC-B1.2** 100% of clients have a completed CDD file with risk assessment **before any service-line job is created**.
- **AC-B1.3** No engagement is created without a counter-signed engagement letter on file.
- **AC-B1.4** High-risk clients are blocked from work until MLRO approval is recorded.
- **AC-B1.5** All HMRC agent authorisation requests are tracked through to confirmed acceptance, with overdue requests surfaced as alerts.

**B1.8 Open questions**

- Which AML provider (SmartSearch / Veriphy / Credas) — affects integration depth and per-check cost passthrough.
- Which e-signature provider (build vs SignWell / HelloSign / Adobe Sign).
- Companies House ECCTA ACSP wrapper: does the product enable the firm to act as ACSP through us, or does the firm self-register?
- Bulk migration from incumbent PM tools: which formats to support natively (Karbon, TaxDome, BrightManager, Senta, IRIS).

---

### B2 — Practice / Workflow Management

**B2.0 Phase tag** — MVP

**B2.1 Purpose & user personas**

The spine of the product. Orchestrates every recurring and ad-hoc piece of work the firm does — scheduling jobs to deadlines, allocating tasks to people, surfacing bottlenecks, automating routine status changes and client comms. Personas: `PracticeAdmin`, `Manager`, `FeeEarner`, `Partner`, `FirmOwner`.

**B2.2 User stories**

- **US-B2.1** As a `PracticeAdmin`, I want recurring jobs auto-created from each client's services so deadlines never go missed.
- **US-B2.2** As a `Manager`, I want to see my book's deadline pipeline by week so I can rebalance work before bottlenecks form.
- **US-B2.3** As a `FeeEarner`, I want a "today" queue of tasks so I know exactly what to work on without firefighting.
- **US-B2.4** As a `FeeEarner`, I want to change job status with one click (with-client / with-manager / ready-for-review / complete) so the firm knows where work is in real time.
- **US-B2.5** As a `Partner`, I want a real-time view of work-at-risk (deadlines breaching SLAs, queries stale, capacity exceeded) so I can intervene early.
- **US-B2.6** As a `Manager`, I want capacity alerts when forecast hours exceed available so I can rebalance proactively.
- **US-B2.7** As a `PracticeAdmin`, I want to define job templates with reusable task checklists so the firm runs the same playbook on every client.
- **US-B2.8** As a `Manager`, I want every job to have a budget vs actual hours view so I can spot scope creep.

**B2.3 Functional requirements**

- **FR-B2.1** [MVP][MUST] The system maintains a **Job Template** catalogue. Each template defines: service line (A1–A12), recurrence rule, default owner / reviewer roles, task checklist, expected hours, deadline rule, dependent integrations.
- **FR-B2.2** [MVP][MUST] On engagement signature (B1), the system instantiates the appropriate **RecurringJobSchedule** per service subscribed; future Job instances are auto-generated at the scheduled cadence.
- **FR-B2.3** [MVP][MUST] Each `Job` has: status (`NotStarted`, `InProgress`, `WithClient`, `WithManager`, `ReadyForReview`, `Complete`, `OnHold`), owner, reviewer, deadline (statutory + internal SLA), priority, hours budget, hours actual.
- **FR-B2.4** [MVP][MUST] A `Job` consists of a checklist of `Task` items inherited from the Job Template. Tasks support sub-status, owner, due date, dependency on other tasks, and optional document/data attachments.
- **FR-B2.5** [MVP][MUST] Status transitions are user-driven via single-click controls; every transition is timestamped and logged.
- **FR-B2.6** [MVP][MUST] Each user has a **personal Today queue** showing assigned tasks ordered by deadline urgency, dependency readiness, and explicit priority.
- **FR-B2.7** [MVP][MUST] **Capacity dashboard**: forecast hours per fee earner per week, derived from active Jobs' budget hours, with traffic-light view of over/under capacity.
- **FR-B2.8** [MVP][MUST] **Deadline risk register**: jobs flagged when (a) deadline is within 14 days and status not `ReadyForReview`/`Complete`; (b) job is over budget by >20%; (c) reviewer queue depth exceeds threshold.
- **FR-B2.9** [MVP][MUST] **Query age tracking** integrates with B5: outstanding-with-client queries surface in B2 dashboards; jobs cannot move to `Complete` while client queries are open.
- **FR-B2.10** [MVP][MUST] When a job moves to `Complete`, the system: locks the working file (B4), triggers a billable event (B3), spawns the next-period recurrence, and notifies the client via B5.
- **FR-B2.11** [MVP][MUST] **Job profitability**: budget vs actual hours per job and per recurring schedule, with margin commentary, surfaced in monthly partner pack (B10) and per-job views.
- **FR-B2.12** [MVP][MUST] **Deadlines library** is centrally maintained: HMRC, Companies House, TPR, ICO, statutory dates baked in; updates to dates (e.g. tax-year changes) propagate to all firms via release.
- **FR-B2.13** [Phase 2][SHOULD] **Reassignment workflow**: a job's owner can hand off with a structured note; reviewer history retained; SLA preserved.
- **FR-B2.14** [Phase 2][SHOULD] **Out-of-office cover**: each fee earner has a designated cover; their tasks reroute automatically during recorded leave.
- **FR-B2.15** [Phase 2][SHOULD] **Bulk operations**: bulk-move jobs across owners, bulk-pause for a client on hold, bulk-update template versions.

**B2.4 AI capabilities**

- **AI-B2.1** [MVP][MUST] AI **deadline-risk forecaster**: predicts which active jobs are at risk of breaching deadlines, considering owner capacity, query age, document-readiness, and historical similar-job timings. Highlights at-risk jobs in the dashboard with reasoning.
- **AI-B2.2** [MVP][SHOULD] AI **task suggestion**: when a fee earner has a free slot, suggests the next-best task given their queue, dependency state, and capacity. Suggestion is advisory; the user accepts or ignores.
- **AI-B2.3** [Phase 2][SHOULD] AI **capacity rebalancer**: suggests reassignment of tasks across the team when capacity is forecast to be exceeded; surfaces options with trade-off explanations.
- **AI-B2.4** [Phase 2][COULD] AI **template improvement**: identifies tasks consistently skipped or extended across jobs and proposes template updates.

**B2.5 Integrations consumed**

- HMRC (for deadline data on filings, returns, payments)
- Companies House (for ARDs, CS01 due dates)
- B4 (documents) — read attachment readiness
- B5 (comms) — read query state
- B3 (billing) — write completion events
- Microsoft Graph / Google Workspace (calendar deadline mirroring — Phase 2)

**B2.6 Data entities**

`EngagementWorkflow` context (research §8.2):

- `Job` — aggregate root with `Task`, `Owner`, `ReviewerAssignment`, `Deadline`, `Status`, `HoursBudget`, `HoursActual`
- `RecurringJobSchedule` — aggregate root with `Cadence`, `Trigger`, `Template`
- `JobTemplate` — entity owned at firm level

**B2.7 Acceptance criteria**

- **AC-B2.1** Every signed engagement results in correctly scheduled recurring jobs with correct deadlines for the next 12 months.
- **AC-B2.2** No job reaches its deadline without prior risk-flagging at minimum 14 days out.
- **AC-B2.3** A typical fee earner can identify their next task in under 5 seconds from login.
- **AC-B2.4** Job status across the firm reflects reality (audit-tested by sampling 20 jobs and confirming UI status matches actual work state).
- **AC-B2.5** Budget vs actual hours visible on every active job, with month-end report covering all clients.

**B2.8 Open questions**

- Granularity of task dependencies (linear vs DAG).
- Calendar mirroring scope (all jobs vs just deadlines).
- Whether to persist forecast vs actual hours separately for prior-period analysis.

---

### B3 — Time, Billing & WIP / Lock-up

**B3.0 Phase tag** — MVP

**B3.1 Purpose & user personas**

The financial nervous system. Captures the time and cost of delivering each engagement, converts that effort into invoices, collects cash promptly, and surfaces the unit economics of every client. Personas: `FeeEarner`, `Manager`, `Partner`, `PracticeAdmin`, `FirmOwner`.

**B3.2 User stories**

- **US-B3.1** As a `FeeEarner`, I want to log time against a client/job in seconds so the discipline doesn't slip.
- **US-B3.2** As a `PracticeAdmin`, I want recurring monthly subscription invoices auto-generated and collected via DD so I never chase clients for fees.
- **US-B3.3** As a `Manager`, I want a WIP dashboard per client/job so I can spot stale balances before they go unrecoverable.
- **US-B3.4** As a `Partner`, I want a real-time lock-up view (WIP days + debtor days) so I know our working capital position.
- **US-B3.5** As a `Manager`, I want one-click bill drafts from WIP so project billing is easy.
- **US-B3.6** As a `PracticeAdmin`, I want aged-debtor reminders to fire automatically at 7/14/30 days so I never have to manually chase.
- **US-B3.7** As a `Partner`, I want a client-profitability view ranked from best to worst so I can prune or re-price the bottom decile annually.

**B3.3 Functional requirements**

- **FR-B3.1** [MVP][MUST] **Time entry**: fee earners log time against (`Client`, `Job`, `Activity`) tuples with start/stop or duration. Default chargeable flag inherited from activity type; chargeOutRate inherited from fee earner's grade unless overridden.
- **FR-B3.2** [MVP][MUST] Time entries support keyboard-fast UX: recent-clients shortcut, last-job recall, weekly-grid view, mobile entry.
- **FR-B3.3** [MVP][MUST] Time entries can be edited within 7 days; thereafter requires manager unlock + audit log entry.
- **FR-B3.4** [MVP][MUST] **WIP balance** is computed per Job and rolled up per Engagement and Client; updated within minutes of time entry.
- **FR-B3.5** [MVP][MUST] **Subscription billing**: each Engagement with a fixed monthly fee generates an Invoice on its monthly anniversary; invoice line items map to subscribed services.
- **FR-B3.6** [MVP][MUST] **Direct Debit collection** via GoCardless: Mandate stored against each Client; collection scheduled automatically per Invoice; bank reconciliation of receipt; auto-mark Invoice as Paid on confirmed collection.
- **FR-B3.7** [MVP][MUST] **Project billing**: Manager can draft an Invoice from accumulated WIP on a Job, with explicit write-down line items and rationale; Partner approves before issue.
- **FR-B3.8** [MVP][MUST] **Aged debtor process**: automated email reminders at 7, 14, 30 days post-due; manager intervention threshold at 45; partner intervention at 60.
- **FR-B3.9** [MVP][MUST] **VAT on invoices**: standard UK rates (20% / 5% / 0% / Exempt) with disbursement handling for HMRC fees and Companies House fees.
- **FR-B3.10** [MVP][MUST] **Lock-up dashboard**: WIP days + Debtor days (DSO), per fee earner, per team, per service line, per client. Trends over 12 months.
- **FR-B3.11** [MVP][MUST] **Client profitability**: revenue – allocated cost (time × cost rate) per client per period; ranked tables; flagged loss-making clients.
- **FR-B3.12** [MVP][MUST] **Realisation / recovery**: billed value vs standard time × rate per Job, per fee earner, per service line.
- **FR-B3.13** [MVP][MUST] **Bad-debt provisioning**: invoices >90 days overdue auto-flagged for review; write-off requires Partner approval and triggers VAT bad-debt-relief tracking after 6 months.
- **FR-B3.14** [MVP][MUST] **Charge-out rates** are configurable per grade with effective-from dates; historical entries retain the rate prevailing at entry time.
- **FR-B3.15** [Phase 2][SHOULD] **Card payment** option via Stripe for ad-hoc invoices (clients who refuse DD).
- **FR-B3.16** [Phase 2][SHOULD] **Payment plans / TTP**: structured instalment plans for late-paying clients with HMRC-compatible language.
- **FR-B3.17** [Phase 2][SHOULD] **Scope-change orders**: when a Job's hours exceed budget by a configurable threshold, a structured change-order workflow is triggered.

**B3.4 AI capabilities**

- **AI-B3.1** [MVP][SHOULD] AI **time-entry assistant**: from a fee earner's calendar entries, emails sent, and document activity, AI suggests draft time entries for the day. The user reviews and confirms — no auto-posting.
- **AI-B3.2** [MVP][MUST] AI **WIP write-down suggestion**: identifies stale WIP and suggests realistic billable amount with reasoning, for Manager review during billing draft.
- **AI-B3.3** [Phase 2][SHOULD] AI **bad-debt risk scoring**: predicts which invoices are at elevated default risk based on client payment history, financial-press signals, sector trends.
- **AI-B3.4** [Phase 2][SHOULD] AI **client-profitability commentary**: drafts the "why is client X loss-making" narrative during partner-pack generation.

**B3.5 Integrations consumed**

- GoCardless (DD mandates and collection)
- Stripe (Phase 2 card payments)
- The firm's own ledger (Xero/QBO) for matching firm-side bank receipts
- B2 (job-completion triggers billable events)
- B5 (invoice issuance via comms channel)

**B3.6 Data entities**

`BillingAndCash` context (research §8.2):

- `TimeEntry` — aggregate root
- `WipBalance` — derived projection per Job and Engagement
- `Invoice` — aggregate root with `LineItem`, `VatAmount`, `Disbursement`
- `Subscription` — aggregate root with `Tier`, `RecurrenceRule`, `Mandate`
- `Payment` — aggregate root
- `AgedDebtor` — derived projection
- `LockUp` — derived projection across `WipBalance + AgedDebtor`

**B3.7 Acceptance criteria**

- **AC-B3.1** A fee earner can log a time entry in **under 10 seconds** for a client/job they've worked on this week.
- **AC-B3.2** Subscription invoices issue and DD-collect on schedule with **<1% manual intervention** in steady state.
- **AC-B3.3** Lock-up dashboard reflects current state with **<10 minute lag** from time entry / payment.
- **AC-B3.4** Aged-debtor cycle progresses without manual chase action for the first 30 days post-due.
- **AC-B3.5** Client profitability available real-time to Partner role; revenue and allocated cost reconcile to firm ledger within rounding tolerance.

**B3.8 Open questions**

- Whether to support hourly billing as a first-class model alongside subscription/project (relevant for advisory in Phase 3).
- Multi-currency for clients with non-GBP fees.
- Granularity of activity codes — fully open vs constrained list per service line.

---

### B4 — Document & Records Management + Client Portal

**B4.0 Phase tag** — MVP

**B4.1 Purpose & user personas**

The document chain of custody. Captures and retains every document the firm produces or receives in client work, in a secure, GDPR-compliant, regulator-defensible system; provides clients a single secure channel to upload, sign, and access their information. Personas: `FeeEarner`, `Manager`, `Partner`, `PracticeAdmin`, `MLRO`, `DPO`, `ClientPrimary`, `ClientStaff`.

**B4.2 User stories**

- **US-B4.1** As a `ClientPrimary`, I want to upload a receipt by snapping a photo on my phone, with no app to install and no password to remember.
- **US-B4.2** As a `ClientPrimary`, I want to e-sign documents in a single tap from my phone.
- **US-B4.3** As a `FeeEarner`, I want every uploaded document auto-classified and indexed against the right client, job, and period so I never have to file manually.
- **US-B4.4** As a `Manager`, I want a single per-client document hierarchy that's searchable in full text.
- **US-B4.5** As a `PracticeAdmin`, I want a structured request list per job that prompts the client for exactly what we need, tracked through to receipt.
- **US-B4.6** As a `DPO`, I want each document tagged with a data class and retention policy so retention is automated.
- **US-B4.7** As an `MLRO`, I want every CDD document immutably stored with full chain of custody for AML inspection.

**B4.3 Functional requirements**

- **FR-B4.1** [MVP][MUST] **Universal intake**: documents enter via portal upload, mobile camera capture (PWA / native), magic-email per client, drag-and-drop in firm UI, API ingest from integrated systems.
- **FR-B4.2** [MVP][MUST] **Auto-classification**: every uploaded document is automatically classified by type (e.g. `Passport`, `BankStatement`, `Invoice`, `Receipt`, `BoardMinute`, `EngagementLetter`, `Tax_P60`).
- **FR-B4.3** [MVP][MUST] **Auto-indexing**: documents are auto-tagged against `Client`, `Job`, `Period` based on content extraction and contextual signals (which job is currently in `WithClient` status; where the upload originated).
- **FR-B4.4** [MVP][MUST] **Versioning**: documents support versioning; signed versions are locked; superseded versions retained per retention policy.
- **FR-B4.5** [MVP][MUST] **Structured request lists**: each Job can produce a templated request list (per service line) that surfaces in the client portal as a checklist; clients see what's needed, what's received, what's outstanding.
- **FR-B4.6** [MVP][MUST] **E-signature** is built into the document model: any document can be marked for signature; signed copies are immutable and retain full audit trail.
- **FR-B4.7** [MVP][MUST] **Data classification**: every document is tagged with a data class (`PersonalData`, `AmlRecord`, `AuditWorkingPaper`, `TaxRecord`, `OperationalData`, `MarketingData`) — drives retention per Section 2.3.
- **FR-B4.8** [MVP][MUST] **Retention enforcement**: documents are auto-flagged for review at retention-period end; Firm Owner / DPO authorises destruction with audit log entry.
- **FR-B4.9** [MVP][MUST] **Search**: full-text search across all firm documents with permission-aware results (a fee earner does not see documents for clients out of their scope).
- **FR-B4.10** [MVP][MUST] **Client portal** is a single web surface, custom-branded per firm (per XR-2.1.5), accessible on mobile and desktop, with the document hierarchy, request lists, signed documents, messages (B5), and invoices (B3).
- **FR-B4.11** [MVP][MUST] **Magic-email**: each Client has a unique inbox email address; emails sent there are auto-classified and ingested with attachments.
- **FR-B4.12** [MVP][MUST] **Mobile camera capture**: client mobile UI supports auto-edge-detection, multi-page document capture, OCR-readiness, immediate classification preview before upload confirmation.
- **FR-B4.13** [MVP][MUST] **Permissions**: every document is access-controlled by client role × document data-class; explicit deny on AML records to non-MLRO roles by default.
- **FR-B4.14** [MVP][MUST] **Audit trail per document**: who uploaded, who viewed, when, from which device/IP, every state change.
- **FR-B4.15** [Phase 2][SHOULD] **Bulk request lists**: a manager can fire a bulk request list to all clients with a year-end approaching, personalised per client.
- **FR-B4.16** [Phase 2][SHOULD] **External shared rooms**: time-bound document rooms for lender / investor / acquirer review with watermarking and view-only access.
- **FR-B4.17** [Phase 3][COULD] **Self-destructing share links** for one-off external shares (e.g. solicitor copy of accounts) with auto-expiry.

**B4.4 AI capabilities**

- **AI-B4.1** [MVP][MUST] **Auto-classification**: the AI engine classifies documents into the firm's document type taxonomy at upload, with confidence scoring and human-review queue for low-confidence items.
- **AI-B4.2** [MVP][MUST] **Field extraction**: structured fields (supplier, date, net, VAT, total on an invoice; ID number, expiry, name on a passport) are extracted at upload and stored alongside the document.
- **AI-B4.3** [MVP][MUST] **AI-drafted request lists**: when a Job opens, AI generates a tailored request list based on the service, prior-year data, and ledger state. The user reviews before send.
- **AI-B4.4** [Phase 2][SHOULD] **Document gap detection**: AI cross-checks the request list against received documents and surfaces missing items in plain language ("we still need your March bank statement").
- **AI-B4.5** [Phase 2][COULD] **Duplicate detection**: AI identifies when an uploaded document is likely a duplicate of an existing one (e.g. the same invoice forwarded twice).

**B4.5 Integrations consumed**

- E-signature provider (built-in via SignWell / HelloSign / etc., TBD)
- Microsoft Graph / Google Workspace (magic-email backend, calendar attachments)
- Cloud storage (S3-compatible, UK/EU residency)
- AI document AI service (per Section 2.4)

**B4.6 Data entities**

`EngagementWorkflow` context (research §8.2):

- `Document` — aggregate root with `Version`, `ClassificationTag`, `RetentionPolicy`, `SignatureRequest`, `AccessLog`
- `DocumentRequestList` — per-Job aggregate
- `Portal` — per-firm branding + per-client experience configuration

**B4.7 Acceptance criteria**

- **AC-B4.1** A client can upload, classify, and have a document indexed against the right Job in **under 30 seconds** on mobile.
- **AC-B4.2** Auto-classification accuracy meets or exceeds **95% on common document types** (invoices, receipts, bank statements, ID, payslips) measured on the evaluation set per XR-2.4.7.
- **AC-B4.3** No documents are lost: 100% of inbound documents are visible in the firm view within 5 minutes of upload, with audit trail.
- **AC-B4.4** Retention policies fire automatically; no document is retained past its policy without explicit DPO override with rationale.
- **AC-B4.5** Permission boundary holds: a fee earner cannot search, list, or retrieve any document outside their scope.

**B4.8 Open questions**

- Native iOS/Android app for client portal vs PWA only at MVP — affects camera capture quality and offline behaviour.
- E-signature provider choice (build vs integrate).
- Whether to provide read-only WORM storage (Write Once Read Many) for the most sensitive document classes for regulatory defence.

---

### B5 — Communication & Query Handling

**B5.0 Phase tag** — MVP

**B5.1 Purpose & user personas**

The constant interface with the client. Be predictably responsive; capture every conversation against the right client and job for institutional memory; keep the outstanding-query pile small and aging slowly. Personas: `FeeEarner`, `Manager`, `Partner`, `PracticeAdmin`, `ClientPrimary`, `ClientStaff`.

**B5.2 User stories**

- **US-B5.1** As a `Manager`, I want all client comms — emails, portal messages, calls, queries — to land in one inbox per client/job so I never miss anything.
- **US-B5.2** As a `FeeEarner`, I want SLA timers on queries so I respond inside our 24-hour first-response promise.
- **US-B5.3** As a `Manager`, I want stale queries surfaced weekly so I can chase or escalate.
- **US-B5.4** As a `ClientPrimary`, I want a single thread per topic, not 14 emails — and I want my responses on mobile to be one-tap.
- **US-B5.5** As a `FeeEarner`, I want AI to draft my reply suggestions so the boring part of comms takes seconds, not minutes.
- **US-B5.6** As a `Partner`, I want sensitive comms (HMRC enquiries, complaints, bereavement) auto-escalated to me within 24 hours.
- **US-B5.7** As a `MLRO`, I want any "tipping-off-risk" comms blocked from outbound delivery and flagged.

**B5.3 Functional requirements**

- **FR-B5.1** [MVP][MUST] **Unified inbox**: per-firm shared inbox with auto-triage; per-client conversation view; per-job conversation view. Native to the product, not a thin wrapper over Outlook.
- **FR-B5.2** [MVP][MUST] **Channels**: inbound and outbound email (via Microsoft Graph / Google Workspace), portal in-app messaging, SMS for time-critical reminders only (Phase 2). Outbound from the firm's domain via authenticated relay.
- **FR-B5.3** [MVP][MUST] **Auto-classification**: inbound comms are classified against (`Client`, `Job`, `Topic`) using the email metadata, content, and attached documents.
- **FR-B5.4** [MVP][MUST] **Query model**: a `Query` is a structured first-class entity with state (`Open`, `WithClient`, `WithFirm`, `Resolved`), owner, age, urgency, attached documents. Queries link to Jobs.
- **FR-B5.5** [MVP][MUST] **SLA clocks**: each conversation has a first-response SLA (default 24 working hours) and a resolution SLA (default 5 working days). Breach is highlighted in dashboards.
- **FR-B5.6** [MVP][MUST] **Templates**: outbound query templates per service line (VAT data request, year-end pack, P11D info, etc.) merge per-client variables. Templates managed at firm level.
- **FR-B5.7** [MVP][MUST] **Auto-chase**: configurable chase cadence (e.g. 5 / 10 / 15 days) for outstanding queries; manager intervenes after 3 chases.
- **FR-B5.8** [MVP][MUST] **Escalation rules**: keywords / categories trigger partner notification within 24 hours (HMRC enquiry, complaint, bereavement, dispute, sale, fundraise).
- **FR-B5.9** [MVP][MUST] **Tipping-off prevention** (AML): outbound comms are automatically scanned for tipping-off-risk language when a SAR is open against the client; risky drafts are blocked and flagged to MLRO.
- **FR-B5.10** [MVP][MUST] **Client portal in-app messaging** is the default channel for non-urgent comms; emails fall back to portal preview to drive client into the secure surface over time.
- **FR-B5.11** [MVP][MUST] **Single named manager**: every client has one assigned `Manager` displayed prominently in client and firm UI; comms default-route to them.
- **FR-B5.12** [MVP][MUST] **Out-of-office cover**: when a manager is on leave, comms re-route to their nominated cover with a transparent note to the client.
- **FR-B5.13** [MVP][MUST] **Audit trail**: every comm is timestamped, attributed, and immutably retained per data-class retention.
- **FR-B5.14** [Phase 2][SHOULD] **Voice / phone integration**: inbound calls log against client; voicemail transcribed; outbound calls (where dialled from product) recorded with consent.
- **FR-B5.15** [Phase 2][SHOULD] **Sentiment & urgency triage**: inbound comms scored for tone; angry / urgent surfaced ahead of routine.
- **FR-B5.16** [Phase 3][COULD] **WhatsApp / SMS official channels** with regulatory-compliant retention.

**B5.4 AI capabilities**

- **AI-B5.1** [MVP][MUST] **AI-drafted reply**: every inbound comm gets a draft reply suggested in the assigned user's queue, drawing on prior comms, current job state, and firm tone-of-voice. The user edits and sends. No auto-send.
- **AI-B5.2** [MVP][MUST] **Thread summarisation**: long threads get a 3-line summary on hover and a structured "what's been agreed" section in the conversation view.
- **AI-B5.3** [MVP][MUST] **Query extraction**: when an inbound comm asks a question, AI extracts the question(s) into structured `Query` entities linked to the Job — clients no longer have to chase by email when something's already been asked.
- **AI-B5.4** [MVP][SHOULD] **Tone normalisation**: AI rewrites a fee earner's draft to firm-defined tone-of-voice (formal / friendly / brief) on demand.
- **AI-B5.5** [Phase 2][SHOULD] **Sentiment / urgency scoring**: AI scores tone and urgency; surfaced in dashboards (US-B5.6 escalation).
- **AI-B5.6** [Phase 2][COULD] **Auto-translate**: for clients corresponding in non-English, AI translates inbound and outbound; original retained.

**B5.5 Integrations consumed**

- Microsoft Graph (email, calendar, contacts) — read & send
- Google Workspace (email, calendar, contacts) — read & send
- B2 (queries link to Jobs)
- B4 (attachments live as Documents)
- AI services (per Section 2.4)
- Phone provider (Phase 2: Aircall / RingCentral / Dialpad)

**B5.6 Data entities**

`ClientRelationship` context (research §8.2):

- `Communication` — aggregate root with `Message`, `Channel`, `SlaClock`
- `Query` — aggregate root linked to Communication and Job
- `Complaint` — aggregate root with regulator-linked workflow

**B5.7 Acceptance criteria**

- **AC-B5.1** Every client comm appears in the unified inbox within **2 minutes** of arrival.
- **AC-B5.2** First-response SLA breach rate is visible per fee earner and per team in real time.
- **AC-B5.3** No comm is lost between channels: 100% of comms across email + portal + integrated channels are searchable from a single view.
- **AC-B5.4** Tipping-off blocks fire on every SAR-open client; at least 99.9% block recall on adversarial test set.
- **AC-B5.5** AI-drafted replies are accepted (with or without edits) at >70% rate after 4 weeks of use; rejection feedback feeds the eval set.

**B5.8 Open questions**

- Sole-channel mode: do firms want to enforce all comms via portal, or remain hybrid?
- WhatsApp / SMS regulatory boundaries — confidentiality + AML retention requirements may make these non-starters.
- Voice/phone integration scope at Phase 2 — Aircall vs full PSTN.

---

### A1 — Bookkeeping / Client Accounting Services (orchestration)

**A1.0 Phase tag** — MVP

**A1.1 Purpose & user personas**

The product does not own the ledger; it **orchestrates** over Xero, QuickBooks Online, FreeAgent, and Sage Business Cloud. The firm's bookkeepers, managers, and clients work in one product; the books of record stay in the cloud ledger; AI runs anomaly and category checks across the client book in aggregate. Personas: `Bookkeeper`, `FeeEarner`, `Manager`, `Partner`, `ClientPrimary`.

**A1.2 User stories**

- **US-A1.1** As a `Bookkeeper`, I want all my clients' ledgers visible in one place without bouncing between Xero tabs.
- **US-A1.2** As a `Bookkeeper`, I want AI to flag uncategorised or anomalous transactions across my entire book of clients in one review queue.
- **US-A1.3** As a `Bookkeeper`, I want to push categorisation decisions and journals back to Xero/QBO without rekeying.
- **US-A1.4** As a `ClientPrimary`, I want to answer "is this transaction X or Y?" queries from my portal with a tap.
- **US-A1.5** As a `Manager`, I want a month-end close checklist that's driven by the actual state of the ledger, not a manual spreadsheet.
- **US-A1.6** As a `Bookkeeper`, I want missing receipts auto-flagged so I don't have to scan ledger lines manually.

**A1.3 Functional requirements**

- **FR-A1.1** [MVP][MUST] **Ledger connectors**: OAuth 2.0 connectors for Xero and QuickBooks Online at MVP. Each Client can be associated with one ledger account; reads are scoped to that connection.
- **FR-A1.2** [MVP][MUST] **Read scope**: ledger Transactions, Accounts (chart of accounts), Bank Accounts, Bank Feeds, Reconciliation status, Period state, Tax codes, Contacts (suppliers / customers), Invoices, Bills.
- **FR-A1.3** [MVP][MUST] **Anti-corruption layer**: ledger entities are mapped to the product's domain model (`Transaction`, `Account`, `Reconciliation`, `Period`) — Xero / QBO data shape does not leak into product UI or storage beyond the adapter.
- **FR-A1.4** [MVP][MUST] **Sync cadence**: incremental sync via webhook-where-supported with fallback poll every 15 minutes. Manual "sync now" available to fee earners.
- **FR-A1.5** [MVP][MUST] **Multi-client view**: a fee earner sees all their clients' ledger state in cross-client dashboards (uncategorised count, reconciliation status, last close date, query age).
- **FR-A1.6** [MVP][MUST] **Per-client view**: for a single Client, surface the trial balance, P&L, BS, recent transactions, bank feed status, and outstanding categorisation decisions.
- **FR-A1.7** [MVP][MUST] **Categorisation push-back**: when a Bookkeeper categorises a transaction in the product, the change is written back to the source ledger. Failures (rate limits, conflicts) surface in an integration health panel.
- **FR-A1.8** [MVP][MUST] **Journal push-back**: structured journals (e.g. wages from payroll, accruals, depreciation) can be pushed to the source ledger from within the product, including iXBRL-compatible nominal mapping.
- **FR-A1.9** [MVP][MUST] **Bank feed health**: each Client's bank feeds are monitored; broken / re-auth-needed feeds surface as a per-client issue with one-click remediation flow.
- **FR-A1.10** [MVP][MUST] **Receipt capture integration**: ingest from Dext / Hubdoc with link-back to source documents in B4. Captured items appear as suggested categorisations in the product before posting to ledger.
- **FR-A1.11** [MVP][MUST] **Month-end close checklist**: per-Client `MonthEndClose` workflow with ledger-state-driven steps (bank rec complete? VAT control reasonable? Wages journal posted? Accruals run?). Status visible to Manager.
- **FR-A1.12** [MVP][MUST] **Query-from-transaction**: any transaction can be flagged with a question that becomes a structured `Query` (B5) sent to the client; the client's response auto-attaches the resolution to the transaction.
- **FR-A1.13** [MVP][MUST] **Trial balance review**: when a period is locked in the source ledger, the product produces a TB review in product UI with prior-period comparison, materiality flagging, and exception reasoning.
- **FR-A1.14** [MVP][MUST] **VAT-control reconciliation** between ledger control accounts and (Phase 2) the VAT return draft from A3.
- **FR-A1.15** [Phase 2][SHOULD] **FreeAgent and Sage Business Cloud** ledger connectors.
- **FR-A1.16** [Phase 2][SHOULD] **Pre-accounting AI assistant**: from receipts in B4, the product proposes ledger postings with high confidence; bookkeepers approve in batches.
- **FR-A1.17** [Phase 3][COULD] **Multi-ledger Client**: a Client with operations across multiple ledgers (e.g. Xero for trade, separate FreeAgent for property) consolidated in product views.

**A1.4 AI capabilities**

- **AI-A1.1** [MVP][MUST] **Anomaly detection**: AI scores every new transaction for anomaly likelihood (size vs prior periods; account type mismatch; supplier unusual; date out of period). High-anomaly items appear in a daily review queue per client.
- **AI-A1.2** [MVP][MUST] **Mis-categorisation flagging**: AI cross-checks recent categorisations against historical patterns and account-type rules (e.g. capital vs revenue; standard-rated VAT applied to exempt supply); flags potential errors with reasoning.
- **AI-A1.3** [MVP][MUST] **Auto-categorisation suggestions**: for uncategorised transactions, AI suggests the most-likely nominal code with confidence. Bookkeeper accepts/rejects in bulk; rejections train per-firm patterns.
- **AI-A1.4** [MVP][MUST] **Missing-receipt detection**: AI cross-references ledger transactions against received receipts (B4) and flags transactions over a configurable threshold without supporting documents.
- **AI-A1.5** [Phase 2][SHOULD] **VAT-rate verification**: AI checks VAT codes applied to transactions against supplier history, sector norms, and recent rule changes.
- **AI-A1.6** [Phase 2][SHOULD] **Director's loan account watcher**: AI surfaces DLA movements approaching S455 thresholds in real time.

**A1.5 Integrations consumed**

- **Xero** — full read; categorisation/journal write
- **QuickBooks Online** — full read; categorisation/journal write
- **Dext / Hubdoc** — receipt capture
- **FreeAgent**, **Sage Business Cloud** — Phase 2

**A1.6 Data entities**

`ComplianceServices` context (research §8.2):

- `Ledger` — reference / read-only projection (anti-corruption layer over Xero/QBO)
- `Transaction`, `Account`, `Reconciliation`, `Period` — projections per Client
- `MonthEndClose` — aggregate root per Client per Period
- `AnomalyFlag` — produced by AI, attached to Transaction
- `CategorisationRule` — firm-level + client-level patterns, written back to ledger where supported

**A1.7 Acceptance criteria**

- **AC-A1.1** A firm can connect a client's Xero account in under **5 minutes** end-to-end.
- **AC-A1.2** Anomaly detection achieves at least **80% recall on the evaluation set** of seeded anomalies, with <20% false-positive rate at the firm-default threshold.
- **AC-A1.3** Categorisation push-back to source ledger succeeds **>99.5%** of attempts; failures auto-retry with operator visibility.
- **AC-A1.4** A bookkeeper can review and clear a typical day's anomaly queue across 30 clients in **under 60 minutes**.
- **AC-A1.5** Bank feed health is surfaced within **1 hour** of breakage, with remediation flow visible in the firm UI.

**A1.8 Open questions**

- Categorisation rules: do we own them in product (richer) or sync to/from Xero rules (simpler)?
- Real-time vs batch sync cadence for high-volume clients.
- Whether to integrate IRIS / Sage Final Accounts / Xero Tax for the year-end pass-through to A5/A6 in Phase 3.
- Sage Business Cloud (Sage 50 desktop) presents a meaningful proportion of UK SME ledgers — acceptable to defer to Phase 2?

---

### A2 — Management Accounts

**A2.0 Phase tag** — Phase 2

**A2.1 Purpose & user personas**

The bridge between compliance and advisory. Translate the cleaned monthly ledger into a pack of P&L, balance sheet, cash flow, KPIs, and AI-drafted commentary that lets an owner-manager see how the business is performing and identify next-30-day decisions. Personas: `Manager`, `Partner`, `FeeEarner`, `ClientPrimary`.

**A2.2 User stories**

- **US-A2.1** As a `Manager`, I want a monthly pack auto-generated within 5 working days of period close so the firm hits its publish SLA.
- **US-A2.2** As a `Partner`, I want AI to draft the commentary so I can focus on the conversation, not the prose.
- **US-A2.3** As a `ClientPrimary`, I want a 1-page summary on my phone and a deeper view on desktop with KPIs and trends.
- **US-A2.4** As a `Manager`, I want to compare actuals to budget and prior period with explained variances.
- **US-A2.5** As a `Manager`, I want a 13-week rolling cash forecast for cash-stressed clients.
- **US-A2.6** As a `Partner`, I want to schedule a recurring monthly review call with each VCFO/management-accounts client.

**A2.3 Functional requirements**

- **FR-A2.1** [P2][MUST] **Pack template library**: firm-managed pack templates (e.g. "Standard Monthly", "Cash-Stressed", "Multi-Entity") with configurable sections (P&L, BS, Cash, KPIs, Commentary).
- **FR-A2.2** [P2][MUST] **Period close trigger**: when A1 month-end close completes, A2 is auto-scheduled per the client's pack cadence.
- **FR-A2.3** [P2][MUST] **KPI library**: firm-defined KPIs with formulas; client-specific overrides; common defaults (gross margin %, debtor/creditor days, payroll %, cash runway weeks, MRR, headcount).
- **FR-A2.4** [P2][MUST] **Variance analysis**: actual vs budget vs prior year, % and £; auto-flag of material movements per firm-defined materiality thresholds.
- **FR-A2.5** [P2][MUST] **13-week cash forecast**: built from ledger + scheduled receipts/payments + payroll calendar; rolling forward weekly; configurable sensitivity scenarios.
- **FR-A2.6** [P2][MUST] **Pack publication**: published packs delivered as a portal page (preferred) or PDF; clients receive a notification with a 1-line headline result.
- **FR-A2.7** [P2][MUST] **Review-call scheduling**: integrated calendar booking that drops a review meeting on partner + client calendars on the publish-day cadence.
- **FR-A2.8** [P2][MUST] **Decision log**: actions agreed in review calls captured against the engagement; followed up at the next pack.
- **FR-A2.9** [P2][SHOULD] **Multi-entity consolidation** for clients with several connected entities.
- **FR-A2.10** [P3][SHOULD] **Budget vs forecast vs actual**: three-way variance with rolling reforecasts.

**A2.4 AI capabilities**

- **AI-A2.1** [P2][MUST] **AI-drafted commentary**: AI writes the variance narrative in firm tone-of-voice ("Revenue up 8% MoM, driven by…"). Partner edits before publish.
- **AI-A2.2** [P2][MUST] **KPI explainer**: each KPI movement gets a "why" explanation drawing on transaction-level evidence from A1.
- **AI-A2.3** [P2][SHOULD] **3 wins / 3 risks / 3 actions**: AI proposes the structured highlights for the partner to curate.
- **AI-A2.4** [P3][SHOULD] **Forecast reasoning**: AI explains forecast vs actual discrepancies and proposes reforecast adjustments.

**A2.5 Integrations consumed**

- A1 (ledger orchestration) — read trial balance, transactions, accounts
- B4 (publish pack as document)
- B5 (notifications)
- Microsoft Graph / Google Calendar (review-call scheduling)
- Optional in P3: Fathom / Spotlight / Syft as upstream — though most firms will prefer built-in

**A2.6 Data entities**

`AdvisoryServices` context:

- `ManagementPack` — aggregate root with `PeriodKpi`, `VarianceCommentary`, `ForecastSnapshot`
- `PackTemplate` — firm-level
- `KpiDefinition` — firm-level + client-level overrides
- `DecisionLog` — per Engagement

**A2.7 Acceptance criteria**

- **AC-A2.1** Pack publish completes within **5 working days of A1 close** for 95% of subscriptions.
- **AC-A2.2** AI commentary acceptance rate (no edits or minor edits only) reaches **>60% within 8 weeks** of firm onboarding.
- **AC-A2.3** Cash forecast accuracy: 13-week-out forecast within ±10% of actual receipt/payment for 80% of weeks (over a 12-month run).
- **AC-A2.4** Decision log entries are followed up in the next pack with explicit status.

**A2.8 Open questions**

- Whether to embed Fathom/Spotlight as the rendering engine vs build native — affects time-to-ship and depth.
- Multi-entity model: subgroups, eliminations, currency.
- KPI-formula DSL: how flexible vs constrained.

---

### A3 — VAT / Indirect Tax

**A3.0 Phase tag** — Phase 2

**A3.1 Purpose & user personas**

Highest-cadence compliance. Quarterly clockwork. MTD-mandated. Return preparation orchestrated over Xero/QBO; reasonableness review and submission via the product. Personas: `Bookkeeper`, `FeeEarner`, `Manager`, `Partner`, `ClientPrimary`, `MLRO` (registration screening only).

**A3.2 User stories**

- **US-A3.1** As a `Bookkeeper`, I want the VAT return draft ready the day after period close, with all reasonableness checks pre-run.
- **US-A3.2** As a `Manager`, I want exception items surfaced clearly so I can review in minutes, not hours.
- **US-A3.3** As a `Partner`, I want partial-exemption and margin-scheme returns clearly flagged for specialist review.
- **US-A3.4** As a `ClientPrimary`, I want to receive the return summary with a one-line plain-English "you owe £X by Y" and a one-tap approval.
- **US-A3.5** As a `PracticeAdmin`, I want the firm-wide VAT submission calendar visible by stagger and by deadline.
- **US-A3.6** As a `PracticeAdmin`, I want HMRC penalty points monitored per client, with alerts on threshold approach.

**A3.3 Functional requirements**

- **FR-A3.1** [P2][MUST] **VAT registration tracking**: per-Client VAT registration status, scheme (Standard / Flat Rate / Cash / Annual / Margin / Retail), stagger group, FRS sector & rate where applicable.
- **FR-A3.2** [P2][MUST] **Period schedule**: each VAT-registered client has scheduled return periods aligned to stagger, generating Jobs in B2 with statutory deadline (1 month + 7 days).
- **FR-A3.3** [P2][MUST] **Return draft**: at period close (A1), the product pulls draft return data from Xero/QBO via their VAT API; product persists the 9-box position with full transaction-level traceability.
- **FR-A3.4** [P2][MUST] **Reasonableness checks**: Box 1 vs sales × expected rate; Box 4 vs purchases; variance vs prior period; reverse-charge transactions itemised; margin-scheme calc separated; CIS reverse-charge flagged for construction clients.
- **FR-A3.5** [P2][MUST] **Exception queue**: items requiring review (zero-rated, exempt, out-of-scope, reverse charge, capital goods scheme, fuel scale charge entries) listed with one-click approval/correction.
- **FR-A3.6** [P2][MUST] **Client approval**: draft return surfaced in the portal with summary; client e-signs approval; submission then proceeds.
- **FR-A3.7** [P2][MUST] **MTD submission**: submit via HMRC MTD VAT API; persist confirmation receipt; auto-update Job status to `Complete`.
- **FR-A3.8** [P2][MUST] **Payment instruction**: notify client of amount, due date, DD timing (if applicable); track payment confirmation against HMRC.
- **FR-A3.9** [P2][MUST] **Penalty point monitoring**: per-Client point tally for late submissions; alert at threshold approach; alert on £200 fine assessment.
- **FR-A3.10** [P2][MUST] **Digital-link rule compliance**: data flows from source records to submission without manual rekeying; the audit trail proves digital lineage.
- **FR-A3.11** [P2][SHOULD] **Bridging mode**: for clients with spreadsheet-based records, support bridging-software flow with controlled data ingest.
- **FR-A3.12** [P2][SHOULD] **Partial-exemption support**: standard method with override for special methods agreed with HMRC.
- **FR-A3.13** [P3][SHOULD] **Group VAT registration** support.
- **FR-A3.14** [P3][COULD] **Postponed VAT accounting** (PVA) for imports with monthly statement reconciliation.

**A3.4 AI capabilities**

- **AI-A3.1** [P2][MUST] **AI reasonableness review**: AI evaluates the draft return against expected ratios (turnover-to-output-VAT, purchases-to-input-VAT, sector benchmarks), surfaces anomalies with reasoning. Final review remains human.
- **AI-A3.2** [P2][MUST] **AI scheme-suitability check**: at registration / annually, AI flags whether the client's scheme is still optimal vs alternatives (FRS vs standard; cash vs accrual).
- **AI-A3.3** [P2][SHOULD] **AI reverse-charge / partial-exemption flagger**: AI scans the period's transactions for items that should have triggered reverse-charge or partial-exemption treatment but didn't.
- **AI-A3.4** [P3][SHOULD] **AI deregistration assist**: AI alerts when a client's rolling 12-month turnover may fall below the deregistration threshold (£88k) and models the impact.

**A3.5 Integrations consumed**

- Xero / QuickBooks Online — read VAT report draft and transaction lines
- HMRC MTD VAT API — submit and read submission status
- HMRC ASA — agent authorisation prerequisite (from B1)
- B5 — client approval messaging
- B3 — payment tracking

**A3.6 Data entities**

`ComplianceServices` context (research §8.2):

- `VatReturn : Filing` — aggregate root with `VatScheme`, `Box1..Box9`, `DigitalLink`, `Submission`, `ApprovalRecord`
- `VatRegistration` — per-Client entity with scheme, stagger, FRS sector
- `PenaltyPointBalance` — per-Client tally

**A3.7 Acceptance criteria**

- **AC-A3.1** For 95% of subscribed VAT clients, draft return is ready within **24 hours of A1 period close**.
- **AC-A3.2** Submission to HMRC MTD VAT API succeeds **>99.5%** of attempts; failures retry transparently.
- **AC-A3.3** No VAT return submitted without explicit client approval recorded with timestamp.
- **AC-A3.4** Penalty point alerts fire **before** threshold breach for 100% of monitored clients.
- **AC-A3.5** AI reasonableness flags maintain >80% precision (flagged-as-anomaly that was actually anomalous) on the evaluation set.

**A3.8 Open questions**

- Bridging mode: support fully, or push spreadsheet clients to migrate to cloud bookkeeping (matches MTD strategic narrative)?
- Margin schemes (second-hand, motor): bake in or refer to specialist tooling?
- Group VAT registration: Phase 3 or earlier?

---

### A7 — Personal Tax / Self Assessment & MTD-IT

**A7.0 Phase tag** — Phase 2

**A7.1 Purpose & user personas**

The most clock-driven service in the calendar — 31 January dominates the year. From April 2026, MTD for Income Tax adds quarterly submissions for taxpayers with self-employment or property income over £50k. Personas: `Bookkeeper`, `FeeEarner`, `Manager`, `Partner`, `ClientPrimary`.

**A7.2 User stories**

- **US-A7.1** As a `Manager`, I want every SA/MTD-IT client visible on a single dashboard with deadline state, document readiness, and current liability estimate.
- **US-A7.2** As a `FeeEarner`, I want the data request list auto-generated per client based on prior-year sources.
- **US-A7.3** As a `ClientPrimary`, I want to upload my P60s/P11D/dividend vouchers via portal and have them auto-classified.
- **US-A7.4** As a `Manager`, I want quarterly MTD-IT updates submitted automatically from cloud bookkeeping for in-scope clients.
- **US-A7.5** As a `ClientPrimary`, I want a one-page tax summary in plain English: "you owe £X by 31 January."
- **US-A7.6** As a `PracticeAdmin`, I want a January-peak resource view across the whole SA book showing which returns are at risk.
- **US-A7.7** As a `PracticeAdmin`, I want CGT-on-property 60-day returns auto-flagged and tracked separately.

**A7.3 Functional requirements**

- **FR-A7.1** [P2][MUST] **Per-Client tax profile**: identifies sources of income (employment, self-employment, partnership, property, dividends, savings, foreign, other), MTD-IT scope status, and prior-year liability/payments-on-account.
- **FR-A7.2** [P2][MUST] **MTD-IT scope detection**: AI evaluates rolling-12-month qualifying income; flags clients crossing the £50k (Apr 2026), £30k (Apr 2027), £20k (Apr 2028) thresholds.
- **FR-A7.3** [P2][MUST] **Annual SA workflow**: scheduled per UK tax year; generates Job in B2 with deadlines (paper 31 Oct; online 31 Jan; payments-on-account 31 Jul).
- **FR-A7.4** [P2][MUST] **Quarterly MTD-IT update workflow** (April 2026+): generates Jobs at 7 Aug, 7 Nov, 7 Feb, 7 May per in-scope client; final declaration at 31 January.
- **FR-A7.5** [P2][MUST] **Data collection**: tailored request list per client based on prior-year sources; portal-driven; auto-chase per B5 cadence.
- **FR-A7.6** [P2][MUST] **Income aggregation**: from cloud bookkeeping (A1) for self-employment / property; portal-uploaded documents (B4) for employment / dividends / savings.
- **FR-A7.7** [P2][MUST] **Computation orchestration**: route the prepared dataset to the chosen tax engine (TaxCalc / IRIS / Capium API) for SA100 generation in Phase 2; product holds the result for client approval.
- **FR-A7.8** [P2][MUST] **MTD-IT quarterly submission**: submit cumulative quarterly updates to HMRC MTD-IT API; persist receipts; alert client of period-position.
- **FR-A7.9** [P2][MUST] **Final Declaration submission**: submit annual final declaration (replaces SA100 for in-scope clients); persist receipts.
- **FR-A7.10** [P2][MUST] **CGT-on-property 60-day workflow**: separate event-driven workflow; auto-detect CGT events from ledger (large credits to bank from property sale); generate Job with 60-day deadline.
- **FR-A7.11** [P2][MUST] **Client approval and e-sign**: every return / final declaration approved by client before submission, with one-page summary in firm tone-of-voice.
- **FR-A7.12** [P2][MUST] **Payment reminders**: 31 January and 31 July reminders; DD setup for HMRC where supported by client.
- **FR-A7.13** [P2][MUST] **January-peak dashboard**: aggregate view across the SA book — done / in-progress / awaiting-client / at-risk.
- **FR-A7.14** [P2][SHOULD] **Trust returns (SA900) and partnership returns (SA800)** — basic support deferred to P3 if scope permits.
- **FR-A7.15** [P3][SHOULD] **Foreign income (FTC, treaty, residency)** — specialist module.
- **FR-A7.16** [P3][SHOULD] **EIS/SEIS/VCT compliance certificates** issued and tracked from advisory engagements (A10).

**A7.4 AI capabilities**

- **AI-A7.1** [P2][MUST] **AI data request generator**: tailored request list per client per year based on prior-year sources, flagged life events, and ledger signals.
- **AI-A7.2** [P2][MUST] **CGT-event detector**: AI scans bank transactions and document uploads for likely CGT-triggering events (property sale, share disposal); pre-flags 60-day requirement.
- **AI-A7.3** [P2][MUST] **HICBC and personal-allowance taper detector**: AI flags clients whose income trajectory crosses key thresholds.
- **AI-A7.4** [P2][SHOULD] **AI cross-source reconciliation**: AI flags inconsistencies (e.g. dividend voucher vs ledger; P60 vs PAYE record).
- **AI-A7.5** [P2][SHOULD] **AI plain-English summary**: "you owe £X. Last year you owed £Y. Change is mainly due to Z. Three things to do before next year."
- **AI-A7.6** [P3][SHOULD] **MTD-IT readiness audit**: AI reviews each in-scope client's setup and surfaces gaps before April 2026 (or each subsequent threshold drop).

**A7.5 Integrations consumed**

- HMRC MTD-IT API
- HMRC SA online
- Tax engine (TaxCalc / IRIS / Capium API) for SA100 generation
- Xero / QBO / FreeAgent (for self-employment + property income)
- B4 (document ingest of P60, P45, P11D, dividend vouchers)
- B5 (data request comms)

**A7.6 Data entities**

`ComplianceServices` context:

- `SelfAssessmentReturn : Filing` — aggregate root with `IncomeSource`, `Deduction`, `MtdQuarterlyUpdate`, `FinalDeclaration`
- `TaxYearProfile` — per Client per UK tax year
- `MtdItScope` — per-Client status & threshold tracking
- `CgtEvent` — event-driven aggregate with 60-day tracking

**A7.7 Acceptance criteria**

- **AC-A7.1** Every in-scope client successfully submits MTD-IT quarterly updates by 7 Aug / Nov / Feb / May with confirmed HMRC receipt.
- **AC-A7.2** January peak: 95% of SA returns ready for client approval by 15 December; **0% submitted late** for clients on subscription.
- **AC-A7.3** CGT 60-day events detected and a Job created within 7 days of the triggering bank movement.
- **AC-A7.4** Penalty avoidance: no client receives a late-filing penalty due to firm error.
- **AC-A7.5** AI data-request lists accepted with no human edits in **>50% of cases** within 12 weeks.

**A7.8 Open questions**

- Tax engine selection: TaxCalc, IRIS, Capium, or build a thin layer over multiple via adapter?
- Trust (SA900) and partnership (SA800) — Phase 2 or 3?
- HMRC MTD-IT private beta vs general release timing for our launch alignment.

---

### A12 — Company Secretarial

**A12.0 Phase tag** — Phase 2

**A12.1 Purpose & user personas**

Keep each client company's statutory records and Companies House filings accurate — directors, shareholders, PSCs, share capital, registered office, articles. Quietly expanding scope under ECCTA 2023 (mandatory ID verification). Personas: `PracticeAdmin`, `Manager`, `Partner`, `ClientPrimary`.

**A12.2 User stories**

- **US-A12.1** As a `PracticeAdmin`, I want CS01 confirmation statements auto-prepared each year with one-click filing.
- **US-A12.2** As a `Manager`, I want share allotments / transfers / dividend events captured with all paperwork generated automatically.
- **US-A12.3** As a `ClientPrimary`, I want to confirm "no changes" each year in a single tap from email.
- **US-A12.4** As a `PracticeAdmin`, I want ECCTA identity-verification status tracked per director and PSC.
- **US-A12.5** As a `Manager`, I want PSC changes detected from share movements and filed within the 14-day deadline.

**A12.3 Functional requirements**

- **FR-A12.1** [P2][MUST] **Statutory registers**: per-Client electronic registers of members, directors, secretaries, PSCs, allotments, charges. Updated transactionally on every event.
- **FR-A12.2** [P2][MUST] **CS01 workflow**: Job auto-scheduled per Client review date; pulls current CH state; reconciles against internal register; surfaces any deltas.
- **FR-A12.3** [P2][MUST] **CS01 client confirmation**: portal page asking client to confirm registered office, email, SIC codes, shareholders, PSCs; one-tap "no changes" path.
- **FR-A12.4** [P2][MUST] **CS01 submission** to Companies House API; £50 fee processed.
- **FR-A12.5** [P2][MUST] **Share allotment workflow**: board resolution generation, share certificate issue, register update, SH01 filing within 1 month, PSC change filing within 14 days.
- **FR-A12.6** [P2][MUST] **Share transfer workflow**: J30 generation (internal), register update, PSC delta detection, next-CS01 reflection.
- **FR-A12.7** [P2][MUST] **Dividend workflow**: distributable-reserves check (from A1 ledger), board minute, dividend voucher, register update; feeds A7 (SA dividend income).
- **FR-A12.8** [P2][MUST] **Director / secretary changes**: AP01, AP03, TM01, TM02 generation and filing within 14 days.
- **FR-A12.9** [P2][MUST] **Registered office / SAIL changes**: AD01–AD03 generation and filing.
- **FR-A12.10** [P2][MUST] **ECCTA identity-verification tracking**: per-director and per-PSC verification status; surface gaps; integrate to chosen IDV provider; firm registers as ACSP via the product where applicable.
- **FR-A12.11** [P3][SHOULD] **Articles of Association management**: store, version, special-resolution-driven amendments.
- **FR-A12.12** [P3][SHOULD] **Group restructure tooling**: share-for-share, demerger, holdco insertion — workflow templates for common patterns.

**A12.4 AI capabilities**

- **AI-A12.1** [P2][MUST] **PSC delta detection**: AI watches share movements and director changes; flags when PSC threshold (25% / 50% / 75%) is crossed and a PSC notification is required within 14 days.
- **AI-A12.2** [P2][SHOULD] **AI-drafted resolutions and minutes**: from event triggers, AI drafts the appropriate board / shareholder resolution; user reviews.
- **AI-A12.3** [P2][SHOULD] **AI confirmation statement narrative**: AI summarises any deltas in plain language for client confirmation.

**A12.5 Integrations consumed**

- Companies House API (read + filing)
- IDV provider (SmartSearch / Veriphy / Credas) for ECCTA
- B1 (initial register population)
- A1 (reserves check for dividends)
- B5 (client confirmation comms)

**A12.6 Data entities**

`ComplianceServices` context:

- `ConfirmationStatement : Filing` — aggregate root with `StatementOfCapital`, `PscDelta`
- `StatutoryRegister` — per Client (members, directors, secretaries, PSCs, allotments, charges)
- `ShareEvent` — allotment / transfer / cancellation
- `DirectorAppointment` — appointment / cessation / role change

**A12.7 Acceptance criteria**

- **AC-A12.1** 100% of subscribed clients' CS01s filed within the 14-day window after their review date.
- **AC-A12.2** PSC delta events filed within statutory 14 days for 100% of cases.
- **AC-A12.3** Statutory registers always reconcile to Companies House public record (audit-tested monthly).
- **AC-A12.4** ECCTA verification status visible per director/PSC; gaps actioned within firm-defined SLA.

**A12.8 Open questions**

- ACSP wrapper depth: do we register as the ACSP for the firm, or do firms self-register and we provide tooling?
- Articles management — how rich at MVP / Phase 2?
- Group restructures: stay in P3 backlog or pull forward for niche-specialist firms?

---

### B8 — Pricing & Productisation (proposal / packaging)

> *(The CRM / marketing portion of B8 is in Backlog — see §3.4.)*

**B8.0 Phase tag** — Phase 2

**B8.1 Purpose & user personas**

Define what the firm sells, how it's packaged, and what it costs. Drive consistent pricing across all proposals (B1) and renewals (B3 subscriptions). Make annual price reviews a controlled, auditable change rather than an ad-hoc spreadsheet exercise. Personas: `FirmOwner`, `Partner`, `Manager`, `PracticeAdmin`.

**B8.2 User stories**

- **US-B8.1** As a `FirmOwner`, I want a single service catalogue with named tiers (Starter / Growth / Scale) so we stop quoting bespoke fees per prospect.
- **US-B8.2** As a `FirmOwner`, I want fee logic configurable in plain English (e.g. "Growth Ltd: £450/month base + £25 per employee over 5") so I can adjust pricing without engineering help.
- **US-B8.3** As a `Manager`, I want proposal generation in B1 to use this catalogue with one-click tier selection.
- **US-B8.4** As a `FirmOwner`, I want an annual price-review workflow that lets me model the firm-wide impact of an X% across-the-book increase before applying it.
- **US-B8.5** As a `PracticeAdmin`, I want renewal-anniversary alerts so we never miss a fee review window.
- **US-B8.6** As a `Manager`, I want change orders (scope creep) handled as a formal flow that updates the engagement and the subscription.

**B8.3 Functional requirements**

- **FR-B8.1** [P2][MUST] **Service catalogue**: firm-managed list of services aligned to A1–A12 service lines, each with a description, target client profile, and inclusion list.
- **FR-B8.2** [P2][MUST] **Package editor**: bundle services into named tiers (Starter / Growth / Scale by default; firm-renamable). Each tier has a base fee, bundled service set, and per-volume pricing rules.
- **FR-B8.3** [P2][MUST] **Fee model DSL**: configurable formula language for fees (base + per-employee + per-transaction band + per-property + complexity factor); test-runnable on prospect inputs.
- **FR-B8.4** [P2][MUST] **Discount governance**: discount levels with role-required approval (e.g. ≥10% requires Partner; ≥20% requires Firm Owner); audit-logged.
- **FR-B8.5** [P2][MUST] **Proposal generation** (used by B1): given a discovery form output, returns the recommended tier + price + scope; manual override available.
- **FR-B8.6** [P2][MUST] **Annual price review**: model the impact of a percentage uplift, a tier-restructure, or a per-service repricing across the existing subscription book; preview revenue/churn-risk; apply with renewal cadence.
- **FR-B8.7** [P2][MUST] **Renewal cadence**: each subscription has an anniversary review; alerts trigger proposal-renewal workflow with current vs new pricing.
- **FR-B8.8** [P2][MUST] **Change-order flow**: when work scope changes mid-year, a structured change order amends the engagement letter and the subscription, with client e-sign on the variation.
- **FR-B8.9** [P2][SHOULD] **Sector benchmarks**: surface anonymised industry pricing benchmarks per service for firm context (Phase 2 if data set available; Phase 3 otherwise).
- **FR-B8.10** [P3][SHOULD] **A/B-testable proposal templates**: track proposal-to-signature conversion by template variant.

**B8.4 AI capabilities**

- **AI-B8.1** [P2][MUST] **AI tier-fit recommendation**: from prospect discovery inputs, AI recommends the most appropriate tier with reasoning; flags upsell paths (e.g. likely needing payroll within 12 months).
- **AI-B8.2** [P2][MUST] **AI fee-quote sanity check**: AI compares the proposed fee against firm history for similar-profile clients; flags under-pricing or over-pricing outliers.
- **AI-B8.3** [P3][SHOULD] **AI annual review impact narrative**: AI drafts the partner-facing memo on the proposed annual price review, with churn-risk flags.

**B8.5 Integrations consumed**

- B1 (proposal flow consumer)
- B3 (subscription model — fee changes propagate)
- B4 (engagement letter generation)

**B8.6 Data entities**

`PracticeOperations` → GrowthAndPricing sub-context:

- `Service` — firm-defined offering aligned to A1–A12
- `ServicePackage` — named tier with bundled services
- `FeeModel` — formula / rules
- `Proposal` — generated for a Prospect (links to B1)
- `RenewalCycle` — per Subscription

**B8.7 Acceptance criteria**

- **AC-B8.1** Every new proposal in B1 uses the catalogue + fee model with no manual override in **>80% of cases** within 8 weeks.
- **AC-B8.2** An annual price-review can be modelled and applied across 200 clients in **under 30 minutes**.
- **AC-B8.3** Change orders update both the engagement letter and the subscription atomically with no manual reconciliation.
- **AC-B8.4** Discount governance: no discount above threshold ever applied without recorded approval.

**B8.8 Open questions**

- DSL flexibility: how expressive (Excel-level formulas) vs constrained (drop-down + sliders)?
- Sector benchmark data source — internal cohort vs licensed third-party data.
- CRM/marketing portion: stay in Backlog or pull forward if a customer demands it?

---

### A4 — Payroll & Employment Taxes

**A4.0 Phase tag** — Phase 3

**A4.1 Purpose & user personas**

Orchestrate BrightPay (and one alternative) to deliver payroll-as-a-service. The product owns the workflow, client comms, approval, and journal posting; BrightPay owns the gross-to-net calculation and HMRC filings. Personas: `PayrollAdmin` (a `FeeEarner` specialism), `Manager`, `Partner`, `ClientPrimary` (employer), `MLRO` (off-payroll IR35 risk overlay).

**A4.2 User stories**

- **US-A4.1** As a `ClientPrimary`, I want a templated form each month to enter starters, leavers, hours, bonuses — with no payroll jargon.
- **US-A4.2** As a `PayrollAdmin`, I want to review the draft from BrightPay, push approval to client, and submit FPS in one flow.
- **US-A4.3** As a `Manager`, I want every payroll's status visible across the firm with deadline alerts.
- **US-A4.4** As a `ClientPrimary`, I want a final summary on payroll day showing total to pay employees, total PAYE/NIC due, and pension contribution due, with all dates.
- **US-A4.5** As a `PayrollAdmin`, I want auto-enrolment status surfaced per employee, with re-enrolment cycles tracked.
- **US-A4.6** As a `PayrollAdmin`, I want P11D and P11D(b) workflow for the 6 July deadline annually.

**A4.3 Functional requirements**

- **FR-A4.1** [P3][MUST] **BrightPay integration**: bidirectional API; firm's BrightPay tenant connected; per-Client schemes mapped.
- **FR-A4.2** [P3][MUST] **Pay-period workflow**: monthly (default) / weekly / fortnightly / four-weekly per scheme; client-data request → review → client approval → submit.
- **FR-A4.3** [P3][MUST] **Client data form**: structured form per pay period with new starters / leavers / hours / bonuses / absences / benefits; submitted via portal.
- **FR-A4.4** [P3][MUST] **DPS pull**: HMRC tax-code notifications pulled and applied before processing.
- **FR-A4.5** [P3][MUST] **Approval and submission**: client e-sign on draft summary; FPS submitted on/before payday via BrightPay; receipt persisted.
- **FR-A4.6** [P3][MUST] **EPS submission** when triggered (statutory pay reclaim, no payment month, CIS suffered).
- **FR-A4.7** [P3][MUST] **Payslips and P60s**: published to employee portal (or BrightPay Connect equivalent) with firm branding.
- **FR-A4.8** [P3][MUST] **PAYE/NIC payment instruction** with 22nd deadline tracking; pension provider file submission within 22-day window.
- **FR-A4.9** [P3][MUST] **Auto-enrolment compliance**: per-employee status; re-enrolment 3-yearly cycle; TPR Declaration of Compliance support.
- **FR-A4.10** [P3][MUST] **Year-end cycle**: P60 distribution by 31 May; P11D/P11D(b) by 6 July; Class 1A NIC payment by 22 July.
- **FR-A4.11** [P3][MUST] **Wages journal**: structured journal posted to client's ledger (A1) for the pay run.
- **FR-A4.12** [P3][SHOULD] **IR35 status determination** workflow for off-payroll engagements.
- **FR-A4.13** [P3][SHOULD] **Moneysoft / Sage Payroll** as alternative engines.

**A4.4 AI capabilities**

- **AI-A4.1** [P3][MUST] **AI starter / leaver detector**: from comms (B5) and bank movements, AI flags likely employee changes the client may have forgotten to mention.
- **AI-A4.2** [P3][SHOULD] **AI tax-code change explainer**: when an employee's tax code changes, AI drafts a plain-English explanation for the employee.
- **AI-A4.3** [P3][SHOULD] **AI NMW/NLW breach detector** on salary sacrifice arrangements.

**A4.5 Integrations consumed**

- BrightPay API (and BrightPay Connect for employee portal)
- HMRC RTI gateway (FPS, EPS, EYU)
- HMRC DPS (Data Provisioning Service)
- Pension providers (NEST, People's Pension, Smart Pension, Aviva)
- A1 (journal posting)

**A4.6 Data entities**

`ComplianceServices` context:

- `PayrollRun : Filing` — aggregate root with `EmployeeSnapshot`, `Fps`, `Eps`, `PensionContribution`, `Approval`
- `PayrollScheme` — per-Client; cadence, employer references, scheme metadata
- `EmployeeRecord` — denormalised for the firm's view; source of truth in BrightPay
- `AutoEnrolmentStatus` — per employee per scheme

**A4.7 Acceptance criteria**

- **AC-A4.1** No FPS submitted late: 100% on-or-before-payday submission for subscribed clients.
- **AC-A4.2** Pension contributions filed within 22-day window: 100%.
- **AC-A4.3** P60 / P11D deadlines met: 100% for subscribed clients.
- **AC-A4.4** Client can submit pay-period data in **under 5 minutes** for routine months.

**A4.8 Open questions**

- BrightPay API depth: are all required operations (FPS, EPS, P11D) exposed?
- Director-only payrolls: keep in scope or push back to client to file directly?
- Pension provider integration depth (file-upload vs full API for each).

---

### A5 — Year-End Statutory Accounts

**A5.0 Phase tag** — Phase 3

**A5.1 Purpose & user personas**

Annual statutory accounts under the appropriate UK GAAP framework — FRS 105 micro / FRS 102 1A small / full FRS 102 — filed at Companies House and with HMRC alongside CT600 (A6). Orchestrate Xero Tax / IRIS / Capium / Sage Final Accounts as the production engine. Personas: `FeeEarner` (Senior), `Manager`, `Partner`, `ClientPrimary` (director).

**A5.2 User stories**

- **US-A5.1** As a `FeeEarner`, I want the year-end pack request list auto-generated based on prior-year working papers and current ledger state.
- **US-A5.2** As a `Manager`, I want lead schedules per balance-sheet area built from ledger data with prior-year comparatives.
- **US-A5.3** As a `Partner`, I want disclosures auto-drafted (FRS 102 1A directors' transactions, related parties) with material items flagged.
- **US-A5.4** As a `ClientPrimary`, I want a 1-page tax summary alongside the accounts so I can see the cash impact, not just the framework.
- **US-A5.5** As a `Manager`, I want one-click filing to Companies House and HMRC once the accounts are signed.

**A5.3 Functional requirements**

- **FR-A5.1** [P3][MUST] **Year-end Job** auto-scheduled per Client ARD; statutory deadline 9 months post-ARD; firm internal SLA configurable (default 5 months pre-deadline kickoff).
- **FR-A5.2** [P3][MUST] **Framework selection** per Client per period: FRS 105 / FRS 102 1A / FRS 102 / FRS 101 (rare for SMB). Eligibility checks against thresholds (with new April 2025 limits).
- **FR-A5.3** [P3][MUST] **Year-end pack request**: structured request list (bank confirmations, stock count, debtors review, accruals, prepayments, fixed asset additions, dividend record, DLA reconciliation).
- **FR-A5.4** [P3][MUST] **Working papers**: lead schedule per balance-sheet area, auto-built from A1 trial balance, with prior-year comparatives and AI-suggested explanatory notes.
- **FR-A5.5** [P3][MUST] **Production engine integration**: route the cleaned dataset to the chosen accounts production engine (Xero Tax / IRIS / Capium / Sage); receive back iXBRL-tagged accounts.
- **FR-A5.6** [P3][MUST] **Disclosures workflow**: FRS 102 1A required disclosures (directors' advances, related-party transactions); FRS 102 full disclosures including new 2026 lease accounting.
- **FR-A5.7** [P3][MUST] **Going concern review**: 12-month cash forecast review (links to A2 forecast); board representation captured.
- **FR-A5.8** [P3][MUST] **Director approval and e-sign**; statutory accounts and minutes finalised.
- **FR-A5.9** [P3][MUST] **Companies House filing** via API (abridged or filleted for small/micro).
- **FR-A5.10** [P3][MUST] **HMRC filing** alongside CT600 (A6) — full accounts + CT computation + CT600.
- **FR-A5.11** [P3][SHOULD] **S455 director's loan tracking** through year-end; alerts when overdrawn beyond 9-month repayment window.
- **FR-A5.12** [P3][SHOULD] **Audit-bridging**: where the client crosses thresholds, hand-off workflow to A9 (Backlog) — at minimum, tag for external audit referral.

**A5.4 AI capabilities**

- **AI-A5.1** [P3][MUST] **AI year-end pack generator**: tailored request list per Client based on prior year + current ledger state.
- **AI-A5.2** [P3][MUST] **AI disclosure draft**: AI drafts FRS 102 1A and full FRS 102 disclosures from ledger and supporting documents.
- **AI-A5.3** [P3][SHOULD] **AI lead-schedule narrative**: AI explains material movements per balance-sheet line.
- **AI-A5.4** [P3][SHOULD] **AI 2026-FRS-change explainer**: where new disclosure requirements apply, AI surfaces them with what's needed.

**A5.5 Integrations consumed**

- A1 (ledger; trial balance, transactions)
- Accounts production engine (Xero Tax / IRIS / Capium / Sage Final Accounts)
- Companies House API
- HMRC CT online (joint with A6)

**A5.6 Data entities**

`ComplianceServices` context:

- `YearEndAccounts : Filing` — aggregate root with `TrialBalance`, `LeadSchedule`, `Disclosure`, `Approval`
- `AccountingFramework` — per Client per period (FRS105 / FRS102_1A / FRS102 / FRS101)
- `WorkingPaperFile` — collection of lead schedules and supporting docs

**A5.7 Acceptance criteria**

- **AC-A5.1** 100% of year-end accounts filed at Companies House within statutory 9-month deadline.
- **AC-A5.2** Working papers auto-populated from A1 with **<10% manual data entry** at Senior level.
- **AC-A5.3** AI disclosure drafts pass partner review with no edits in **>40% of small-FRS-102 1A cases** within 6 months of go-live.
- **AC-A5.4** S455 alerts fire 30 days before the 9-month repayment deadline.

**A5.8 Open questions**

- Production engine: build adapters for all of Xero Tax / IRIS / Capium / Sage, or pick one for MVP launch of the area?
- 2026 FRS 102 changes (lease on balance sheet, supplier finance) — full coverage at P3 launch or phased?
- LLP, partnership, and charity SORP variants — Phase 3 or backlog?

---

### A6 — Corporation Tax

**A6.0 Phase tag** — Phase 3

**A6.1 Purpose & user personas**

CT600 alongside A5. Compute taxable profit, apply the right rate including marginal relief, claim available reliefs and allowances, file at HMRC, manage payment timing. Personas: `FeeEarner`, `Manager`, `Partner` (especially for marginal relief / R&D / groups), `ClientPrimary`.

**A6.2 User stories**

- **US-A6.1** As a `Manager`, I want the CT computation auto-built from A5 trial balance with disallowables, capital allowances, and brought-forward losses pre-applied.
- **US-A6.2** As a `Partner`, I want associated-companies marginal relief calculated correctly across a client's group.
- **US-A6.3** As a `ClientPrimary`, I want a one-page summary: "CT due £X on Y date. Last year £W. Three things to consider for next year."
- **US-A6.4** As a `Manager`, I want R&D claim workflow integrated with proper Additional Information Form support and HMRC pre-notification.
- **US-A6.5** As a `PracticeAdmin`, I want CT payment dates tracked with reminder cadence (9-month-and-1-day rule).

**A6.3 Functional requirements**

- **FR-A6.1** [P3][MUST] **CT computation** built from A5 output: accounting profit + disallowables – capital allowances – B/F losses + add-backs (S455) – R&D enhancement.
- **FR-A6.2** [P3][MUST] **Rate application**: 19% small-profits, 25% main, marginal relief between, with associated-companies sharing of limits.
- **FR-A6.3** [P3][MUST] **Capital allowances**: AIA up to £1m, WDA, FYA, Full Expensing on plant, structures-and-buildings allowance.
- **FR-A6.4** [P3][MUST] **Loss management**: trading, capital, non-trade losses tracked across years; carry-back claims supported; group relief modelled.
- **FR-A6.5** [P3][MUST] **iXBRL-tagged CT600 + computation** generated via integrated tax engine; filed via HMRC Corporation Tax online API.
- **FR-A6.6** [P3][MUST] **R&D claim workflow** (links to A8): pre-submission notification (within 6 months of period end for new claimants); CT600L; Additional Information Form; merged-scheme rules (April 2024+).
- **FR-A6.7** [P3][MUST] **S455 tracking**: overdrawn DLA at year-end attracts 33.75%; refundable when repaid; 9-month repayment window tracked.
- **FR-A6.8** [P3][MUST] **Payment scheduling**: 9 months + 1 day after period end; QIPs for "large" companies (>£1.5m profits, divided by associates).
- **FR-A6.9** [P3][SHOULD] **Patent Box election** support.
- **FR-A6.10** [P3][SHOULD] **Group structure modelling**: associated companies, group relief, consortium.

**A6.4 AI capabilities**

- **AI-A6.1** [P3][MUST] **AI marginal-relief explainer**: drafts the plain-English narrative on why this year's effective rate is what it is.
- **AI-A6.2** [P3][MUST] **AI capital-allowance categoriser**: AI proposes which fixed asset additions qualify for AIA / Full Expensing / WDA / SBA, with reasoning.
- **AI-A6.3** [P3][SHOULD] **AI R&D-eligibility pre-screen**: AI scans the year's expenditure for likely R&D-eligible activities, highlights candidate projects.
- **AI-A6.4** [P3][SHOULD] **AI tax-planning prompts**: at year-end-minus-90-days, AI surfaces planning options (pension contributions, capex timing, dividend timing) with quantified saving estimates.

**A6.5 Integrations consumed**

- A5 (trial balance and accounts dataset)
- HMRC Corporation Tax online API
- Tax engine for CT600 production (TaxCalc / IRIS / Capium / CCH)
- Companies House (group structure data)

**A6.6 Data entities**

`ComplianceServices` context:

- `CorporationTaxReturn : Filing` — aggregate root with `TaxComputation`, `MarginalReliefCalc`, `RdClaim`, `PatentBoxElection`
- `CapitalAllowancesPool` — per Client tracked across years
- `LossMemorandum` — per Client tracked across years

**A6.7 Acceptance criteria**

- **AC-A6.1** 100% CT600 filings within statutory 12-month deadline.
- **AC-A6.2** No client receives a marginal-relief miscalculation due to associated-companies oversight.
- **AC-A6.3** R&D claim notifications filed within 6 months of period-end for first-time/lapsed claimants — 100%.
- **AC-A6.4** AI capital-allowance categoriser reaches **>85% acceptance** rate on the evaluation set.

**A6.8 Open questions**

- Tax engine selection (same decision as A7).
- R&D claim depth: fully in-product vs handoff to specialist boutique with workflow integration.
- Patent Box: in scope at P3 or backlog?

---

### A8 — Specialist Compliance (CIS, R&D, EMI, ATED, etc.)

**A8.0 Phase tag** — Phase 3 *(CIS first; R&D, EMI, ATED, SDLT layered in across Phase 3)*

**A8.1 Purpose & user personas**

The discrete regulatory regimes that apply to subsets of clients. CIS (construction), R&D (innovation), EMI (share schemes), ATED (residential property held in companies), Patent Box, SDLT (stamp duty), Capital Goods Scheme, charity / SORP. Each has its own filings, deadlines, computation rules. Specialist work, highest-margin compliance. Personas: `FeeEarner`, `Manager`, `Partner` (specialist), `ClientPrimary`.

**A8.2 User stories**

- **US-A8.1** As a `PayrollAdmin`, I want CIS300 auto-prepared each month from CIS-flagged transactions, with subcontractor verification status maintained.
- **US-A8.2** As a `Manager`, I want a cross-client scanner that flags clients with R&D-eligible activity I should pitch to.
- **US-A8.3** As a `Partner`, I want EMI grant workflow with the 92-day filing clock from the moment options are issued.
- **US-A8.4** As a `Manager`, I want ATED returns auto-scheduled annually for clients holding qualifying residential property.
- **US-A8.5** As a `Partner`, I want SDLT calculations and filing for client property purchases within the 14-day deadline.

**A8.3 Functional requirements**

- **FR-A8.1** [P3][MUST] **CIS300 monthly workflow**: subcontractor verification with HMRC; deduction calc on labour element only (excl. materials & VAT); P&D statements to subcontractors by 19th; CIS300 filing by 19th.
- **FR-A8.2** [P3][MUST] **CIS-suffered offset**: where the client is also a subcontractor, CIS suffered offset against PAYE liability via EPS in A4.
- **FR-A8.3** [P3][MUST] **R&D claim workflow**: project description capture; competent professional sign-off; eligible cost quantification (staff, subcontractors, software, consumables); merged-scheme calc; CT600L + Additional Information Form generation.
- **FR-A8.4** [P3][MUST] **R&D pre-notification**: within 6 months of period end for new/lapsed claimants — clock tracked, alerts fired.
- **FR-A8.5** [P3][MUST] **EMI grant workflow**: scheme rules check, valuation reference, grant agreement generation, employee eligibility check, **92-day grant notification** to HMRC tracked.
- **FR-A8.6** [P3][MUST] **ERS annual return** (Employment Related Securities) by 6 July — auto-scheduled where any share-scheme activity exists.
- **FR-A8.7** [P3][MUST] **ATED workflow**: per-Client property register; annual return 1 April–30 April; relief applications (let to third party, employee accommodation); ATED-related CGT.
- **FR-A8.8** [P3][MUST] **SDLT workflow**: completion-data capture; calculation including additional rate, FTB, mixed-use; SDLT1 filing within 14 days.
- **FR-A8.9** [P3][SHOULD] **Patent Box** election and computation (links to A6).
- **FR-A8.10** [P3][SHOULD] **Capital Goods Scheme** monitoring (10-year window for land/buildings >£250k, computer equipment >£50k).
- **FR-A8.11** [P3][SHOULD] **Charity / SORP** annual return + accounts (Charity Commission filing, within 10 months of year end).

**A8.4 AI capabilities**

- **AI-A8.1** [P3][MUST] **AI cross-client specialist scanner**: AI surfaces the firm's clients with: likely R&D eligibility, ATED-threshold-crossing properties, EMI-suitable equity events, SDLT mixed-use opportunities. Driver of pitching upsell.
- **AI-A8.2** [P3][MUST] **AI R&D narrative drafting**: from project descriptions and time records, AI drafts the technical advance and uncertainty narratives required by HMRC.
- **AI-A8.3** [P3][SHOULD] **AI CIS materials/labour split**: AI proposes the split for borderline construction transactions.

**A8.5 Integrations consumed**

- HMRC CIS online (CIS300, P&D)
- HMRC ASA (R&D claim filing alongside CT600)
- HMRC ERS online (annual share-scheme returns)
- HMRC ATED online
- HMRC SDLT online
- Charity Commission API (for charity clients)
- A4 (CIS suffered → EPS offset)

**A8.6 Data entities**

`ComplianceServices` context:

- `CisReturn : Filing`, `RdClaim : Filing`, `EmiGrant`, `AtedReturn : Filing`, `SdltReturn : Filing`, `ErsReturn : Filing`, `PatentBoxElection`, `CharityAnnualReturn : Filing`
- `Subcontractor` — per-Client register with verification status

**A8.7 Acceptance criteria**

- **AC-A8.1** 100% CIS300s filed by 19th of the month.
- **AC-A8.2** EMI grant notifications filed within 92 days — 100%.
- **AC-A8.3** ATED returns filed within April window — 100%.
- **AC-A8.4** Specialist scanner identifies at least 10 cross-client R&D opportunities per 100-client firm in the first run, with >50% partner-validated as worth pitching.

**A8.8 Open questions**

- R&D depth: full in-product vs partnered-specialist hand-off (HMRC enforcement is sharp; reputational risk if poor claims).
- Charity SORP variant: phase 3 or backlog?
- Insolvency / probate: definitively out of scope.

---

### A10 — Advisory & Tax Planning

**A10.0 Phase tag** — Phase 3

**A10.1 Purpose & user personas**

The margin layer. Use deep client knowledge from compliance work to proactively identify opportunities — save tax, restructure, plan succession, fund expansion, optimise extraction, navigate transactions. Productised where possible (Annual Tax Review, Exit Readiness), bespoke where necessary. Personas: `Partner`, `Manager`, `FeeEarner`, `ClientPrimary`.

**A10.2 User stories**

- **US-A10.1** As a `Partner`, I want a cross-client advisory-opportunity dashboard surfacing clients due for profit-extraction reviews, EIS/SEIS investments, IHT planning, exit prep.
- **US-A10.2** As a `Manager`, I want productised advisory engagements (Annual Tax Review, Incorporation Review, Exit Readiness) with scope, fee, deliverable templates ready to go.
- **US-A10.3** As a `Partner`, I want to model profit-extraction scenarios with one click, factoring spouse income, NIC thresholds, personal-allowance taper, HICBC.
- **US-A10.4** As a `Partner`, I want HMRC clearance applications drafted for share-for-share, demergers, etc.
- **US-A10.5** As a `ClientPrimary`, I want to see the engagement deliverable in plain English with quantified outcomes.

**A10.3 Functional requirements**

- **FR-A10.1** [P3][MUST] **Advisory engagement catalogue**: productised engagements with scope, fee model, deliverables, time budget templates.
- **FR-A10.2** [P3][MUST] **Cross-client opportunity scanner**: dashboard surfacing clients matching planning patterns (e.g. clients with personal allowance taper risk; clients with R&D-eligible activity; clients with property holding triggering BPR loss-of-relief risk).
- **FR-A10.3** [P3][MUST] **Profit extraction model**: scenario tool — salary / dividend / pension / benefits / DLA — across the household, year-on-year multi-period.
- **FR-A10.4** [P3][MUST] **Incorporation review model**: sole trader → Ltd comparison; s162 incorporation relief; 3-year projection.
- **FR-A10.5** [P3][MUST] **Exit readiness assessment**: pre-sale grooming checklist; BADR eligibility check; share-for-share / EOT / MBO scenarios.
- **FR-A10.6** [P3][MUST] **Engagement workflow**: scope → workplan → deliverable production → action log → review meeting → file note.
- **FR-A10.7** [P3][MUST] **Advice memo retention**: every advice memo retained for 7 years minimum, e-signed where formal.
- **FR-A10.8** [P3][SHOULD] **HMRC clearance application** drafting: share-for-share, demerger, valuation for EMI, etc.
- **FR-A10.9** [P3][SHOULD] **PCRT / DOTAS / GAAR red-flag check**: every advisory output is scanned for risk-flag patterns; partner sign-off required where flagged.

**A10.4 AI capabilities**

- **AI-A10.1** [P3][MUST] **AI opportunity scanner**: AI scans the firm's client book monthly and surfaces a ranked list of advisory opportunities by client, with reasoning and quantified estimate.
- **AI-A10.2** [P3][MUST] **AI tax-planning model generator**: from a client's data, AI produces an extraction-strategy comparison with quantified savings and explanation.
- **AI-A10.3** [P3][MUST] **AI scenario explainer**: AI writes the partner-facing memo on a tax-planning scenario in firm tone-of-voice.
- **AI-A10.4** [P3][SHOULD] **AI HMRC clearance drafter**: AI drafts standard clearance applications from structured engagement inputs.
- **AI-A10.5** [P3][SHOULD] **AI PCRT / DOTAS / GAAR scanner**: AI flags advisory drafts containing patterns associated with abusive arrangements.

**A10.5 Integrations consumed**

- A1 (ledger data feeds models)
- A2 (forecasts feed scenarios)
- A6 / A7 (tax-position data)
- B4 (advice memos retained as documents)
- B3 (engagement billing — bespoke pricing)

**A10.6 Data entities**

`AdvisoryServices` context:

- `AdvisoryEngagement` — aggregate root with `Brief`, `WorkPlan`, `Deliverable`, `ActionLog`, `FileNote`
- `OpportunityFlag` — generated by AI per Client per planning category
- `AdviceMemo` — retained per A10.7 retention rule

**A10.7 Acceptance criteria**

- **AC-A10.1** Opportunity scanner produces at least 1 actionable advisory opportunity per 5 clients per year for 80% of firms after 6 months operation.
- **AC-A10.2** Profit-extraction model output validated against worked examples to within £50 across 95% of test cases.
- **AC-A10.3** PCRT / DOTAS / GAAR scanner flags ≥99% of seeded high-risk patterns.
- **AC-A10.4** Productised advisory engagements with fixed scope account for **>60%** of advisory revenue within 12 months of P3 launch.

**A10.8 Open questions**

- HMRC clearance drafting depth — generic drafts vs full filing.
- Bespoke advisory: how much workflow vs free-form file-noting?
- Whether to embed external technical research (Croner-i, Tolley's) or stay LLM-driven with citations.

---

### A11 — Virtual CFO Retainer

**A11.0 Phase tag** — Phase 3

**A11.1 Purpose & user personas**

The natural endpoint of the advisory ladder for £1m–£20m turnover SMBs. Embedded finance function: bookkeeping + management accounts + KPIs + cash + strategic finance leadership at fractional cost. Personas: `Partner` (named VCFO), `Manager` (operational finance manager), `FeeEarner` (bookkeeping support), `ClientPrimary` (founder/MD), client board members (read-only).

**A11.2 User stories**

- **US-A11.1** As a `Partner` acting as VCFO, I want a per-client VCFO dashboard pulling all relevant data (cash, pipeline, KPIs, month-end close, year-end progress, tax plan).
- **US-A11.2** As a `ClientPrimary`, I want a board pack each month produced by the firm with no input from me.
- **US-A11.3** As a `Partner`, I want event-driven workflows for fundraise, sale, acquisition with pre-built data-room and DD-pack templates.
- **US-A11.4** As a `Manager`, I want a 13-week cash-runway alert that fires before any client hits liquidity stress.
- **US-A11.5** As a board member (read-only), I want to access the latest pack via a dedicated portal with no firm onboarding.

**A11.3 Functional requirements**

- **FR-A11.1** [P3][MUST] **VCFO retainer engagement type** with monthly cadence and event surcharge mechanics; sits on top of bundled A1+A2.
- **FR-A11.2** [P3][MUST] **VCFO dashboard** per client aggregating: cash position, runway, pipeline, KPIs, month-end status, year-end progress, tax position, advisory action log.
- **FR-A11.3** [P3][MUST] **Board pack** generator: monthly pack template with structured commentary, KPIs, trends, decisions taken, decisions for board, financial statements, forecast.
- **FR-A11.4** [P3][MUST] **Board portal**: dedicated, time-bounded read-only access for non-firm board members with audit trail of who viewed what.
- **FR-A11.5** [P3][MUST] **Reforecast workflow**: rolling 12-month reforecast updated quarterly; variance commentary auto-drafted.
- **FR-A11.6** [P3][MUST] **Cash-runway alerts**: configurable threshold (e.g. <12 weeks); auto-alert to VCFO partner with proposed actions.
- **FR-A11.7** [P3][MUST] **Fundraise workflow**: data-room workflow; investor pack template; financial model template; question-and-answer log.
- **FR-A11.8** [P3][MUST] **Sale workflow**: commercial DD pack template; broker/buyer Q&A management; deal-team coordination.
- **FR-A11.9** [P3][SHOULD] **Cap-table integration**: Vestd / Capdesk / Carta where available for real-time cap table view.
- **FR-A11.10** [P3][SHOULD] **Lender portal**: facility-renewal pack management for lenders.

**A11.4 AI capabilities**

- **AI-A11.1** [P3][MUST] **AI board pack drafter**: extends A2 commentary into full board narrative with sections (CEO update support, performance, cash, KPI deep-dive, risks, decisions).
- **AI-A11.2** [P3][MUST] **AI cash-runway interpreter**: when runway falls, AI explains drivers with action options (cost cuts, pricing actions, fundraise timing).
- **AI-A11.3** [P3][SHOULD] **AI fundraise FAQ generator**: drafts answers to common investor questions from client data.
- **AI-A11.4** [P3][SHOULD] **AI deal-team summary**: keeps the partner up to date with daily summaries of activity in active fundraise / sale workflows.

**A11.5 Integrations consumed**

- A1 (ledger), A2 (management accounts), A5/A6 (year-end/CT integration)
- Cap-table providers (Vestd / Capdesk / Carta) — Phase 3+
- Email / Calendar — board-cadence scheduling
- DocuSign-equivalent / portal for board-member access

**A11.6 Data entities**

`AdvisoryServices` context:

- `VirtualCfoRetainer` — aggregate root with `MonthlyCadence`, `BoardMeeting`, `Reforecast`, `EventSurcharge`
- `BoardPack` — per cadence
- `BoardMember` — extended `ClientReadOnly` with bound time window

**A11.7 Acceptance criteria**

- **AC-A11.1** Board pack delivered on agreed monthly cadence with **<5% miss rate** over 12 months.
- **AC-A11.2** Cash runway alert fires before any subscribed VCFO client hits liquidity stress.
- **AC-A11.3** Board portal access audit trail captures every read by every external board member.
- **AC-A11.4** Fundraise workflow demonstrably reduces partner data-room build time by **>30%** vs spreadsheet baseline.

**A11.8 Open questions**

- VCFO partner leverage model: how many clients per partner is the design assumption?
- Cap-table integration depth — read-only is enough, or full management?
- Independence: where the firm also audits a VCFO client, the workflow must block — confirm UI guardrails.

---

### B6 — Quality, Risk & Regulatory Compliance (firm's own)

**B6.0 Phase tag** — Phase 3

**B6.1 Purpose & user personas**

The firm's own compliance — to professional bodies (ICAEW / ACCA / CIOT / AAT), AML supervisors, FRC if audit-registered, ICO, PII insurer, Companies House (under ECCTA). Mostly invisible; existential when failed. Personas: `FirmOwner`, `Partner` (compliance partner), `MLRO`, `DPO`.

**B6.2 User stories**

- **US-B6.1** As an `MLRO`, I want a firm-level AML risk assessment refreshed annually with audit-trail evidence.
- **US-B6.2** As a `Compliance Partner`, I want a cold-file review programme that samples completed jobs and tracks findings to remediation.
- **US-B6.3** As a `FirmOwner`, I want CPD records aggregated per individual with deficiency alerts.
- **US-B6.4** As a `MLRO`, I want SAR filing workflow (to NCA) with tipping-off prevention overlaid on B5.
- **US-B6.5** As a `Compliance Partner`, I want ISQM 1 evaluation tooling for the firm's audit practice (where audit-registered).
- **US-B6.6** As a `DPO`, I want SAR (Subject Access Request) tooling that produces a per-individual data extract on demand.
- **US-B6.7** As a `FirmOwner`, I want a complaints register tracking each complaint to closure within professional-body procedure.
- **US-B6.8** As a `Compliance Partner`, I want PII renewal tracked with year-on-year claim history visible.

**B6.3 Functional requirements**

- **FR-B6.1** [P3][MUST] **Firm-level AML risk assessment**: structured annual workflow per MLR 2017 §28 (geography, sector, delivery, structure); annual partner sign-off.
- **FR-B6.2** [P3][MUST] **Per-client AML risk re-assessment**: triggered annually for low/medium, more frequently for high; pulls from B1 risk score; updates ongoing-monitoring schedule.
- **FR-B6.3** [P3][MUST] **Cold-file review programme**: configurable sampling rules; reviewer assignment; findings register; remediation tracking.
- **FR-B6.4** [P3][MUST] **CPD records**: per-individual records aggregated from internal training, external courses, professional-body declarations; deficiency alerts.
- **FR-B6.5** [P3][MUST] **SAR filing workflow**: structured workflow for MLRO to file Suspicious Activity Reports with the NCA; tipping-off prevention enforced via B5 hook (FR-B5.9).
- **FR-B6.6** [P3][MUST] **ISQM 1 evaluation** (audit-registered firms): documentation of the 8 quality-management components; annual evaluation report.
- **FR-B6.7** [P3][MUST] **Independence register** (audit-registered): tracks audit-client relationships, fee dependencies, non-audit services, long-association threats.
- **FR-B6.8** [P3][MUST] **Conflicts register**: cross-firm conflict checks at engagement acceptance (B1) plus periodic refresh.
- **FR-B6.9** [P3][MUST] **Complaints register**: per professional-body procedure; lodge → investigate → resolve → log to regulator if required; SLA tracking.
- **FR-B6.10** [P3][MUST] **Breach register**: any departure from professional standards logged; partner sign-off on remediation.
- **FR-B6.11** [P3][MUST] **Subject Access Request workflow**: for the firm's data subjects; per-individual extract within 30 days.
- **FR-B6.12** [P3][MUST] **Data-class retention enforcement** (links to XR-2.3): automated retention end-of-life flagging.
- **FR-B6.13** [P3][MUST] **PII tracking**: policy details, premium history, claim history, renewal alerts.
- **FR-B6.14** [P3][MUST] **Practice Assurance return** support to ICAEW (or equivalent).
- **FR-B6.15** [P3][SHOULD] **Cyber incident response workflow**: 72-hour GDPR breach reporting clock; 24-hour ICO reporting clock; structured notification templates.

**B6.4 AI capabilities**

- **AI-B6.1** [P3][MUST] **AI independence-threat scanner** (audit firms): cross-references audit-client list with non-audit services, fee dependency, long-association; flags threats requiring documented mitigation.
- **AI-B6.2** [P3][MUST] **AI AML re-screening trigger**: AI surfaces clients whose risk profile has changed (e.g. new sector exposure, new BOOC) and recommends re-assessment.
- **AI-B6.3** [P3][SHOULD] **AI complaints clustering**: identifies recurring complaint themes across the firm to drive process improvement.

**B6.5 Integrations consumed**

- AML / IDV provider (re-screening triggers)
- ICAEW / ACCA member portal (CPD declaration sync, where API exists)
- ICO portal (data-protection registration; breach notifications)
- HMRC AML supervision (where supervised by HMRC)
- PII broker portal (Lockton / Howden / etc.)

**B6.6 Data entities**

`PracticeOperations` → QualityAndRisk sub-context:

- `IsqmEvaluation` — annual aggregate
- `FileReview` — per cold-file review with findings
- `IndependenceDeclaration`
- `BreachRecord`
- `ConflictCheck`
- `ComplaintRecord`
- `CpdRecord` — per individual
- `AmlRiskAssessment` — firm-level + per-client (links to B1)
- `SarRecord` — structured for NCA filing

**B6.7 Acceptance criteria**

- **AC-B6.1** AML risk assessments complete for 100% of clients with documented evidence chain.
- **AC-B6.2** SAR filings always tipping-off-prevented in B5.
- **AC-B6.3** ISQM 1 evaluation produced annually for audit firms with full component coverage.
- **AC-B6.4** SAR (data subject) responses always within 30-day GDPR window.

**B6.8 Open questions**

- Audit-firm-only features: include in P3 or defer with A9 to backlog?
- ECCTA verification flow: subsumed under B1 or surfaced here too?
- Practice Assurance form template currency (ICAEW updates these periodically).

---

### B10 — Practice Finance (firm's own P&L)

**B10.0 Phase tag** — Phase 3

**B10.1 Purpose & user personas**

The firm running its own books. Partner remuneration, capital decisions, peer benchmarking, valuation. Often the cobbler's-children gap in mid-tier UK firms — the product opportunity is to give partners proper MI in real time. Personas: `FirmOwner`, `Partner` (managing partner, finance partner), `PracticeAdmin`.

**B10.2 User stories**

- **US-B10.1** As a `FirmOwner`, I want a real-time partner pack: revenue, profit, lock-up, utilisation, realisation, peer-benchmark comparison.
- **US-B10.2** As a `Partner`, I want my profit allocation visible with the firm's profit-sharing model applied (lockstep / performance / hybrid).
- **US-B10.3** As a `FirmOwner`, I want capital accounts tracked per partner with buy-in / buy-out workflow.
- **US-B10.4** As a `FirmOwner`, I want a firm-level forecast with sensitivity to fee growth, headcount, lock-up.
- **US-B10.5** As a `FirmOwner`, I want the firm's own VAT and tax filings handled by the same product (eat-our-own-dogfood).

**B10.3 Functional requirements**

- **FR-B10.1** [P3][MUST] **Partner pack**: monthly partner-only dashboard with revenue, costs, lock-up, utilisation, realisation, profit per partner — across the firm + per partner / team.
- **FR-B10.2** [P3][MUST] **Peer benchmarks**: surface anonymised cohort comparisons (firm size, region) against ICAEW / Crowe Practice Track data where licensed.
- **FR-B10.3** [P3][MUST] **Profit allocation engine**: configurable model (lockstep / modified lockstep / performance / hybrid); points or shares; quarterly distribution + year-end true-up.
- **FR-B10.4** [P3][MUST] **Partner capital accounts**: per partner; contributions, drawings, profit allocation, capital changes; buy-in / buy-out workflow.
- **FR-B10.5** [P3][MUST] **Firm-level forecasting**: P&L + cash forecast for the firm; sensitivity scenarios.
- **FR-B10.6** [P3][MUST] **Self-service mode**: the firm's own books can be run via the product's internal A1/A3/A6 features (the product dogfoods itself for its accountancy customers).
- **FR-B10.7** [P3][MUST] **Client profitability ranking**: top/bottom-decile clients by profit; flagged for renewal-cycle action.
- **FR-B10.8** [P3][SHOULD] **Acquisition appraisal tooling**: target firm DD support — fees by client, lock-up, retention, partner concentration risk.
- **FR-B10.9** [P3][SHOULD] **Firm valuation refresh**: GRF and EBITDA-multiple scenarios with peer benchmarks.

**B10.4 AI capabilities**

- **AI-B10.1** [P3][MUST] **AI partner pack drafter**: drafts the monthly narrative — what's up / down / at risk in firm performance.
- **AI-B10.2** [P3][MUST] **AI lock-up driver explainer**: when lock-up moves, AI identifies drivers (specific clients, specific stages of the WIP-to-cash cycle).
- **AI-B10.3** [P3][SHOULD] **AI client-profitability ranking commentary**: drafts the partner-facing memo on the bottom decile with re-pricing / renewal options.

**B10.5 Integrations consumed**

- B3 (the firm's own time, billing, WIP, lock-up data)
- A1/A3/A6 (the firm's own ledger, VAT, CT)
- B7 (people cost data — Phase 3 sub-area)
- External benchmark data (ICAEW, Crowe Practice Track)

**B10.6 Data entities**

`PracticeOperations` → PracticeFinance sub-context:

- `FirmLedger` — firm's own accounts (separate tenant data scope from clients)
- `PartnerCapitalAccount`
- `ProfitAllocation`
- `Benchmark` — peer cohort data
- `FirmValuation`

**B10.7 Acceptance criteria**

- **AC-B10.1** Partner pack updates within **5 minutes** of underlying data changes.
- **AC-B10.2** Profit allocation produces correct partner-by-partner amounts to penny on test cases.
- **AC-B10.3** Peer benchmarks delivered with explicit cohort definition and confidentiality safeguards.
- **AC-B10.4** Firm dogfooding: the product's own partnership runs its accounts, billing, VAT, and CT through itself.

**B10.8 Open questions**

- Benchmark data licensing: ICAEW survey, Crowe Practice Track, or aggregated within-product cohort.
- Multi-currency for firms with international clients: Phase 3 or defer.
- Acquisition appraisal: how far to go vs partner with M&A advisers.

---

### Backlog Areas

The following areas are captured for completeness but are **not on the roadmap** at this design stage. Each gets a brief scope statement and the key deferred decisions.

---

### B7 — People, Resourcing & Training *(Backlog)*

**Scope statement.** Workforce planning, recruitment pipeline, qualification tracking (ACA / ACCA / AAT), CPD plans, appraisal cycle, capacity planning. Most small UK firms run this in Breathe HR, BambooHR, BrightHR, or spreadsheets; the product surface here is thin until firms are large enough to feel the pain.

**Why backlog.** Capacity-planning data is needed in B2 (already covered as part of workflow), CPD records are needed in B6 (covered), and salary/payroll for the firm's own staff is covered by A4 dogfooding. The remaining HR features (recruitment, ATS, performance management, engagement surveys) are commodity and well-served externally.

**If pulled forward:** integration to BreatheHR or BambooHR is more valuable than building. Phase tag would be Phase 3 if a customer demands it.

**Deferred decisions:** Whether to integrate to one HR vendor as a default; whether to host CPD content (training); whether apprenticeship-levy management is in scope.

---

### B8 — CRM, Marketing & Pipeline *(Backlog — non-proposal portion)*

**Scope statement.** Prospect pipeline (CRM), email marketing campaigns, content / blog hosting, SEO, social, LinkedIn ABM, win/loss attribution, NPS surveys, lapsed-client win-back. The proposal/pricing/productisation portion of B8 is in Phase 2; this is the marketing-and-pipeline rest.

**Why backlog.** Most UK practices run pipeline in HubSpot, Pipedrive, or Capsule; marketing in Mailchimp / HubSpot. Building these functions natively duplicates excellent commodity tooling.

**If pulled forward:** integrations to HubSpot/Pipedrive on the lead-capture seam; native NPS via Delighted-equivalent. Phase 3 at earliest.

**Deferred decisions:** whether to build a content hub; whether to integrate-vs-build pipeline; ICAEW / ACCA marketing rules compliance enforcement.

---

### B9 — Technology Stack Management *(Backlog)*

**Scope statement.** Tooling for the firm to manage its own tech stack — integration register, OAuth token management, MFA enrolment, joiner/leaver, security training, audit logs, vendor risk assessment.

**Why backlog.** Largely subsumed by Section 2 cross-cutting (XR-2.5 integration framework, XR-2.2 identity, XR-2.3 audit log, XR-2.6 NFRs). The additional B9 surface is mostly admin UI for the cross-cutting features. Treated as part of MVP delivery rather than a separate area.

**If pulled forward:** richer firm-admin UI (integration health dashboard, security posture scorecard, joiner/leaver workflow). Likely Phase 2/3 alongside other admin features.

**Deferred decisions:** whether to track non-product SaaS tools the firm uses externally (a "tech-stack register" for the firm); cyber-essentials self-attestation tooling.

---

### A9 — Audit & Assurance *(Backlog)*

**Scope statement.** Statutory audit work for clients above the (post-April-2025) thresholds — turnover £15m / balance sheet £7.5m / 50 employees, 2-of-3 for two consecutive years. ISA UK methodology, audit working papers, audit reports. Specialist registration required (ICAEW RSB, RI status per partner).

**Why backlog.** Three reasons. (1) Target firm size (5–20 staff) typically does not hold audit registration — the post-2025 threshold uplift moved ~14,000 medium-sized companies into audit-exempt small. (2) Audit methodology is a separate product domain (Caseware, Inflo, Mercia compete here, with deep ISA workflow logic). (3) Regulatory bar is significantly higher (FRC inspection regime). Building audit risks distracting from the core product.

**If pulled forward:** likely partnership / OEM with Inflo or Mercia rather than build. Significant ISQM 1 implications (B6 deepens). Phase 4+ at earliest.

**Deferred decisions:** build vs OEM; whether to support audit-exempt assurance engagements (independent examination for charities) as a lower-bar alternative; group-audit support depth.

---

## 4. Glossary

### UK regulatory & tax terms

| Term | Definition |
|---|---|
| **AAT / ACA / ACCA / CIOT / ICAEW / ICAS / CIMA** | Professional bodies for UK accountants and tax advisers |
| **AML** | Anti-Money Laundering — the regulatory regime under MLR 2017 |
| **AOE** | Attachment of Earnings Order — court order requiring deductions from pay |
| **ARD** | Accounting Reference Date — a company's year-end at Companies House |
| **ASA** | Agent Services Account — HMRC account for agents acting under MTD |
| **ATED** | Annual Tax on Enveloped Dwellings — applies to companies holding UK residential property worth >£500k |
| **AIA** | Annual Investment Allowance — capital allowances regime allowing 100% relief up to £1m |
| **BADR** | Business Asset Disposal Relief — 10% CGT rate on qualifying business sales (formerly Entrepreneurs' Relief) |
| **BPR** | Business Property Relief — IHT relief on qualifying business assets |
| **CDD** | Customer Due Diligence — the AML identity-and-risk process |
| **CGT** | Capital Gains Tax |
| **CIS** | Construction Industry Scheme — withholding regime on payments to construction subcontractors |
| **CS01** | Confirmation Statement — annual filing at Companies House confirming company details |
| **CT** | Corporation Tax |
| **CT600** | Corporation Tax return form |
| **DPS** | Data Provisioning Service — HMRC's mechanism for pushing tax-code notifications to agents |
| **ECCTA** | Economic Crime and Corporate Transparency Act 2023 — introduces ID verification for directors and PSCs |
| **EIS / SEIS / VCT** | Investment relief schemes for tax-efficient equity investment |
| **EMI** | Enterprise Management Incentive — tax-advantaged share option scheme |
| **EPS** | Employer Payment Summary — RTI submission complementing FPS |
| **ERS** | Employment Related Securities — annual return for share-scheme activity |
| **FPS** | Full Payment Submission — RTI submission for each pay run, on or before payday |
| **FRC** | Financial Reporting Council — UK audit regulator |
| **FRS 102 / FRS 105 / FRS 101** | UK GAAP frameworks; FRS 105 for micro-entities, FRS 102 1A for small, full FRS 102 for medium/large |
| **GAAR** | General Anti-Abuse Rule — UK rule against abusive tax arrangements |
| **HICBC** | High Income Child Benefit Charge — claws back child benefit at high incomes |
| **HMRC** | His Majesty's Revenue and Customs |
| **iXBRL** | Inline eXtensible Business Reporting Language — required tagging for accounts and CT computations |
| **ISA UK** | International Standards on Auditing — methodology for UK statutory audits |
| **ISQM 1** | International Standard on Quality Management 1 — quality framework for audit firms |
| **MLRO** | Money Laundering Reporting Officer — statutory designated principal under MLR 2017 |
| **MTD / MTD-IT / MTD VAT** | Making Tax Digital — HMRC's digital filing regime; MTD-IT mandatory for £50k+ income from April 2026 |
| **NCA** | National Crime Agency — receives Suspicious Activity Reports |
| **NIC** | National Insurance Contributions |
| **PCRT** | Professional Conduct in Relation to Taxation — cross-body code |
| **P11D / P11D(b)** | Annual return of employee benefits-in-kind / employer Class 1A NIC declaration |
| **P60** | Annual employee statement of pay and tax |
| **PII** | Professional Indemnity Insurance |
| **PSC** | Person with Significant Control — beneficial owner ≥25% under the PSC regime |
| **QAD** | Quality Assurance Department — ICAEW's monitoring function |
| **R&D / RDEC** | Research & Development tax relief schemes |
| **RI** | Responsible Individual — partner with personal authorisation to sign audit reports |
| **RTI** | Real Time Information — HMRC's PAYE reporting regime |
| **SA / SA100 / SA800 / SA900** | Self Assessment — annual tax returns for individuals (SA100), partnerships (SA800), trusts (SA900) |
| **SAR** | (1) Suspicious Activity Report — AML filing to NCA. (2) Subject Access Request — GDPR right of access |
| **SDLT** | Stamp Duty Land Tax — applies on UK property purchases |
| **SH01** | Return of allotment of shares — Companies House filing |
| **SIC** | Standard Industrial Classification — sector codes |
| **SORP** | Statement of Recommended Practice — sector-specific accounting (e.g. for charities) |
| **S455** | Section 455 CTA 2010 — 33.75% tax on overdrawn director's loan accounts |
| **TPR** | The Pensions Regulator |
| **TTP** | Time to Pay — HMRC instalment arrangement |
| **VAT** | Value Added Tax |

### Practice-management terms

| Term | Definition |
|---|---|
| **Lock-up** | WIP days + Debtor days; cash tied up between work delivered and cash received |
| **WIP** | Work in Progress — unbilled time × rate |
| **Realisation** (recovery) | Billed value as % of standard time × rate; how much of potential revenue is captured |
| **Utilisation** | Chargeable hours ÷ available hours |
| **GRF** | Gross Recurring Fees — basis of historic practice valuations |
| **PEP** | Profit per Equity Partner |
| **OMB** | Owner-Managed Business |
| **HNWI** | High Net Worth Individual |

### Product-specific terms

| Term | Definition |
|---|---|
| **Engagement** | The contractual relationship for one or more services with a Client; aggregate root in `EngagementWorkflow` context |
| **Job** | A unit of recurring or event-driven work, e.g. "Q3 VAT for Acme Ltd"; aggregate root in `EngagementWorkflow` context |
| **Filing** | A regulatory submission produced by a Job (VAT return, CT600, CS01, etc.); polymorphic aggregate in `ComplianceServices` context |
| **Task** | A checklist item within a Job |
| **RecurringJobSchedule** | The pattern that auto-creates future Job instances per cadence and Client |
| **Document** | An immutable, classified, retained artifact tied to Client/Job/Period; aggregate root in `EngagementWorkflow` context |
| **Query** | A structured outstanding question linked to a Job, with an SLA clock; aggregate root in `ClientRelationship` context |
| **Tenant / Firm** | A single accountancy practice using the product as a single tenant |

### Role definitions (full table)

See Section 2.2 for the complete role model.

---

## 5. Open Questions

These decisions are intentionally deferred until product strategy crystallises further or the relevant area's design is opened. Each is logged so it does not get silently assumed.

### Provider selections

- **OQ-1.** AML / KYC provider selection: SmartSearch vs Veriphy vs Credas. Affects B1, B6 integration depth and per-check cost passthrough.
- **OQ-2.** E-signature: build a thin layer over SignWell / HelloSign / Adobe Sign, or build native? Affects B1, B4.
- **OQ-3.** AI inference platform: Anthropic / OpenAI / multi-provider gateway with EU/UK residency. Affects every AI feature and Section 2.4 governance.
- **OQ-4.** Cloud / data residency: AWS eu-west-2 vs Azure UK South vs hybrid. Knock-on effects on which AI inference services qualify.
- **OQ-5.** Tax engine for SA100 / CT600: TaxCalc vs IRIS vs Capium. Affects A6, A7. Could be multi-provider via adapter.
- **OQ-6.** Accounts-production engine for year-end: Xero Tax vs IRIS Elements vs Capium vs Sage Final Accounts. Affects A5.
- **OQ-7.** Payroll engine: BrightPay confirmed; whether Moneysoft / Sage Payroll alternatives needed. Affects A4.

### Product / business model

- **OQ-8.** Trial mechanics: length, gating (full functionality vs limited), conversion path. Affects B1 onboarding flow and pricing-page features.
- **OQ-9.** Pricing tier definitions: per-user vs per-client vs hybrid; affects feature flagging across all phases.
- **OQ-10.** Mobile delivery: PWA-only at MVP for client portal, or native iOS/Android from day one? Native gives better camera and offline; PWA ships faster.
- **OQ-11.** Buyer model: self-serve signup vs sales-led for the £200–£1k/month firm tier. Affects B1 self-onboarding depth.
- **OQ-12.** Multi-currency: any client work in non-GBP — required from MVP or backlog? Affects A1, A3, A6, B3.

### Regulatory interfaces

- **OQ-13.** Companies House ACSP: does the product enable the firm to act as an Authorised Corporate Service Provider through us, or do firms self-register and we provide tooling? Affects B1, A12.
- **OQ-14.** HMRC software-vendor accreditation: required for MTD VAT, MTD-IT submissions. Process and timeline.
- **OQ-15.** ICAEW / ACCA marketing-rules compliance: enforce in B8 marketing features when built.
- **OQ-16.** Audit-firm features (B6 audit-only): include in P3 or defer with A9?

### Data & integration

- **OQ-17.** Bulk-migration tooling at MVP: which incumbent PM-tool formats to support natively (Karbon, TaxDome, BrightManager, Senta, IRIS, Capium)?
- **OQ-18.** Sage Business Cloud (Sage 50 desktop) ledger integration depth.
- **OQ-19.** Whether to support a "tech-stack register" for the firm's external tools (B9 backlog scope question).
- **OQ-20.** Categorisation rules: own them in the product (richer) or sync to/from Xero rules (simpler)?

### Cross-firm features

- **OQ-21.** Cross-firm staff memberships (one user across multiple firms): when, if ever?
- **OQ-22.** Anonymised peer-benchmark data: aggregate within-product cohort vs licensed third-party data (ICAEW survey, Crowe Practice Track)?

---

*End of design document.*


