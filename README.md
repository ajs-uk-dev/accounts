# Accounts — SaaS for UK SME Accountancy Practices

Multi-tenant practice-management SaaS. See `docs/superpowers/specs/2026-05-10-saas-functional-requirements-design.md` for full FR spec and `PROJECT-STATE.md` for current status.

## Local development

```bash
docker compose -f docker/docker-compose.yml up -d
dotnet restore
dotnet build
dotnet test
cd client/accounts-web && npm install && npm run dev
```

## Solution layout

- `src/Shared/` — SharedKernel
- `src/PracticeOperations/` — first bounded context (users, firms, audit)
- `src/Web/` — ASP.NET Core composition root
- `client/accounts-web/` — React + TS frontend

Architecture: selective DDD + 4 bounded contexts (PracticeOperations, ClientRelationship, EngagementAndCompliance, BillingAndCash) + vertical slices per context. Only PracticeOperations is materialised at Foundation Core; the others come online in later sub-plans.
