# SMB Accountancy Practice — Working Model Research

**Compiled:** 10 May 2026
**Jurisdiction focus:** United Kingdom (HMRC, Companies House)
**Scope:** End-to-end working model of an SMB-focused UK accountancy practice — service lines, internal operating functions, ecosystem context, and a candidate domain model for system design.

This is a starting-point reference. It captures the firm as an organism: what it sells, how it runs, the regulatory frame it sits inside, the economics that drive it, and a candidate software domain model derived from those mechanics. Each drill-down is structured against a fixed 10-section template so areas can be compared apples-to-apples.

---

## Table of Contents

1. **Overview — High-Level Functions**
2. **The Wider Context** — value chain, ecosystem, history, macro forces
3. **Firm Mechanics** — structure, regulation, segments, pricing, staffing, KPIs, tech, trends, economics
4. **Drill-Down Framework** — the 10-section template
5. **Client-Facing Service Lines (A1–A12)**
6. **Internal Operating Functions (B1–B10)**
7. **Synthesis** — three loops, three layers, three engines
8. **A Candidate Domain Model** — bounded contexts, aggregates, sequencing

---

## 1. Overview — High-Level Functions

An SMB accountancy practice is two businesses sharing a roof: a set of **client-facing service lines** (what it sells) and a set of **internal operating functions** (how it runs). They are often confused; they matter very differently.

### 1.1 Client-Facing Service Lines

| # | Function | What it involves | Cadence |
|---|---|---|---|
| A1 | **Bookkeeping / Client Accounting Services (CAS)** | Recording transactions, bank reconciliations, AP/AR, expense capture, cloud ledger maintenance | Weekly–monthly |
| A2 | **Management Accounts** | Periodic P&L, BS, cash-flow, KPI packs, variance commentary | Monthly/quarterly |
| A3 | **VAT / Indirect Tax** | Registration, scheme selection, quarterly returns, EC sales, reverse charge | Quarterly |
| A4 | **Payroll & Employment Taxes** | Pay runs, RTI/FPS, pensions auto-enrolment, P11D | Monthly + annual |
| A5 | **Year-End Statutory Accounts** | Annual financial statements (FRS 105 / FRS 102 1A / full FRS 102) | Annual |
| A6 | **Corporation Tax** | Computing taxable profit, CT600, tax provisioning | Annual |
| A7 | **Personal Tax / Self Assessment** | SA returns + (from April 2026) MTD-IT for £50k+ | Annual + quarterly |
| A8 | **Specialist Compliance** | CIS, R&D credits, EMI/share schemes, ATED, Patent Box, SDLT | Event/annual |
| A9 | **Audit & Assurance** | Statutory audits where thresholds met | Annual |
| A10 | **Advisory & Tax Planning** | Forecasting, structuring, finance, valuations, M&A, exit | Project/retainer |
| A11 | **Virtual CFO / Outsourced Finance** | Embedded finance leadership for growing SMBs | Ongoing |
| A12 | **Company Secretarial** | Incorporations, confirmation statements, share events, registers | Event/annual |

### 1.2 Internal Operating Functions

1. **B1 Client Onboarding & Engagement** — KYC/AML, professional clearance, engagement letters, fee scoping, software setup, data migration.
2. **B2 Practice / Workflow Management** — Job tracking, deadlines, capacity planning, recurring task automation.
3. **B3 Time, Billing & WIP / Lock-up** — Timesheets, fixed-fee vs hourly recovery, invoicing, debtor management.
4. **B4 Document & Records Management + Client Portal** — Secure portal, e-signature, working papers, retention.
5. **B5 Client Communication & Query Handling** — Query lists, deadline reminders, advisory check-ins, secure messaging.
6. **B6 Quality, Risk & Regulatory Compliance (firm's own)** — Professional body rules, AML supervision, PII, file reviews, CPD.
7. **B7 People, Resourcing & Training** — Recruitment, qualifications pipeline, partner progression.
8. **B8 Business Development, Pricing & Productisation** — Niche positioning, referrals, content, productised services.
9. **B9 Technology Stack Management** — Ledger software, tax software, practice management, AI tooling, integrations.
10. **B10 Practice Finance** — Owner remuneration, profitability per client, lock-up (WIP + debtors).

### 1.3 Recurring Compliance Calendar (UK lens)

Most of the firm's load is **deadline-driven** rather than project-driven.

- **Weekly/monthly:** bookkeeping, payroll RTI, pension submissions
- **Quarterly:** VAT returns (and MTD digital records)
- **Annual:** statutory accounts (within 9 months of year-end), Corporation Tax (within 12 months), Self Assessment (31 Jan), P11Ds (6 July), confirmation statement
- **Ad-hoc:** dividends/minutes, share transactions, HMRC enquiries

### 1.4 Two structural notes

- **Compliance is the floor; advisory is the margin.** Compliance work is commoditising under cloud + AI; firms increasingly bundle it into fixed fees and earn premium on advisory, virtual-CFO, and niche specialisms.
- **The firm's "product" is really a workflow.** When you study an accountancy practice as a system, the dominant entities are *Client → Engagement → Job → Task → Deadline*, with documents and queries hanging off them. Most practice-management software is built around exactly that schema.

---

## 2. The Wider Context

### 2.1 The Value Chain

The firm sits between **clients** (paying for outcomes) and **the State** (demanding filings), funded by fees, mediated by software, staffed by a regulated talent pipeline, and increasingly under pressure from capital (PE) and AI.

```
                       ┌──────────────────────────┐
                       │  HMRC / Companies House  │  ← filings, taxes
                       │  (the State)             │
                       └─────────────▲────────────┘
                                     │ submissions
   Capital ───┐                      │
   (PE, banks)│           ┌──────────┴─────────┐
              ├──────────►│  Accountancy Firm  │◄────── Software vendors
              │           │  (the Practice)    │        (Xero, IRIS, Karbon…)
   Talent ────┤           └──────▲─────▲───────┘
   (ICAEW/ACCA│                  │     │
    pipeline) │           fees   │     │ work
              │           ┌──────┴─────┴───────┐
              └──────────►│   SMB Clients      │
                          └────────────────────┘
                                  ▲
                                  │ banking, ledgers, payments
                          ┌───────┴────────────┐
                          │ Banks, Stripe, etc │
                          └────────────────────┘
```

### 2.2 Ecosystem Actors

| Layer | Actors | Role |
|---|---|---|
| **State / regulators** | HMRC, Companies House, ICO, FRC | Tax collection, statutory filings, data protection, audit oversight |
| **Professional bodies** | ICAEW, ACCA, CIOT, ICAS, AAT, CIMA | Qualifications, practising certificates, ethics, AML supervision, file reviews, CPD |
| **Top-tier firms** | Big Four (Deloitte, PwC, EY, KPMG) | Largest clients, set methodology trends |
| **Mid-tier** | BDO, RSM, Grant Thornton, Mazars, Azets, Evelyn | National coverage, mid-market; >80% have done acquisitions |
| **SMB practices** | ~63,000 UK firms | The long tail — sole practitioners to ~50-partner regional firms |
| **Niche specialists** | Property, e-commerce, contractors, dental, charities, R&D boutiques | Vertical depth, often productised |
| **Software vendors** | Xero, QuickBooks, Sage, FreeAgent (ledgers); IRIS, TaxCalc, BTC, CCH, Capium (compliance); Karbon, TaxDome, BrightManager (PM); Dext, Hubdoc (capture); Ignition, GoProposal (engagement) | Operating system of the modern practice |
| **Outsourcers / offshore** | India, Philippines, South Africa providers | Prep-and-review labour arbitrage |
| **Capital** | PE houses (HgCapital, Inflexion, Apiary, Tenzing); banks; specialist practice-finance lenders | Roll-up M&A; working-capital lending |
| **Adjacent advisers** | IFAs, solicitors, brokers, R&D specialists, insolvency practitioners | Referral network — both source and competition |
| **Client banks & fintech** | High-street banks, Stripe, GoCardless, Wise, open-banking aggregators | Source feeds for bookkeeping; payment rails for fees |
| **Industry media** | AccountingWEB, Accountancy Age, Accountex, ICAEW/ACCA conferences | Where best practice diffuses |

### 2.3 Historical Evolution — seven eras

1. **Late 1800s** — Profession formalised. ICAEW founded 1880. Audit emerges to serve the joint-stock company.
2. **20th-century build-out** — Audit + tax becomes the twin pillars. Big Eight emerges via mergers.
3. **1980s consolidation** — Big Eight → Big Six → Big Five. Tax advisory grows into a profit centre alongside audit.
4. **2002 — Andersen collapse** — Enron/WorldCom triggers Andersen's dissolution. Big Four cemented. Sarbanes-Oxley follows in the US; FRC strengthens in UK.
5. **2000s–2010s — Cloud era begins** — Sage's desktop dominance challenged by Xero (NZ, 2006) and QuickBooks Online. Bookkeeping shifts from quarterly drop-off to real-time cloud feeds.
6. **2015–2020 — MTD & practice management** — HMRC mandates digital VAT records (MTD VAT, 2019). Practice-management category emerges. Subscription pricing displaces hourly.
7. **2022–present — PE roll-ups & AI** — Azets, Evelyn, Cooper Parry, Dains, Xeinadin and others build national platforms via PE-funded acquisitions. ~20 of UK Top 60 are now PE-backed. Generative AI moves from pilot (2024) to embedded workflow (2026). MTD for Income Tax (April 2026) forces final cloud migration.

### 2.4 Macro Forces

| Force | Direction | Effect |
|---|---|---|
| **Tax complexity** | ↑ | More work, higher fees, more risk; rewards specialists |
| **Digitisation (MTD, e-invoicing)** | ↑ | Forces cloud adoption; ~5× submission volume by 2028 |
| **Generative AI** | ↑↑ | Compresses prep time; threatens hourly billing; rewards advisory + niche |
| **Talent shortage** | ↑ | Wage inflation, offshoring, apprenticeship push (73% of UK firms cite as top concern) |
| **PE consolidation** | ↑ | Roll-ups raise valuations short-term, pressure cultures, set 3–5yr exit horizons |
| **Regulatory expansion** | ↑ | New tax-adviser registration regime (May 2026); enhanced HMRC powers; AML scrutiny |
| **Client digital expectation** | ↑ | Portal, e-sign, mobile capture become table stakes |
| **Fee pressure on commodity work** | ↑ | Bookkeeping/compliance margins compress; advisory becomes the margin story |
| **Demographic shift in partners** | ↑ | Wave of partner retirements drives M&A supply |

### 2.5 Adjacent Comparables

The practice is structurally cousin to:
- **Law firms** — same partnership/LLP shape, same lock-up problem, same WIP/realisation KPIs, similar regulatory layering.
- **Architecture & engineering consultancies** — project-based with deadlines, fee-earner leverage, PI insurance.
- **Wealth/IFA firms** — recurring-fee annuities, ageing principal demographic, current PE roll-up wave.

Differences:
- **vs Law:** higher cadence (monthly/quarterly compliance vs matter-based), more software-mediated, more standardised outputs.
- **vs IFA:** less product-distribution baggage, more deadline-driven, harder regulatory perimeter.
- **vs Management consulting:** dramatically more recurring revenue and lower CAC — but lower per-engagement fees.

### 2.6 Three Frames for Reading a Practice

1. **Service-line lens** — what the firm sells.
2. **Operations lens** — how work flows: *Client → Engagement → Job → Task → Deadline → Document → Query → Bill*.
3. **Economic lens** — *Recurring fee + advisory upside − people − software − partner draw = profit*; lock-up, utilisation, realisation as the operating dials.

Most "modernisation" conversations miss each other because participants are silently using different lenses.

---

## 3. Firm Mechanics

### 3.1 Firm structures & ownership

| Form | Typical use | Notes |
|---|---|---|
| **Sole practitioner** | Smallest firms | Practising certificate from ICAEW / ACCA / etc. required |
| **Traditional partnership** | Long-standing small/mid firms | Unlimited liability — increasingly rare |
| **LLP** | Dominant form for mid-tier and Top 100 | Separate legal entity, limited liability, members taxed as partners |
| **Limited company** | Newer/smaller practices | For "Chartered Accountants" branding ≥50% of directors must be chartered |

### 3.2 Regulation & supervisory bodies

A practice is regulated on multiple, overlapping axes:

- **Professional body** — ICAEW, ACCA, CIOT, AAT, ICAS, CIMA. Practising certificates, CPD, ethics, file reviews.
- **AML supervision** — Required by law. Either the professional body or HMRC supervises.
- **Audit registration** — Separate, more onerous (RSB-registered).
- **Insolvency, probate, investment business** — Each has its own licence.
- **ICO / GDPR** — Data protection registration.
- **PII** — Mandatory professional indemnity insurance, scaled to fee income.

### 3.3 Client segmentation

| Segment | Typical entity | Typical needs |
|---|---|---|
| **Micro / freelancer** | Sole trader, single-director Ltd | Self Assessment, light bookkeeping, basic year-end |
| **Owner-managed Ltd (OMB)** | 1–10 staff | Year-end, CT, payroll, VAT, dividend planning |
| **Growing SMB** | 10–50 staff | Management accounts, cashflow, finance raising, tax planning |
| **Established SME** | 50–250 staff | Audit (if thresholds met), group structures, R&D, share schemes, virtual CFO |
| **Specialist niches** | Construction, property, e-commerce, healthcare, charities, contractors | CIS, ATED, multi-channel VAT, fund accounting, SORP |

Most firms find unit economics work best when they pick 2–3 niches.

### 3.4 Pricing & revenue models

- **Hourly billing** — Legacy, still used for irregular/advisory; under pressure as AI compresses time.
- **Fixed fee per service** — Menu pricing for compliance.
- **Monthly subscription / bundles** — Dominant new-client model: bookkeeping + VAT + payroll + year-end + SA bundled into a tier.
- **Value-based pricing** — Reserved for advisory.
- **Hybrid** — Most common: subscription floor + value-priced advisory + change orders for scope creep.

Tools like **Ignition** and **GoProposal** institutionalise proposal-and-engagement automation around this.

### 3.5 Staffing pyramid

```
Partner / Director  ──── client relationships, advisory, signing
Manager            ──── job ownership, review, planning
Senior / Qualified ──── preparation, complex compliance, queries
Semi-senior        ──── routine accounts, returns
Junior / Trainee   ──── bookkeeping, data entry, postings
Bookkeepers        ──── ledger work
Admin / Practice mgr ── workflow, billing, onboarding
```

Many firms also use offshore/outsourced teams (India/Philippines/SA) for prep work with UK-side review.

### 3.6 Practice KPIs

ICAEW's headline practice KPIs are profitability, net gains, productivity, recovery, and lock-up.

| KPI | Definition | Healthy benchmark |
|---|---|---|
| **Lock-up days** | WIP days + Debtor days | < 45 best-in-class; 60–90 average |
| **Utilisation** | Chargeable ÷ available hours | 65–80% for fee earners |
| **Realisation / recovery** | Billed ÷ standard rate × hours | > 90% strong |
| **GP per partner / per FTE** | Profit margin per fee-earner | Peer-firm benchmark |
| **Average revenue per client** | Total fees ÷ active clients | Rising = moving up-market |
| **Client retention** | % retained year-on-year | > 95% in healthy practices |
| **Service mix** | % compliance vs % advisory | Advisory share is the strategic dial |

### 3.7 Tech stack

Layered, not single-product:

- **Cloud ledgers (client-side):** Xero, QuickBooks Online, Sage, FreeAgent
- **Pre-accounting / capture:** Dext, Hubdoc, AutoEntry
- **Tax compliance:** IRIS, TaxCalc, BTCSoftware, Digita, CCH, Capium
- **Practice management / workflow:** Karbon, TaxDome, BrightManager, Senta, AccountancyManager, Canopy
- **Proposal / engagement / billing:** Ignition, GoProposal
- **Document portal & e-sign:** TaxDome, Suralink, native PM
- **AML / KYC:** First AML, SmartSearch, VinciWorks
- **Payroll:** BrightPay, Moneysoft, Sage Payroll, Xero Payroll
- **Reporting / advisory:** Fathom, Spotlight, Syft, Float

Strategic story of recent years: practice management eating the centre.

### 3.8 Industry trends 2025–26

- **MTD for Income Tax (April 2026)** — Sole traders & landlords with income > £50k must keep digital records and file quarterly.
- **Generative AI** — 2025 experimentation → 2026 integration into prep, review, query handling, advisory.
- **Consolidation & PE roll-ups** — 44% of Top 100 in M&A; ~20 of Top 60 PE-backed; "frenzy" cooling as buyers scrutinise client-base quality.
- **Advisory shift** — 60% of firms offer advisory; 41% plan to expand.
- **Talent crunch** — Tight pipeline; firms invest in offshoring, AI, apprenticeships.
- **Niche specialisation** — Generalist high-street firms losing share to vertical specialists.

### 3.9 Economics — how a small firm makes money

- **Recurring-fee base** (bookkeeping + VAT + payroll + year-end + SA) is the **annuity** — predictable, valued by acquirers.
- **One-off advisory and tax-planning fees** are the **upside** — high margin, lumpy.
- **Cost base** is overwhelmingly people + software + premises. Salaries 45–55% of fee income.
- **Profit per partner** is the headline. Levers: leverage (juniors per partner), pricing, niche, productisation, tech.
- **Lock-up** is the silent killer of cashflow.

---

## 4. Drill-Down Framework

Each of the 22 functions below is documented against a fixed 10-section template:

```
1.  Purpose             — what this function exists to do
2.  Trigger & cadence   — what kicks it off; recurring vs event-driven
3.  Inputs              — data, documents, source systems
4.  Process steps       — the actual sequence of work
5.  Outputs & filings   — deliverables, statutory submissions
6.  Roles & handoffs    — who in the firm does what
7.  Deadlines, risks & penalties — regulatory clock, common failure modes
8.  Tools & software    — typical UK tech stack
9.  Economics           — pricing, time budget, margin profile
10. Client experience   — what the SMB client actually sees
```

---

## 5. Client-Facing Service Lines

### A1 — Bookkeeping / Client Accounting Services (CAS)

The recurring fee engine. Modern bookkeeping for an SMB is a **continuous data pipeline** running between the client's bank, invoicing tools, and payroll into a cloud ledger that the practice maintains. Everything downstream (VAT, management accounts, year-end, tax) inherits the quality of this layer.

**1. Purpose** — Maintain a complete, accurate, MTD-compliant general ledger for the client so tax obligations are met, decisions can be made on real numbers, and year-end becomes a checkpoint rather than a reconstruction project.

**2. Trigger & cadence**
- Continuous at the data layer (bank feeds, receipt capture push transactions in real time)
- Weekly review for high-volume / high-risk clients (e-commerce, hospitality, construction)
- Monthly close is the standard pulse for SMBs
- Quarterly is the slower end (sub-VAT-threshold) — but post-MTD-IT (April 2026) this becomes minimum viable for sole traders / landlords > £50k

**3. Inputs**
- Bank, credit-card, PayPal/Stripe feeds (Open Banking direct)
- Sales invoices (Xero/QBO or Shopify/Amazon/Etsy via A2X, Link My Books, Dext Commerce)
- Purchase invoices and receipts (Dext / Hubdoc / native QBO capture)
- Payroll journal (BrightPay / Xero Payroll / Moneysoft)
- Expense claims (Pleo, Soldo, Xero Expenses)
- Loan, lease, director-loan statements
- Stock / inventory data
- Prior month's locked trial balance

**4. Process steps**
```
DAILY / WEEKLY
1. Bank feed sync — auto-import all transactions
2. Receipt capture — AI extracts supplier, date, net, VAT, total
3. Auto-categorisation — bank rules + AI suggest nominal codes
4. Match purchases to receipts; sales to lodgements
5. Flag exceptions / queries

MONTHLY CLOSE
6. Bank reconciliation to statement balance
7. Credit-card / merchant reconciliation
8. Wages journal posted from payroll software
9. VAT control account check
10. Accruals & prepayments posted
11. Fixed-asset additions and depreciation run
12. Inter-company / director-loan check
13. Trial balance review by senior
14. Manager / partner review for unusual items
15. Lock the period; produce monthly pack
16. Send query list to client
```

The "**3-way handshake**" is the heart of the discipline: every transaction has a *bank line*, a *document* (invoice/receipt), and a *ledger entry* — and they must agree.

**5. Outputs & filings**
- Reconciled trial balance, P&L, balance sheet, cash flow
- Aged debtors and creditors
- VAT-ready transaction set (used by A3)
- Management accounts pack (used by A2)
- Bank reconciliation reports (audit trail)
- Open query list

No statutory filings come directly from bookkeeping — but everything downstream depends on it. Under MTD, the **digital-link rule** means data must flow electronically from source → ledger → return without manual rekeying.

**6. Roles & handoffs**
- **Bookkeeper / Junior** — daily data capture, categorisation, reconciliation, queries
- **Senior bookkeeper / Semi-senior** — month-end close, journals, accruals, review
- **Client manager** — reviews monthly pack before client sees it
- **Client** — owns the document supply chain
- **Partner** — spot-checks risk clients

Increasingly common pattern: **offshored prep + onshore review** — saves 50–70% on labour cost; demands tight documentation and PM software.

**7. Deadlines, risks & penalties**
- No fixed external deadline — but downstream deadlines (VAT, payroll RTI, year-end) cascade
- MTD digital-link rule: manual copy-paste between systems is a breach
- Common failure modes: bank feed breaks (90-day re-auth), capital vs revenue mis-categorisation, wrong VAT codes, director's loan account drift (S455), duplicate purchases
- Reputational risk: bookkeeping is what the client *sees* every month

**8. Tools & software**
| Layer | Examples |
|---|---|
| Cloud ledger | Xero, QuickBooks Online, Sage Business Cloud, FreeAgent |
| Receipt capture | Dext, Hubdoc (free with Xero), AutoEntry |
| E-commerce ingestion | A2X, Link My Books, Dext Commerce, Synder |
| Bank feeds | Open Banking via the ledger; Yodlee/Plaid behind the scenes |
| Expense management | Pleo, Soldo, Xero Expenses |
| AI bookkeeping | Truewind, Booke AI, Keeper.app, Dext's ledger-AI |
| Workflow | Karbon, TaxDome, Senta, BrightManager |
| Reporting | Fathom, Spotlight, Syft |

**9. Economics**
- Pricing: £100–£500/month per SMB depending on volume/complexity. Construction/e-commerce/hospitality at the high end. Often bundled with VAT and year-end.
- Time budget: ~30 min – 4 hours per client per month
- Margin: mid-range (40–60%) — historically high but compressing as automation eats it
- Automation pressure: most exposed area to AI in the firm
- Strategic dial: bookkeeping is the **primary feeder for advisory** — without clean monthly numbers, no virtual-CFO conversation is possible

**10. Client experience**
- Cloud ledger with bank feeds connected
- Snap a photo of every receipt; throw it into the Dext/Xero app
- Forward supplier emails to a magic inbox
- Each month, get a management pack
- See a short query list in the portal
- Know the books are *always done* — no January panic

Bad: client still posting invoices themselves, 3-month backlog, the practice asks for documents twice, year-end accounts contain "find me" balances. This is where **client churn originates** — bookkeeping is the touchpoint they feel.

---

### A2 — Management Accounts

The bridge between **compliance** and **advisory**. Statutory accounts speak to HMRC and Companies House; management accounts speak to the owner-manager. The most-read document the firm produces.

**1. Purpose** — Translate the cleaned monthly ledger into a concise pack of P&L, balance sheet, cash flow, KPIs, and commentary so an owner-manager can see how the business is performing versus prior period and budget, and identify the next 30-day decisions.

**2. Trigger & cadence**
- Monthly is the standard for clients on bookkeeping subscriptions
- Quarterly for smaller / lower-complexity clients
- Weekly cash dashboard for cash-stressed or rapidly growing clients
- Triggered by close of A1 — usually within 5–10 working days of month-end

**3. Inputs**
- Locked trial balance (from A1)
- Budget / forecast for variance comparison
- Last year same-period figures
- Non-financial KPIs (units sold, headcount, sales pipeline, project hours, occupancy)
- Operational commentary from the client
- Cash-flow position and 13-week forecast

**4. Process steps**
```
1. Confirm A1 close complete
2. Run accruals/prepayments/depreciation review
3. Build P&L variance: actual vs budget vs prior year
4. Build balance sheet movement & 13-week cash forecast
5. Calculate KPIs (gross margin %, debtor/creditor days,
   payroll %, cash runway, MRR, utilisation)
6. Draft 1-page commentary: 3 wins, 3 risks, 3 actions
7. Manager / partner review
8. Issue pack via portal / Fathom / Spotlight; book monthly review call
9. Run the call; capture decisions; log advisory follow-ups
10. Update rolling forecast if material changes
```

**5. Outputs & filings**
- Monthly management pack (P&L, BS, cash flow, KPIs, commentary)
- Cash forecast (13-week rolling for stressed clients)
- No external filings
- Decision log / action list from the review call

**6. Roles & handoffs**
- **Senior bookkeeper / Manager** — builds the pack
- **Client manager / Partner** — writes commentary, runs the review call
- **Client (MD/FD)** — consumes, decides, owns the actions

**7. Deadlines, risks & penalties**
- No statutory deadline; internal SLA (e.g. "by 10th working day")
- Risks: late pack = no decision; commentary that just describes the numbers (clients want **why** and **so what**); unreliable underlying data; numbers materially wrong if used externally

**8. Tools & software**
- **Reporting:** Fathom (UK favourite), Spotlight Reporting, Syft, Futrli, Float
- **Native ledger reports:** Xero/QBO/Sage built-ins
- **Forecasting:** Float, Fathom, Spotlight, Brixx
- **Multi-entity consolidation:** Fathom, Joiin
- **AI commentary:** Fathom Commentary Writer, Syft AI assistants

**9. Economics**
- Pricing: £250–£1,500/month depending on cadence, depth, review call inclusion
- Time budget: 2–8 hrs/month routine; up to a day for board-style pack
- Margin: strong (50–70%) — captures advisory value off the bookkeeping data
- Strategic dial: most natural upsell from bookkeeping; launchpad for VCFO retainers

**10. Client experience**
The owner gets a 5-page PDF (or live dashboard) on the 8th of each month. Page 1 says *"You made £42k profit (vs £31k forecast). Cash is £180k, runway 14 weeks. Top 3 actions this month."* They get a 30-minute call. They feel **informed**. They start asking advisory questions. The bad version: a 28-page accountancy-jargon report with no commentary, sent on the 25th, that the client never opens.

---

### A3 — VAT / Indirect Tax

The **highest-cadence, highest-penalty** compliance the practice handles. Quarterly clockwork, MTD-mandated, with a points-based penalty regime that punishes lateness.

**1. Purpose** — Ensure every VAT-registered client correctly identifies output VAT charged on sales, recovers input VAT on legitimate purchases, applies the right scheme and rates, and submits an MTD-compliant return on time.

**2. Trigger & cadence**
- Quarterly for the vast majority of SMBs
- Monthly for repayment traders (zero-rated exporters)
- Annual for clients on the Annual Accounting Scheme
- Registration when turnover crosses **£90,000** (rolling 12 months); deregistration below **£88,000**
- Stagger groups (1 = Mar/Jun/Sep/Dec ends; 2 = Apr/Jul/Oct/Jan; 3 = May/Aug/Nov/Feb)
- Submission by **1 month + 7 days** after period end

**3. Inputs**
- Bookkeeping for the period (depends entirely on A1 being clean)
- Sales invoices with correct VAT rates (20%, 5%, 0%, exempt, outside scope)
- Purchase invoices with valid VAT numbers and tax points
- Bank reconciliation completed
- Last quarter's return + outstanding adjustments
- Records of imports/exports, EU/NI trade, reverse charge, partial exemption, fuel scale charges, capital goods scheme entries
- VAT scheme parameters

**4. Process steps**
```
1. Period close in the ledger (A1 must be done)
2. Run VAT return draft from Xero / QBO / MTD bridging tool
3. Reasonableness review (Box 1 vs sales × expected rate; Box 4 vs purchases; variance vs prior; flag exceptions)
4. Specific checks:
   - CIS reverse charge correctly applied
   - Margin scheme calc
   - Partial exemption recovery rate
   - Fuel scale charge entries
   - Bad debt relief (>6 months old)
   - Director / staff entertainment disallowed
5. Manager / partner review
6. Send draft to client with explanation
7. Client approves
8. Submit via MTD-compatible software
9. Confirm acceptance receipt from HMRC
10. Update payment status; remind client of due date
11. File working papers and approval audit trail
```

**5. Outputs & filings**
- HMRC submission (the 9 boxes via MTD API)
- Confirmation receipt stored against the period
- Working papers (return reconciliation to ledger, scheme calcs, partial-exemption working)
- Client communication
- Internal audit trail

**Digital-link rule** (in force since April 2021): data must travel from source records to the return without manual rekeying.

**6. Roles & handoffs**
- **Bookkeeper / Junior** — runs the draft return
- **Senior / Manager** — reviews, runs reasonableness checks, applies scheme nuances
- **Partner** — complex returns (partial exemption, margin, group VAT)
- **Client** — signs off before submission (legally responsible)
- **VAT specialist** — sectoral schemes, HMRC enquiries, group registrations

80% routine (30 min – 2 hrs); 20% complex (can take a day for partial exemption or margin schemes).

**7. Deadlines, risks & penalties**
- Submission: 1 month + 7 days after VAT period end
- Payment: same date — DD payers get 3 extra working days
- **Late submission penalties (points-based, since Jan 2023):**
  - Quarterly filers: 4 points → £200 fine
  - Monthly filers: 5 points
  - Annual filers: 2 points
  - Each subsequent late submission = another £200 until 24 months clean
- **Late payment penalties:**
  - Days 1–15 late: no penalty if paid or TTP arranged
  - Days 16–30: 3% of unpaid VAT
  - Day 31+: further 3% + 10% per annum daily rate
- Common errors: wrong rate (5% vs 20%), reclaiming VAT on entertainment / fuel without scale charge / exempt supplies, missed reverse charge on overseas digital services / CIS, wrong postponed VAT accounting, pre-registration input VAT outside limits

**8. Tools & software**
- **Cloud ledgers (built-in MTD):** Xero, QBO, Sage, FreeAgent
- **Bridging software:** VitalTax, 123 Sheets, Tax Optimiser, Avalara
- **Specialist:** Avalara, Sovos, Vertex (multi-country / e-commerce)
- **HMRC interface:** MTD VAT API
- **Workflow:** Karbon, TaxDome, Senta — recurring VAT jobs auto-scheduled per stagger
- **CIS reverse charge:** built into Xero/QBO; Clear Books for construction

**9. Economics**
- Pricing: £75–£250 per return for routine SMB; bundled into monthly subscription. Complex returns (partial exemption, margin schemes, multi-country) £300–£1,500
- Time budget: 30 min – 2 hours routine; 4–8 hours complex
- Margin: healthy on routine (60–70%); thin on complex unless specialist-priced
- Automation pressure: ~95% automated for clean books — value has shifted from preparation to review and exception handling
- Strategic dial: VAT is the **clock that disciplines the rest of the practice**

**10. Client experience**
- Each quarter: "Your VAT return for Q3 is ready — £4,237 due on 7 November"
- Click a link to review with one-line plain-English summary
- Click approve
- DD goes out automatically on the 10th
- They never log in to HMRC

Bad: panicked email three days before deadline asking for missing receipts; surprise £8,000 bill they didn't budget for; confusion about whether they're on flat rate or standard; penalty point notice in the post; year three a compliance check letter.

---

### A4 — Payroll & Employment Taxes

The most **deadline-dense, error-visible** function in the practice. Every payday is a real-time submission to HMRC. Touches **two regulators** (HMRC + The Pensions Regulator) and **three counterparties** (HMRC, pension provider, the employee).

**1. Purpose** — Calculate and pay each employee correctly, deduct and remit the right PAYE, NI, student-loan, and pension amounts to the right bodies on the right dates, and produce the statutory documentation.

**2. Trigger & cadence**
- Every pay run (typically monthly; some weekly, fortnightly, four-weekly)
- **FPS** — Full Payment Submission, **on or before each payday**
- **EPS** — by 19th of following tax month (only when something to report)
- **PAYE/NIC payment to HMRC** — by 22nd electronic / 19th cheque of following tax month
- **Pension contributions** — within 22 days of tax-month-end
- **Annual:** final FPS, P60s by 31 May, P11D / P11D(b) by 6 July, Class 1A NIC by 22 July
- **Triennial:** auto-enrolment re-enrolment cycle every 3 years
- Event-driven: starters, leavers, statutory pay (SSP/SMP/SPP), tax code changes, AOEs, salary sacrifice, benefit changes

A practice running 80 client payrolls processes ~900 monthly events plus ~80 year-end packages.

**3. Inputs**
- Master data: employees, NI numbers, addresses, DOBs, tax codes, NI categories, pay rates, hours
- Pay-period changes from client: hours, overtime, bonuses, leavers, new starters (P45 or starter checklist)
- Statutory absence info
- Pension scheme details (provider, scheme reference, contribution %s, salary sacrifice rules)
- Benefits-in-kind data (cars, medical, fuel, loans, accommodation)
- HMRC tax code notices (P6, P9 — pulled via DPS)
- Student loan / postgraduate loan notifications
- Attachment of Earnings Orders, court orders, CSA deductions
- Year-end: salary sacrifice arrangements, P11D scope, dividend timing for director payrolls

**4. Process steps**
```
EACH PAY PERIOD
1. Pull DPS notices (HMRC tax-code updates) into payroll software
2. Send pay-period data request to client
3. Process input: starters, leavers, hours, bonuses, absences
4. Update pension assessment (auto-enrolment status)
5. Run gross-to-net calculation:
   - PAYE (using tax code, allowances)
   - Employee NI (12% / 2% above UEL)
   - Employer NI (15% above £5,000 secondary threshold for 2026/27)
   - Student loan deductions
   - Pension contributions
   - Statutory pay
   - Other deductions (AOEs, salary sacrifice)
6. Run pre-submission review (variance vs prior month, exceptions)
7. Send draft payroll summary to client for approval
8. Client approves → generate:
   - Payslips (PDF / portal)
   - Bacs / FPS payment file
   - Journal for the bookkeeping ledger
9. Submit FPS on or before payday
10. Submit EPS if required
11. Notify pension provider
12. Confirm to client: total to pay employees, total PAYE/NIC due, due dates

ANNUAL CYCLE
13. Final FPS for tax year
14. Issue P60s by 31 May
15. Collate benefits data; prepare P11Ds and P11D(b)
16. Submit P11Ds by 6 July; pay Class 1A NIC by 22 July
17. Roll over to new tax year
18. Triennial: process re-enrolment of opted-out staff
```

**5. Outputs & filings**
- Payslips, FPS, EPS, payroll journal (to A1), payment file (Bacs/Faster Payments), pension submission, P60, P45, P11D / P11D(b), PSA computations

**6. Roles & handoffs**
- **Payroll administrator** — runs the cycle, processes inputs, generates draft
- **Senior payroll / Manager** — review, complex cases, statutory pay, salary sacrifice, P11Ds
- **Pensions specialist** — auto-enrolment, opt-ins/outs, re-enrolment, TPR Declaration of Compliance
- **Client (employer)** — provides data on time, approves run, makes payments
- **Bureau manager / Partner** — owns SLA, escalations

Many practices run a **separate payroll bureau** as a logical sub-entity (different skill set, deadlines, PI treatment).

**7. Deadlines, risks & penalties**
- Late FPS: £100 (1–9 employees) up to £400 (250+) per failure; one "free" miss per tax year
- Late PAYE/NIC payment: 1% → 4% percentage penalty depending on number of defaults; interest from due date
- Late P11D: £100 per 50 employees per month
- Late P60 to employees: £300 per failure + £60/day
- Auto-enrolment failures (TPR): Fixed Penalty £400 + Escalating Penalty up to £10,000/day
- Common errors: wrong tax code (no penalty but loses trust); NI category errors (multi-year corrections); director payrolls (monthly when client wanted annual; or wrong NI letter); salary sacrifice gone wrong (NMW breaches); off-payroll working (IR35); client sends data late via WhatsApp; DPS not pulled before run; new starter without P45

**8. Tools & software**
- **Bureau:** BrightPay (dominant in UK practices), Moneysoft Payroll Manager, IRIS Payroll Professional, Sage 50 Payroll
- **Embedded ledger:** Xero Payroll, QuickBooks Payroll
- **Pension providers (API/upload):** NEST, The People's Pension, Smart Pension, Aviva, Now: Pensions, Royal London
- **HR / time integration:** Breathe HR, Personio, Deputy, Planday
- **HMRC interface:** RTI gateway (FPS/EPS/EYU), DPS
- **Workflow:** Karbon, BrightManager, TaxDome
- **Employee portal:** BrightPay Connect, MyEPay

**9. Economics**
- Pricing: typically **£3–£6 per payslip per month**, small monthly base fee per scheme (£10–£25). Director-only payrolls flat £150–£300/yr. Year-end (P60s, P11Ds) £15–£40 per employee
- Time budget: 15–45 min per scheme per pay run routine
- Margin: **the thinnest service line** in most firms (15–35%) — run at near-cost to retain wider relationship
- Strategic dial: payroll as a **stickiness moat** rather than profit centre; PE roll-ups specifically buy firms to add payroll volume to a central bureau

**10. Client experience**
Good: monthly templated email; fill in hours/bonuses/leavers in 5 minutes; draft summary morning of payroll day; one-click approve; payslips appear in employee app; Bacs file ready; final summary with all payment dates; year-end P60s drop into employee inboxes automatically.

Bad: client emails hours late on payroll evening; two new starters they hadn't mentioned; payslips the morning *after* payday; new tax code retrospectively; TPR letter about missed pension contributions. Most common reason clients leave — "they messed up my staff's pay and we lost trust."

---

### A5 — Year-End Statutory Accounts

The annual checkpoint: the legal record of the company's financial position, filed publicly at Companies House and forming the basis of the corporation tax return. For most SMBs this means **FRS 105 (micro-entity)** or **FRS 102 Section 1A (small)**.

**1. Purpose** — Prepare a true and fair set of statutory financial statements under the appropriate UK GAAP framework, get them approved and signed by the directors, file at Companies House and HMRC (with the CT return), and use the work as the foundation for tax-planning conversations.

**2. Trigger & cadence**
- Annual, triggered by company's accounting reference date (ARD)
- Filing deadline: **9 months after ARD** for private companies (6 months for public)
- Practice typically schedules accounts work **2–4 months before deadline**

**3. Inputs**
- Locked year-end trial balance from A1
- Bank, loan, credit-card statements at year-end (3rd-party verification)
- Stock count results
- Fixed asset register + capital additions
- Director's loan account reconciliation
- Dividend minutes / vouchers
- Salary sacrifice / benefit data
- Lease agreements
- Prior-year accounts and tax computation
- Sundry: accruals, prepayments, deferred income, provisions

**4. Process steps**
```
1. Job opens; check engagement letter scope, framework
   (FRS 105/102), audit applicability
2. Trigger client request list (year-end pack — bank confs,
   stock, debtors review, accruals)
3. Receive data; pre-clearance review
4. Roll forward last year's working papers
5. Lead schedules per balance sheet area:
   - Cash, debtors, creditors, fixed assets, stock,
     loans, share capital, reserves
6. P&L analytical review — variance vs prior year, GP%, expense ratios
7. Disclosures review (FRS 102 1A: directors' transactions, related parties)
8. Tax computation drafted in parallel (feeds A6)
9. Draft accounts produced (iXBRL tagged)
10. Manager review / partner sign-off
11. Send to client with explanation pack and tax summary
12. Director approves accounts and signs minutes
13. File at Companies House (online); file CT600 + accounts with HMRC
14. Archive working papers; trigger next year's recurring job
```

**5. Outputs & filings**
- Statutory accounts (FRS 105 micro / FRS 102 1A small / full FRS 102) — directors' report, P&L, BS, notes, signed
- Companies House filing (iXBRL) — abridged or filleted versions for small/micro
- HMRC filing — full accounts + CT600 + tax computation, all iXBRL-tagged
- Internal: working papers, lead schedules, approval audit trail
- Client: signed PDF set, tax summary, planning points

From **1 Jan 2026**, FRS 102 disclosure requirements expand (revenue recognition, leases on balance sheet for lessees, supplier finance).

**6. Roles & handoffs**
- **Junior / semi-senior** — prepares the file, lead schedules, drafts accounts
- **Manager** — reviews, queries, finalises disclosures
- **Partner / Director** — signs off the accounts
- **Director (client)** — approves and signs accounts and minutes
- **Audit team** — only if the client crosses thresholds (see A9)

**7. Deadlines, risks & penalties**
- **Companies House late filing:** £150 (≤1 month) → £375 → £750 → **£1,500** (>6 months); doubled if late two consecutive years
- **HMRC late accounts/CT600:** £100 / £200 + tax-geared penalties (10–20%) once 6+ months late
- Wrong framework applied — restatement risk
- iXBRL tagging errors — HMRC rejection
- Director's loan account overdrawn at year-end — **S455 tax** (33.75%) on overdrawn balance
- Going concern judgement — director liability
- Disclosure errors under FRS 102 1A — particularly related-party transactions
- Stock valuation errors — material to profit and tax

**8. Tools & software**
- **Accounts production:** IRIS, CCH, TaxCalc, Capium, BTCSoftware, Sage Final Accounts, Xero Tax, Wolters Kluwer (CCH)
- **iXBRL tagging:** built into the above
- **Working papers:** AlphaTax, Caseware, IRIS — or custom Excel templates
- **Companies House interface:** Software Filing API
- **Workflow:** Karbon, BrightManager
- **Document management:** Virtual Cabinet, Suralink, native PM portals

**9. Economics**
- Pricing: £600–£3,000 small/micro Ltd; £3,000–£15,000+ medium-sized; bundled into monthly subscription
- Time budget: 8–25 hrs typical small Ltd; 50+ hrs medium with disclosures
- Margin: medium-strong (40–60%); compressing as automation eats prep time
- Automation pressure: **highest in the firm right now** — Xero Tax, Sage Final Accounts, IRIS Elements, Capium pushing toward 70–80% draft automation
- Strategic dial: year-end is the **annual tax-planning conversation**

**10. Client experience**
Good: practice asks for pre-defined list 2 months before deadline, comes back with draft accounts and a 1-page tax summary ("CT due £18k; here are 3 things we'd consider for next year"), director e-signs, filed.

Bad: scramble in month 8 with practice chasing data, CT bill that surprises by £20k, accounts filed at 11pm on the deadline.

---

### A6 — Corporation Tax

Each year-end's natural sibling. The **CT600** is filed alongside the statutory accounts (A5) but with its own deadlines, computation, and payment timetable. Since April 2023 the regime has had **two rates and a marginal band**.

**1. Purpose** — Compute each client company's taxable profit (adjusted from accounting profit), apply the right rate including marginal relief, claim available reliefs and allowances, file the CT600 at HMRC with iXBRL accounts and computations, and pay the right amount on time.

**2. Trigger & cadence**
- Annual, triggered by accounting period end
- **Payment due:** 9 months + 1 day after period end (*before* the return is due)
- **Filing due:** 12 months after period end
- **Quarterly instalments (QIPs):** for "large" companies (profits > £1.5m, divided by associated companies)
- Triggered with A5

**3. Inputs**
- Statutory accounts and trial balance (from A5)
- Prior-year CT600, computation, and tax pool balances
- Capital additions / disposals register (for AIA / WDA / FYA)
- R&D expenditure breakdown (if claiming, see A8)
- Loss memorandum (trading, capital, non-trade)
- Group structure (for associated companies, consortium, group relief)
- Director's loan account year-end position (S455)
- Dividend record (informational)
- Patent box election status
- Pension contribution timing

**4. Process steps**
```
1. Build the corporation tax computation:
   - Accounting profit
   - + Disallowables (entertainment, depreciation, fines)
   - − Capital allowances (AIA up to £1m, WDA, FYA, Full Expensing)
   - + Add-backs (S455 director's loan)
   - − Brought-forward losses
   - − R&D enhancement
   - = Taxable trading profits
2. Add other income (property, non-trade interest, chargeable gains)
3. Apply rate:
   - 19% if profits ≤ £50k
   - 25% if profits ≥ £250k
   - Marginal relief between (effective marginal rate 26.5%)
   - Limits divided by number of associated companies
4. Apply reliefs (R&D credit, group relief, CFC relief)
5. Generate iXBRL-tagged CT600 and computation
6. Manager / partner review
7. Send to client with summary
8. Director approves
9. File CT600 with HMRC; ensure payment instruction
10. File CT loss carry-back claim if relevant
```

**5. Outputs & filings**
- CT600 (via HMRC API)
- Tax computation (iXBRL)
- Statutory accounts (iXBRL) — same set as A5
- R&D claim form (CT600L + Additional Information Form, mandatory since Aug 2023)
- Payment instruction or DD setup
- Tax memo for client file

**6. Roles & handoffs**
- **Accounts preparer** — drafts the basic computation
- **Tax senior / Manager** — reviews, applies reliefs, checks marginal relief, group considerations
- **Tax partner / specialist** — R&D, group restructures, complex CGT items
- **Director (client)** — approves and pays

In larger SMB practices, A6 is owned by a **separate Tax team** distinct from the Accounts team.

**7. Deadlines, risks & penalties**
- Late filing penalties: £100 → £200 → 10% / 20% of tax due (6/12 months)
- Late payment: interest at HMRC official rate (~7%+), plus 5% / 10% / 15% surcharges at 30 / 6 / 12 months
- Wrong rate / missed associated companies: under-declaration; HMRC routinely enquires
- R&D claim risk: HMRC enquiry rate spiked; bad claims invite penalties up to 100% of disputed amount
- S455 (director's loan): 33.75% on overdrawn DLA balance not repaid within 9 months
- Marginal relief errors common when associated companies aren't properly identified
- Capital allowances errors (claiming AIA on cars, missing Full Expensing)
- Pension contributions only deductible when *paid*, not accrued

**8. Tools & software**
- **CT computation:** TaxCalc, IRIS Business Tax, CCH Corporation Tax, Capium Tax, BTC, AlphaTax
- **iXBRL tagging:** built-in
- **HMRC interface:** Corporation Tax online API
- **R&D claim software:** WhisperClaims, Empirical, ForrestBrown's tools
- **Workflow:** Karbon, BrightManager
- **Modelling:** Excel still dominates for tax-planning scenarios

**9. Economics**
- Pricing: £400–£2,000 routine SMB CT600; bundled in monthly fee. R&D claims often **contingent-fee at 15–25%** of the credit
- Time budget: 3–10 hrs routine; 20+ hrs complex with R&D/groups
- Margin: strong (50–65%) — core fee work
- Strategic dial: CT is the **biggest single tax exposure** for the typical SMB owner; tax-planning conversation off the back of CT is **where most advisory revenue originates**

**10. Client experience**
Good: one-pager — *"Profit £X. CT due £Y on Z date. Last year £W. Here's why it changed. Three things to consider next year."* Sign approval; pay by DD or BACS.

Bad: a five-figure surprise tax bill landing 9 months after they thought the year was closed, with the only action being "find £80k by next Tuesday."

---

### A7 — Personal Tax / Self Assessment

The most **clock-driven** service in the calendar — **31 January** dominates the year. From **April 2026**, **MTD for Income Tax** layers four extra quarterly submissions on top of the annual return for taxpayers with self-employment or property income over £50k.

**1. Purpose** — Prepare and file each individual client's annual UK Self Assessment (and from April 2026, MTD-IT quarterly updates plus a Final Declaration), claim available allowances and reliefs, calculate tax due (income tax, NIC, CGT, student loan, HICBC), and ensure timely payment.

**2. Trigger & cadence**

**Pre-MTD (current default):**
- Annual: SA100 / SA800 (partnership) / SA900 (trust)
- Paper deadline: 31 October following tax-year end
- Online deadline: 31 January following tax-year end
- Payment: 31 January (balancing) + 31 July (payment on account)

**Post-April 2026 (MTD-IT, qualifying income > £50k):**
- Quarterly updates: 7 Aug, 7 Nov, 7 Feb, 7 May
- Final Declaration: 31 January (replaces SA100 for in-scope taxpayers)
- Threshold drops to **£30k from April 2027**, **£20k from April 2028** (anticipated)

**Event-driven:**
- 60-day **CGT-on-UK-residential-property** return (since April 2020)
- Trust / estate tax returns
- Non-residency certificates, double-tax treaty claims

**3. Inputs**
- P60 / P45 / P11D from each employment
- Self-employment / partnership accounts and profit shares
- Rental income & expenses (property by property)
- Dividend vouchers, savings interest summaries
- Bank statements showing taxable interest
- Pension contribution certificates
- Gift Aid donations
- Capital gains data (sales, base costs, dates, allowable expenses)
- Foreign income / tax paid
- Student loan plan info
- HICBC info, marriage allowance status
- Last year's SA, payments on account paid

**4. Process steps**
```
1. Annual data request to client (templated checklist by client type)
2. Receive and log documents; chase missing items
3. Prepare draft computation:
   - Total income (employment, self-employment, property,
     dividends, savings, foreign, other)
   - Less personal allowance (tapered from £100k)
   - Less reliefs (pension, Gift Aid, EIS/SEIS, VCT)
   - Apply rates (basic 20%, higher 40%, additional 45%)
   - Add NIC (Class 2 and Class 4 for self-employed)
   - Add HICBC if applicable
   - Add CGT on disposals
4. Calculate balancing payment + payments on account
5. Send draft to client with one-page summary
6. Client approves
7. File SA100 via HMRC API
8. Send tax payment letter / DD reminder
9. Diary the July payment-on-account reminder

MTD-IT (from April 2026)
Quarterly:
1. Pull income/expense summary from Xero/QBO/MTD-IT software
2. Submit cumulative quarterly update to HMRC
3. Confirm acceptance
Annual:
4. Final Declaration combining all sources, reliefs, adjustments
5. Calculate balancing payment
```

**5. Outputs & filings**
- SA100 + supplementary pages (SA102, SA103, SA105, SA104, SA108, SA106)
- MTD-IT quarterly update + Final Declaration (post-April 2026)
- 60-day CGT return for UK residential property disposals
- Client tax summary, payment instruction + reminders for January and July

**6. Roles & handoffs**
- **Tax junior / semi-senior** — drafts the return
- **Tax senior / Manager** — reviews, complex items (CGT, foreign, EIS, residency, EBTs)
- **Partner** — signs off complex returns; client meetings on planning
- **Client** — approves, pays
- **Bookkeeper** — provides self-employment / property data feed (esp. under MTD-IT)

The **MTD-IT shift** structurally pulls bookkeeping into A7 — clients without a cloud ledger become operationally infeasible. Forces practices to **bundle bookkeeping into SA pricing** at the £50k+ tier.

**7. Deadlines, risks & penalties**
- Late filing: £100 immediate, then £10/day from 3 months (capped £900), then 5% / £300 (whichever greater) at 6 and 12 months
- Late payment: 5% surcharge at 30 days, 6 months, 12 months — plus interest
- MTD-IT late quarterly update: new points-based regime — 4 points = £200 fine
- CGT-on-property 60-day: £100 immediate; £10/day after 3 months
- HMRC enquiry windows: 12 months from filing; up to 4 (careless) / 20 (deliberate) years
- Common errors: missed dividends (shown gross); wrong basis period (the basis-period reform from 2024/25); foreign income missed (HMRC has CRS data); HICBC missed; CGT on residential property missed 60-day return; pension annual allowance breaches
- Reasonable excuse appeals — reliance on agent generally not accepted

**8. Tools & software**
- **SA software:** TaxCalc, IRIS Personal Tax, CCH, Capium, BTCSoftware, Andica, Digita
- **MTD-IT compatible:** Xero, QBO, FreeAgent, Sage, 123 Sheets, TaxCalc MTD, AccountsOS, Coconut
- **Specialist:** Andica (trusts), CCH (HNW), Hammock (landlords)
- **Workflow:** Karbon / BrightManager / TaxDome — manage the **January peak** (60–70% of annual SA workload in 4 weeks)

**9. Economics**
- Pricing: £150–£350 simple SA; £400–£900 sole trader/landlord; £1,200–£3,500 complex; £3,000+ HNWI
- MTD-IT pricing: firms repackaging at **£60–£150/month** for in-scope clients to cover bookkeeping + quarterly updates + final declaration
- Time budget: 1–8 hrs depending on complexity
- Margin: strong on bundled monthly model; thin on January-only one-off SA without bookkeeping
- Capacity: **the single biggest capacity pressure** in the firm
- Strategic dial: **MTD-IT is the largest structural disruption** to SA the profession has seen — kills the £200 January-only client model, rewards practices with year-round client engagement

**10. Client experience**
Good: an email each April asking for a year-pack; draft return in October ("you owe £4,200, here's why; here's how to reduce it next year"); approve by e-sign; payment reminder in January and July. MTD-IT version: data flowing automatically from cloud bookkeeping; quarterly check-in calls; Final Declaration is a 5-minute review.

Bad: "happy new year, please send your tax info" email on 5 January; chaotic document collection; £15k surprise bill the night before deadline; £100 fine because two receipts went missing.

---

### A8 — Specialist Compliance

Everything that doesn't fit into the four big columns (VAT, payroll, year-end, SA) but still has its own filings and clocks. **3–5 of these as everyday work** depending on client mix. Typically the **highest-margin compliance work** because the rules deter generalist competitors.

**1. Purpose** — Handle discrete regulatory regimes that apply to subsets of the SMB client base — CIS, R&D, EMI/share schemes, ATED, Patent Box, SDLT, Capital Goods Scheme, charity/SORP returns.

**2. Trigger & cadence**
| Regime | Trigger | Cadence |
|---|---|---|
| **CIS** | Construction client engaging subcontractors (or being one) | Monthly |
| **R&D tax relief** | Client doing qualifying innovation work | Annual (with CT) |
| **EMI / share schemes** | Client granting employee share options | Event + annual ERS return |
| **ATED** | Company holds UK residential property worth >£500k | Annual (1 April–30 April) |
| **Patent Box** | Patented IP generating profit | Annual (with CT) |
| **SDLT** | Property purchase | Event-driven (14 days) |
| **Capital Goods Scheme** | Land/buildings >£250k or computer equipment >£50k | 10-year monitoring |
| **Charity / SORP** | Charity client | Annual (Charity Commission) |

**3. Inputs (varies by regime)**
- **CIS:** subcontractor UTRs, verification status, payment & deduction records, materials breakdown
- **R&D:** project descriptions, technical advances, eligible costs (staff, subcontractors, software, consumables), competent professional sign-off
- **EMI:** option agreements, valuation, employee status, working-time declaration, scheme rules
- **ATED:** property valuation, ownership structure, exemption eligibility
- **SDLT:** completion statement, lease terms, residential vs commercial split, purchaser status

**4. Process steps (CIS as exemplar)**
```
CIS — Monthly cycle
1. Receive payment data from client
2. Verify each subcontractor with HMRC (gross / 20% / 30%)
3. Calculate deduction (labour element only, exclude materials & VAT)
4. Issue payment & deduction statements to subcontractors by 19th
5. File CIS300 monthly return by 19th
6. Reconcile CIS suffered into PAYE EPS for offset
7. Manage gross-payment status applications and reviews

R&D — Annual cycle
1. Pre-submission notification (mandatory for first-time/lapsed claimants)
2. Identify qualifying projects with client's competent professional
3. Quantify costs (staff, EPWs, subcontractors, software, consumables)
4. Apply merged-scheme rules (April 2024+):
   - 20% above-line credit (RDEC-style)
   - Or 14.5% enhanced credit if R&D-intensive (≥30% of total spend)
5. Prepare technical narrative + financial calc
6. File CT600L + Additional Information Form + claim notification
7. Manage HMRC enquiry if it arises
```

**5. Outputs & filings (selection)**
- **CIS300** monthly return + CIS Payment & Deduction Statements
- **CT600L** R&D claim form + Additional Information Form (mandatory)
- **R&D claim notification** (within 6 months of period end for new claimants)
- **EMI grant notification** (within 92 days of grant)
- **ERS annual return** (Employment Related Securities — by 6 July)
- **ATED return + ATED-related CGT** (annual, by 30 April)
- **SDLT1** (within 14 days of completion)
- **Charity Commission Annual Return + accounts** (within 10 months of year end)

**6. Roles & handoffs**
Each specialism is typically owned by a **specialist within the practice**. R&D and EMI work often involve **boutique sub-contractors** (ForrestBrown, Source Advisors, Leyton, Vialto for EMI). CIS is usually handled by the payroll team alongside A4.

**7. Deadlines, risks & penalties**
- CIS300 late: £100 → £200 → £300 → bigger; 19th deadline is hard
- R&D claim error: post-2023 HMRC enforcement is **dramatically tougher** — claim rejections, penalties up to 100% of disputed amount; failure to file claim notification = entire claim invalid
- EMI 92-day filing: miss it and the option **loses tax-favoured status entirely**
- ATED late return: £100 → £700 → £1,200; tax-geared penalty if tax due
- SDLT late: interest + tax-geared penalty
- Common pitfalls: CIS materials/labour split done wrong; R&D claim too generous → enquiry; EMI valuation not pre-agreed with HMRC; ATED reliefs not claimed; SDLT multiple-dwellings relief / mixed-use claims under HMRC scrutiny

**8. Tools & software**
- **CIS:** Xero CIS module, Sage CIS, BrightPay CIS, HMRC CIS online
- **R&D:** WhisperClaims, Empirical, ForrestBrown's tools
- **EMI:** Vestd, Capdesk, Carta, SeedLegals — most practices outsource the legal docs
- **ATED:** HMRC ATED online
- **SDLT:** HMRC SDLT online; SDLT Compass for complex cases
- **Charity:** IRIS Charities, Charity Commission online filing

**9. Economics**
- Pricing:
  - CIS: ~£25–£60 per subcontractor per month
  - R&D: typically **15–25% of credit** (contingent), or fixed £5–25k for non-contingent firms
  - EMI: £1,500–£5,000 per scheme + annual maintenance
  - ATED: £400–£1,500 per property
  - SDLT: £200–£1,500 per transaction
- Margin: **the highest realisation rates** in the firm — sub-£500/hr or £2k/day equivalent on R&D and EMI
- Strategic dial: specialist compliance is **the most defensible niche** an SMB practice can build

**10. Client experience**
Specialist compliance is **invisible until needed**. Good experience: practice **proactively flagging** ("you've crossed the £500k ATED threshold"; "your project sounds R&D-eligible — let's scope it"). Bad: client discovering they should have claimed three years of R&D, or EMI options expired worthless because the 92-day filing was missed.

---

### A9 — Audit & Assurance

The most regulated service line — requires its own separate registration, methodology, and quality regime. Most pure-SMB practices **do not hold audit registration** because the regulatory overhead doesn't pay back at SMB-scale fees. The **April 2025 threshold uplift** (turnover £15m, balance sheet £7.5m) moved ~14,000 medium-sized companies into the audit-exempt small bracket — meaningfully shrinking the SMB audit market.

**1. Purpose** — Express an independent professional opinion on whether a client's financial statements give a "true and fair view" in accordance with the relevant framework (UK GAAP / IFRS) — providing assurance to shareholders, lenders, regulators, and acquirers, and meeting the statutory audit obligation.

**2. Trigger & cadence**
- Annual for any client meeting the audit requirement
- Audit-required if **at least 2 of 3** thresholds breached (post-April 2025) **for two consecutive years**:
  - Turnover **> £15m**
  - Balance sheet total **> £7.5m**
  - Average employees **> 50**
- **Group rule:** if the parent group is *not* small, even small subsidiaries lose exemption
- Mandatory regardless of size for: PIEs, regulated entities (FCA-authorised firms, charities above thresholds, pension schemes, LLPs in some circumstances), shareholders ≥10% demanding one
- Audit work clusters in **Sept–Mar** for March year-ends

**3. Inputs**
- Trial balance and full ledger access for the year
- Statutory accounts draft from A5
- Prior year audit file
- Engagement letter and acceptance procedures (audit-specific independence checks)
- Internal controls documentation
- Board minutes
- Bank, debtor, creditor, stock count results — third-party confirmations
- Going-concern assessment (12+ months cash forecast)
- Related-party register, group structure
- Subsequent events to date of signing

**4. Process steps (ISA UK driven)**
```
PLANNING
1. Client acceptance / continuance — independence, fees, ethics
2. Risk assessment: understanding entity, environment, controls
3. Materiality — overall, performance, clearly trivial
4. Identify significant risks (revenue recognition presumed)
5. Audit strategy & detailed plan
6. Engagement letter signed

FIELDWORK
7. Walkthroughs of key transaction cycles
8. Tests of controls (where reliance taken)
9. Substantive procedures:
   - Cash: bank confirmations, reconciliations
   - Debtors: external confirmations, subsequent receipts
   - Creditors: search for unrecorded liabilities
   - Stock: attendance at year-end count
   - Fixed assets: existence & valuation
   - Revenue: cut-off, completeness, occurrence
   - Payroll: testing
   - Estimates: provisions, impairments, going concern
10. Group / consolidation work if applicable
11. Related parties & journal entries testing
12. Going concern review

COMPLETION
13. Final analytical review
14. Summary of unadjusted misstatements
15. Going-concern / subsequent-events confirmations
16. Letter of representation from directors
17. Engagement Quality Review (EQR) if required
18. Audit report drafted (clean / modified)
19. Sign accounts; file at CH; deliver audit report
20. Management letter / report to those charged with governance
```

**5. Outputs & filings**
- Audit report — signed and dated, attached to the statutory accounts
- Management letter / Report to Those Charged with Governance
- Audit working papers file — retained for **6 years** post-audit (FRC requirement)
- Independence and ethics records
- Quality control review documentation

**6. Roles & handoffs**
| Role | Responsibility |
|---|---|
| **Audit junior / trainee** | Vouching, testing, sample selection |
| **Audit senior** | Section ownership, reviews juniors |
| **Audit manager** | Planning, complex areas, file completion, drafts the report |
| **Audit partner (RI — Responsible Individual)** | Signs the report, owns ethical compliance |
| **Engagement Quality Reviewer (EQR / "second partner")** | Independent review on listed/PIE and risk-flagged audits |
| **Client (FD/CFO)** | Provides data, signs LoR, addresses points |

The firm itself must be a **Registered Auditor** (registered with ICAEW, ACCA, or another RSB). The audit partner must be a **Responsible Individual (RI)** with personal authorisation.

**7. Deadlines, risks & penalties**
- Audit must be signed before statutory accounts can be filed (9-month deadline for private cos)
- **FRC inspection regime** — for major audits, public-record findings; for SMB-tier audits, inspection by RSB (e.g. ICAEW QAD)
- Risk to firm: misstated audit opinion exposes to negligence claims (PII), FRC sanctions, loss of audit registration
- Risk to RI personally: exclusion, fines, name on FRC enforcement record
- Common pitfalls: insufficient evidence on revenue recognition / cut-off; going concern conclusions inadequately documented; independence threats (long association, fee dependency, non-audit services); group audits with weak coverage of components; fraud risk procedures (ISA 240) not properly executed
- **Major-client fee dependency:** ICAEW/FRC rules cap a single audit client at typically 10–15% of firm fee income

**8. Tools & software**
- **Audit methodology:** Caseware, MyWorkPapers, AlphaAudit, Inflo, AuditBoard
- **Audit planning / risk:** Mercia (UK SMB-focused methodology), Croner-i, ICAEW Audit Manual
- **Confirmations:** Confirmation.com (Confirma)
- **Data analytics:** Inflo, IDEA, ACL, Tableau for journal-entry testing
- **AI / automation:** rapidly emerging — JE testing, anomaly detection, document review

**9. Economics**
- Pricing: £8k–£40k typical SMB audit (post-threshold, mostly group subs and FCA-regulated firms); £40k+ medium; £100k+ complex
- Time budget: 60–250+ hours per audit
- Margin: mid (30–45%) — labour-intensive; recovery rate often the lowest in the firm
- Automation pressure: highest mid-term — sampling → full-population testing changes economics
- Strategic dial: post-2025 threshold change, **the SMB audit market is shrinking by ~20%**

**10. Client experience**
**Structured but intrusive**: audit team arrives for fieldwork (1–2 weeks on-site or remote), asks probing questions, attends year-end stock count, sends bank confirmation requests, produces a management letter pointing out internal-control weaknesses. Good: predictable timeline, no surprises, planning meeting in month 8, clean audit signed by month 11. Bad: delayed audit blocking dividend declarations, qualified opinion derailing a refinancing, management letter the FD takes personally.

---

### A10 — Advisory & Tax Planning

The **margin layer** of the firm. Compliance is the floor; advisory is what differentiates a £150k-fees-per-partner firm from a £600k one. Advisory is also the **least standardised** service line — every engagement is partly bespoke, depends heavily on the senior individual delivering it.

**1. Purpose** — Use the deep client knowledge accumulated through compliance work to **proactively identify opportunities** — to save tax, restructure for growth, plan succession, fund expansion, optimise extraction, navigate transactions — and deliver structured advice.

**2. Trigger & cadence**
- Event-driven: business sale, fundraise, property purchase, inheritance, divorce, retirement, group restructure, new venture
- Calendar-driven: annual tax-planning review (typically Q4 of tax year), pension top-ups (5 April deadline), CGT crystallisation, dividend timing
- Threshold-driven: crossing income tax, NI, CT, VAT, audit, IHT thresholds
- Project-driven: business plan, forecast, valuation
- Advisory retainer: monthly check-ins on a planned topic roadmap

**3. Inputs**
- Compliance work product (accounts, tax returns, management accounts) — the data foundation
- Personal financial info (pensions, ISAs, properties, investments)
- Family situation (spouse income, kids, succession plans)
- Business situation (shareholders, growth ambition, exit horizon)
- Sector knowledge & precedents
- HMRC manuals, statute, case law
- Software outputs: forecasts, what-if models

**4. Common advisory engagements & process**
```
PROFIT EXTRACTION REVIEW (annual, ~6–12hrs)
1. Model director's optimal mix of salary, dividend, pension,
   benefits, and director's loan
2. Factor in spouse income, NIC thresholds, tax bands,
   loss of personal allowance >£100k, HICBC, child care
3. Quantify saving vs current strategy
4. Diary the actions across the tax year

INCORPORATION REVIEW (one-off, ~10–20hrs)
1. Compare sole trader vs Ltd: tax savings vs admin cost
2. Assess incorporation relief (s162 TCGA) on goodwill
3. Project forward 3 years
4. Implement: CH formation, HMRC registrations, transfer of trade

EXIT / SALE PLANNING (project, £5k–£50k)
1. Pre-sale grooming (clean up DLA, separate property,
   employment status, R&D claims)
2. BADR (Business Asset Disposal Relief) eligibility check
3. Share-for-share rollover, EOT, MBO planning
4. Valuation; data room; deal team coordination

FUNDRAISE / EIS-SEIS
1. Assess SEIS / EIS eligibility for the company
2. Apply for advance assurance from HMRC
3. Issue compliance certificates post-investment
4. Investor tax notes

SUCCESSION & IHT PLANNING
1. Review BPR eligibility on shares
2. Trust structures, lifetime gifting (PETs)
3. Pensions as IHT-efficient wrapper
4. Family Investment Companies (FIC) consideration

R&D / EMI / PATENT BOX (specialist projects) — see A8

STRATEGIC FORECASTING (project)
1. Build 3-year integrated P&L / BS / cash forecast
2. Sensitivity analysis
3. Funding gap analysis; scenario planning
4. Present to board / lenders
```

**5. Outputs & filings**
- Advice memos (file-noted, e-signed, retained)
- Tax-planning models (Excel)
- Forecast packs
- HMRC clearance applications (e.g. share-for-share rollover, demergers)
- Valuation reports
- Engagement-specific deliverables (board pack, business plan, due diligence pack)
- Generally **no recurring statutory filings** — but actions taken often trigger filings in other service lines

**6. Roles & handoffs**
- **Partner / Director** — owns the relationship, frames the advice, signs off
- **Tax manager / specialist** — does the technical work
- **Junior / Senior** — supports modelling, research
- **External specialists** — IFA, solicitor, valuer, surveyor, R&D specialist, often coordinated by the practice as **deal-team leader**
- **Client** — provides info, makes decisions

The bottleneck is almost always **partner time**. Firms that productise advisory (e.g. fixed-scope "Annual Tax Review", "Exit Readiness Assessment") scale better than firms that price every engagement bespoke.

**7. Deadlines, risks & penalties**
- Tax-year-end deadlines: 5 April for personal pension/EIS/CGT; 31 March for company year-end planning
- HMRC clearance windows: statutory deadlines per regime
- **PCRT (Professional Conduct in Relation to Taxation)** — the cross-body code; advice must not promote artificial avoidance
- **DOTAS (Disclosure of Tax Avoidance Schemes)** — failure to disclose notifiable schemes
- **GAAR (General Anti-Abuse Rule)** — applies to "abusive" arrangements
- **PII risk:** advice negligence claims are the firm's biggest tail risk
- Reputational: mid-2010s aggressive tax planning collapses (film schemes, EBTs) badly damaged firms involved
- Engagement-letter scope: advisory engagements often creep — need explicit scope documents

**8. Tools & software**
- **Tax research:** Croner-i (Tolley's), LexisNexis, Bloomsbury Professional, ICAEW TaxLine, ACCA Technical
- **Modelling:** Excel (universal), Fathom, Spotlight, Float for forecasts
- **Tax planning specific:** TaxCalc Tax Planner, IRIS Personal Tax with planning, CapitaxPlanner, CCH Personal Tax planner
- **Document management:** Suralink, Virtual Cabinet
- **Workflow:** Karbon for productised advisory pipelines

**9. Economics**
- Pricing:
  - **Hourly** still common (£200–£600 partner; £100–£250 manager)
  - **Fixed fee per engagement** (e.g. £750 profit extraction review; £2.5k incorporation; £8k exit-readiness; £25k+ deal advisory)
  - **Value-based** (e.g. % of tax saved, contingent fee on R&D — under regulatory pressure)
  - **Retainer** (£500–£3k/month for ongoing access to a partner)
- Time budget: 5–500+ hrs per engagement
- Margin: **highest in the firm (60–80%)** when productised; lower if scoped loosely
- Strategic dial: advisory is where the firm's **competitive moat** lives — relationships, sector specialism, named-individual expertise. PE acquirers price firms on advisory mix; valuations of advisory revenue are 1.5–2× compliance multiples.

**10. Client experience**
The transformative experience: the partner calls in October — *"we've been looking at next year — here are three things we'd suggest before tax-year-end that could save you £14k. Want a 30-minute call?"* The client books, advice is implemented, saving lands, relationship deepens, fee scales.

The poor experience: silence between year-end meetings, no proactive contact, advisory only when the client asks — by which point they've already googled it, done it wrong, and the firm is fixing rather than advising. **The single biggest predictor of advisory revenue per client is who initiated the conversation.**

---

### A11 — Virtual CFO / Outsourced Finance

The natural endpoint of the advisory ladder for the **growing SMB**. Once a business outgrows "owner does the books on Sunday + accountant files annually" but isn't yet big enough to hire a full-time Finance Director (£130k–£200k+ all-in), it sits in a sweet spot for **fractional finance leadership**.

**1. Purpose** — Act as the client's outsourced finance function — providing the strategic finance leadership of a CFO/FD plus the operational scaffolding (bookkeeping, management accounts, controls, KPIs, cash) — at fractional cost.

**2. Trigger & cadence**
- Trigger: business hits £1m+ turnover, takes external investment, plans a fundraise, hits a working-capital crisis, prepares for sale, or outgrows the founder's ability to manage cash and reporting
- Cadence: **monthly retainer**, with weekly contact in active periods (fundraise, deal, year-end)
- Engagement length: typically 12–36 months; some convert to permanent FD hire (often a *successful* outcome for the practice)

**3. Inputs**
- Continuous live ledger access (Xero/QBO)
- Real-time bank, payments, Stripe data
- Operational data: pipeline, hours, delivery, headcount
- Client's strategic context: growth plans, owner ambitions, industry dynamics
- External stakeholder positions: bank, investors, board, key customers/suppliers

**4. Process steps (typical monthly retainer)**
```
WEEKLY
1. Cash review — 13-week forecast updated, payment runs reviewed
2. Pipeline / sales pacing review with sales lead
3. Issue / decision triage with founder

MONTHLY
4. Bookkeeping closed (delivered by practice or supervised)
5. Management accounts pack produced (A2)
6. Variance review — actuals vs budget vs forecast
7. KPI review with operational owners
8. Board pack assembled (if applicable)
9. Monthly Finance Review meeting with founder/MD
10. Action log with owners and dates

QUARTERLY
11. Reforecast (rolling 12-month)
12. Pricing / margin review
13. Cap table / equity housekeeping
14. Tax-planning checkpoint

ANNUAL
15. Budget process (3-year strategic plan + Year-1 detailed budget)
16. Year-end accounts (A5) seamlessly integrated
17. Audit liaison (if applicable)
18. Bank facility renewal / lender comms

EVENT-DRIVEN
19. Fundraise — financial model, data room, IM input
20. Sale — commercial due diligence support
21. Acquisition — target evaluation, integration
22. Crisis — restructure, refinance, cost-out programmes
```

**5. Outputs & filings**
- Monthly board pack
- Cash forecast (rolling 13-week + annual)
- Budget vs actual analysis
- KPI dashboards
- Quarterly / annual reforecasts
- Strategic memos
- Lender / investor reporting
- Standardised management info schedule
- *(Statutory filings come through the underlying compliance services; A11 is the strategic wrapper)*

**6. Roles & handoffs**
| Role | Responsibility |
|---|---|
| **Virtual CFO partner / director** | Strategic relationship; attends board / monthly review; named individual the client trusts |
| **Senior manager / Finance Manager** | Owns the monthly cycle, builds the packs, runs queries |
| **Bookkeeper / Junior** | Day-to-day data quality; works directly with client's ops team |
| **Tax / specialist partners** | Pulled in for episodic technical needs |
| **Client (founder/MD)** | Receives the finance function as a service |

The model is **leverage-heavy** — one VCFO partner can support 8–15 monthly retainer clients with a layered team beneath them.

**7. Deadlines, risks & penalties**
- No statutory deadlines specific to A11; absorbs underlying compliance service deadlines
- Operational risks:
  - Scope creep — without tight engagement letter, "VCFO" becomes "do everything"
  - Boundary confusion — when is the VCFO advising vs deciding? Decisions must remain the founder's
  - Independence — if the firm also audits a VCFO client, that is **prohibited** (post-Carillion ethical rules)
  - Conflicts — VCFO advice that affects audit-client positions
  - Liability — operational involvement raises PII exposure beyond pure advisory
- Reputational: a VCFO client failure reflects on the firm in a way a compliance failure does not

**8. Tools & software**
- Cloud ledger + PM stack as per A1, A2
- Cash forecasting: Float, Fathom, Brixx, native Xero
- Reporting: Fathom, Spotlight, Syft, Power BI
- Modelling: Excel, Causal, Cube
- Project / collaboration: Slack, Notion, Loom, monday.com — most VCFO teams operate in the client's stack
- AI assist: increasingly using LLM-driven draft commentary and forecasting tooling

**9. Economics**
- Pricing: **£1,500–£8,000 per month retainer** (typical SMB band £2,000–£5,000); event surcharges for fundraise / deal work; some practices charge **£20k–£60k flat** for fundraise support
- Annual revenue per client: £20k–£100k+ (vs £3k–£20k for compliance-only)
- Time budget: 1–4 days/month per client
- Margin: strong (50–70%) with the right team leverage; thin if delivered partner-only
- Strategic dial: **the highest revenue-per-client service line** by an order of magnitude

**10. Client experience**
The transformation: founder no longer manages a bookkeeper, doesn't chase month-end, has a board-grade pack each month, gets a named senior person on the phone within hours. Cash visibility goes from gut to spreadsheet to live forecast. Decisions are sharper. Investors trust the numbers. The firm becomes part of the **executive operating cadence**.

The bad version: practice rebadges existing services as "VCFO" without changing cadence or seniority involved, client doesn't feel the upgrade, £4k/month feels expensive next to the previous £600/month bookkeeping.

---

### A12 — Company Secretarial

The least glamorous service line, the easiest to forget — and the one that's quietly **expanding** under the Economic Crime and Corporate Transparency Act 2023.

**1. Purpose** — Keep each client company's statutory records and Companies House filings accurate and up to date — directors, shareholders, persons with significant control (PSCs), share capital, registered office, articles — and document corporate events (share issues, transfers, dividends, name changes, ARD changes).

**2. Trigger & cadence**
- **Annual:** confirmation statement (CS01) — due at least once every 12 months; **14-day filing window** after the review date
- **Event-driven:** share allotments (SH01), share transfers (J30 internal + next CS01), changes of director (AP01/TM01), registered office (AD01), articles changes, dividends (board minutes + vouchers)
- **One-off:** company formations, restorations, strike-off applications
- **Triennial-ish:** PSC reviews and identity verification under ECCTA 2023

**3. Inputs**
- Existing statutory registers (members, directors, PSCs, charges)
- Companies House WebFiling authentication code or API authority
- Articles of Association
- Shareholders' agreement (if any)
- Board minutes / written resolutions for the year
- Share allotment / transfer paperwork
- Latest filed CS01
- ID verification status of directors and PSCs (under ECCTA)

**4. Process steps**
```
ANNUAL CS01
1. Pull current data from Companies House
2. Reconcile vs internal registers and prior year
3. Confirm SIC codes, registered office, email address (now mandatory)
4. Confirm shareholder list (only required if changed)
5. Confirm PSC details
6. Send "anything to update?" check to client
7. File CS01 + £50 fee (online)

SHARE ALLOTMENT
1. Board resolution + (if needed) shareholder resolution
2. Issue share certificates
3. Update internal register of members
4. File SH01 within one month
5. Update PSC register if percentages crossed thresholds
6. File PSC change forms (PSC01–09) within 14 days

DIVIDEND
1. Verify distributable reserves
2. Board minute + dividend voucher
3. Update register; nothing filed at CH but feeds A7 + A6

NAME / OFFICE / DIRECTOR CHANGE
1. Resolution / appointment paperwork
2. File AP01 / TM01 / AD01 / NM01 within 14 days
```

**5. Outputs & filings**
- **CS01** — Confirmation statement (annual)
- **SH01, SH06, SH08** — share allotments, reductions, conversions
- **PSC01–09** — PSC register updates
- **AP01 / AP03 / TM01 / TM02** — director and secretary appointments / terminations
- **AD01 / AD02 / AD03** — registered office / SAIL
- **NM01** — change of name
- **Statutory registers** — members, directors, secretaries, PSCs, charges, allotments
- **Board minutes & resolutions**
- **Dividend vouchers**
- **ID verification status** under ECCTA 2023

**6. Roles & handoffs**
- **Practice manager / Co-sec specialist** — runs the recurring filings
- **Manager / Partner** — handles complex events (restructures, share schemes interface)
- **Client (director)** — signs resolutions, provides ID for verification
- **Solicitor** — referred in for material restructuring (group reorganisations, share-for-share, demergers)

Many SMB practices use a co-sec specialist sub-contractor (Inform Direct, Elements CoSec) for routine work.

**7. Deadlines, risks & penalties**
- Confirmation statement: technically a criminal offence to fail to file; in practice **company strike-off** is the consequence (~3 months no contact)
- Share allotment SH01: must file within **1 month**
- PSC changes: **14 days** to update internal register, then **14 days** to notify Companies House
- **ECCTA 2023 (rolling in 2026):** mandatory **identity verification** for directors, PSCs, and people filing on behalf of companies; failure = inability to file
- Strike-off risk if confirmation statement persistently missed → company dissolved, assets become bona vacantia
- Wrong PSC declared → criminal offence under PSC regime
- Articles ignored (e.g. share issue without authority) → invalid issue, technical insolvency risk

**8. Tools & software**
- **Co-sec packages:** Inform Direct, Elements (formerly Eureka), Diligent Entities, Blueprint OneWorld, IRIS Company Secretarial
- **Companies House API** (some practices integrate directly)
- **Practice management:** Karbon / BrightManager schedules the annual CS01 trigger per company
- **ID verification (ECCTA):** Authorised Corporate Service Provider (ACSP) regime — practice must register; software via SmartSearch, Veriphy

**9. Economics**
- Pricing: £40–£100 per CS01 (often bundled into monthly fee); £75–£250 per share-issue / event filing; £500–£2,000 for a restructure
- Time budget: 15 min routine CS01; 1–4 hrs per share event
- Margin: thin on routine, healthy on events
- Strategic dial: **PE roll-ups have made co-sec strategically interesting** — every group restructure, share-for-share exchange, or EMI grant generates significant co-sec work. ECCTA's verification regime turns the practice into a quasi-regulated gatekeeper (an Authorised Corporate Service Provider).

**10. Client experience**
Good: **invisible** — once-a-year email "your confirmation statement is filed, here are your current details, please confirm any changes"; sign an annual dividend resolution pack; never see a Companies House letter.

Bad: finding out in February that the company was struck off in November because the CS01 was missed and post went to an old address. ECCTA verification will, for a window in 2026, be the **most intrusive** thing many SMB directors experience from their accountant.

---

## 6. Internal Operating Functions

### B1 — Client Onboarding & Engagement

The front door of every practice. Done well it filters bad-fit clients out, captures clean data in, locks the regulatory perimeter, and sets fee discipline before a single hour is spent.

**1. Purpose** — Convert a prospect into a properly scoped, compliant, and economically viable client — captured in the firm's systems with all permissions, identities, and obligations on both sides clearly set out before work begins.

**2. Trigger & cadence**
- Trigger: enquiry (referral, website, networking, switch-from-prior-accountant)
- Cadence: event-driven, but with steady stream — typical SMB practices add 3–20 new clients per month
- Switching season (Mar–Apr around year-ends) and Jan SA aftermath are peaks

**3. Inputs**
- Prospect's identity documents (passport / driving licence) for every Beneficial Owner, Officer & Controller (BOOC) — directors, shareholders ≥25%, sole trader
- Proof of address (utility bill, bank statement) per individual
- Company information: name, number, registered office, year-end, directors, PSCs (Companies House extract)
- Prior-period accounts, tax returns, payroll history (if switching)
- Source of funds / source of wealth (for higher-risk profiles)
- Prior accountant's contact details (for clearance)
- Bank statements / ledger access (for scoping)

**4. Process steps**
```
1. Initial enquiry & qualification call
   → confirm services wanted, entity type, expected complexity
2. Fact-find / discovery (often a structured form)
   → turnover band, transaction volume, payroll size, VAT scheme
3. Proposal & fee quote
   → menu-priced or value-priced; bundled monthly fee preferred
4. AML / KYC — Customer Due Diligence (CDD)
   → ID + address verification on every BOOC; e-verification or document upload
   → sanctions / PEP screening (EU/UK/UN/OFAC lists)
5. Money-Laundering Risk Assessment (MLR 2017 §28)
   → score on geography, sector, transaction profile, structure complexity
   → outcome: Low / Medium → standard CDD; High → Enhanced DD + senior sign-off
6. Engagement letter
   → scope, exclusions, fees, payment terms, responsibilities, GDPR, liability cap
   → e-signed by both parties (per ICAEW Code of Ethics R330.5)
7. Professional clearance letter to prior accountant
   → "any professional reasons we shouldn't act?" + request for handover info
8. HMRC agent authorisation
   → ASA (Agent Services Account) for MTD VAT / MTD-IT
   → 64-8 / online agent codes for SA, PAYE, CT, CIS — must be in place before any HMRC contact
9. Companies House authentication code (if appointing as filing agent)
10. Software setup
    → ledger (Xero/QBO) — invite, chart of accounts, bank feeds
    → portal access, e-signature, secure messaging
    → migration of prior data if switching
11. Practice-management setup
    → client record created; recurring jobs scheduled (year-end, VAT, payroll, SA)
    → fee schedule + Direct Debit / GoCardless mandate
    → assigned manager / partner
12. Internal kick-off & welcome
    → introductory pack, key dates, who-does-what, comms preferences
```

**5. Outputs & filings**
- Signed engagement letter (retained for life of relationship + 6 years)
- Completed CDD file with risk assessment & ongoing-monitoring trigger dates
- Active HMRC agent authorisations across applicable taxes
- Companies House filing access (where applicable)
- Active fee mandate (Direct Debit)
- Scheduled recurring jobs in practice-management software
- Welcome confirmation to client (including portal credentials)

**6. Roles & handoffs**
| Role | Responsibility |
|---|---|
| **Partner / Director** | Final approval on engagement, high-risk AML sign-off, fee discount approval |
| **Practice Manager / Onboarding specialist** | Owns the workflow end-to-end, chases documents, runs the checklist |
| **MLRO (Money Laundering Reporting Officer)** | Risk-rates the client, signs off Enhanced DD, files SARs if needed |
| **Manager** | Becomes the assigned day-to-day client owner once live |
| **Admin / Bookkeeper** | Software setup, data migration, bank-feed connection |

A typical SMB practice consolidates 2–3 of these into one person. The MLRO role is **mandatory** under MLR 2017 — even sole practitioners must designate themselves.

**7. Deadlines, risks & penalties**
- **AML / KYC** — mandatory *before* providing any regulated service; HMRC and ICAEW conduct supervisory inspections; failures lead to fines (£1,000s–£100,000s), removal of supervision, personal liability
- **Engagement letter absent** — claim risk, professional-body breach (ICAEW Code R330.5)
- **Acting without HMRC authorisation** — invalid filings, exposed adviser
- **Tipping-off** — telling a client they're being SAR'd is a criminal offence (Proceeds of Crime Act 2002)
- **Ongoing monitoring** — CDD must be refreshed periodically (typically every 1–3 years by risk level)

**8. Tools & software**
- **Onboarding & engagement automation:** Ignition, GoProposal, FigsFlow, Glasscubes
- **AML / KYC:** SmartSearch, First AML, VinciWorks, Veriphy, Credas
- **Practice management:** Karbon, TaxDome, BrightManager, Senta
- **E-signature:** built into above, or DocuSign / Adobe Sign
- **Payment mandate:** GoCardless (de-facto UK practice DD standard)
- **HMRC:** ASA + online agent services portal
- **Companies House:** WebFiling agent code

**9. Economics**
- Time budget: 2–6 hours per new client onboarding (compressed if a unified PM tool drives the workflow). Mid-tier and PE-backed firms target <2 hours via automation
- Cost: ~£20–£50 in software/AML check costs per onboarded client
- Pricing: typically *not* charged separately — absorbed into the recurring fee; some firms charge a "setup fee" (£150–£500) for switching clients with messy migrations
- Margin pressure: time spent here is non-recoverable WIP if prospect doesn't sign; firms that don't qualify hard waste 20–40% of onboarding hours on dead leads
- Strategic dial: every minute saved in onboarding compounds — practices with strong onboarding scale faster and have lower lock-up

**10. Client experience**
1. A 30-minute discovery call where someone actually understands their business
2. A clear written proposal with **one fixed monthly fee** (not "we'll see")
3. An e-signed engagement letter — no PDFs by email, just one click
4. An ID-verification email asking for passport selfie + address proof
5. A handover email going to their old accountant *for them*
6. Software invites: one for Xero, one for the firm's portal, one for GoCardless
7. A welcome message with the name and direct line of "your manager"

The good firms make this feel like a **two-week glide**; bad ones make it feel like opening a bank account in the 1990s. This is also where most prospects form their durable opinion of the firm — switching costs are emotional, and onboarding either justifies the switch or seeds buyer's remorse.

---

### B2 — Practice / Workflow Management

The **operating system of the modern firm**. Once a practice has more than ~5 staff and ~50 recurring clients, the deadline volume is too large to track in spreadsheets and inboxes. Workflow management converts a jumble of jobs, deadlines, queries, and documents into a coordinated production line.

**1. Purpose** — Orchestrate every piece of recurring and ad-hoc work the firm does for every client — scheduling jobs to deadlines, allocating tasks to people, surfacing bottlenecks, automating routine status changes and client comms, and giving partners a real-time view of capacity, risk, and progress.

**2. Trigger & cadence**
- **Continuous** at the operational level — every day generates new tasks, completed tasks, queries, escalations
- **Recurring jobs auto-generated** from each client's calendar of obligations:
  - VAT (per stagger, every 3 months)
  - Payroll (every pay period)
  - Year-end accounts (per ARD)
  - CT (per ARD)
  - SA (annual)
  - CS01 (annual)
- **Sprint-style internal cadence:** daily standup or kanban review (smaller firms), weekly team meeting, monthly partner review of WIP and capacity

**3. Inputs**
- Client master data: ARDs, VAT staggers, payroll frequencies, services subscribed
- Engagement letter scope (drives recurring job creation)
- Staff capacity: hours available, qualifications, holiday calendar
- Deadline calendar (HMRC, Companies House, TPR, statutory)
- Job templates (the standard playbook for "year-end small Ltd", "VAT quarterly")
- Live work product (queries answered, documents uploaded, tasks ticked)

**4. Process steps**
```
ON CLIENT ONBOARD
1. Client record created with services, key dates, contacts
2. Recurring jobs auto-generated (VAT, year-end, CT, SA, payroll, CS01)
3. Each job inherits a job template — a checklist of standard tasks
4. Default owner / reviewer assigned per job type

DAILY
5. Tasks appear on each staff member's "today" list, prioritised
6. Staff member tags the job: "with client" / "with manager" /
   "ready for review" / "complete"
7. Automated emails fire on status change

WEEKLY
8. Manager reviews capacity vs upcoming deadlines
9. Bottleneck rebalancing — reassign tasks if a deadline at risk
10. Query age review — anything over 7 days chased

MONTHLY
11. Partner WIP / lock-up review (ties into B3)
12. Job profitability review (budget vs actual hours)
13. Client risk register update

ON JOB COMPLETE
14. File closed; archive triggered
15. Bill raised; fee booked; recurring job for next period spawned
```

**5. Outputs & filings**
- Live capacity dashboard
- Deadline risk register
- WIP report (jobs in progress, hours invested vs budget)
- Lock-up report (B3 input)
- Job profitability per client and per type
- Staff utilisation report
- Client query log
- Audit trail of every status change (compliance-relevant)

**6. Roles & handoffs**
- **Practice manager / Operations director** — owns the system, creates templates, runs the cadence
- **Team / Department managers** — own deadline performance for their book
- **Partners** — strategic view, capacity-vs-fee decisions
- **All fee earners** — execute against tasks, log time, change status
- **Onboarding/admin** — feeds clean client data in
- **Software vendor support** — significant — workflow tools require ongoing configuration

**7. Deadlines, risks & penalties**
No statutory deadlines for the function itself — but workflow is the **defence layer** that prevents missed external deadlines. Risks:
- **Job creation gaps** — a client's services not properly mapped → recurring job never generated → missed deadline
- **Over-customisation** — too many bespoke job templates fragment the firm's standardisation
- **Status drift** — staff don't update status reliably → dashboard becomes fiction
- **Tool sprawl** — workflow + email + Slack + WhatsApp + spreadsheet = no single source of truth
- **Adoption failure** — partners not using the system signals to staff that they don't have to either; common kill-shot for implementations
- **Queries-with-client gap** — outstanding info requests unanswered are the biggest practical bottleneck

**8. Tools & software**
| Tier | Examples (UK SMB market) |
|---|---|
| All-in-one PM | Karbon, TaxDome, BrightManager (formerly AccountancyManager), Senta, Pixie, Glasscubes |
| Workflow + CRM lighter | Aero Workflow, Jetpack Workflow, AbacusNext / Canopy |
| Mid-tier integrated | IRIS Practice Management, CCH Central, AccountancyManager, Capium |
| Top-tier enterprise | CCH Axcess, Engagement Manager (Big Four-grade) |

UK SMB practices in 2026 concentrate on **Karbon, TaxDome, BrightManager, Senta** for under-50-staff firms; **IRIS / CCH / Capium** for mid-tier-and-above. Pricing typically £25–£50 per user per month.

**9. Economics**
- Direct cost: £25–£50/user/month software + ~10–20% of a practice manager's time
- Indirect ROI: highest-leverage internal investment a firm can make. Common claims: 15–30% productivity gain, 20–40% reduction in lock-up, 50%+ reduction in missed deadlines
- Switching cost: very high — migrating from one PM tool to another is a 6–12 month project
- Strategic dial: a firm's PM platform increasingly **defines the firm's operating model**

**10. Client experience**
The client doesn't directly use B2 — but feels its presence in:
- **Predictability** (deadlines never missed)
- **Single inbox** (no dozen email threads with different staff)
- **Fast queries** (the system makes outstanding queries visible)
- **Transparent status** (the portal shows where each piece of work is)
- **Consistent comms** (templated, branded, on-time)

The bad version: every client is a separate snowflake; tasks live in personal inboxes; the firm only finds out about a missed deadline when HMRC writes to the client.

---

### B3 — Time, Billing & WIP / Lock-up Management

The **financial nervous system** of the firm. If B2 is the operating system, B3 is the cash heart. **Service delivery converts into revenue and revenue converts into cash** — and where most under-performing practices leak profit.

**1. Purpose** — Capture the time and cost of delivering each client engagement, convert that effort into invoices billed at the right point in the cycle, collect the cash promptly, and surface the **unit economics of every client and engagement**.

**2. Trigger & cadence**
- **Daily:** time entry by fee earners
- **Monthly:** subscription invoices auto-generated; project bills raised on milestones; debt-collection cycle
- **Per engagement:** project bills at agreed milestones (e.g. 50% on engagement, 50% on delivery)
- **Quarterly:** WIP review; lock-up reporting to partners
- **Annual:** rate review; client profitability review; bad-debt write-offs

**3. Inputs**
- Time entries (fee earner, client, job, activity, hours, charge-out rate)
- Engagement letter / proposal (fee structure: fixed, hourly, value, retainer)
- Disbursements (Companies House fees, HMRC payments, third-party costs)
- Job completion / billable trigger from B2
- Client master: payment method (DD, BACS, card), billing contact, credit terms
- Bank receipts feed
- Aged debtors

**4. Process steps**
```
DAILY (each fee earner)
1. Log time against client/job/activity (target: 7 chargeable hrs/day)
2. Note any unbillable items (training, admin, holiday)

WEEKLY
3. Manager reviews team time entries; scrubs errors
4. Job WIP balance updated; flagged if over budget

MONTHLY (subscription clients)
5. Auto-generated invoice for monthly fee
6. GoCardless DD collected on due date
7. Bank reconciliation matches receipts to invoices

MONTHLY (project clients)
8. Manager reviews jobs ready to bill
9. Bill drafts produced from WIP (time × rate − writedowns)
10. Partner approves bills; sent to client
11. Payment terms typically 14–30 days
12. Aged debtor reminders auto-fire at 7, 14, 30 days

QUARTERLY / ANNUAL
13. WIP review — write-down stale balances; chase old jobs to billing
14. Lock-up dashboard reviewed by partners
15. Client profitability analysis
16. Debt write-offs and bad-debt provisioning
17. Charge-out rate review (annually, usually April)
```

**5. Outputs & filings**
- Sales invoices (with VAT) issued to clients
- Statements of account
- WIP reports
- Lock-up dashboard
- Realisation / recovery reports
- Aged debtor analysis
- Client profitability schedule
- Time-by-fee-earner reports
- Internal KPI pack for partners

**6. Roles & handoffs**
- **All fee earners** — daily time entry (the discipline most often resented and most often broken)
- **Practice / billing manager** — generates bills, manages DD runs, debt chase
- **Manager / Partner** — bill approval, write-down decisions
- **Finance / accounts (firm's own)** — sales ledger, bank rec, P&L
- **Client** — pays (or doesn't); manages own AP cycle

**7. Deadlines, risks & penalties**
- No statutory deadlines beyond the firm's own VAT, accounts, and tax obligations
- Internal targets — typical benchmarks:
  - **Lock-up:** <45 days best-in-class; 60–90 days average
  - **Utilisation:** 65–80% for fee earners
  - **Realisation:** >90% strong
  - **Cash collection days (DSO):** <30 best; 60+ poor
- Risks:
  - Time-entry slippage — entered late, becomes fiction
  - Scope creep, no change order — work done unbillable
  - Stale WIP — jobs older than 60 days unbilled rarely collect in full
  - Bad debt — typically 0.5–2% of fees; rises to 4%+ in poor recessions
  - Client perception — surprise bills generate disputes
  - VAT on bad debts — relief available after 6 months; needs tracking

**8. Tools & software**
- **Built into PM:** Karbon, TaxDome, BrightManager, Senta, IRIS PM
- **Standalone time/billing:** Quickbooks Time, Toggl, Harvest (less common in UK practices)
- **Proposal-to-payment:** **Ignition** (the dominant UK tool for engagement → recurring DD), GoProposal, FigsFlow
- **Direct Debit collection:** **GoCardless** is the de facto UK standard; Stripe ACH for cards
- **Sales ledger / firm accounts:** Xero (the firm runs its own books on Xero, often), QBO, or specialist (CCH Practice Management)
- **AR / collections automation:** Chaser, Satago, native PM dashboards
- **Lock-up reporting:** typically built into PM; some firms use Power BI on PM exports

**9. Economics**
- Direct cost of B3: ~5% of revenue (billing manager + software + payment processing + bad debt provision)
- Strategic ROI: most direct profit lever in the firm — every day of lock-up reduced is a day of partner capital freed
- **Subscription + DD model:** when implemented well, **takes lock-up from 70+ days to <20 days** — a one-off transformation in working capital
- Client mix economics:
  - Top 20% of clients usually deliver 60%+ of profit
  - Bottom 20% often loss-making after time invested
  - Few firms regularly run client-profitability — those that do typically prune or re-price the bottom decile each year

**10. Client experience**
- **Subscription + DD:** invisible — one consistent monthly debit, no invoice arguments, no surprises
- **Project billing done well:** scope and fee agreed up front, milestone bills predictable
- **Project billing done badly:** "we've worked X hours @ £Y, please pay £Z" with no advance warning — disputes, written-down realisation, churn

For staff, B3 is felt as the **timesheet ritual** — universally disliked, often gamed, but the discipline that backs every fee decision the firm makes.

---

### B4 — Document & Records Management + Client Portal

The **document chain of custody** of the modern firm. Every piece of advice given, return filed, account signed, AML check completed — and every supporting document — must be captured, secured, retained, and producible on demand.

**1. Purpose** — Capture and retain every document the firm produces or receives in client work, in a secure, organised, GDPR-compliant, regulator-defensible system; provide clients a **single secure channel** to upload, sign, and access their information; give the firm an evidentiary record that work was done correctly.

**2. Trigger & cadence**
- **Continuous** — documents flow constantly: client uploads, firm-generated drafts, e-signed approvals, HMRC correspondence
- **Per engagement:** document set tied to each job (working papers, supporting docs, deliverables)
- **Annual:** statutory accounts, tax returns, signed engagement letters
- **Periodic:** AML re-screening (1–3 yr cycle), retention reviews
- **Event:** subject access requests (GDPR), HMRC enquiries, professional-body inspections, PII claims

**3. Inputs**
- Documents from clients (ID, bank statements, invoices, agreements, board minutes)
- Documents from third parties (HMRC, Companies House, banks, solicitors, brokers)
- Firm-generated drafts (accounts, returns, advice memos, working papers)
- E-signed documents (engagement letters, accounts approval, dividend resolutions)
- Internal review evidence (file notes, approvals, partner sign-offs)
- Email correspondence (the actual decision trail — increasingly captured into the DMS)

**4. Process steps**
```
INTAKE
1. Client uploads document via portal app or magic email address
2. Auto-classified (passport, bank statement, invoice, etc.)
3. Indexed against client / job / period
4. Notification to assigned staff member

WORK PRODUCT
5. Drafts produced in compliance/tax/accounts software
6. Iterations stored (versioned)
7. Working papers compiled per job
8. Reviewer evidence captured (file note, sign-off)

OUTBOUND
9. Final document published to portal
10. E-signature request sent (1 click for client)
11. Signed copy returned, indexed, locked
12. Filing to HMRC / Companies House triggered

ARCHIVE
13. On engagement close, file locked
14. Retention clock starts (typically 6–7 years; longer for AML / tax)
15. Periodic retention review; secure destruction at end of period
```

**5. Outputs & filings**
- Client-facing: portal pages of documents, signed deliverables, year-end packs
- Internal: indexed working-paper files per engagement
- Regulator-facing: producible on demand for QAD inspection, FRC inspection (audit), HMRC enquiry, ICO subject access request
- Audit trail: who accessed what, when, from where

**6. Roles & handoffs**
- **All staff** — index documents to the right place; never store on local drives
- **Practice manager** — owns retention schedule, portal templates, structure
- **Compliance / MLRO** — oversees AML evidence file completeness
- **Partner / RI (audit)** — owns audit working-paper retention
- **DPO (Data Protection)** — handles SARs, breaches, retention reviews
- **IT / security** — backups, access controls, MFA, encryption, breach response

**7. Deadlines, risks & penalties**
- **Retention periods (broad):**
  - General records: **6 years** from end of accounting period
  - AML records: **5 years** after end of business relationship
  - Tax records (HMRC): **6 years** (longer if enquiry open)
  - Audit working papers (FRC): **6 years**
  - Companies House records: **for life of company + 10 years** for some
  - PII claim records: indefinitely advisable
- **GDPR risks:**
  - Subject Access Request response within **1 month**; failure → ICO complaint, fines (up to 4% of revenue)
  - Data breach notification within **72 hours** if material
- **Cybersecurity:** firm holds tax IDs, bank details, ID copies — high-value target for phishing, ransomware, business-email-compromise. PII insurance now mandates MFA and endpoint protection
- Common pitfalls: documents in personal email; local saves on laptops; inconsistent indexing; over-retention; under-retention (deleted before HMRC enquiry window closes)

**8. Tools & software**
| Function | Examples |
|---|---|
| All-in-one portal + DMS | TaxDome, Karbon, BrightManager, Senta, Glasscubes |
| Audit-grade DMS | Suralink (request lists + portal), Caseware, Virtual Cabinet, IRIS DocumentMix |
| Standalone e-sign | DocuSign, Adobe Sign, HelloSign — but most PM tools have it built in |
| Cybersecurity wrappers | Practice Protect, OGL, Egress, CyberSmart, Mimecast |
| Backup / archive | Microsoft 365 with retention policies, Backupify, Datto |
| Identity verification | SmartSearch, Veriphy, Credas |

**9. Economics**
- Direct cost: typically £15–£40 per user per month for portal+DMS (often bundled into the PM tool); £5–£15 per user for cybersecurity wrappers
- PII insurance benefit: demonstrated MFA, encryption, training, document control reduces premium 5–15%
- Strategic dial: the **portal experience is increasingly the firm's brand** — clients judge professionalism by the quality of the portal more than the design of the office. AI-driven document intake (auto-classification, OCR, query extraction) is the next major step — early implementations cut 30–60% of admin time on document handling.

**10. Client experience**
Good — frictionless:
1. Client gets an email: "Please upload your bank statements + sign your engagement letter — link below"
2. They click; biometric login (Face ID / fingerprint) on phone
3. Camera capture of documents → uploaded
4. Tap-to-sign engagement letter → done
5. Everything they've ever signed lives in one place, searchable
6. They never deal with passwords, ZIP files, or "please confirm receipt"

Bad: 14 separate emails with PDFs to download, password to a separate email, "please print, sign, scan and return", a Dropbox link that expired, a confidentiality agreement attached as a Word file. Each touchpoint erodes the perception of competence.

---

### B5 — Client Communication & Query Handling

The function with the highest **emotional gradient** in the firm. Every other function is internal craft; this is the constant interface with the client. A practice can be technically excellent and *still* lose clients on communication alone.

**1. Purpose** — Be **predictably responsive** to every client touch-point (queries, requests, deadline reminders, advisory check-ins), capture every conversation against the right client and job for institutional memory, and keep the **outstanding-query pile small and aging slowly**.

**2. Trigger & cadence**
- **Continuous (inbound):** client emails, portal messages, calls, WhatsApp, queries from third parties (HMRC, banks, brokers)
- **Continuous (outbound):** automated reminders (data requests, deadline alerts, document uploads), manager-driven check-ins
- **Cyclical:** monthly management-account reviews, quarterly VAT comms, year-end planning calls
- **Event:** HMRC letters, urgent client issues, bereavement, business crises, sale enquiries

**3. Inputs**
- Inbound emails / portal messages / call notes
- Outbound query templates (data requests, year-end packs, VAT confirms)
- Client preference data (how they like to be contacted, who's their primary contact)
- Open-query register (per client, per job)
- Triage rules (who handles what, escalation thresholds)

**4. Process steps**
```
INBOUND
1. All client comms route into a shared inbox / PM tool triage
2. Auto-classify (which client, which job, urgency)
3. Assigned to the right owner (account manager / senior / specialist)
4. SLA clock starts (typical: 24 hr first response, 5 working days resolution)
5. Owner responds; logs the outcome against the client/job

OUTBOUND
6. Templated requests fired at predictable cadence
   (e.g. "VAT data due", "Year-end pack request", "P11D info")
7. Personalised cover note from named manager
8. Auto-chase if no response in 5/10/15 days
9. Manager intervenes after 3 chases

QUERY MANAGEMENT (the hard part)
10. Every job has an open-query list visible to client and firm
11. Queries auto-age; manager reviews stale queries weekly
12. Closed queries archive; resolution evidence retained

ESCALATION
13. Complex / sensitive (HMRC enquiry, bereavement, dispute)
    escalate to partner within 24 hours
14. PII-relevant items flagged to risk function (B6)
```

**5. Outputs & filings**
- Captured comms log per client (the institutional memory)
- Open-query register
- Communication audit trail (who said what, when)
- Templated outbound packs (all branded, version-controlled)
- Internal escalation logs

No statutory filings — but **comms records are evidentiary** in HMRC enquiries, professional-body complaints, and PII claims.

**6. Roles & handoffs**
- **Account manager** — the client's named contact and primary owner
- **Senior / specialist** — handles technical queries
- **Partner** — escalations, sensitive matters, retention conversations
- **Admin / front-of-house** — call routing, basic triage
- **All staff** — log meaningful conversations centrally
- **Client** — increasingly self-serve via the portal

The single highest-leverage decision: **one named manager per client**. Practices where clients are bounced between staff have measurably lower NPS and higher churn.

**7. Deadlines, risks & penalties**
- No statutory SLAs — but internal benchmarks:
  - First response: **within 24 working hours**
  - Standard query resolution: **5 working days**
  - Escalations to partner: **within 24 hours**
  - HMRC enquiry letters: **same day** acknowledgement to client
- Risks:
  - **Channel sprawl** — comms across personal email, WhatsApp, phone, portal → no central record → things fall through
  - **Out-of-office gaps** — single point of failure if the named manager is unavailable
  - **Tone risk** — informal channels lead to commitments not properly scoped (PII risk)
  - **GDPR risk** — sensitive data on insecure channels (consumer email, SMS, WhatsApp)
  - **Privilege / confidentiality** — must not discuss other clients
  - **Tipping-off (AML)** — must never alert client to a SAR
  - **Complaints regime** — every professional body requires a documented complaints procedure

**8. Tools & software**
- **Shared inbox + triage:** Karbon (built for accountancy comms), Front, Hiver, Outlook Shared Mailbox
- **Portal messaging:** built into TaxDome, BrightManager, Senta
- **Query lists:** native to Karbon, TaxDome, Senta
- **Templates / mail-merge:** built into PM tools; Mailchimp / HubSpot for marketing-style updates
- **Phone / video:** Zoom, Microsoft Teams (also doubles as recording for retention)
- **Calendar / booking:** Calendly (review-call scheduling)
- **Secure messaging (regulated):** TaxDome / portal channels avoid GDPR risk of consumer apps

**9. Economics**
- Direct cost: ~10–25% of fee earner time goes into comms — for many seniors, over half
- Indirect cost: poor comms → churn → CAC re-spend; measured over years, can be the **biggest single P&L lever** in the firm
- NPS / retention link: firms with ≥9-of-10 NPS measurably retain clients longer and grow faster
- Strategic dial: as compliance commoditises, **client experience becomes the differentiator** — and communication is the sharp end of client experience

**10. Client experience**
Good — **one calm professional relationship**:
1. They have a named manager and a direct number
2. Every email is replied to inside a working day, even if "we'll come back to you Friday"
3. The portal shows what they've been asked for, what's outstanding, and what's been done
4. They never get the same question twice
5. They get proactive nudges before deadlines, not panic afterwards
6. When something complex happens, a partner phones within hours

Bad: emails to a generic info@ inbox, unanswered for days; queries answered by different people with different positions; the same data requested three times across three jobs; an HMRC letter forwarded with no commentary; a partner only making contact when it's time to negotiate the renewal fee. The cumulative impression: "they don't care about me."

---

### B6 — Quality, Risk & Regulatory Compliance (the firm's own)

The **internal regulator** of the firm. Where every client-facing service deals with the client's compliance, B6 deals with the firm's compliance — to the professional bodies (ICAEW, ACCA, etc.), to AML supervisors, to the FRC if audit-registered, to ICO for data, and increasingly to Companies House under ECCTA. Done well, it's invisible; done badly, it's existential.

**1. Purpose** — Ensure that the firm itself meets all the professional, ethical, regulatory, and legal obligations that allow it to call itself a firm of accountants — supervised, insured, ethically sound, with documented quality processes — and manage the risks (reputational, regulatory, financial, cyber).

**2. Trigger & cadence**
- **Continuous** at the cultural / habitual level (ethics, independence checks)
- **Per engagement** (acceptance & continuance, conflict checks, independence)
- **Annual** (ISQM 1 evaluation, AML risk assessment, PII renewal, CPD declarations, Practice Assurance return)
- **Cyclical (monitoring):** ICAEW QAD visits typically **every 6 years**; AML inspection irregular; FRC audit inspection **annually for major firms**, less for SMB-tier
- **Event-driven:** complaints, breaches, disciplinary, PII claims, regulatory queries, data breaches

**3. Inputs**
- Engagement letters and acceptance procedures
- AML risk assessment file (firm-level + per-client)
- Independence registers (audit-registered firms)
- Complaints log
- Breach register (AML, GDPR, professional)
- Conflicts register
- Insurance policies (PII, cyber, employer's liability, office)
- CPD records per individual
- Practising certificates per principal
- ISQM 1 quality framework documentation (audit firms)
- Quality monitoring results (cold file reviews, hot reviews)
- Regulatory correspondence

**4. Process steps**
```
ANNUAL CYCLE
1. Update firm-wide AML risk assessment (geography, sector,
   delivery, structure)
2. ISQM 1 evaluation (audit firms): document and assess each
   of the 8 components
3. Cold-file review programme (audit + non-audit) — typically
   sample of completed jobs reviewed by independent reviewer
4. CPD records collected from every member; deficiencies chased
5. PII renewal — minimum cover scaled to fee income
6. Practising-certificate renewals lodged
7. Practice Assurance return submitted to ICAEW (or equivalent)
8. ICO data-protection registration renewed
9. Annual training: AML, GDPR, ethics, cyber

PER ENGAGEMENT
10. Acceptance / continuance check (independence, integrity,
    capability, fee)
11. Conflict-of-interest check across the firm
12. AML CDD on new client / refresh on existing
13. Engagement letter signed before work starts
14. Reviewer assignment per quality plan

ONGOING
15. Suspicious-activity reporting (MLRO files SAR to NCA where needed)
16. Complaint handling per documented procedure
17. Breach logging (any departure from professional standards)
18. Cyber incident response (24/72-hr clocks)

INSPECTION READINESS
19. Maintain documentation as if a regulator would arrive next week
20. Pre-inspection internal review when a visit is scheduled
21. Manage QAD / AML / FRC visit; receive feedback; remediate
```

**5. Outputs & filings**
- **Practice Assurance return** to ICAEW (or equivalent for ACCA / AAT firms)
- **Annual ISQM 1 evaluation report** (audit-registered)
- **AML supervisor returns** (annual, plus event-driven SARs to NCA)
- **PII certificate** retained, premium paid
- **CPD declarations** per individual member
- **ICO registration** renewal (annual)
- **FRC AAR** (audit firms)
- **Internal:** quality manuals, monitoring results, complaints register, breach register
- **External, public:** firm name appears on supervisory registers

**6. Roles & handoffs**
- **Compliance partner / Head of Risk** — owns the function
- **MLRO** — owns AML, files SARs (statutory role)
- **Ethics partner** — independence, conflicts (audit firms)
- **DPO** — data protection (mandatory if processing at scale)
- **Quality partner / Designated principal** — ISQM 1, file reviews
- **All staff** — annual declarations (independence, conflicts, CPD)
- **External reviewers** — many firms outsource cold-file review to specialists (Mercia, Croner, Wolters Kluwer)

In a sole-practitioner firm, **all of these roles are the principal**. ICAEW expects this to be acknowledged formally — not pretended away.

**7. Deadlines, risks & penalties**
- **Loss of audit registration** for sustained quality failures (FRC / RSB action)
- **Loss of professional-body membership** for ethical breaches → cannot practise as that designation
- **AML supervisory action** — fines £1,000s–£100,000s; named on supervisor's enforcement list; ultimate removal of supervision = inability to operate AML-regulated services
- **PII increase or refusal** — critical for ongoing operation
- **GDPR fines** — up to 4% of global revenue; enforcement notices, undertakings
- Common pitfalls:
  - AML CDD documentation thin (especially for older clients onboarded pre-MLR 2017)
  - Cold-file reviews not done or not documented
  - CPD records reactive (gathered at year-end vs continuous)
  - Independence threats not formally assessed
  - Cyber controls not evidenced (training, MFA, patching)
  - ISQM 1 implemented "on paper" without real risk-based design

**8. Tools & software**
- **Quality management:** ICAEW QM Toolkit, Mercia, Croner-i, MyWorkPapers (file reviews)
- **AML:** SmartSearch, First AML, Veriphy, Credas
- **Risk register / breach log:** native to PM tool, or specialist (LogicGate, Vanta for cyber)
- **CPD tracking:** ICAEW CPD record, ACCA MyCPD, native records in HR platforms
- **Cyber compliance:** Practice Protect, CyberSmart, IT Governance (Cyber Essentials / Plus)
- **Conflicts:** native to PM tool; specialist (Intapp) at top tier
- **PII renewal:** Lockton Accountants, Hiscox, AIB, Howden — most firms broker via specialist

**9. Economics**
- Direct cost: typically **5–10% of revenue** for a well-run small firm (compliance partner time, PII premium, AML software, cybersecurity, ICO fee, body subs, CPD, file-review costs)
- PII premium: typically 2–5% of fee income, scaled by claim history and risk profile (audit > non-audit)
- Strategic dial: the **operating licence layer**. Skimping here doesn't show in P&L until something goes wrong, at which point it's existential. PE acquirers heavily diligence quality and risk processes before completion — a poor B6 record can knock 20–30% off a sale multiple.

**10. Client experience**
B6 is **mostly invisible** to clients — they don't see the file reviews, the AML risk register, the ISQM 1 evaluation. They feel its presence in:
- The annual ID re-verification request (under ongoing CDD)
- The professional, calm response when something goes wrong
- The visible PII coverage and professional-body badging
- Confidence that their data is genuinely secure

When B6 fails publicly — a PII claim, a regulatory action, a data breach — the firm's brand absorbs the entire impact. It is the **single most under-funded function** in mid-tier UK practices, and the most common source of partner sleeplessness.

---

### B7 — People, Resourcing & Training

The **single biggest constraint on growth** across the UK profession in 2026. Every UK firm survey ranks people as the top challenge — 73% cite recruitment and retention as the dominant concern.

**1. Purpose** — Attract, develop, deploy, retain, and progress the right blend of qualified accountants, trainees, specialists, bookkeepers, and operations staff to deliver the firm's services profitably — while satisfying the structural requirement that public-practice firms must be majority-led by qualified, regulated individuals.

**2. Trigger & cadence**
- **Continuous:** workforce planning, individual performance management, training delivery
- **Annual:** appraisal cycle, salary review, promotion round, qualification fund commitments, training needs analysis
- **Cyclical:** trainee intake (typically September), exam sittings (multiple per year), CPD year
- **Event:** unexpected resignations, partner retirements, lateral hires, parental leave, disciplinary, illness
- **Strategic:** workforce model shifts (offshoring, AI redeployment, niche-team building)

**3. Inputs**
- Demand forecast (based on client book + pipeline + B2 capacity reports)
- Existing capacity (qualifications, levels, hours, holiday)
- Pipeline of trainees by exam stage
- External market data (salary benchmarks — Hays, Robert Half, Dains, Reed surveys)
- Apprenticeship levy balance and policy
- Regulatory requirements (qualified-staff ratios for Chartered branding, RIs for audit)
- Engagement / NPS feedback from staff
- Exit interview data

**4. Process steps**
```
WORKFORCE PLANNING
1. Quarterly capacity review: forecast demand vs supply by team/grade
2. Identify gaps → recruitment requisitions
3. Build pipeline from apprentices, graduates, school-leavers, qualified hires

RECRUITMENT
4. Branding (careers site, LinkedIn, university milkround for grads)
5. Apprenticeship intake (Level 4 AAT / Level 7 ACCA-ACA mix)
6. Lateral hiring (qualified seniors, managers) — typically agency-led
7. Selection (technical test, case study, interviews, partner sign-off)
8. Offer + onboarding

DEVELOPMENT
9. Trainee study contracts (firm pays for tuition + exam fees + study leave)
10. CPD plan per individual — audit / tax / specialist
11. Mentoring + appraisal cycle (annual + mid-year)
12. Performance management for under-performers
13. Promotion criteria (clear bands: Junior → Semi-senior → Senior →
    Manager → Senior Manager → Director → Salaried Partner → Equity)

DEPLOYMENT
14. Resource allocation in B2 weekly review
15. Utilisation monitored (target ~70% chargeable for fee earners)
16. Workload balancing across teams

RETENTION
17. Salary benchmarking annually
18. Flexible / hybrid working policy
19. Wellbeing / mental health support (esp. around January peak)
20. Engagement surveys; act on feedback
21. Stay interviews for high-performers

OFFBOARDING
22. Resignation handling, knowledge transfer, exit interview
23. Restrictive covenants (clients, staff)
24. Re-recruitment plan
```

**5. Outputs & filings**
- Job descriptions and contracts
- Training plans per individual
- Appraisal records
- Compensation / promotion decisions
- Apprenticeship levy / training-provider records
- CPD logs (per professional body — feeds B6)
- Employment-law filings (P11D / payroll on the firm itself, redundancy notices)
- ED&I monitoring (increasingly expected)

**6. Roles & handoffs**
- **Managing partner / Equity partners** — set strategy, sign off pay/promotions
- **People director / HR manager** — owns the function (sole practitioners do this themselves)
- **Department / Service-line heads** — line management, appraisals, capacity
- **Training partner / mentors** — individual development
- **External:** ICAEW/ACCA student-status admin, training providers (BPP, Kaplan, First Intuition, FNA Tuition), apprenticeship providers
- **All staff** — own their CPD, appraisal participation

**7. Deadlines, risks & penalties**
- Regulatory: practising-certificate renewals; annual CPD declarations; audit RI status maintenance; apprenticeship-levy compliance
- Employment-law: standard UK regime — TUPE on acquisitions, IR35 on contractors, working-time, equality
- Risks:
  - **Capacity-led growth ceiling** — turning down work because no one to do it (now common)
  - **Single-point-of-failure partners** — knowledge concentrated in a leaving partner
  - **Salary inflation outpacing fee inflation** — margin compression
  - **Trainee pipeline gap** — multi-year effect of low intake compounds
  - **Burnout in January / June** peaks → resignations after them
  - **AI-redundancy debate** — handled badly, drives talent away
- Strategic risks:
  - Failure to invest in apprenticeship intake → 3–5 years later, manager shortage
  - Failure to retain top talent → competitors hire them with their client knowledge
  - Cultural drift after PE acquisition → mass departures

**8. Tools & software**
- **HR / payroll:** BambooHR, BreatheHR, Personio, BrightHR, Sage HR
- **Recruitment / ATS:** Workable, Teamtailor, LinkedIn Recruiter, Indeed
- **Performance / engagement:** Lattice, 15Five, CultureAmp, Officevibe
- **Learning / CPD:** ICAEW Learning, ACCA Learning Hub, CCH Learning, internal LMS (Docebo, TalentLMS)
- **Apprenticeship management:** apprenticeship-provider portals, ESFA digital account
- **Salary benchmarking:** Hays UK Salary Guide, Robert Walters, ICAEW Profitability/Recovery Survey

**9. Economics**
- People cost: typically **45–55% of fee income** — the dominant line in the P&L
- Recruitment cost: typically 15–25% of first-year salary (agency); apprentice intake is lower direct cost but higher development time
- Trainee economics: unprofitable in years 1–2 (low chargeable hours, study leave), break-even year 3, profitable year 4+; firms that don't take this multi-year view under-invest in pipeline
- Salary inflation: UK accountancy salaries up materially in 2023–25 due to talent shortage; outstripped fee inflation in many segments
- Strategic dial: **the highest-leverage investment a firm can make** is in trainee intake + structured progression. PE acquirers diligence senior bench depth as a key value driver.

**10. Client experience**
B7 only **indirectly** touches clients — but its effects are everywhere:
- Continuity (the same manager year after year)
- Competence (the right grade of person on the right work)
- Capacity (work done on time, no "we're at capacity" letters)
- Energy (engaged staff communicate better, advise better, retain clients better)

The bad version is felt as: a new junior every year, the manager left in November, the year-end is being done by someone who joined last week, the partner only appears for the renewal call. Once a client perceives **staff churn**, it compounds: they start to feel like they belong to the firm rather than to a person, and the relationship loses its anchor.

---

### B8 — Business Development, Pricing & Productisation

The function the typical mid-tier UK partnership has historically **under-invested** in. **62% of UK firms still source >50% of new clients via referral** — but that caps growth at the rate the existing book generates word-of-mouth.

**1. Purpose** — Build and maintain a **predictable inflow of well-fitting new clients** at fees that reflect the value delivered, by defining what the firm sells, who it sells it to, how it's packaged and priced, and how prospects discover and choose it — and manage the existing client book for upsell, retention, and margin.

**2. Trigger & cadence**
- **Continuous** at the marketing layer (content, SEO, referral nurture, networking, social)
- **Quarterly** sales pipeline review
- **Annual** strategy: niche selection, service mix, pricing review, marketing budget, growth target
- **Event-driven:** competitor moves, regulatory shifts (e.g. MTD-IT creates a mass-onboarding event), acquisition opportunities
- **Renewal cycle:** annual fee review per client (typically engagement-letter-anniversary based)

**3. Inputs**
- Client book analysis (sectors, sizes, profitability, fee bands, retention, NPS)
- Pipeline data (prospects, source, stage, value, conversion rate)
- Competitor intelligence
- Market research / sector trend data
- Client testimonials and case studies
- Cost-to-serve and target margin data (from B10)
- Brand assets, website analytics, social engagement
- Referral source mapping (introducers — solicitors, IFAs, brokers, banks)

**4. Process steps**
```
STRATEGY
1. Analyse current book — segment by sector, size, profitability
2. Identify niches where the firm has differentiated capability
3. Define the firm's proposition for each niche (the "right to win")
4. Set growth targets, channel mix, fee benchmarks per service

PRODUCTISATION
5. Design service packages with clear scope and pricing
   (e.g. "Starter Ltd £150/m", "Growth Ltd £450/m", "Scale £900/m")
6. Documented exclusions and out-of-scope rates
7. Proposal templates per package
8. Onboarding workflow per package (links to B1)

PRICING
9. Annual price review; bands inflation-adjusted
10. Value-based override on advisory engagements
11. Discount governance (who can approve what)
12. Fee-renewal communication for existing clients

DEMAND GENERATION
13. Referral programme (introducer relationships, MoUs, kickback rules)
14. Content marketing (blog, LinkedIn, video)
15. SEO + paid (where commercially justified)
16. Events / sponsorships / industry presence
17. PR / thought leadership
18. ABM (account-based marketing) for target firms

PIPELINE MANAGEMENT
19. Inbound enquiry triage and qualification
20. Discovery call → proposal (Ignition / GoProposal)
21. Win/loss tracking and feedback
22. Post-onboard NPS survey

RETENTION & UPSELL
23. Annual relationship reviews per client
24. Upsell mapping: bookkeeping → mgmt accounts → VCFO; CT → R&D
25. At-risk client identification and partner intervention
26. Lapsed-client win-back outreach
```

**5. Outputs & filings**
- Service catalogue / packages with prices
- Proposal templates (per service / per package)
- Marketing assets (website, content, social, PR)
- Pipeline dashboard
- Win-loss and source attribution data
- Client NPS / CSAT scores
- Upsell map per client
- Annual strategy document
- *(No statutory filings; some marketing claims subject to ICAEW/ACCA marketing rules — must not be misleading or comparative beyond evidence)*

**6. Roles & handoffs**
- **Managing partner / Growth partner** — strategy
- **Marketing manager / agency** — execution (small firms: outsourced; larger: in-house team)
- **Sales / new-business partner** — discovery calls, proposal close
- **All partners** — referral nurture, networking, content
- **Account managers** — relationship retention, upsell
- **Onboarding** — handoff to B1 once signed

The classic friction: **partners think marketing is admin; admin teams can't generate technical content; the result is generic, undifferentiated firm marketing.**

**7. Deadlines, risks & penalties**
- Regulatory:
  - ICAEW / ACCA marketing rules (no misleading, no comparative claims without evidence, must be professionally appropriate)
  - **PCRT** rules apply to tax marketing
  - Use of "Chartered Accountants" / "Chartered Certified Accountants" subject to control thresholds
  - GDPR on prospect data
- Strategic risks:
  - Underpricing — once embedded across a book, hard to correct without churn
  - Niche failure — picking a niche that doesn't pay
  - Brand drift — service mix expanding beyond capability
  - Reputational — bad client experience amplified online
  - Onboarding overload — winning too much, too fast → service quality collapse → churn

**8. Tools & software**
- **CRM:** HubSpot, Pipedrive, Capsule (small firms), Salesforce (larger); native to PM tools
- **Proposals:** Ignition, GoProposal, FigsFlow, PandaDoc
- **Marketing automation:** HubSpot, Mailchimp, ActiveCampaign
- **Website / SEO:** WordPress / Webflow + SEO tools (SEMrush, Ahrefs); specialist agencies (Rapport Digital, JDR, PracticeWeb)
- **Social:** LinkedIn (the dominant B2B accounting channel), YouTube (rising), Twitter/X (declining)
- **Reviews:** Google Business, Trustpilot, VouchedFor (financial-adjacent)
- **NPS / surveys:** Delighted, Survicate, native PM tool
- **Pipeline analytics:** PM-tool dashboards, Power BI on CRM exports

**9. Economics**
- Marketing spend: typically **2–6% of fee income** for growth-oriented UK firms; less than 2% in coast-along firms; **8–12%** in PE-backed roll-ups actively acquiring market share
- Cost per acquisition (CPA): £200–£1,500 per new SMB client depending on segment and channel
- LTV / CAC: subscription accountancy clients have very high LTV (£12k–£40k+ over typical 5–7 year tenure) → LTV:CAC ratios often >20:1 — among the best of any professional services category
- Pricing leverage: a **5% across-the-book price increase** typically converts almost entirely to bottom-line; a 10% increase can lift profit per partner by 30%+
- Strategic dial: **Productisation** is the dominant operational lever. Firms with packaged, named services scale faster, charge more, and integrate post-acquisition cleaner.

**10. Client experience**
A new prospect's experience is shaped almost entirely by B8:
1. They Google an issue, find a useful article on the firm's site, sign up to a download
2. They get a non-spammy follow-up offering a free 20-min call
3. The call is genuinely diagnostic, not a pitch
4. They get a clear three-tier proposal with named pricing
5. They sign in 24 hours via Ignition; onboarding kicks off automatically

The bad experience: a website that looks like 2014, "fees on application," a partner who wants two more meetings before quoting, and a hand-typed Word-doc engagement letter sent by email.

---

### B9 — Technology Stack Management & Data Architecture

The **plumbing of the modern firm**. Every service line, every internal function, every client-facing interaction now sits on a layered stack of cloud apps with APIs. The strategic question shifted in 2025–2026 from "which tools to add" to **"how to consolidate, integrate, and govern the stack you already have."** Firms with **highly integrated tech stacks see ~80% revenue growth versus <50% for those without.**

**1. Purpose** — Select, integrate, govern, and continuously evolve the firm's technology stack — ensuring data flows cleanly between systems, users have a coherent experience, security is robust, costs are controlled, and the architecture can absorb new tools (especially AI agents) without each addition increasing operational fragility.

**2. Trigger & cadence**
- **Continuous:** software vendor releases, integration health, user issues
- **Quarterly:** stack review (cost, adoption, integration, redundancy)
- **Annual:** technology strategy refresh; budget; renewal negotiations
- **Event-driven:** major regulatory changes (MTD-IT 2026), M&A integration, security incidents, vendor M&A or price hikes (e.g. QuickBooks UK 2025 hikes drove Xero migrations)

**3. Inputs**
- Current stack inventory (every SaaS subscription, every login, every API)
- User adoption data (logins, feature usage)
- Integration map (data flows between systems)
- Cost stack (per seat, per client, per usage)
- Security & compliance map (where does each data class live?)
- Vendor roadmaps (Xero, IRIS, Karbon, etc. publish 12-month plans)
- AI / automation pipeline (what's piloted, what's in production)

**4. Process steps**
```
INVENTORY
1. Maintain a stack register: every tool, owner, contract date,
   cost, users, data class held
2. Map integrations and data flows (where does client master live?
   how does it propagate to ledger / tax / portal?)

GOVERNANCE
3. Tool intake process — no shadow IT; new tools require
   security + integration review
4. Identity & access management — single sign-on (SSO),
   MFA mandatory, joiner/leaver process
5. Data classification — what's personal data, what's audit
   working paper, what's marketing data
6. Backup, retention, disaster recovery policies
7. Vendor risk assessment (DPA, SOC 2, GDPR posture)

OPERATIONS
8. Tier-1 support (in-firm IT lead or MSP)
9. Change management for releases
10. Training on new features
11. Integration monitoring (broken bank feeds, failed API syncs)

STRATEGY
12. Annual rationalisation review — eliminate redundancy
13. Architecture roadmap — what's the "system of record" for
    each data class
14. AI strategy — which workflows to automate, governance,
    risk acceptance
15. M&A integration planning if acquiring or being acquired

SECURITY
16. Cyber Essentials (UK government baseline) or Cyber
    Essentials Plus
17. Penetration testing (annual minimum for mid-tier+)
18. Phishing simulation training
19. Incident response plan and rehearsal
```

**5. Outputs & filings**
- Stack register / asset inventory
- Integration architecture diagram
- Data classification map
- Annual security & DPIA documentation
- Cyber Essentials / ISO 27001 certification (if pursued)
- Vendor contract register
- Incident log
- *(Filings to ICO if breaches require; otherwise internal)*

**6. Roles & handoffs**
- **Tech / Operations partner** — strategy, owns the budget
- **IT lead / MSP** — day-to-day operations and security
- **Practice manager** — adoption and process integration
- **DPO** — data governance overlay (links to B6)
- **All staff** — adoption, password hygiene, phishing awareness
- **Vendor account managers** — renewal, escalations, roadmap

For SMB practices (<25 staff), B9 is typically run by a **single principal + outsourced MSP** (e.g. Practice Protect, OGL, BCN, local IT firms). For mid-tier+, an in-house IT team with 3–10 staff plus an architect.

**7. Deadlines, risks & penalties**
- No statutory deadlines specific to B9, but several adjacent:
  - GDPR breach reporting (72 hours)
  - PII insurance cyber requirements (MFA, encryption, training)
  - HMRC/ICAEW/ACCA expectation of "appropriate" technology controls
  - PCI-DSS if processing cards (most firms outsource via Stripe/GoCardless)
- Risks:
  - **Tool sprawl** — 30+ SaaS apps, no integration, duplicate data
  - **Vendor lock-in** — switching cost on a deeply embedded PM tool can be 6–12 months
  - **Cyber attack** — mid-2020s saw multiple ransomware events at UK practices; cost £100k–£1m+ each
  - **AI governance gap** — staff using public LLMs with client data → confidentiality breach
  - **Vendor risk** — single-vendor dependency where the vendor pivots, gets acquired, or exits
  - **MTD readiness gap** — firms not on cloud bookkeeping by April 2026 mass-onboarding face a structural disadvantage
  - **Architecture debt** — every layered tool adds maintenance load

**8. Tools & software (about tooling itself)**
- **Stack management:** Zluri, BetterCloud, Productiv (enterprise); spreadsheet for SMB
- **SSO / IAM:** Microsoft 365 / Entra, Google Workspace, Okta, JumpCloud
- **Endpoint security:** Microsoft Defender, Sophos, CrowdStrike (mid-tier+)
- **MSP wrappers for accountancy:** Practice Protect, BCN, CyberSmart
- **Backup:** Backupify, Datto, Acronis
- **Penetration testing:** specialist firms (NCC Group, Pentest People)
- **AI governance:** Microsoft Copilot for M365 (most common), proprietary RAG implementations (mid-tier+), guardrails like Lakera, Robust Intelligence
- **Integration:** native APIs, Zapier / Make for lightweight, Mulesoft / Boomi for enterprise

**9. Economics**
- Direct cost: typically **3–7% of revenue** in tech stack + IT operations for a digitally-native UK SMB practice; up to 10%+ in firms actively transforming
- Per-FTE benchmark: roughly **£3,000–£8,000 per fee earner per year** in software + IT
- Hidden waste: typical SMB firm has 20–40% redundancy (overlapping tools, unused seats, expired contracts)
- AI ROI: early case studies show 15–40% time savings on prep work; the savings only **convert to profit if pricing is held constant** — many firms have already passed the savings to clients
- Strategic dial: the firms most likely to **double in size by 2030** are those that consolidate to a coherent architecture in 2026–27 and absorb agentic AI inside it. The firms most likely to be **acquired at a discount** are those running a sprawling stack of point tools with manual data passing.

**10. Client experience**
B9 is invisible **when it works**. Clients feel it as:
- Single login to a portal
- Documents arriving in the right place automatically
- No "the system is down today, please email it instead"
- Real-time data — month-end pack visible the day it's posted
- AI-enabled responsiveness (queries answered faster because staff are using AI tooling internally)

When it fails: bank feeds break repeatedly, the portal asks for the document a second time because it didn't sync, a phishing email goes out on the firm's domain. **Trust evaporates faster on technology failures than on technical errors** — clients read "they don't have their tech together" as "they don't have themselves together."

---

### B10 — Practice Finance (the firm's own P&L)

The **firm running its own books** — the often-quietly-ironic function where accountants apply to themselves the discipline they sell. B10 is where partner remuneration is determined, where capital decisions get made, where the firm's own VAT and CT are filed, and where the fundamental questions — "are we as profitable as our peers?", "should we acquire or merge?", "what's the firm worth?" — get answered.

**1. Purpose** — Run the firm itself as a financially disciplined business — managing its own P&L, balance sheet, cash, capital, and tax; remunerating partners and staff; benchmarking against peer firms; and producing the data needed for strategic decisions about growth, investment, succession, and ownership change.

**2. Trigger & cadence**
- **Daily / weekly:** cashflow, partner draws, lock-up monitoring
- **Monthly:** firm management accounts (yes — for the firm itself)
- **Quarterly:** partner profit allocations, KPI reviews, peer benchmarking
- **Annual:** budget, partner appointments / equity changes, firm year-end accounts, partner tax planning, salary/promotion rounds
- **Periodic:** firm valuation (succession events, refinancing, M&A discussions)

**3. Inputs**
- Fee income data (from B3)
- People cost (salaries, partner draws, pensions, NICs)
- Property, IT, marketing, professional indemnity
- Lock-up data (WIP + debtors)
- Capital accounts of partners
- Tax position of the firm and partners individually
- Bank facilities and capital structure
- Peer benchmarking data (ICAEW survey, Crowe Practice Track, AccountancyAge Top 50+50)

**4. Process steps**
```
ROUTINE
1. Bookkeeping — the firm's own ledger (often run on Xero)
2. Monthly close + management accounts for partners
3. Partner draws administered (typically monthly fixed +
   quarterly profit catchup)
4. VAT, payroll, CT for the firm itself
5. PII, body subscription, ICO renewals scheduled

CAPITAL & PARTNERS
6. Partner capital contributions tracked
7. New partner buy-in arrangements
8. Retiring partner buy-out — annuity or capital settlement
9. Equity / fixed-share / salaried partner classification

PROFIT ALLOCATION
10. Profit-sharing model applied:
    - Lockstep (seniority-based)
    - Modified lockstep with performance overlay
    - Performance / origination based ("eat what you kill")
    - Hybrid (most UK SMB firms)
11. Partner appraisals translated into points / shares
12. Quarterly distributions; year-end true-up

STRATEGY
13. Annual budget and target setting
14. Fee growth, headcount, capex plans
15. Acquisition appraisal (target firm DD)
16. Refinancing / banking facilities
17. Firm valuation refresh (if succession active or M&A)

REPORTING
18. Internal partner pack (MI, KPIs, peer benchmarks)
19. Statutory accounts of the firm (LLP / Ltd / partnership)
20. Tax returns for firm and partners
```

**5. Outputs & filings**
- **Internal:** monthly partner pack, KPIs, peer comparison
- **Statutory (LLP):** LLP accounts at Companies House (within 9 months); LLP self-assessment return; member SA returns
- **Statutory (Ltd):** company accounts + CT600
- **Statutory (partnership):** Partnership SA800; partner SA returns
- **VAT:** firm's quarterly returns
- **Payroll:** firm's RTI on its employees
- **PII / body returns:** as B6
- **Valuation reports** — for succession, M&A, partner buy-in

**6. Roles & handoffs**
- **Managing partner** — sets strategy
- **Finance partner / FD of the firm** — runs B10 day-to-day; in larger firms a permanent (often qualified-but-non-equity) role
- **Practice / billing manager** — links to B3
- **External advisers** — banker, wealth manager, M&A adviser, the firm's *own* tax adviser (yes, firms hire other firms for their own complex tax)
- **All partners** — receive MI, vote on capital decisions

It's strikingly common for **mid-tier UK firms to have less rigorous internal MI than they sell to clients** — the cobbler's children syndrome. PE buyers consistently identify this as the most fixable upside.

**7. Deadlines, risks & penalties**
- All standard external deadlines apply to the firm itself (statutory accounts, CT, VAT, payroll, etc.)
- **Member tax timing** for LLPs: members pay tax on profit *allocated*, not *drawn* — January and July payments-on-account can hurt cashflow if not budgeted
- Risks:
  - **Cashflow stress** — partners drawing ahead of profit; lock-up over 90 days = chronic liquidity issue
  - **Partner disputes** — profit-sharing perceived unfair; succession terms unfunded
  - **Succession failure** — retiring partner equity not affordable to remaining partners
  - **Banking covenant breach** — common where firms borrow to fund partner buy-outs
  - **PII tail risk** — uncrystallised liabilities sit on the balance sheet for years
  - **Acquisition mis-pricing** — buying a "GRF £1m" firm at 1.2× ignoring lock-up, attrition, and earn-out structure can destroy value

**8. Tools & software**
- **Firm's ledger:** Xero or QBO (most common); IRIS Practice Management embedded option for mid-tier
- **Time / billing:** B3 stack feeds in
- **MI / dashboarding:** Power BI, Fathom, Spotlight (the firm dogfoods what it sells to clients)
- **Forecasting / modelling:** Excel + Float / Brixx
- **Partnership equity admin:** custom spreadsheets dominate; specialist (Diligent, Filevine) at top tier
- **Benchmarking sources:** ICAEW Profitability and Recovery Survey, Crowe Practice Track, AccountancyAge Top 50+50, peer-firm data via Firmology

**9. Economics — the practice's own dials**
Typical UK SMB / mid-tier benchmarks (drawn from recent surveys):

| Metric | Median benchmark | Comments |
|---|---|---|
| **People cost** | 45–55% of fee income | The dominant line |
| **Property + IT + admin** | 15–22% of fee income | Trending down on remote/hybrid |
| **Marketing** | 2–6% of fee income | Higher in growth firms |
| **PII + regulatory** | 2–5% of fee income | Audit firms higher |
| **Net profit margin (pre-partner)** | 25–35% | Top quartile 35–45% |
| **Profit per equity partner (PEP)** | **~£253k median** (recent UK survey) | Top quartile £400k+ |
| **Fee income per equity partner** | **~£901k median** | Top quartile £1.3m+ |
| **Lock-up days** | 60–90 average; <45 best | The biggest cash lever |
| **Fee growth** | 5–8% organic; 15–25% with bolt-on M&A | |
| **Practice valuation** | **1.0–1.4× GRF** historically; **5–8× EBITDA** in PE-backed deals | Plateaued in 2026 after multi-year rise |

**10. Client experience (indirect)**
Clients don't see B10. But they feel its consequences:
- A **financially healthy firm** invests in tech, training, partner succession, and proactive advisory — the client experience improves
- A **financially stressed firm** under-invests, cuts marketing, loses partners, runs hot on capacity, and the client experience visibly degrades
- A **PE-acquired firm** typically goes through a 12–24 month period where the client *can* feel the change — service standardisation, fee normalisation, sometimes new technology stack, sometimes loss of long-tenured staff. Done well, the client gains capability; done badly, the client gains a sales call.

The longest-tenured client relationships outlast multiple iterations of B10 — partners retire, ownership changes, technology reshapes — but the **named manager and the named partner remain the relationship anchor**. B10's job is to ensure those people still want to be at the firm next year.

---

## 7. Synthesis — The Working Model in One View

The 22 sections above resolve into **three loops, three layers, three economic engines**.

### Three loops

The practice is not a sequence — it's three concurrent loops that feed each other:

```
COMPLIANCE LOOP        ADVISORY LOOP             PRACTICE LOOP
(deadline-driven)      (event/relationship)      (firm itself)

A1 Bookkeeping         A10 Advisory              B1 Onboarding
A2 Mgmt accounts       A11 Virtual CFO           B2 Workflow
A3 VAT                                           B3 Time/billing
A4 Payroll             "What should I do?"       B4 Documents
A5 Year-end                                      B5 Comms
A6 CT                                            B6 Quality/risk
A7 SA / MTD-IT                                   B7 People
A8 Specialist                                    B8 BD / pricing
A9 Audit                                         B9 Tech
A12 Co-Sec                                       B10 Practice finance

"Did we file?"         ◄── feeds ──              "Can we deliver?"
```

The compliance loop produces the data that fuels the advisory loop. The practice loop is the chassis both ride on.

### Three layers

1. **The client's regulatory perimeter** — HMRC, Companies House, TPR, ICO, FCA (where relevant). The practice acts as the client's interface to all of these.
2. **The firm's own regulatory perimeter** — ICAEW/ACCA, AML supervisor, FRC (if audit), ICO, PII insurer. The practice operates inside this perimeter.
3. **The commercial perimeter** — clients, prospects, capital, software vendors, talent market. The practice competes here.

A senior partner spends time across all three; a junior usually only across the first.

### Three economic engines

1. **The recurring-fee annuity** (A1–A7, A12) — predictable, deadline-driven, increasingly automated, **commoditising on price**. This is the **valuation backbone**.
2. **The advisory/specialist margin** (A8, A10, A11) — relationship-led, value-priced, partner-dependent, **the strategic upside**. This is the **valuation multiplier**.
3. **The practice itself as a business** (B1–B10) — needs the same operational discipline and capital structure as any professional services firm. **Done well, it's the operating leverage that makes engines 1 and 2 actually pay.**

### The ten things that determine whether a firm wins or stalls

1. **Niche clarity** — what kind of SMB is this firm built for?
2. **Productisation** — three packages, named, scoped, priced
3. **Subscription billing on Direct Debit** — kills lock-up
4. **Cloud-first bookkeeping** — the data foundation
5. **A real practice-management system** — Karbon/TaxDome/etc. with 100% adoption
6. **One named manager per client** — the relationship anchor
7. **Proactive advisory cadence** — partner calls in October, not January
8. **A trainee pipeline that's bigger than retirement attrition**
9. **Functioning quality, AML, and PII discipline** — the operating licence
10. **Disciplined practice-finance MI** — knowing each client's profitability, lock-up, NPS

The firms that will be worth the most in 2030 are not the firms with the cleverest tax planning — they're the firms that do the above ten things consistently, and let the cleverness compound on top.

---

## 8. A Candidate Domain Model

A starting frame — architectural level, not field-level — for translating the working model above into a clean-architecture .NET system.

### 8.1 Bounded Contexts

Six contexts cleanly absorb the 22 functional areas, each with its own ubiquitous language and lifecycle.

```
┌───────────────────────────────────────────────────────────────┐
│                                                               │
│   ┌────────────────────┐      ┌──────────────────────────┐    │
│   │ ClientRelationship │◄────►│  EngagementWorkflow      │    │
│   │   (B1, B5)         │      │   (B2, B4)               │    │
│   └─────────┬──────────┘      └─────────────┬────────────┘    │
│             │                               │                 │
│             ▼                               ▼                 │
│   ┌────────────────────┐      ┌──────────────────────────┐    │
│   │ ComplianceServices │      │  AdvisoryServices        │    │
│   │ (A1,A3-A9,A12,A8)  │      │   (A2, A10, A11)         │    │
│   └─────────┬──────────┘      └─────────────┬────────────┘    │
│             │                               │                 │
│             └─────────────┬─────────────────┘                 │
│                           ▼                                   │
│              ┌──────────────────────────────┐                 │
│              │   BillingAndCash (B3)        │                 │
│              └──────────────┬───────────────┘                 │
│                             │                                 │
│              ┌──────────────▼───────────────┐                 │
│              │ PracticeOperations            │                │
│              │ (B6, B7, B8, B9, B10)         │                │
│              └───────────────────────────────┘                │
└───────────────────────────────────────────────────────────────┘
```

**Why this split (and not service-line-per-context)?**
- The 10 client-facing service lines (A1–A12) share **far more model than they differ** — almost all of them are "a recurring or event-driven Job, that consumes Documents, produces Filings, and bills via Engagement." Treating each as a separate context creates duplication. Instead, one `ComplianceServices` context with a polymorphic `Filing` aggregate and discriminated subtypes is cleaner.
- `AdvisoryServices` is genuinely different — non-recurring, value-priced, narrative-output, partner-led — so it earns its own context.
- `PracticeOperations` is internal-only and never crosses the wire to clients; isolating it lets you secure it differently.

### 8.2 Aggregate Roots & Key Entities

**Context: ClientRelationship** *(B1, B5)*

| Aggregate root | Notable entities / value objects | Key behaviours |
|---|---|---|
| **Client** | `BeneficialOwner`, `Contact`, `RiskAssessment`, `OngoingMonitoringSchedule`, `IdVerification` | Onboard, riskAssess (low/med/high), enhanceDD, periodicReview, archive |
| **Communication** | `Message`, `Query`, `Channel`, `SlaClock` | Open, assign, respond, close, age, escalate |
| **Complaint** | `ComplaintStage`, `Resolution` | Lodge, investigate, resolve, log to regulator |

`Client` here owns identity, KYC, and risk — it deliberately does **not** own the engagement.

**Context: EngagementWorkflow** *(B2, B4)*

| Aggregate root | Notable entities | Key behaviours |
|---|---|---|
| **Engagement** | `EngagementLetter` (signed), `Scope`, `FeeAgreement`, `LiabilityCap` | Propose, sign, vary, terminate |
| **Job** | `Task`, `Owner`, `ReviewerAssignment`, `Deadline`, `Status` | Schedule, assign, progress, review, complete, archive |
| **RecurringJobSchedule** | `Cadence`, `Trigger`, `Template` | Generate next instance, pause, retire |
| **Document** | `Version`, `ClassificationTag`, `RetentionPolicy`, `SignatureRequest` | Ingest, classify, version, request signature, retain, destroy |

`Engagement` is the contract; `Job` is the work; `Document` is the evidence. The `Job` aggregate is the **busiest write target** in the whole system.

**Context: ComplianceServices** *(A1, A3–A9, A12)*

| Aggregate root | Notable entities | Key behaviours |
|---|---|---|
| **Filing** *(abstract / discriminated)* | `FilingPeriod`, `Submission`, `ApprovalRecord`, `Attachment` | Prepare, review, approveByClient, submit, recordReceipt, amend |
| `VatReturn : Filing` | `VatScheme`, `Box1..Box9`, `DigitalLink` | runReasonablenessChecks, applyScheme |
| `PayrollRun : Filing` | `EmployeeSnapshot`, `Fps`, `Eps`, `PensionContribution` | calculateGrossToNet, generateBacs, fileFps |
| `YearEndAccounts : Filing` | `TrialBalance`, `LeadSchedule`, `Disclosure` | rollForward, reviewAnalytical, finalise |
| `CorporationTaxReturn : Filing` | `TaxComputation`, `MarginalReliefCalc`, `RdClaim` | computeLiability, applyReliefs |
| `SelfAssessmentReturn : Filing` | `IncomeSource`, `Deduction`, `MtdQuarterlyUpdate` | reconcileAcrossSources, fileFinalDeclaration |
| `ConfirmationStatement : Filing` | `StatementOfCapital`, `PscDelta` | confirm, file |
| `CisReturn : Filing` | `SubcontractorPayment`, `Verification`, `Deduction` | verifySubbie, calculateDeduction |
| **Ledger** *(reference only — owned externally by Xero/QBO)* | `Account`, `Transaction`, `Reconciliation` | Read-only projection; firm doesn't own it |

`Filing` is the abstraction giving you a single dashboard, single workflow plug-in to `Job`, single query interface, while each subtype encapsulates its own regime-specific rules. The `Ledger` is intentionally an **anti-corruption layer** over Xero/QBO — you do not want Xero's data model leaking into your domain.

**Context: AdvisoryServices** *(A2, A10, A11)*

| Aggregate root | Notable entities | Key behaviours |
|---|---|---|
| **AdvisoryEngagement** | `Brief`, `WorkPlan`, `Deliverable`, `ActionLog` | Scope, plan, deliver, capture decisions |
| **ManagementPack** | `PeriodKpi`, `VarianceCommentary`, `ForecastSnapshot` | Compose, comment, publish, reviewMeeting |
| **VirtualCfoRetainer** | `MonthlyCadence`, `BoardMeeting`, `Reforecast` | Run cycle, escalate event work |

`Filing` is regulator-facing and rule-driven; `AdvisoryEngagement` is client-facing and outcome-driven. Different invariants, different lifecycles, different review patterns.

**Context: BillingAndCash** *(B3)*

| Aggregate root | Notable entities | Key behaviours |
|---|---|---|
| **TimeEntry** | `FeeEarner`, `ChargeableFlag`, `ChargeOutRate` | Log, scrub, lock |
| **WipBalance** | (per Job + per Engagement) | Accumulate, writeDown, billOut |
| **Invoice** | `LineItem`, `VatAmount`, `Disbursement` | Draft, approve, issue, credit |
| **Subscription** | `Tier`, `RecurrenceRule`, `Mandate` | Activate, change tier, suspend |
| **Payment** | `Method` (DD, BACS, card), `MatchedInvoice` | Collect, allocate, refund |
| **AgedDebtor** | (projection) | Chase, escalate, write-off |

**Lock-up** (the headline KPI) is a derived projection over `WipBalance + AgedDebtor`, not a stored field — keeps the truth in the source.

**Context: PracticeOperations** *(B6, B7, B8, B9, B10)*

This is closer to **five small bounded contexts than one** — and likely the part you'd build last (or buy from off-the-shelf HR/CRM tooling rather than build).

| Sub-context | Aggregate roots | Notes |
|---|---|---|
| **QualityAndRisk** | `IsqmEvaluation`, `FileReview`, `IndependenceDeclaration`, `BreachRecord`, `ConflictCheck` | Internal-only; integrates to `Engagement` for acceptance/continuance gates |
| **People** | `Staff`, `CpdRecord`, `Appraisal`, `CapacityPlan`, `PractisingCertificate` | Provides capacity into `EngagementWorkflow` |
| **GrowthAndPricing** | `Prospect`, `Proposal`, `ServicePackage`, `PriceList`, `WinLossRecord` | Hands off to `Engagement` on signature |
| **PlatformAndIntegration** | `IntegrationConnection`, `SecretRotation`, `WebhookSubscription`, `AuditLog` | Cross-cutting; could be a shared kernel |
| **PracticeFinance** | `FirmLedger`, `PartnerCapitalAccount`, `ProfitAllocation`, `Benchmark` | The firm's *own* P&L |

### 8.3 Cross-Cutting Concerns

Some concepts cut across every context — model these as a **shared kernel** carefully, not as accidental coupling:

- **Money** value object (currency-aware, never a `decimal`)
- **TaxYear / FilingPeriod** value objects (UK fiscal calendar, with stagger groups for VAT)
- **DeadlineCalendar** (HMRC + Companies House + TPR + ICO clocks)
- **Identity** (the firm staff identity, federated; client identities verified separately)
- **AuditLog** (every state change everywhere — mandatory for B6)
- **PiiClassifier** (every entity tagged for retention + GDPR)

### 8.4 Domain Events Worth Modelling Explicitly

Several events fan out across contexts and are good candidates for an event-driven seam (in-process MediatR notifications or an event bus):

- `ClientOnboarded` → triggers recurring-job generation in `EngagementWorkflow`, opens a `WipBalance`, creates a `Subscription`
- `EngagementSigned` → unlocks job creation, records in `QualityAndRisk` acceptance log
- `FilingSubmitted` → updates client portal, generates invoice trigger if billable, archives docs
- `DocumentSigned` → may trigger downstream filing readiness
- `JobCompleted` → triggers billing review, frees capacity in `People`
- `RiskAssessmentEscalated` → blocks further work pending senior sign-off
- `ClientChurned` → triggers retention archival, document destruction schedule, capacity release

### 8.5 Suggested Implementation Sequence

If this is a clean-architecture .NET build, sequence the contexts roughly **inside-out** — build the spine that everything else needs first:

1. **EngagementWorkflow** core (Engagement + Job + RecurringJobSchedule + Document) — the spine
2. **ClientRelationship** (Client + RiskAssessment + Communication) — the entity that everything hangs off
3. **BillingAndCash** (TimeEntry + Invoice + Subscription) — needed before any service line goes live
4. **ComplianceServices** — start with **one filing type** end-to-end. **VAT** is the recommended first cut: high cadence, well-defined rules, clean integration with Xero, MTD already mature. Then add others as `Filing` subtypes.
5. **AdvisoryServices** (ManagementPack first, since it's the most data-driven)
6. **PracticeOperations** sub-contexts as they become bottlenecks — Quality first (it gates Engagement), then People, then Growth, etc.

Each step gets its own slice of Domain → Application → Persistence → API, vertically — not a horizontal "build all entities, then all repos, then all controllers" approach.

### 8.6 Strategic Architecture Decisions to Make Early

Things that are very expensive to change later:

- **Multi-tenant model.** Single firm? Multiple firms in one instance? Determines auth, data isolation, schema design. *(For an internal practice tool, single-tenant is simpler; a SaaS platform is multi-tenant from day one.)*
- **Source-of-truth for the ledger.** Are you the ledger (compete with Xero)? Or do you orchestrate over Xero/QBO (anti-corruption layer)? The domain model assumes the latter — much smaller scope.
- **Where filings are computed.** In-house engine vs. integration to a third-party engine (TaxCalc, IRIS, Capium). Massively different effort.
- **Identity & AML.** Build vs. integrate to SmartSearch/Veriphy/Credas. Almost always integrate.
- **Audit log fidelity.** Event-sourced vs. snapshot-with-deltas. Regulatory pressure favours full event sourcing in `Filing` and `Document` aggregates.
- **AI seam.** Where do agentic AI features plug in? A clean port pattern (`IDraftSuggestionService`, `IDocumentClassifier`) lets you swap providers and models without rippling changes.

---

## Where to take this next

This document is a starting point, not a destination. Three natural next moves:

1. **Drill into one context** — flesh out `ComplianceServices` or `EngagementWorkflow` to entity/property/method level so you can start coding it.
2. **Decide the strategic architecture questions in §8.6** — structure the trade-offs for each.
3. **Validate against existing products** — map this model against IRIS / Karbon / TaxDome data shapes to see where you'd compete vs. integrate.
4. **Start scaffolding** — invoke the `clean-architecture` skill and lay down the .NET solution structure for the spine (Engagement + Client + one Filing subtype).

---

*End of document.*

