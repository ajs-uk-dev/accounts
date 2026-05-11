# Foundation Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the multi-tenant foundation — solution scaffold, PostgreSQL + EF Core, FirmId-scoped tenancy with global query filters, append-only audit log, email/password + TOTP authentication with role-based authorization, a React + TypeScript admin shell, structured observability, Dockerised dev environment, and CI — proven by an end-to-end test where a firm registers, signs in with MFA, and lands on a tenant-scoped admin page.

**Architecture:** Selective DDD with bounded contexts as solution folders. This plan materialises one production context — `PracticeOperations` — plus a `SharedKernel`. Each context follows Clean Architecture (Domain → Application → Infrastructure) and registers its HTTP endpoints into the `Accounts.Web` composition root via extension methods. Inside each context we use **vertical slices** (one folder per use case in the Application project) with MediatR for command/query handling and pipeline behaviors for cross-cutting concerns (validation, auditing). Multi-tenancy is enforced at the EF Core layer via a global query filter on `ITenantScopedEntity`; tenant context flows in from a JWT claim through middleware. The audit log is append-only by construction — the `EfAuditWriter` is the only writer to the table and `SaveChangesAsync` is overridden to reject modifications/deletions of `AuditEvent`. Domain events are dispatched in-process via MediatR after `SaveChangesAsync` succeeds.

**Tech Stack:** .NET 10 (C# 14) · ASP.NET Core Minimal APIs · EF Core 10 · PostgreSQL 16 (Testcontainers for tests) · MediatR · FluentValidation · `Microsoft.AspNetCore.Identity` (for `IPasswordHasher<T>` only) · `Otp.NET` (TOTP) · `Microsoft.IdentityModel.Tokens` + `System.IdentityModel.Tokens.Jwt` · Serilog · OpenTelemetry · xUnit · FluentAssertions · `Microsoft.AspNetCore.Mvc.Testing` · `Testcontainers.PostgreSql` · Vite 5 · React 19 · TypeScript 5 · React Router 7 · TanStack Query · Tailwind CSS · shadcn/ui · Vitest · Playwright · GitHub Actions

**Out of scope (covered by later sub-plans):** SSO (1b), integration-framework skeleton (1b), data-class tagging / retention (1b), client-portal magic-link + WebAuthn auth (Sub-plan 4), any business domain (Sub-plans 2–7), AI gateway (Sub-plan 8).

---

## File Structure

```
Accounts/
├── .editorconfig
├── .gitignore
├── Directory.Build.props                # central .NET version, langversion, analyzers
├── Directory.Packages.props             # central package management (CPM)
├── Accounts.sln
├── README.md
├── docker/
│   └── docker-compose.yml               # postgres + seq for local dev
├── .github/workflows/
│   ├── backend.yml
│   ├── frontend.yml
│   └── e2e.yml
├── docs/superpowers/plans/2026-05-11-foundation-core.md   # this file
├── src/
│   ├── Shared/
│   │   ├── Accounts.SharedKernel/
│   │   │   ├── Domain/
│   │   │   │   ├── Entity.cs                    # base entity with Id + timestamps
│   │   │   │   ├── AggregateRoot.cs             # base aggregate with domain events
│   │   │   │   ├── ValueObject.cs               # base value object
│   │   │   │   ├── IDomainEvent.cs
│   │   │   │   └── ITenantScopedEntity.cs       # marker for FirmId scoping
│   │   │   ├── Identity/FirmId.cs               # strongly-typed tenant identifier
│   │   │   ├── Identity/UserId.cs
│   │   │   ├── Time/IClock.cs
│   │   │   ├── Time/SystemClock.cs
│   │   │   └── Results/Result.cs                # Result<T> + DomainError
│   │   └── Accounts.SharedKernel.Tests/
│   ├── PracticeOperations/
│   │   ├── Accounts.PracticeOperations.Domain/
│   │   │   ├── Firms/
│   │   │   │   ├── Firm.cs                      # aggregate root
│   │   │   │   ├── FirmStatus.cs
│   │   │   │   └── Events/FirmRegistered.cs
│   │   │   ├── Users/
│   │   │   │   ├── User.cs                      # aggregate root
│   │   │   │   ├── EmailAddress.cs              # value object
│   │   │   │   ├── Role.cs
│   │   │   │   ├── UserStatus.cs
│   │   │   │   └── Events/UserRegistered.cs
│   │   │   └── Audit/
│   │   │       ├── AuditEvent.cs                # append-only
│   │   │       └── AuditAction.cs
│   │   ├── Accounts.PracticeOperations.Application/
│   │   │   ├── Abstractions/
│   │   │   │   ├── IFirmContext.cs              # ambient FirmId+UserId
│   │   │   │   ├── IAuditWriter.cs
│   │   │   │   ├── IFirmRepository.cs
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── IPasswordHasher.cs
│   │   │   │   ├── ITotpService.cs
│   │   │   │   ├── IJwtIssuer.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── AuditingBehavior.cs
│   │   │   ├── Firms/Register/
│   │   │   │   ├── RegisterFirmCommand.cs
│   │   │   │   ├── RegisterFirmHandler.cs
│   │   │   │   └── RegisterFirmValidator.cs
│   │   │   └── Users/
│   │   │       ├── SignIn/
│   │   │       │   ├── SignInCommand.cs
│   │   │       │   ├── SignInHandler.cs
│   │   │       │   └── SignInValidator.cs
│   │   │       └── EnrollTotp/
│   │   │           ├── EnrollTotpCommand.cs
│   │   │           └── EnrollTotpHandler.cs
│   │   ├── Accounts.PracticeOperations.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── PracticeOperationsDbContext.cs
│   │   │   │   ├── Configurations/
│   │   │   │   │   ├── FirmConfiguration.cs
│   │   │   │   │   ├── UserConfiguration.cs
│   │   │   │   │   └── AuditEventConfiguration.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── EfFirmRepository.cs
│   │   │   │   │   ├── EfUserRepository.cs
│   │   │   │   │   └── EfUnitOfWork.cs
│   │   │   │   └── Migrations/                  # generated
│   │   │   ├── Auth/
│   │   │   │   ├── Pbkdf2PasswordHasher.cs
│   │   │   │   ├── OtpNetTotpService.cs
│   │   │   │   └── JwtIssuer.cs
│   │   │   ├── Audit/EfAuditWriter.cs
│   │   │   ├── Endpoints/
│   │   │   │   ├── FirmsEndpoints.cs
│   │   │   │   ├── AuthEndpoints.cs
│   │   │   │   ├── AdminEndpoints.cs
│   │   │   │   └── EndpointRouteBuilderExtensions.cs
│   │   │   └── DependencyInjection.cs
│   │   ├── Accounts.PracticeOperations.UnitTests/
│   │   │   ├── Domain/
│   │   │   │   ├── FirmTests.cs
│   │   │   │   ├── UserTests.cs
│   │   │   │   └── EmailAddressTests.cs
│   │   │   └── Application/
│   │   │       ├── RegisterFirmHandlerTests.cs
│   │   │       └── SignInHandlerTests.cs
│   │   └── Accounts.PracticeOperations.IntegrationTests/
│   │       ├── Fixtures/
│   │       │   ├── PostgresFixture.cs
│   │       │   └── ApiFactory.cs
│   │       ├── HealthCheckTests.cs
│   │       ├── TenantIsolationTests.cs
│   │       ├── AuditLogTests.cs
│   │       ├── RegisterFirmEndpointTests.cs
│   │       ├── SignInEndpointTests.cs
│   │       └── AuthorizationTests.cs
│   └── Web/
│       └── Accounts.Web/
│           ├── Program.cs                       # composition root
│           ├── Middleware/
│           │   ├── CorrelationIdMiddleware.cs
│           │   └── TenantResolutionMiddleware.cs
│           ├── Auth/FirmContextAccessor.cs      # IFirmContext impl
│           ├── Observability/
│           │   ├── SerilogConfig.cs
│           │   └── OpenTelemetryConfig.cs
│           ├── appsettings.json
│           ├── appsettings.Development.json
│           └── Dockerfile
└── client/
    └── accounts-web/
        ├── package.json
        ├── tsconfig.json
        ├── vite.config.ts
        ├── tailwind.config.ts
        ├── postcss.config.js
        ├── index.html
        ├── Dockerfile
        ├── src/
        │   ├── main.tsx
        │   ├── App.tsx
        │   ├── lib/
        │   │   ├── api.ts
        │   │   ├── auth.tsx
        │   │   └── routes.ts
        │   ├── components/
        │   │   ├── ProtectedRoute.tsx
        │   │   ├── TopNav.tsx
        │   │   └── ui/                           # shadcn-generated
        │   ├── pages/
        │   │   ├── RegisterPage.tsx
        │   │   ├── SignInPage.tsx
        │   │   ├── EnrollTotpPage.tsx
        │   │   └── DashboardPage.tsx
        │   └── styles/globals.css
        ├── tests/unit/                           # vitest
        └── e2e/signin.spec.ts                    # playwright
```

---

## Phase 1: Bootstrap

### Task 1: Initialize repository and solution

**Files:**
- Create: `C:/Repos/Claude/Accounts/.gitignore`
- Create: `C:/Repos/Claude/Accounts/.editorconfig`
- Create: `C:/Repos/Claude/Accounts/Directory.Build.props`
- Create: `C:/Repos/Claude/Accounts/Directory.Packages.props`
- Create: `C:/Repos/Claude/Accounts/Accounts.sln`
- Create: `C:/Repos/Claude/Accounts/README.md`

- [ ] **Step 1: Initialise git (if not already)**

Run from `C:/Repos/Claude/Accounts`:
```powershell
if (-not (Test-Path .git)) { git init -b main }
```
Expected: either nothing (already a repo) or "Initialized empty Git repository...".

- [ ] **Step 2: Write `.gitignore`**

```gitignore
# .NET
bin/
obj/
*.user
.vs/
TestResults/

# Node
node_modules/
dist/
.vite/
.turbo/
*.log

# OS
.DS_Store
Thumbs.db

# IDE
.idea/
*.swp
.vscode/*
!.vscode/launch.json
!.vscode/tasks.json

# Secrets
appsettings.*.local.json
.env
.env.local
.env.*.local

# Playwright
test-results/
playwright-report/
playwright/.cache/
```

- [ ] **Step 3: Write `.editorconfig`**

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.{ts,tsx,js,jsx,json,yml,yaml,md}]
indent_size = 2

[*.cs]
csharp_new_line_before_open_brace = all
dotnet_sort_system_directives_first = true
dotnet_style_namespace_match_folder = true:warning
```

- [ ] **Step 4: Write `Directory.Build.props`**

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
  </PropertyGroup>
</Project>
```

- [ ] **Step 5: Write `Directory.Packages.props`**

```xml
<Project>
  <ItemGroup>
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
    <PackageVersion Include="MediatR" Version="12.4.1" />
    <PackageVersion Include="FluentValidation" Version="11.10.0" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.10.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Identity" Version="10.0.0" />
    <PackageVersion Include="Microsoft.IdentityModel.Tokens" Version="8.2.0" />
    <PackageVersion Include="System.IdentityModel.Tokens.Jwt" Version="8.2.0" />
    <PackageVersion Include="Otp.NET" Version="1.4.0" />
    <PackageVersion Include="Serilog.AspNetCore" Version="9.0.0" />
    <PackageVersion Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageVersion Include="Serilog.Sinks.Seq" Version="9.0.0" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.10.0-beta.1" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.10.0" />
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.10.0" />
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.0" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="FluentAssertions" Version="7.0.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    <PackageVersion Include="Testcontainers.PostgreSql" Version="4.1.0" />
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 6: Write `README.md`**

```markdown
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
```

- [ ] **Step 7: Create empty solution**

```powershell
dotnet new sln -n Accounts -o C:/Repos/Claude/Accounts --force
```
Expected: `The template "Solution File" was created successfully.`

- [ ] **Step 8: Commit**

```powershell
git add -A
git commit -m "chore: bootstrap repo with solution, editorconfig, central package management"
```

---

### Task 2: Create SharedKernel and PracticeOperations projects

**Files:**
- Create: `src/Shared/Accounts.SharedKernel/Accounts.SharedKernel.csproj`
- Create: `src/Shared/Accounts.SharedKernel.Tests/Accounts.SharedKernel.Tests.csproj`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Accounts.PracticeOperations.Domain.csproj`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj`
- Create: `src/Web/Accounts.Web/Accounts.Web.csproj`

- [ ] **Step 1: Create projects**

```powershell
cd C:/Repos/Claude/Accounts

dotnet new classlib -n Accounts.SharedKernel -o src/Shared/Accounts.SharedKernel
dotnet new xunit    -n Accounts.SharedKernel.Tests -o src/Shared/Accounts.SharedKernel.Tests

dotnet new classlib -n Accounts.PracticeOperations.Domain         -o src/PracticeOperations/Accounts.PracticeOperations.Domain
dotnet new classlib -n Accounts.PracticeOperations.Application    -o src/PracticeOperations/Accounts.PracticeOperations.Application
dotnet new classlib -n Accounts.PracticeOperations.Infrastructure -o src/PracticeOperations/Accounts.PracticeOperations.Infrastructure
dotnet new xunit    -n Accounts.PracticeOperations.UnitTests       -o src/PracticeOperations/Accounts.PracticeOperations.UnitTests
dotnet new xunit    -n Accounts.PracticeOperations.IntegrationTests -o src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests

dotnet new web -n Accounts.Web -o src/Web/Accounts.Web --no-https=false
```

Expected: eight "The template was created successfully" messages.

- [ ] **Step 2: Add all projects to the solution**

```powershell
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln Accounts.sln add $_.FullName }
```
Expected: each project reports "Project ... added to the solution."

- [ ] **Step 3: Wire up project references (Clean Architecture)**

```powershell
# Domain depends on SharedKernel
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Domain/Accounts.PracticeOperations.Domain.csproj reference src/Shared/Accounts.SharedKernel/Accounts.SharedKernel.csproj

# Application depends on Domain
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj reference src/PracticeOperations/Accounts.PracticeOperations.Domain/Accounts.PracticeOperations.Domain.csproj

# Infrastructure depends on Application
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj reference src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj

# Web depends on Infrastructure of every context (today: just PracticeOperations)
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj reference src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj

# Test references
dotnet add src/Shared/Accounts.SharedKernel.Tests/Accounts.SharedKernel.Tests.csproj reference src/Shared/Accounts.SharedKernel/Accounts.SharedKernel.csproj
dotnet add src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj reference src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj
dotnet add src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj reference src/Web/Accounts.Web/Accounts.Web.csproj
```

- [ ] **Step 4: Delete template stubs**

```powershell
Remove-Item src/Shared/Accounts.SharedKernel/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/PracticeOperations/Accounts.PracticeOperations.Domain/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/PracticeOperations/Accounts.PracticeOperations.Application/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Class1.cs -ErrorAction SilentlyContinue
Remove-Item src/Shared/Accounts.SharedKernel.Tests/UnitTest1.cs -ErrorAction SilentlyContinue
Remove-Item src/PracticeOperations/Accounts.PracticeOperations.UnitTests/UnitTest1.cs -ErrorAction SilentlyContinue
Remove-Item src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/UnitTest1.cs -ErrorAction SilentlyContinue
```

- [ ] **Step 5: Verify build**

```powershell
dotnet build Accounts.sln
```
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "chore: scaffold SharedKernel + PracticeOperations Clean Architecture projects"
```

---

### Task 3: Add Docker Compose for local Postgres

**Files:**
- Create: `docker/docker-compose.yml`

- [ ] **Step 1: Write `docker/docker-compose.yml`**

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: accounts-postgres
    environment:
      POSTGRES_USER: accounts
      POSTGRES_PASSWORD: accounts_dev_password
      POSTGRES_DB: accounts_dev
    ports:
      - "5432:5432"
    volumes:
      - accounts-postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U accounts -d accounts_dev"]
      interval: 5s
      timeout: 5s
      retries: 10

  seq:
    image: datalust/seq:latest
    container_name: accounts-seq
    environment:
      ACCEPT_EULA: Y
      SEQ_FIRSTRUN_NOAUTHENTICATION: "true"   # local dev only; Seq 2025.2+ requires explicit opt-in
    ports:
      - "5341:80"
    volumes:
      - accounts-seq-data:/data

volumes:
  accounts-postgres-data:
  accounts-seq-data:
```

- [ ] **Step 2: Start the stack and verify**

```powershell
docker compose -f docker/docker-compose.yml up -d
docker compose -f docker/docker-compose.yml ps
```
Expected: `accounts-postgres` and `accounts-seq` both show `running (healthy)` or `running (starting)`.

- [ ] **Step 3: Smoke-test the DB connection**

```powershell
docker exec accounts-postgres psql -U accounts -d accounts_dev -c "SELECT version();"
```
Expected: a `PostgreSQL 16.x ...` row.

- [ ] **Step 4: Commit**

```powershell
git add -A
git commit -m "chore: add docker-compose for local postgres and seq"
```

---

### Task 4: SharedKernel — base types and `FirmId`

**Files:**
- Create: `src/Shared/Accounts.SharedKernel/Identity/FirmId.cs`
- Create: `src/Shared/Accounts.SharedKernel/Identity/UserId.cs`
- Create: `src/Shared/Accounts.SharedKernel/Domain/ITenantScopedEntity.cs`
- Create: `src/Shared/Accounts.SharedKernel/Domain/IDomainEvent.cs`
- Create: `src/Shared/Accounts.SharedKernel/Domain/Entity.cs`
- Create: `src/Shared/Accounts.SharedKernel/Domain/AggregateRoot.cs`
- Create: `src/Shared/Accounts.SharedKernel/Domain/ValueObject.cs`
- Create: `src/Shared/Accounts.SharedKernel/Time/IClock.cs`
- Create: `src/Shared/Accounts.SharedKernel/Time/SystemClock.cs`
- Create: `src/Shared/Accounts.SharedKernel/Results/Result.cs`
- Test: `src/Shared/Accounts.SharedKernel.Tests/FirmIdTests.cs`
- Test: `src/Shared/Accounts.SharedKernel.Tests/ResultTests.cs`

- [ ] **Step 1: Add FluentAssertions package to test project**

```powershell
dotnet add src/Shared/Accounts.SharedKernel.Tests/Accounts.SharedKernel.Tests.csproj package FluentAssertions
```

- [ ] **Step 2: Write the failing `FirmId` test**

`src/Shared/Accounts.SharedKernel.Tests/FirmIdTests.cs`:
```csharp
using Accounts.SharedKernel.Identity;
using FluentAssertions;

namespace Accounts.SharedKernel.Tests;

public class FirmIdTests
{
    [Fact]
    public void New_ReturnsDistinctValues()
    {
        var a = FirmId.New();
        var b = FirmId.New();
        a.Should().NotBe(b);
        a.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Equality_IsByValue()
    {
        var guid = Guid.NewGuid();
        var a = new FirmId(guid);
        var b = new FirmId(guid);
        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Parse_RoundTripsThroughString()
    {
        var original = FirmId.New();
        var parsed = FirmId.Parse(original.ToString());
        parsed.Should().Be(original);
    }

    [Fact]
    public void Empty_GuidIsRejected()
    {
        var act = () => new FirmId(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

```powershell
dotnet test src/Shared/Accounts.SharedKernel.Tests/Accounts.SharedKernel.Tests.csproj
```
Expected: build error — `FirmId` does not exist.

- [ ] **Step 4: Implement `FirmId` and `UserId`**

`src/Shared/Accounts.SharedKernel/Identity/FirmId.cs`:
```csharp
namespace Accounts.SharedKernel.Identity;

public readonly record struct FirmId(Guid Value)
{
    public FirmId(Guid Value) : this()
    {
        if (Value == Guid.Empty)
            throw new ArgumentException("FirmId cannot be empty", nameof(Value));
        this.Value = Value;
    }

    public static FirmId New() => new(Guid.NewGuid());
    public static FirmId Parse(string s) => new(Guid.Parse(s));
    public override string ToString() => Value.ToString();
}
```

`src/Shared/Accounts.SharedKernel/Identity/UserId.cs`:
```csharp
namespace Accounts.SharedKernel.Identity;

public readonly record struct UserId(Guid Value)
{
    public UserId(Guid Value) : this()
    {
        if (Value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(Value));
        this.Value = Value;
    }

    public static UserId New() => new(Guid.NewGuid());
    public static UserId Parse(string s) => new(Guid.Parse(s));
    public override string ToString() => Value.ToString();
}
```

- [ ] **Step 5: Implement base domain types**

`src/Shared/Accounts.SharedKernel/Domain/IDomainEvent.cs`:
```csharp
using MediatR;
namespace Accounts.SharedKernel.Domain;
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredAt { get; }
}
```

Wait — `MediatR` is not a dependency of SharedKernel yet. Add it:

```powershell
dotnet add src/Shared/Accounts.SharedKernel/Accounts.SharedKernel.csproj package MediatR
```

`src/Shared/Accounts.SharedKernel/Domain/ITenantScopedEntity.cs`:
```csharp
using Accounts.SharedKernel.Identity;
namespace Accounts.SharedKernel.Domain;
public interface ITenantScopedEntity
{
    FirmId FirmId { get; }
}
```

`src/Shared/Accounts.SharedKernel/Domain/Entity.cs`:
```csharp
namespace Accounts.SharedKernel.Domain;

public abstract class Entity<TId> where TId : struct
{
    public TId Id { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    protected Entity() { }
    protected Entity(TId id, DateTimeOffset now)
    {
        Id = id;
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch(DateTimeOffset now) => UpdatedAt = now;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => Id.GetHashCode();
}
```

`src/Shared/Accounts.SharedKernel/Domain/AggregateRoot.cs`:
```csharp
namespace Accounts.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : struct
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events;

    protected AggregateRoot() { }
    protected AggregateRoot(TId id, DateTimeOffset now) : base(id, now) { }

    protected void Raise(IDomainEvent @event) => _events.Add(@event);
    public void ClearEvents() => _events.Clear();
}
```

`src/Shared/Accounts.SharedKernel/Domain/ValueObject.cs`:
```csharp
namespace Accounts.SharedKernel.Domain;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject vo
        && vo.GetType() == GetType()
        && GetEqualityComponents().SequenceEqual(vo.GetEqualityComponents());

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Select(c => c?.GetHashCode() ?? 0)
            .Aggregate(0, (a, b) => HashCode.Combine(a, b));
}
```

- [ ] **Step 6: Implement Clock and Result**

`src/Shared/Accounts.SharedKernel/Time/IClock.cs`:
```csharp
namespace Accounts.SharedKernel.Time;
public interface IClock { DateTimeOffset UtcNow { get; } }
```

`src/Shared/Accounts.SharedKernel/Time/SystemClock.cs`:
```csharp
namespace Accounts.SharedKernel.Time;
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

`src/Shared/Accounts.SharedKernel/Results/Result.cs`:
```csharp
namespace Accounts.SharedKernel.Results;

public sealed record DomainError(string Code, string Message);

public readonly record struct Result<T>
{
    public T? Value { get; }
    public DomainError? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    private Result(T? value, DomainError? error) { Value = value; Error = error; }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(DomainError error) => new(default, error);
    public static Result<T> Failure(string code, string message) =>
        new(default, new DomainError(code, message));
}
```

- [ ] **Step 7: Write Result tests**

`src/Shared/Accounts.SharedKernel.Tests/ResultTests.cs`:
```csharp
using Accounts.SharedKernel.Results;
using FluentAssertions;

namespace Accounts.SharedKernel.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasValueAndNoError()
    {
        var r = Result<int>.Success(42);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(42);
        r.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_HasErrorAndNoValue()
    {
        var r = Result<int>.Failure("E1", "oops");
        r.IsFailure.Should().BeTrue();
        r.Error.Should().Be(new DomainError("E1", "oops"));
    }
}
```

- [ ] **Step 8: Run tests and verify pass**

```powershell
dotnet test src/Shared/Accounts.SharedKernel.Tests/Accounts.SharedKernel.Tests.csproj
```
Expected: `Passed!  - Failed: 0, Passed: 6`.

- [ ] **Step 9: Commit**

```powershell
git add -A
git commit -m "feat(shared): add FirmId, UserId, base Entity/AggregateRoot/ValueObject, Clock, Result"
```

---

## Phase 2: Database, Web host, and health check

### Task 5: PracticeOperationsDbContext skeleton

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/PracticeOperationsDbContext.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Add EF Core packages to Infrastructure**

```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions
```

- [ ] **Step 2: Write DbContext skeleton**

`src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/PracticeOperationsDbContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence;

public class PracticeOperationsDbContext : DbContext
{
    public PracticeOperationsDbContext(DbContextOptions<PracticeOperationsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("practice_operations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PracticeOperationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

- [ ] **Step 3: Write DI registration**

`src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/DependencyInjection.cs`:
```csharp
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPracticeOperations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PracticeOperations")
            ?? throw new InvalidOperationException("ConnectionStrings:PracticeOperations is required.");

        services.AddDbContext<PracticeOperationsDbContext>(opts =>
            opts.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations", "practice_operations")));

        return services;
    }
}
```

- [ ] **Step 4: Build**

```powershell
dotnet build Accounts.sln
```
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(practice-ops): add DbContext skeleton + DI registration"
```

---

### Task 6: Web composition root with health check

**Files:**
- Create: `src/Web/Accounts.Web/Program.cs` (overwrite)
- Create: `src/Web/Accounts.Web/appsettings.json` (overwrite)
- Create: `src/Web/Accounts.Web/appsettings.Development.json`

- [ ] **Step 1: Add health check packages to Web**

```powershell
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Microsoft.EntityFrameworkCore
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package AspNetCore.HealthChecks.NpgSql --version 9.0.0
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
```

> Notes:
> - `AspNetCore.HealthChecks.NpgSql` 9.0 targets .NET 9 but is forward-compatible with .NET 10.
> - `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` is required for `AddDbContextCheck<T>()` — that extension method lives in this package, not in the Npgsql health-checks package.
> - Adding the HealthChecks.EFCore package will require bumping `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Design` from `10.0.0` to `10.0.7` (transitive constraint). Update both `PackageVersion` entries in `Directory.Packages.props`.

- [ ] **Step 2: Write `appsettings.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PracticeOperations": "Host=localhost;Port=5432;Database=accounts_dev;Username=accounts;Password=accounts_dev_password"
  }
}
```

- [ ] **Step 3: Write `appsettings.Development.json`**

```json
{
  "Logging": { "LogLevel": { "Default": "Debug" } }
}
```

- [ ] **Step 4: Write `Program.cs`**

`src/Web/Accounts.Web/Program.cs`:
```csharp
using Accounts.PracticeOperations.Infrastructure;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPracticeOperations(builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<PracticeOperationsDbContext>("practice-operations-db");

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "accounts", status = "ok" }));

app.Run();

public partial class Program { }   // for WebApplicationFactory<Program>
```

- [ ] **Step 5: Build and run, hit `/health`**

```powershell
dotnet run --project src/Web/Accounts.Web/Accounts.Web.csproj --urls http://localhost:5080 &
Start-Sleep -Seconds 5
Invoke-RestMethod http://localhost:5080/health
```
Expected: `Healthy`.

- [ ] **Step 6: Stop the dev server**

```powershell
Get-Process -Name "Accounts.Web" -ErrorAction SilentlyContinue | Stop-Process
```

- [ ] **Step 7: Commit**

```powershell
git add -A
git commit -m "feat(web): add composition root with /health DB check"
```

---

### Task 7: First integration test — health endpoint

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Fixtures/PostgresFixture.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Fixtures/ApiFactory.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/HealthCheckTests.cs`

- [ ] **Step 1: Add test packages**

```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj package FluentAssertions
dotnet add src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj package Testcontainers.PostgreSql
dotnet add src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj package Microsoft.EntityFrameworkCore.Design
```

- [ ] **Step 2: Write the `PostgresFixture`**

`src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Fixtures/PostgresFixture.cs`:
```csharp
using Testcontainers.PostgreSql;

namespace Accounts.PracticeOperations.IntegrationTests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("accounts_test")
        .WithUsername("accounts")
        .WithPassword("accounts_test_password")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(nameof(PostgresCollection))]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture> { }
```

- [ ] **Step 3: Write the `ApiFactory`**

`src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Fixtures/ApiFactory.cs`:
```csharp
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.IntegrationTests.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiFactory(string connectionString) => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PracticeOperations"] = _connectionString
            });
        });
        builder.ConfigureServices(services =>
        {
            // Apply migrations on startup of the test host.
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Database.Migrate();
        });
    }
}
```

- [ ] **Step 4: Write the failing health-check test**

`src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/HealthCheckTests.cs`:
```csharp
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class HealthCheckTests
{
    private readonly PostgresFixture _pg;
    public HealthCheckTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Health_endpoint_returns_healthy_when_database_reachable()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();

        var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.Should().BeTrue();
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }
}
```

- [ ] **Step 5: Run the test**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj
```

> Expected: **passes**. The plan originally expected this to fail RED until Task 8's migration, but `db.Database.Migrate()` against an empty migrations assembly is a no-op that succeeds (it just creates the migrations history table), and `AddDbContextCheck<T>` defaults to `CanConnectAsync()` which doesn't verify any tables exist. The integration plumbing (Testcontainers, ApiFactory, /health) is exercised — that's the value. Task 8 still adds the initial migration for downstream tasks to extend.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "test(integration): add postgres fixture, api factory, health check test (red)"
```

> The "(red)" suffix is preserved historically — the original intent was a red→green progression. In practice the test goes green immediately; the commit message stays as the plan documents to keep the history readable.

---

### Task 8: Initial EF Core migration (empty)

**Files:**
- Generated under: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Migrations/`

- [ ] **Step 1: Install ef tooling globally (one-time)**

```powershell
dotnet tool install -g dotnet-ef
```
If already installed: `dotnet tool update -g dotnet-ef`.

- [ ] **Step 2: Generate the initial migration**

```powershell
dotnet ef migrations add Initial `
  --project src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj `
  --startup-project src/Web/Accounts.Web/Accounts.Web.csproj `
  --output-dir Persistence/Migrations
```
Expected: `Done. To undo this action, use 'ef migrations remove'`. A `Migrations/` folder with `Initial.cs` appears.

- [ ] **Step 3: Re-run integration tests**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj
```
Expected: `Passed!  - Failed: 0, Passed: 1`. (Testcontainers will pull `postgres:16-alpine` on first run — that may take 30–60s.)

- [ ] **Step 4: Commit**

```powershell
git add -A
git commit -m "feat(practice-ops): initial empty EF migration; health check test passes"
```

---

## Phase 3: Tenant context and isolation

### Task 9: `IFirmContext` abstraction

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IFirmContext.cs`
- Create: `src/Web/Accounts.Web/Auth/FirmContextAccessor.cs`
- Modify: `src/Web/Accounts.Web/Program.cs`

- [ ] **Step 1: Add MediatR to Application + reference Application from Web**

```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj package MediatR
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
```

- [ ] **Step 2: Write the abstraction**

`src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IFirmContext.cs`:
```csharp
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IFirmContext
{
    /// <summary>FirmId of the current request, or null if unauthenticated/anonymous.</summary>
    FirmId? FirmId { get; }
    /// <summary>UserId of the current request, or null if unauthenticated/anonymous.</summary>
    UserId? UserId { get; }
    bool IsAuthenticated { get; }
}
```

- [ ] **Step 3: Write the HTTP-backed implementation**

`src/Web/Accounts.Web/Auth/FirmContextAccessor.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.SharedKernel.Identity;

namespace Accounts.Web.Auth;

public sealed class FirmContextAccessor : IFirmContext
{
    public const string FirmIdClaim = "firm_id";
    private readonly IHttpContextAccessor _accessor;

    public FirmContextAccessor(IHttpContextAccessor accessor) => _accessor = accessor;

    private System.Security.Claims.ClaimsPrincipal? User =>
        _accessor.HttpContext?.User;

    public FirmId? FirmId
    {
        get
        {
            var value = User?.FindFirst(FirmIdClaim)?.Value;
            return Guid.TryParse(value, out var g) && g != Guid.Empty
                ? new FirmId(g) : null;
        }
    }

    public UserId? UserId
    {
        get
        {
            var value = User?.FindFirst("sub")?.Value
                ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var g) && g != Guid.Empty
                ? new SharedKernel.Identity.UserId(g) : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
```

- [ ] **Step 4: Register it in `Program.cs`**

Add after `AddPracticeOperations`:
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Accounts.PracticeOperations.Application.Abstractions.IFirmContext,
                          Accounts.Web.Auth.FirmContextAccessor>();
```

Also add to Web's project reference (Web → Application, not just Web → Infrastructure):
```powershell
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj reference src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj
```

- [ ] **Step 5: Build**

```powershell
dotnet build Accounts.sln
```
Expected: success.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat(auth): IFirmContext abstraction + HttpContext-backed accessor"
```

---

### Task 10: Tenant-scoped query filter on DbContext

**Files:**
- Modify: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/PracticeOperationsDbContext.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/TenantQueryFilter.cs`

- [ ] **Step 1: Reference Application from Infrastructure**

(Already referenced via Application → Domain → SharedKernel and Infrastructure → Application. Verify with `dotnet list reference`.)

- [ ] **Step 2: Update DbContext to apply tenant filter to every `ITenantScopedEntity`**

`src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/PracticeOperationsDbContext.cs` (full replacement):
```csharp
using System.Linq.Expressions;
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence;

public class PracticeOperationsDbContext : DbContext
{
    private readonly IFirmContext _firmContext;

    public PracticeOperationsDbContext(
        DbContextOptions<PracticeOperationsDbContext> options,
        IFirmContext firmContext)
        : base(options)
    {
        _firmContext = firmContext;
    }

    /// <summary>For migration-only / test-fixture use where no firm context exists.</summary>
    internal Guid? CurrentFirmIdRaw =>
        _firmContext.FirmId.HasValue ? _firmContext.FirmId.Value.Value : null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("practice_operations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PracticeOperationsDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScopedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(PracticeOperationsDbContext)
                    .GetMethod(nameof(SetTenantFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScopedEntity
    {
        Expression<Func<TEntity, bool>> filter =
            e => CurrentFirmIdRaw == null || e.FirmId.Value == CurrentFirmIdRaw;
        modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
    }
}
```

> Note: `CurrentFirmIdRaw == null` allows queries to run when there's no tenant in context (e.g. migrations, anonymous health probes, internal admin tasks). Anything querying through an authenticated request will have a `FirmId` set, and the filter will restrict the rows.

- [ ] **Step 3: Build**

```powershell
dotnet build Accounts.sln
```
Expected: success.

- [ ] **Step 4: Commit**

```powershell
git add -A
git commit -m "feat(practice-ops): apply ITenantScopedEntity query filter globally"
```

---

### Task 11: Cross-tenant isolation integration test

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/_Test/TenantTestRow.cs` (test-only entity — promoted out later)
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Configurations/TenantTestRowConfiguration.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/TenantIsolationTests.cs`

> We need a trivially-scoped entity to test isolation before we have real aggregates. We add `TenantTestRow` now in a `_Test` folder; it'll be removed (or repurposed) once `Firm` and `User` exist.

- [ ] **Step 1: Add `TenantTestRow`**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/_Test/TenantTestRow.cs`:
```csharp
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain._Test;

public class TenantTestRow : ITenantScopedEntity
{
    public Guid Id { get; private set; }
    public FirmId FirmId { get; private set; }
    public string Label { get; private set; } = string.Empty;

    private TenantTestRow() { }
    public TenantTestRow(FirmId firmId, string label)
    {
        Id = Guid.NewGuid();
        FirmId = firmId;
        Label = label;
    }
}
```

- [ ] **Step 2: Configure it**

`src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Configurations/TenantTestRowConfiguration.cs`:
```csharp
using Accounts.PracticeOperations.Domain._Test;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Configurations;

internal sealed class TenantTestRowConfiguration : IEntityTypeConfiguration<TenantTestRow>
{
    public void Configure(EntityTypeBuilder<TenantTestRow> b)
    {
        b.ToTable("tenant_test_rows");
        b.HasKey(x => x.Id);
        b.Property(x => x.FirmId)
            .HasConversion(v => v.Value, v => new FirmId(v))
            .HasColumnName("firm_id")
            .IsRequired();
        b.HasIndex(x => x.FirmId);
        b.Property(x => x.Label).HasMaxLength(200).IsRequired();
    }
}
```

- [ ] **Step 3: Add the migration**

```powershell
dotnet ef migrations add AddTenantTestRow `
  --project src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj `
  --startup-project src/Web/Accounts.Web/Accounts.Web.csproj `
  --output-dir Persistence/Migrations
```

- [ ] **Step 4: Write the failing isolation test**

`src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/TenantIsolationTests.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain._Test;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class TenantIsolationTests
{
    private readonly PostgresFixture _pg;
    public TenantIsolationTests(PostgresFixture pg) => _pg = pg;

    private sealed class FakeFirmContext : IFirmContext
    {
        public FirmId? FirmId { get; set; }
        public UserId? UserId { get; set; }
        public bool IsAuthenticated => FirmId.HasValue;
    }

    [Fact]
    public async Task Query_filter_hides_rows_from_other_tenants()
    {
        var firmA = SharedKernel.Identity.FirmId.New();
        var firmB = SharedKernel.Identity.FirmId.New();
        var ctx = new FakeFirmContext();

        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        // seed both firms (use raw DbContext to bypass filter)
        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            ctx.FirmId = null;        // bypass — null FirmId = no filter
            db.Set<TenantTestRow>().AddRange(
                new TenantTestRow(firmA, "alpha-A"),
                new TenantTestRow(firmB, "beta-B"));
            await db.SaveChangesAsync();
        }

        // scope to firmA — should see only alpha-A
        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firmA;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var rows = await db.Set<TenantTestRow>().Select(r => r.Label).ToListAsync();
            rows.Should().BeEquivalentTo(new[] { "alpha-A" });
        }

        // scope to firmB — should see only beta-B
        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firmB;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var rows = await db.Set<TenantTestRow>().Select(r => r.Label).ToListAsync();
            rows.Should().BeEquivalentTo(new[] { "beta-B" });
        }
    }
}
```

- [ ] **Step 5: Extend `ApiFactory` to accept a customisation callback**

Update `ApiFactory.cs`:
```csharp
using System;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.IntegrationTests.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly Action<IServiceCollection>? _configureServices;

    public ApiFactory(string connectionString, Action<IServiceCollection>? configureServices = null)
    {
        _connectionString = connectionString;
        _configureServices = configureServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PracticeOperations"] = _connectionString
            }));
        builder.ConfigureServices(services =>
        {
            _configureServices?.Invoke(services);
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Database.Migrate();
        });
    }
}
```

- [ ] **Step 6: Run isolation test**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj --filter FullyQualifiedName~TenantIsolation
```
Expected: `Passed!`.

- [ ] **Step 7: Commit**

```powershell
git add -A
git commit -m "test(tenancy): cross-tenant isolation proven via global query filter"
```

---

## Phase 4: Append-only audit log

### Task 12: `AuditEvent` domain type and EF configuration

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Audit/AuditAction.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Audit/AuditEvent.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Configurations/AuditEventConfiguration.cs`

- [ ] **Step 1: Write unit test for `AuditEvent` construction**

`src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Domain/AuditEventTests.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.SharedKernel.Identity;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class AuditEventTests
{
    [Fact]
    public void Create_sets_all_fields_and_generates_id()
    {
        var firm = FirmId.New();
        var user = UserId.New();
        var now = DateTimeOffset.UtcNow;

        var e = AuditEvent.Record(
            firm, user, AuditAction.UserSignedIn,
            "User", user.Value.ToString(),
            payload: "{\"ip\":\"127.0.0.1\"}",
            correlationId: "corr-1",
            occurredAt: now);

        e.Id.Should().NotBe(Guid.Empty);
        e.FirmId.Should().Be(firm);
        e.ActorUserId.Should().Be(user);
        e.Action.Should().Be(AuditAction.UserSignedIn);
        e.EntityType.Should().Be("User");
        e.EntityId.Should().Be(user.Value.ToString());
        e.Payload.Should().Be("{\"ip\":\"127.0.0.1\"}");
        e.CorrelationId.Should().Be("corr-1");
        e.OccurredAt.Should().Be(now);
    }
}
```

Add FluentAssertions reference if missing:
```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj package FluentAssertions
```

- [ ] **Step 2: Run — should fail (types missing)**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj
```
Expected: compile errors referencing `AuditEvent`/`AuditAction`.

- [ ] **Step 3: Implement `AuditAction`**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Audit/AuditAction.cs`:
```csharp
namespace Accounts.PracticeOperations.Domain.Audit;

public enum AuditAction
{
    Unknown = 0,

    // Firm lifecycle
    FirmRegistered = 100,

    // User lifecycle / auth
    UserRegistered = 200,
    UserSignedIn = 201,
    UserSignInFailed = 202,
    UserSignedOut = 203,
    UserTotpEnrolled = 204,
    UserRoleChanged = 205,
}
```

- [ ] **Step 4: Implement `AuditEvent`**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Audit/AuditEvent.cs`:
```csharp
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Audit;

public sealed class AuditEvent : ITenantScopedEntity
{
    public Guid Id { get; private set; }
    public FirmId FirmId { get; private set; }
    public UserId? ActorUserId { get; private set; }
    public AuditAction Action { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? Payload { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditEvent() { }

    public static AuditEvent Record(
        FirmId firmId,
        UserId? actorUserId,
        AuditAction action,
        string entityType,
        string entityId,
        string? payload,
        string? correlationId,
        DateTimeOffset occurredAt) => new()
        {
            Id = Guid.NewGuid(),
            FirmId = firmId,
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Payload = payload,
            CorrelationId = correlationId,
            OccurredAt = occurredAt,
        };
}
```

- [ ] **Step 5: Configure EF mapping (and forbid mutations)**

`src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Configurations/AuditEventConfiguration.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Configurations;

internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> b)
    {
        b.ToTable("audit_events");
        b.HasKey(x => x.Id);

        b.Property(x => x.FirmId)
            .HasConversion(v => v.Value, v => new FirmId(v))
            .HasColumnName("firm_id").IsRequired();

        b.Property(x => x.ActorUserId)
            .HasConversion(
                v => v!.Value.Value,
                v => new UserId(v))
            .HasColumnName("actor_user_id");

        b.Property(x => x.Action).HasConversion<string>().HasMaxLength(64).IsRequired();
        b.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
        b.Property(x => x.Payload).HasColumnType("jsonb");
        b.Property(x => x.CorrelationId).HasMaxLength(64);
        b.Property(x => x.OccurredAt).IsRequired();

        b.HasIndex(x => new { x.FirmId, x.OccurredAt });
        b.HasIndex(x => new { x.FirmId, x.Action, x.OccurredAt });
    }
}
```

- [ ] **Step 6: Run unit tests**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj --filter FullyQualifiedName~AuditEvent
```
Expected: `Passed`.

- [ ] **Step 7: Generate migration**

```powershell
dotnet ef migrations add AddAuditEvents `
  --project src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj `
  --startup-project src/Web/Accounts.Web/Accounts.Web.csproj `
  --output-dir Persistence/Migrations
```

- [ ] **Step 8: Commit**

```powershell
git add -A
git commit -m "feat(audit): AuditEvent entity + EF configuration + migration"
```

---

### Task 13: `IAuditWriter` + EF implementation + append-only enforcement

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IAuditWriter.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Audit/EfAuditWriter.cs`
- Modify: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/PracticeOperationsDbContext.cs`

- [ ] **Step 1: Write abstraction**

`src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IAuditWriter.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Audit;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IAuditWriter
{
    Task RecordAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string? payload = null,
        CancellationToken ct = default);
}
```

Add Domain reference to Application (already present via project reference chain; verify).

- [ ] **Step 2: Write implementation**

`src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Audit/EfAuditWriter.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.SharedKernel.Time;
using Microsoft.AspNetCore.Http;

namespace Accounts.PracticeOperations.Infrastructure.Audit;

public sealed class EfAuditWriter : IAuditWriter
{
    private readonly PracticeOperationsDbContext _db;
    private readonly IFirmContext _ctx;
    private readonly IClock _clock;
    private readonly IHttpContextAccessor _http;

    public EfAuditWriter(PracticeOperationsDbContext db, IFirmContext ctx, IClock clock, IHttpContextAccessor http)
    {
        _db = db; _ctx = ctx; _clock = clock; _http = http;
    }

    public async Task RecordAsync(
        AuditAction action, string entityType, string entityId,
        string? payload = null, CancellationToken ct = default)
    {
        var firmId = _ctx.FirmId
            ?? throw new InvalidOperationException("Audit write requires an authenticated firm context.");

        var correlationId = _http.HttpContext?.TraceIdentifier;

        var evt = AuditEvent.Record(
            firmId, _ctx.UserId, action, entityType, entityId, payload, correlationId, _clock.UtcNow);

        _db.Set<AuditEvent>().Add(evt);
        await _db.SaveChangesAsync(ct);
    }
}
```

Add `Microsoft.AspNetCore.Http.Abstractions` package to Infrastructure:
```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Microsoft.AspNetCore.Http.Abstractions
```

- [ ] **Step 3: Enforce append-only by overriding `SaveChanges` in DbContext**

Append to `PracticeOperationsDbContext`:
```csharp
public override int SaveChanges() { GuardAuditAppendOnly(); return base.SaveChanges(); }
public override Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    GuardAuditAppendOnly();
    return base.SaveChangesAsync(ct);
}

private void GuardAuditAppendOnly()
{
    foreach (var entry in ChangeTracker.Entries<Accounts.PracticeOperations.Domain.Audit.AuditEvent>())
    {
        if (entry.State is EntityState.Modified or EntityState.Deleted)
            throw new InvalidOperationException(
                "AuditEvent is append-only; updates and deletes are not permitted.");
    }
}
```

- [ ] **Step 4: Register `IAuditWriter` and `IClock` in Infrastructure DI**

Update `DependencyInjection.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Infrastructure.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.SharedKernel.Time;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPracticeOperations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PracticeOperations")
            ?? throw new InvalidOperationException("ConnectionStrings:PracticeOperations is required.");

        services.AddDbContext<PracticeOperationsDbContext>(opts =>
            opts.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations", "practice_operations")));

        services.AddSingleton<IClock, SystemClock>();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditWriter, EfAuditWriter>();

        return services;
    }
}
```

- [ ] **Step 5: Write integration test for append-only enforcement**

`src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/AuditLogTests.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class AuditLogTests
{
    private readonly PostgresFixture _pg;
    public AuditLogTests(PostgresFixture pg) => _pg = pg;

    private sealed class FakeFirmContext : IFirmContext
    {
        public FirmId? FirmId { get; set; }
        public UserId? UserId { get; set; }
        public bool IsAuthenticated => FirmId.HasValue;
    }

    [Fact]
    public async Task RecordAsync_persists_event_for_current_firm()
    {
        var firm = FirmId.New();
        var ctx = new FakeFirmContext { FirmId = firm, UserId = UserId.New() };
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        using (var scope = api.Services.CreateScope())
        {
            var writer = scope.ServiceProvider.GetRequiredService<IAuditWriter>();
            await writer.RecordAsync(AuditAction.UserSignedIn, "User", ctx.UserId!.Value.ToString());
        }

        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var rows = await db.Set<AuditEvent>().ToListAsync();
            rows.Should().HaveCount(1);
            rows[0].FirmId.Should().Be(firm);
            rows[0].Action.Should().Be(AuditAction.UserSignedIn);
        }
    }

    [Fact]
    public async Task SaveChanges_throws_when_AuditEvent_is_modified()
    {
        var firm = FirmId.New();
        var ctx = new FakeFirmContext { FirmId = firm, UserId = UserId.New() };
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
        var evt = AuditEvent.Record(firm, ctx.UserId, AuditAction.UserSignedIn,
            "User", ctx.UserId!.Value.ToString(), null, null, DateTimeOffset.UtcNow);
        db.Set<AuditEvent>().Add(evt);
        await db.SaveChangesAsync();

        // Tamper
        db.Entry(evt).Property(nameof(AuditEvent.EntityId)).CurrentValue = "tampered";

        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }
}
```

- [ ] **Step 6: Run audit tests**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj --filter FullyQualifiedName~AuditLog
```
Expected: `Passed: 2`.

- [ ] **Step 7: Commit**

```powershell
git add -A
git commit -m "feat(audit): IAuditWriter + EfAuditWriter; append-only enforced in SaveChanges"
```

---

### Task 14: MediatR `AuditingBehavior` pipeline

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Behaviors/IAuditedCommand.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Behaviors/AuditingBehavior.cs`

- [ ] **Step 1: Marker interface for audited commands**

`src/PracticeOperations/Accounts.PracticeOperations.Application/Behaviors/IAuditedCommand.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Audit;

namespace Accounts.PracticeOperations.Application.Behaviors;

/// <summary>Commands that produce an audit entry after they complete successfully.</summary>
public interface IAuditedCommand
{
    AuditAction Action { get; }
    string EntityType { get; }
    string EntityId { get; }
    string? Payload => null;
}
```

- [ ] **Step 2: Pipeline behavior**

`src/PracticeOperations/Accounts.PracticeOperations.Application/Behaviors/AuditingBehavior.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using MediatR;

namespace Accounts.PracticeOperations.Application.Behaviors;

public sealed class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditWriter _audit;
    public AuditingBehavior(IAuditWriter audit) => _audit = audit;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next();

        if (request is IAuditedCommand cmd)
        {
            await _audit.RecordAsync(cmd.Action, cmd.EntityType, cmd.EntityId, cmd.Payload, ct);
        }

        return response;
    }
}
```

- [ ] **Step 3: Register MediatR + behavior in Infrastructure DI**

Update `DependencyInjection.cs` — add to top:
```csharp
using Accounts.PracticeOperations.Application.Behaviors;
using MediatR;
```
And inside `AddPracticeOperations`:
```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IFirmContext).Assembly); // Application asm
});
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditingBehavior<,>));
```

Add MediatR package:
```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package MediatR
```

- [ ] **Step 4: Build**

```powershell
dotnet build Accounts.sln
```
Expected: success.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(audit): MediatR AuditingBehavior for IAuditedCommand"
```

---

## Phase 5: Domain — Firm and User aggregates

### Task 15: `EmailAddress` value object

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/EmailAddress.cs`
- Test: `src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Domain/EmailAddressTests.cs`

- [ ] **Step 1: Write the failing tests**

`src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Domain/EmailAddressTests.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Users;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class EmailAddressTests
{
    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("a.b+tag@example.co.uk")]
    public void Valid_email_is_accepted_and_lowercased(string input)
    {
        var e = EmailAddress.Create(input);
        e.IsSuccess.Should().BeTrue();
        e.Value!.Value.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("alice@")]
    public void Invalid_email_returns_failure(string input)
    {
        var e = EmailAddress.Create(input);
        e.IsFailure.Should().BeTrue();
        e.Error!.Code.Should().Be("Email.Invalid");
    }

    [Fact]
    public void Equality_is_case_insensitive_via_normalization()
    {
        var a = EmailAddress.Create("Alice@Example.com").Value!;
        var b = EmailAddress.Create("alice@example.COM").Value!;
        a.Should().Be(b);
    }
}
```

- [ ] **Step 2: Run — fail**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj --filter FullyQualifiedName~EmailAddress
```

- [ ] **Step 3: Implement**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/EmailAddress.cs`:
```csharp
using System.Text.RegularExpressions;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Results;

namespace Accounts.PracticeOperations.Domain.Users;

public sealed class EmailAddress : ValueObject
{
    // RFC-5322 simplified: one @, non-empty local, dot in domain.
    private static readonly Regex Pattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static Result<EmailAddress> Create(string? raw)
    {
        var trimmed = raw?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(trimmed) || !Pattern.IsMatch(trimmed))
            return Result<EmailAddress>.Failure("Email.Invalid", $"'{raw}' is not a valid email address.");
        return Result<EmailAddress>.Success(new EmailAddress(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
```

- [ ] **Step 4: Run — pass**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj --filter FullyQualifiedName~EmailAddress
```
Expected: all green.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(domain): EmailAddress value object with validation + normalization"
```

---

### Task 16: `Role` enum and `FirmStatus`, `UserStatus`

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/Role.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/UserStatus.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Firms/FirmStatus.cs`

- [ ] **Step 1: Write enums**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/Role.cs`:
```csharp
namespace Accounts.PracticeOperations.Domain.Users;

/// <summary>
/// Internal staff roles. Per spec §2.2. Client portal roles
/// (ClientPrimary/ClientStaff/ClientReadOnly) and MLRO/DPO overlays come in later sub-plans.
/// </summary>
public enum Role
{
    FirmOwner = 1,
    Partner = 2,
    Manager = 3,
    FeeEarner = 4,
    PracticeAdmin = 5,
}
```

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/UserStatus.cs`:
```csharp
namespace Accounts.PracticeOperations.Domain.Users;

public enum UserStatus
{
    PendingVerification = 1,
    Active = 2,
    Suspended = 3,
    Deactivated = 4,
}
```

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Firms/FirmStatus.cs`:
```csharp
namespace Accounts.PracticeOperations.Domain.Firms;

public enum FirmStatus
{
    Trial = 1,
    Active = 2,
    Suspended = 3,
    OffboardingScheduled = 4,
    Offboarded = 5,
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build src/PracticeOperations/Accounts.PracticeOperations.Domain/Accounts.PracticeOperations.Domain.csproj
```
Expected: success.

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "feat(domain): Role, UserStatus, FirmStatus enums"
```

---

### Task 17: `Firm` aggregate

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Firms/Events/FirmRegistered.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Firms/Firm.cs`
- Test: `src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Domain/FirmTests.cs`

- [ ] **Step 1: Write failing tests**

`src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Domain/FirmTests.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Firms.Events;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class FirmTests
{
    [Fact]
    public void Register_creates_firm_in_Trial_status_and_raises_event()
    {
        var now = DateTimeOffset.UtcNow;
        var firm = Firm.Register("Smith & Co", "smith-co", now);

        firm.Name.Should().Be("Smith & Co");
        firm.Slug.Should().Be("smith-co");
        firm.Status.Should().Be(FirmStatus.Trial);
        firm.CreatedAt.Should().Be(now);
        firm.DomainEvents.Should().ContainSingle(e => e is FirmRegistered);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_rejects_blank_name(string name)
    {
        var act = () => Firm.Register(name, "ok-slug", DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("UPPER")]                 // must be lowercased / kebab
    [InlineData("with spaces")]
    [InlineData("special!chars")]
    public void Register_rejects_invalid_slug(string slug)
    {
        var act = () => Firm.Register("Valid", slug, DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*slug*");
    }

    [Fact]
    public void Activate_moves_from_Trial_to_Active()
    {
        var firm = Firm.Register("Acme", "acme", DateTimeOffset.UtcNow);
        firm.Activate(DateTimeOffset.UtcNow);
        firm.Status.Should().Be(FirmStatus.Active);
    }
}
```

- [ ] **Step 2: Implement event**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Firms/Events/FirmRegistered.cs`:
```csharp
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Firms.Events;

public sealed record FirmRegistered(FirmId FirmId, string Name, DateTimeOffset OccurredAt) : IDomainEvent;
```

- [ ] **Step 3: Implement `Firm` aggregate**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Firms/Firm.cs`:
```csharp
using System.Text.RegularExpressions;
using Accounts.PracticeOperations.Domain.Firms.Events;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Firms;

public sealed class Firm : AggregateRoot<FirmId>
{
    private static readonly Regex SlugPattern = new(@"^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$", RegexOptions.Compiled);

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public FirmStatus Status { get; private set; }

    private Firm() { }

    public static Firm Register(string name, string slug, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Firm name must not be blank.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug) || !SlugPattern.IsMatch(slug))
            throw new ArgumentException("Firm slug must be lowercase, kebab-case, 1–64 chars.", nameof(slug));

        var id = FirmId.New();
        var firm = new Firm
        {
            Id = id,
            Name = name.Trim(),
            Slug = slug,
            Status = FirmStatus.Trial,
            CreatedAt = now,
            UpdatedAt = now,
        };
        firm.Raise(new FirmRegistered(id, firm.Name, now));
        return firm;
    }

    public void Activate(DateTimeOffset now)
    {
        if (Status != FirmStatus.Trial)
            throw new InvalidOperationException($"Cannot activate firm in status {Status}.");
        Status = FirmStatus.Active;
        Touch(now);
    }
}
```

> Note: `AggregateRoot<TId>` uses protected setters for `Id`, `CreatedAt`, `UpdatedAt` — confirmed in Task 4. We're assigning via object initializer; that works only because they're `protected` from outside the assembly. Within Domain assembly, `private`-via-`protected` from the base type. Adjust base type if you'd rather use a `protected internal` setter. For this plan we leave it as-is and assign through inheritance.

Actually, that won't compile — `CreatedAt`, `UpdatedAt`, `Id` are `protected set` in the base, accessible from derived classes but **not** through an object initializer on `Firm` from a sibling method. Use the constructor pattern:

Replace the body of `Register` with:
```csharp
        var id = FirmId.New();
        var firm = new Firm();
        firm.AssignIdentity(id, now);
        firm.Name = name.Trim();
        firm.Slug = slug;
        firm.Status = FirmStatus.Trial;
        firm.Raise(new FirmRegistered(id, firm.Name, now));
        return firm;
```

And add a helper to the base class. In `src/Shared/Accounts.SharedKernel/Domain/AggregateRoot.cs`, add:
```csharp
protected void AssignIdentity(TId id, DateTimeOffset now)
{
    Id = id;
    CreatedAt = now;
    UpdatedAt = now;
}
```

Make `Name`, `Slug`, `Status` settable via `private set` from a derived `Firm` — but the factory is a static method on `Firm` itself, so `private set` works fine since the factory is inside the type. Keep `private set` and remove redundant `protected`.

- [ ] **Step 4: Run tests**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj --filter FullyQualifiedName~FirmTests
```
Expected: `Passed`.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(domain): Firm aggregate with Register/Activate + FirmRegistered event"
```

---

### Task 18: `User` aggregate

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/Events/UserRegistered.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/User.cs`
- Test: `src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Domain/UserTests.cs`

- [ ] **Step 1: Write failing tests**

`src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Domain/UserTests.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Users;
using Accounts.PracticeOperations.Domain.Users.Events;
using Accounts.SharedKernel.Identity;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Register_creates_user_in_PendingVerification_with_event()
    {
        var firm = FirmId.New();
        var email = EmailAddress.Create("alice@example.com").Value!;
        var user = User.Register(firm, email, "hash-of-password", Role.FirmOwner, DateTimeOffset.UtcNow);

        user.FirmId.Should().Be(firm);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be("hash-of-password");
        user.Role.Should().Be(Role.FirmOwner);
        user.Status.Should().Be(UserStatus.PendingVerification);
        user.TotpEnrolled.Should().BeFalse();
        user.DomainEvents.Should().ContainSingle(e => e is UserRegistered);
    }

    [Fact]
    public void Activate_moves_PendingVerification_to_Active()
    {
        var user = NewUser();
        user.Activate(DateTimeOffset.UtcNow);
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Activate_throws_if_not_PendingVerification()
    {
        var user = NewUser();
        user.Activate(DateTimeOffset.UtcNow);
        var act = () => user.Activate(DateTimeOffset.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnrollTotp_sets_secret_and_flag()
    {
        var user = NewUser();
        user.EnrollTotp("BASE32SECRETXXXX", DateTimeOffset.UtcNow);
        user.TotpEnrolled.Should().BeTrue();
        user.TotpSecret.Should().Be("BASE32SECRETXXXX");
    }

    private static User NewUser() => User.Register(
        FirmId.New(),
        EmailAddress.Create("alice@example.com").Value!,
        "hash", Role.FirmOwner, DateTimeOffset.UtcNow);
}
```

- [ ] **Step 2: Implement event**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/Events/UserRegistered.cs`:
```csharp
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Users.Events;

public sealed record UserRegistered(
    UserId UserId, FirmId FirmId, string Email, Role Role, DateTimeOffset OccurredAt) : IDomainEvent;
```

- [ ] **Step 3: Implement `User`**

`src/PracticeOperations/Accounts.PracticeOperations.Domain/Users/User.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Users.Events;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Users;

public sealed class User : AggregateRoot<UserId>, ITenantScopedEntity
{
    public FirmId FirmId { get; private set; }
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role Role { get; private set; }
    public UserStatus Status { get; private set; }
    public bool TotpEnrolled { get; private set; }
    public string? TotpSecret { get; private set; }
    public DateTimeOffset? LastSignInAt { get; private set; }
    public int FailedSignInAttempts { get; private set; }

    private User() { }

    public static User Register(FirmId firmId, EmailAddress email, string passwordHash, Role role, DateTimeOffset now)
    {
        var user = new User();
        user.AssignIdentity(UserId.New(), now);
        user.FirmId = firmId;
        user.Email = email;
        user.PasswordHash = passwordHash;
        user.Role = role;
        user.Status = UserStatus.PendingVerification;
        user.Raise(new UserRegistered(user.Id, firmId, email.Value, role, now));
        return user;
    }

    public void Activate(DateTimeOffset now)
    {
        if (Status != UserStatus.PendingVerification)
            throw new InvalidOperationException($"Cannot activate user in status {Status}.");
        Status = UserStatus.Active;
        Touch(now);
    }

    public void EnrollTotp(string totpSecret, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(totpSecret))
            throw new ArgumentException("TOTP secret required.", nameof(totpSecret));
        TotpSecret = totpSecret;
        TotpEnrolled = true;
        Touch(now);
    }

    public void RecordSuccessfulSignIn(DateTimeOffset now)
    {
        LastSignInAt = now;
        FailedSignInAttempts = 0;
        Touch(now);
    }

    public void RecordFailedSignIn(DateTimeOffset now)
    {
        FailedSignInAttempts++;
        Touch(now);
    }
}
```

- [ ] **Step 4: Run tests**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj --filter FullyQualifiedName~UserTests
```
Expected: `Passed`.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(domain): User aggregate with Register/Activate/EnrollTotp"
```

---

### Task 19: EF mapping for `Firm` and `User` + migration

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Configurations/FirmConfiguration.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Configurations/UserConfiguration.cs`

- [ ] **Step 1: `FirmConfiguration`**

```csharp
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Configurations;

internal sealed class FirmConfiguration : IEntityTypeConfiguration<Firm>
{
    public void Configure(EntityTypeBuilder<Firm> b)
    {
        b.ToTable("firms");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(v => v.Value, v => new FirmId(v))
            .HasColumnName("id");
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();
        b.Ignore(x => x.DomainEvents);
    }
}
```

- [ ] **Step 2: `UserConfiguration`**

```csharp
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(v => v.Value, v => new UserId(v))
            .HasColumnName("id");
        b.Property(x => x.FirmId)
            .HasConversion(v => v.Value, v => new FirmId(v))
            .HasColumnName("firm_id").IsRequired();
        b.HasIndex(x => x.FirmId);

        b.Property(x => x.Email)
            .HasConversion(
                v => v.Value,
                v => EmailAddress.Create(v).Value!)
            .HasMaxLength(256).IsRequired();
        b.HasIndex(nameof(User.FirmId), nameof(User.Email)).IsUnique();

        b.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.TotpEnrolled).IsRequired();
        b.Property(x => x.TotpSecret).HasMaxLength(128);
        b.Property(x => x.LastSignInAt);
        b.Property(x => x.FailedSignInAttempts).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();
        b.Ignore(x => x.DomainEvents);
    }
}
```

- [ ] **Step 3: Generate migration**

```powershell
dotnet ef migrations add AddFirmsAndUsers `
  --project src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj `
  --startup-project src/Web/Accounts.Web/Accounts.Web.csproj `
  --output-dir Persistence/Migrations
```
Expected: migration generated; review it briefly for sensible column types.

- [ ] **Step 4: Build + run existing tests**

```powershell
dotnet build Accounts.sln
dotnet test
```
Expected: all previously-green tests still pass.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(persistence): EF mappings for Firm and User + migration"
```

---

## Phase 6: Registration

### Task 20: Repository abstractions + EF implementations

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IFirmRepository.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IUserRepository.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IUnitOfWork.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Repositories/EfFirmRepository.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Repositories/EfUserRepository.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Persistence/Repositories/EfUnitOfWork.cs`

- [ ] **Step 1: Abstractions**

`IFirmRepository.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IFirmRepository
{
    Task<Firm?> GetAsync(FirmId id, CancellationToken ct = default);
    Task<Firm?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Firm firm, CancellationToken ct = default);
}
```

`IUserRepository.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetAsync(UserId id, CancellationToken ct = default);
    /// <summary>Look up by email within a firm. Bypasses tenant filter (used by SignIn before context is set).</summary>
    Task<User?> GetByEmailAcrossFirmsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
```

`IUnitOfWork.cs`:
```csharp
namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 2: `EfFirmRepository`**

```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Repositories;

internal sealed class EfFirmRepository : IFirmRepository
{
    private readonly PracticeOperationsDbContext _db;
    public EfFirmRepository(PracticeOperationsDbContext db) => _db = db;

    public Task<Firm?> GetAsync(FirmId id, CancellationToken ct = default) =>
        _db.Set<Firm>().IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<Firm?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _db.Set<Firm>().IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Slug == slug, ct);

    public async Task AddAsync(Firm firm, CancellationToken ct = default) =>
        await _db.Set<Firm>().AddAsync(firm, ct);
}
```

`EfUserRepository`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Repositories;

internal sealed class EfUserRepository : IUserRepository
{
    private readonly PracticeOperationsDbContext _db;
    public EfUserRepository(PracticeOperationsDbContext db) => _db = db;

    public Task<User?> GetAsync(UserId id, CancellationToken ct = default) =>
        _db.Set<User>().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAcrossFirmsAsync(string email, CancellationToken ct = default)
    {
        var lower = email.Trim().ToLowerInvariant();
        return _db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.Value == lower, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _db.Set<User>().AddAsync(user, ct);
}
```

`EfUnitOfWork`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Infrastructure.Persistence;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Repositories;

internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly PracticeOperationsDbContext _db;
    public EfUnitOfWork(PracticeOperationsDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
```

- [ ] **Step 3: Register in DI**

Append to `DependencyInjection.cs`:
```csharp
services.AddScoped<IFirmRepository, EfFirmRepository>();
services.AddScoped<IUserRepository, EfUserRepository>();
services.AddScoped<IUnitOfWork, EfUnitOfWork>();
```

- [ ] **Step 4: Build**

```powershell
dotnet build Accounts.sln
```
Expected: success.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(persistence): repository abstractions and EF implementations"
```

---

### Task 21: `IPasswordHasher` and PBKDF2 implementation

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IPasswordHasher.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Auth/Pbkdf2PasswordHasher.cs`
- Test: `src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Application/Pbkdf2PasswordHasherTests.cs` (skipped here — covered by integration)

- [ ] **Step 1: Abstraction**

`IPasswordHasher.cs`:
```csharp
namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string plaintextPassword);
    bool Verify(string hash, string plaintextPassword);
}
```

- [ ] **Step 2: Implementation using ASP.NET Core Identity's `PasswordHasher`**

Add package:
```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Microsoft.AspNetCore.Identity
```

`Pbkdf2PasswordHasher.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Accounts.PracticeOperations.Infrastructure.Auth;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _inner = new();
    private static readonly object User = new();

    public string Hash(string plaintextPassword) =>
        _inner.HashPassword(User, plaintextPassword);

    public bool Verify(string hash, string plaintextPassword)
    {
        var result = _inner.VerifyHashedPassword(User, hash, plaintextPassword);
        return result is PasswordVerificationResult.Success
                       or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
```

- [ ] **Step 3: Register**

In `DependencyInjection.cs`, add:
```csharp
services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
```

- [ ] **Step 4: Smoke test**

`src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Application/Pbkdf2PasswordHasherTests.cs`:
```csharp
using Accounts.PracticeOperations.Infrastructure.Auth;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Application;

public class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Round_trips_password()
    {
        var h = new Pbkdf2PasswordHasher();
        var hash = h.Hash("correct horse battery staple");
        h.Verify(hash, "correct horse battery staple").Should().BeTrue();
        h.Verify(hash, "wrong").Should().BeFalse();
    }
}
```

Add reference UnitTests → Infrastructure:
```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj reference src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj
```

Run:
```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj --filter FullyQualifiedName~Pbkdf2
```
Expected: pass.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(auth): IPasswordHasher + PBKDF2 implementation"
```

---

### Task 22: `RegisterFirmCommand` + handler + validator

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Firms/Register/RegisterFirmCommand.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Firms/Register/RegisterFirmValidator.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Firms/Register/RegisterFirmHandler.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Behaviors/ValidationBehavior.cs`
- Test: `src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Application/RegisterFirmHandlerTests.cs`

- [ ] **Step 1: Add FluentValidation packages**

```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Application/Accounts.PracticeOperations.Application.csproj package FluentValidation
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package FluentValidation.DependencyInjectionExtensions
dotnet add src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj package NSubstitute
```

- [ ] **Step 2: Validation behavior**

`Behaviors/ValidationBehavior.cs`:
```csharp
using FluentValidation;
using MediatR;

namespace Accounts.PracticeOperations.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();
            if (failures.Count != 0) throw new ValidationException(failures);
        }
        return await next();
    }
}
```

Register in `DependencyInjection.cs`:
```csharp
services.AddValidatorsFromAssembly(typeof(IFirmContext).Assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

> Note: ordering matters — `ValidationBehavior` must run **before** `AuditingBehavior`. MediatR runs `IPipelineBehavior` in registration order, so register Validation **before** Auditing.

Reorder in `DependencyInjection.cs` (validation before auditing):
```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditingBehavior<,>));
```

- [ ] **Step 3: Command + validator + handler**

`RegisterFirmCommand.cs`:
```csharp
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Domain.Users;
using MediatR;

namespace Accounts.PracticeOperations.Application.Firms.Register;

public sealed record RegisterFirmCommand(
    string FirmName,
    string FirmSlug,
    string OwnerEmail,
    string OwnerPassword) : IRequest<RegisterFirmResult>;

public sealed record RegisterFirmResult(Guid FirmId, Guid OwnerUserId);
```

`RegisterFirmValidator.cs`:
```csharp
using FluentValidation;

namespace Accounts.PracticeOperations.Application.Firms.Register;

public sealed class RegisterFirmValidator : AbstractValidator<RegisterFirmCommand>
{
    public RegisterFirmValidator()
    {
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FirmSlug).NotEmpty().MaximumLength(64).Matches("^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$");
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.OwnerPassword).NotEmpty().MinimumLength(12)
            .WithMessage("Password must be at least 12 characters.");
    }
}
```

`RegisterFirmHandler.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Time;
using MediatR;

namespace Accounts.PracticeOperations.Application.Firms.Register;

public sealed class RegisterFirmHandler : IRequestHandler<RegisterFirmCommand, RegisterFirmResult>
{
    private readonly IFirmRepository _firms;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public RegisterFirmHandler(
        IFirmRepository firms, IUserRepository users,
        IPasswordHasher hasher, IUnitOfWork uow, IClock clock)
    {
        _firms = firms; _users = users; _hasher = hasher; _uow = uow; _clock = clock;
    }

    public async Task<RegisterFirmResult> Handle(RegisterFirmCommand cmd, CancellationToken ct)
    {
        var existing = await _firms.GetBySlugAsync(cmd.FirmSlug, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Firm slug '{cmd.FirmSlug}' is already taken.");

        var email = EmailAddress.Create(cmd.OwnerEmail);
        if (email.IsFailure)
            throw new InvalidOperationException(email.Error!.Message);

        var existingUser = await _users.GetByEmailAcrossFirmsAsync(email.Value!.Value, ct);
        if (existingUser is not null)
            throw new InvalidOperationException($"Email '{email.Value.Value}' is already registered.");

        var now = _clock.UtcNow;
        var firm = Firm.Register(cmd.FirmName, cmd.FirmSlug, now);
        var owner = User.Register(firm.Id, email.Value, _hasher.Hash(cmd.OwnerPassword), Role.FirmOwner, now);

        await _firms.AddAsync(firm, ct);
        await _users.AddAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        return new RegisterFirmResult(firm.Id.Value, owner.Id.Value);
    }
}
```

- [ ] **Step 4: Unit-test handler**

`src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Application/RegisterFirmHandlerTests.cs`:
```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Time;
using FluentAssertions;
using NSubstitute;

namespace Accounts.PracticeOperations.UnitTests.Application;

public class RegisterFirmHandlerTests
{
    [Fact]
    public async Task Creates_firm_and_owner_user_when_slug_and_email_are_free()
    {
        var firms = Substitute.For<IFirmRepository>();
        var users = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var uow = Substitute.For<IUnitOfWork>();
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 11, 9, 0, 0, TimeSpan.Zero));
        firms.GetBySlugAsync("acme").Returns((Firm?)null);
        users.GetByEmailAcrossFirmsAsync("alice@example.com").Returns((User?)null);
        hasher.Hash("super-secret-password").Returns("$hash$");

        var handler = new RegisterFirmHandler(firms, users, hasher, uow, clock);
        var result = await handler.Handle(
            new RegisterFirmCommand("Acme & Co", "acme", "alice@example.com", "super-secret-password"),
            CancellationToken.None);

        result.FirmId.Should().NotBeEmpty();
        result.OwnerUserId.Should().NotBeEmpty();
        await firms.Received(1).AddAsync(Arg.Any<Firm>());
        await users.Received(1).AddAsync(Arg.Is<User>(u => u.Role == Role.FirmOwner));
        await uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Fails_when_slug_already_taken()
    {
        var firms = Substitute.For<IFirmRepository>();
        firms.GetBySlugAsync("acme").Returns(Firm.Register("Acme", "acme", DateTimeOffset.UtcNow));
        var handler = new RegisterFirmHandler(
            firms, Substitute.For<IUserRepository>(),
            Substitute.For<IPasswordHasher>(), Substitute.For<IUnitOfWork>(),
            Substitute.For<IClock>());

        var act = () => handler.Handle(
            new RegisterFirmCommand("Acme", "acme", "x@y.com", "long-enough-pwd"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*slug*taken*");
    }
}
```

Run:
```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.UnitTests/Accounts.PracticeOperations.UnitTests.csproj --filter FullyQualifiedName~RegisterFirmHandler
```
Expected: pass.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(firms): RegisterFirm command/handler/validator + ValidationBehavior"
```

---

### Task 23: `POST /api/firms/register` endpoint

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Endpoints/EndpointRouteBuilderExtensions.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Endpoints/FirmsEndpoints.cs`
- Modify: `src/Web/Accounts.Web/Program.cs`
- Test: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/RegisterFirmEndpointTests.cs`

- [ ] **Step 1: Endpoint module**

`EndpointRouteBuilderExtensions.cs`:
```csharp
using Microsoft.AspNetCore.Routing;

namespace Accounts.PracticeOperations.Infrastructure.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPracticeOperations(this IEndpointRouteBuilder app)
    {
        app.MapFirmsEndpoints();
        app.MapAuthEndpoints();
        app.MapAdminEndpoints();
        return app;
    }
}
```

`FirmsEndpoints.cs`:
```csharp
using Accounts.PracticeOperations.Application.Firms.Register;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Accounts.PracticeOperations.Infrastructure.Endpoints;

public static class FirmsEndpoints
{
    public static IEndpointRouteBuilder MapFirmsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/firms").WithTags("Firms");

        group.MapPost("/register", async (RegisterFirmCommand cmd, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(cmd);
                return Results.Created($"/api/firms/{result.FirmId}", result);
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(
                    ex.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .AllowAnonymous();

        return app;
    }
}
```

> `MapAuthEndpoints` and `MapAdminEndpoints` will be filled in by Tasks 25, 27, 30. For now, add stubs to satisfy the compiler:

`AuthEndpoints.cs`:
```csharp
using Microsoft.AspNetCore.Routing;
namespace Accounts.PracticeOperations.Infrastructure.Endpoints;
public static class AuthEndpointsPartial
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app) => app;
}
```

`AdminEndpoints.cs`:
```csharp
using Microsoft.AspNetCore.Routing;
namespace Accounts.PracticeOperations.Infrastructure.Endpoints;
public static class AdminEndpointsPartial
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app) => app;
}
```

- [ ] **Step 2: Wire endpoints into `Program.cs`**

Append before `app.Run()`:
```csharp
app.MapPracticeOperations();
```

Also add the `using`:
```csharp
using Accounts.PracticeOperations.Infrastructure.Endpoints;
```

- [ ] **Step 3: Integration test**

`RegisterFirmEndpointTests.cs`:
```csharp
using System.Net;
using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class RegisterFirmEndpointTests
{
    private readonly PostgresFixture _pg;
    public RegisterFirmEndpointTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Returns_201_and_ids_when_payload_valid()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();

        var response = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("Smith & Co", $"smith-{Guid.NewGuid():N}".Substring(0, 12),
                $"owner-{Guid.NewGuid():N}@example.com", "long-enough-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterFirmResult>();
        body!.FirmId.Should().NotBeEmpty();
        body.OwnerUserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Returns_400_when_password_too_short()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var response = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", "x", "a@b.com", "short"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

- [ ] **Step 4: Run**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj --filter FullyQualifiedName~RegisterFirmEndpoint
```
Expected: pass.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(firms): POST /api/firms/register endpoint + integration test"
```

---

## Phase 7: Sign-in and TOTP

### Task 24: `ITotpService` + Otp.NET implementation

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/ITotpService.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Auth/OtpNetTotpService.cs`

- [ ] **Step 1: Add Otp.NET**

```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Otp.NET
```

- [ ] **Step 2: Abstraction**

```csharp
namespace Accounts.PracticeOperations.Application.Abstractions;

public interface ITotpService
{
    /// <summary>Generate a new Base32 secret for enrolment.</summary>
    string GenerateSecret();
    /// <summary>otpauth:// URI that authenticator apps consume as a QR code.</summary>
    string BuildOtpAuthUri(string secret, string accountName, string issuer);
    /// <summary>Verify a 6-digit code against the secret with ±1 step tolerance.</summary>
    bool Verify(string secret, string code);
}
```

- [ ] **Step 3: Implementation**

```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using OtpNet;

namespace Accounts.PracticeOperations.Infrastructure.Auth;

public sealed class OtpNetTotpService : ITotpService
{
    public string GenerateSecret()
    {
        var bytes = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(bytes);
    }

    public string BuildOtpAuthUri(string secret, string accountName, string issuer)
    {
        var encIssuer = Uri.EscapeDataString(issuer);
        var encAccount = Uri.EscapeDataString(accountName);
        return $"otpauth://totp/{encIssuer}:{encAccount}?secret={secret}&issuer={encIssuer}&digits=6&period=30";
    }

    public bool Verify(string secret, string code)
    {
        var bytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(bytes);
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }
}
```

Register in `DependencyInjection.cs`:
```csharp
services.AddSingleton<ITotpService, OtpNetTotpService>();
```

- [ ] **Step 4: Build**

```powershell
dotnet build Accounts.sln
```

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(auth): ITotpService + Otp.NET implementation"
```

---

### Task 25: `EnrollTotpCommand` + endpoint

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Users/EnrollTotp/EnrollTotpCommand.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Users/EnrollTotp/EnrollTotpHandler.cs`
- Modify: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Endpoints/AuthEndpoints.cs`

- [ ] **Step 1: Command + handler**

```csharp
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Application.Behaviors;
using MediatR;

namespace Accounts.PracticeOperations.Application.Users.EnrollTotp;

public sealed record EnrollTotpCommand(Guid UserId)
    : IRequest<EnrollTotpResult>, IAuditedCommand
{
    public AuditAction Action => AuditAction.UserTotpEnrolled;
    public string EntityType => "User";
    public string EntityId => UserId.ToString();
}

public sealed record EnrollTotpResult(string Secret, string OtpAuthUri);
```

```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.SharedKernel.Identity;
using Accounts.SharedKernel.Time;
using MediatR;

namespace Accounts.PracticeOperations.Application.Users.EnrollTotp;

public sealed class EnrollTotpHandler : IRequestHandler<EnrollTotpCommand, EnrollTotpResult>
{
    private readonly IUserRepository _users;
    private readonly ITotpService _totp;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public EnrollTotpHandler(IUserRepository users, ITotpService totp, IUnitOfWork uow, IClock clock)
    {
        _users = users; _totp = totp; _uow = uow; _clock = clock;
    }

    public async Task<EnrollTotpResult> Handle(EnrollTotpCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetAsync(new UserId(cmd.UserId), ct)
            ?? throw new InvalidOperationException("User not found.");

        var secret = _totp.GenerateSecret();
        user.EnrollTotp(secret, _clock.UtcNow);
        await _uow.SaveChangesAsync(ct);

        var uri = _totp.BuildOtpAuthUri(secret, user.Email.Value, issuer: "Accounts");
        return new EnrollTotpResult(secret, uri);
    }
}
```

- [ ] **Step 2: Replace `AuthEndpoints.cs` (drop the partial stub)**

```csharp
using Accounts.PracticeOperations.Application.Users.EnrollTotp;
using Accounts.PracticeOperations.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Accounts.PracticeOperations.Infrastructure.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // Sign-in lives in Task 27 below.
        group.MapPost("/enroll-totp", async (ISender sender, IFirmContext ctx) =>
        {
            if (!ctx.IsAuthenticated || ctx.UserId is null)
                return Results.Unauthorized();
            try
            {
                var result = await sender.Send(new EnrollTotpCommand(ctx.UserId.Value.Value));
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).RequireAuthorization();

        return app;
    }
}
```

Remove the previous `AuthEndpointsPartial` class.

- [ ] **Step 3: Build (full auth wiring comes in Tasks 26–27)**

```powershell
dotnet build Accounts.sln
```

> May fail on `RequireAuthorization()` if no auth scheme yet — that's resolved in Task 26.

- [ ] **Step 4: Commit**

```powershell
git add -A
git commit -m "feat(auth): EnrollTotp command/handler/endpoint"
```

---

### Task 26: JWT issuance + bearer authentication

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Abstractions/IJwtIssuer.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Auth/JwtIssuer.cs`
- Modify: `src/Web/Accounts.Web/Program.cs`
- Modify: `src/Web/Accounts.Web/appsettings.json`

- [ ] **Step 1: Abstraction**

```csharp
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IJwtIssuer
{
    /// <summary>Returns (accessToken, expiresAt).</summary>
    (string Token, DateTimeOffset ExpiresAt) Issue(FirmId firmId, UserId userId, Role role);
}
```

- [ ] **Step 2: Add packages**

```powershell
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package Microsoft.IdentityModel.Tokens
dotnet add src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Accounts.PracticeOperations.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
```

- [ ] **Step 3: Implementation**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;
using Accounts.SharedKernel.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Accounts.PracticeOperations.Infrastructure.Auth;

public sealed class JwtIssuer : IJwtIssuer
{
    private readonly IClock _clock;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SigningCredentials _credentials;
    private readonly TimeSpan _lifetime;

    public JwtIssuer(IConfiguration config, IClock clock)
    {
        _clock = clock;
        _issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        _audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret missing");
        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);
        _lifetime = TimeSpan.FromMinutes(int.Parse(config["Jwt:LifetimeMinutes"] ?? "60"));
    }

    public (string Token, DateTimeOffset ExpiresAt) Issue(FirmId firmId, UserId userId, Role role)
    {
        var expires = _clock.UtcNow.Add(_lifetime);
        var claims = new[]
        {
            new Claim("sub", userId.Value.ToString()),
            new Claim("firm_id", firmId.Value.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
        };
        var token = new JwtSecurityToken(
            issuer: _issuer, audience: _audience,
            claims: claims,
            notBefore: _clock.UtcNow.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: _credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
```

Register in `DependencyInjection.cs`:
```csharp
services.AddSingleton<IJwtIssuer, JwtIssuer>();
```

- [ ] **Step 4: Add JWT config to `appsettings.json`**

```json
{
  "Jwt": {
    "Issuer": "accounts-dev",
    "Audience": "accounts-web",
    "Secret": "DEV-ONLY-do-not-use-in-prod-min-32-chars-1234567890",
    "LifetimeMinutes": "60"
  }
}
```

(Production secrets via env vars: `Jwt__Secret`.)

- [ ] **Step 5: Wire JWT bearer auth into `Program.cs`**

After service registration, before `app.Build()`:
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub",
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization();
```

After `var app = builder.Build();`:
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

- [ ] **Step 6: Build**

```powershell
dotnet build Accounts.sln
```
Expected: success.

- [ ] **Step 7: Commit**

```powershell
git add -A
git commit -m "feat(auth): JWT issuer + bearer authentication wired"
```

---

### Task 27: `SignInCommand` + handler + endpoint + integration test

**Files:**
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Users/SignIn/SignInCommand.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Users/SignIn/SignInHandler.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.Application/Users/SignIn/SignInValidator.cs`
- Modify: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Endpoints/AuthEndpoints.cs`
- Create: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/SignInEndpointTests.cs`

- [ ] **Step 1: Command + validator**

```csharp
using MediatR;

namespace Accounts.PracticeOperations.Application.Users.SignIn;

public sealed record SignInCommand(
    string Email,
    string Password,
    string? TotpCode) : IRequest<SignInResult>;

public sealed record SignInResult(string AccessToken, DateTimeOffset ExpiresAt, bool TotpRequired);
```

```csharp
using FluentValidation;
namespace Accounts.PracticeOperations.Application.Users.SignIn;
public sealed class SignInValidator : AbstractValidator<SignInCommand>
{
    public SignInValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
```

- [ ] **Step 2: Handler**

```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Time;
using MediatR;

namespace Accounts.PracticeOperations.Application.Users.SignIn;

public sealed class SignInHandler : IRequestHandler<SignInCommand, SignInResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITotpService _totp;
    private readonly IJwtIssuer _jwt;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;
    private readonly IAuditWriter _audit;

    public SignInHandler(
        IUserRepository users, IPasswordHasher hasher, ITotpService totp,
        IJwtIssuer jwt, IUnitOfWork uow, IClock clock, IAuditWriter audit)
    {
        _users = users; _hasher = hasher; _totp = totp;
        _jwt = jwt; _uow = uow; _clock = clock; _audit = audit;
    }

    public async Task<SignInResult> Handle(SignInCommand cmd, CancellationToken ct)
    {
        var user = await _users.GetByEmailAcrossFirmsAsync(cmd.Email, ct);
        if (user is null || !_hasher.Verify(user.PasswordHash, cmd.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Status != UserStatus.Active && user.Status != UserStatus.PendingVerification)
            throw new UnauthorizedAccessException("Account is not active.");

        if (user.TotpEnrolled)
        {
            if (string.IsNullOrEmpty(cmd.TotpCode))
                return new SignInResult(string.Empty, default, TotpRequired: true);
            if (!_totp.Verify(user.TotpSecret!, cmd.TotpCode))
            {
                user.RecordFailedSignIn(_clock.UtcNow);
                await _uow.SaveChangesAsync(ct);
                throw new UnauthorizedAccessException("Invalid TOTP code.");
            }
        }

        // MFA grace: a freshly-registered user is PendingVerification with no TOTP — auto-activate on first sign-in
        // and require enrolment on next request via a downstream "totp_required" claim if not enrolled.
        if (user.Status == UserStatus.PendingVerification)
            user.Activate(_clock.UtcNow);

        user.RecordSuccessfulSignIn(_clock.UtcNow);
        await _uow.SaveChangesAsync(ct);

        var (token, expires) = _jwt.Issue(user.FirmId, user.Id, user.Role);
        return new SignInResult(token, expires, TotpRequired: false);
    }
}
```

> Note: The audit row for sign-in is written directly via `IAuditWriter` from a domain-event listener in a later sub-plan; here we rely on `AuditingBehavior` only for `IAuditedCommand`s. `SignInCommand` deliberately does **not** implement `IAuditedCommand` because failures must also be audited. Tasks 28–29 add explicit audit calls.

- [ ] **Step 3: Append sign-in endpoint to `AuthEndpoints.cs`**

Inside `MapAuthEndpoints`, after the existing `MapPost("/enroll-totp"...)`:
```csharp
group.MapPost("/sign-in", async (SignInCommand cmd, ISender sender) =>
{
    try
    {
        var result = await sender.Send(cmd);
        return result.TotpRequired
            ? Results.Json(new { totpRequired = true }, statusCode: StatusCodes.Status200OK)
            : Results.Ok(result);
    }
    catch (FluentValidation.ValidationException ex)
    {
        return Results.ValidationProblem(ex.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous();
```

- [ ] **Step 4: Integration test — happy path with TOTP**

```csharp
using System.Net;
using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Application.Users.SignIn;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class SignInEndpointTests
{
    private readonly PostgresFixture _pg;
    public SignInEndpointTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task SignIn_returns_access_token_after_registration_without_totp()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var slug = $"signin-{Guid.NewGuid():N}".Substring(0, 12);
        var email = $"u-{Guid.NewGuid():N}@example.com";
        var pwd = "long-enough-password";

        var register = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", slug, email, pwd));
        register.IsSuccessStatusCode.Should().BeTrue();

        var signIn = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, pwd, TotpCode: null));
        signIn.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await signIn.Content.ReadFromJsonAsync<SignInResult>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SignIn_returns_401_for_wrong_password()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var email = $"u-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", $"x-{Guid.NewGuid():N}".Substring(0, 12), email, "correct-password-1"));
        var bad = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, "wrong-password-1", null));
        bad.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Step 5: Run**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj --filter FullyQualifiedName~SignInEndpoint
```
Expected: pass.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat(auth): SignIn command + endpoint with TOTP step-up; integration tests"
```

---

## Phase 8: Role-based authorization

### Task 28: Authorization policies and a role-gated admin endpoint

**Files:**
- Modify: `src/Web/Accounts.Web/Program.cs`
- Modify: `src/PracticeOperations/Accounts.PracticeOperations.Infrastructure/Endpoints/AdminEndpoints.cs`
- Test: `src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/AuthorizationTests.cs`

- [ ] **Step 1: Add authorization policies in `Program.cs`**

Replace `builder.Services.AddAuthorization();` with:
```csharp
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("RequireFirmOwner",   p => p.RequireRole("FirmOwner"));
    opts.AddPolicy("RequirePartnerOrAbove",
        p => p.RequireRole("FirmOwner", "Partner"));
    opts.AddPolicy("RequireManagerOrAbove",
        p => p.RequireRole("FirmOwner", "Partner", "Manager"));
    opts.AddPolicy("RequireStaff",
        p => p.RequireRole("FirmOwner", "Partner", "Manager", "FeeEarner", "PracticeAdmin"));
});
```

- [ ] **Step 2: Replace `AdminEndpoints.cs` (drop stub)**

```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Accounts.PracticeOperations.Infrastructure.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin");

        group.MapGet("/me", (IFirmContext ctx) => Results.Ok(new
        {
            firmId = ctx.FirmId?.Value,
            userId = ctx.UserId?.Value,
            isAuthenticated = ctx.IsAuthenticated,
        })).RequireAuthorization("RequireStaff");

        group.MapGet("/owner-only", () => Results.Ok(new { ok = true }))
            .RequireAuthorization("RequireFirmOwner");

        return app;
    }
}
```

Remove the previous `AdminEndpointsPartial` class.

- [ ] **Step 3: Authorization integration test**

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Application.Users.SignIn;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class AuthorizationTests
{
    private readonly PostgresFixture _pg;
    public AuthorizationTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Anonymous_request_to_admin_returns_401()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var response = await client.GetAsync("/api/admin/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FirmOwner_can_access_owner_only_endpoint()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var slug = $"a-{Guid.NewGuid():N}".Substring(0, 12);
        var email = $"u-{Guid.NewGuid():N}@example.com";
        var pwd = "long-enough-password";
        await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", slug, email, pwd));
        var signIn = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, pwd, null));
        var token = (await signIn.Content.ReadFromJsonAsync<SignInResult>())!.AccessToken;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/admin/owner-only");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

- [ ] **Step 4: Run**

```powershell
dotnet test src/PracticeOperations/Accounts.PracticeOperations.IntegrationTests/Accounts.PracticeOperations.IntegrationTests.csproj --filter FullyQualifiedName~Authorization
```
Expected: pass.

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(authz): role policies + role-gated admin endpoints + tests"
```

---

## Phase 9: Observability

### Task 29: Serilog with structured properties

**Files:**
- Create: `src/Web/Accounts.Web/Observability/SerilogConfig.cs`
- Modify: `src/Web/Accounts.Web/Program.cs`
- Modify: `src/Web/Accounts.Web/appsettings.json`

- [ ] **Step 1: Add Serilog packages**

```powershell
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Serilog.AspNetCore
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Serilog.Sinks.Console
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package Serilog.Sinks.Seq
```

- [ ] **Step 2: Write Serilog bootstrap**

```csharp
using Accounts.PracticeOperations.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Accounts.Web.Observability;

public static class SerilogConfig
{
    public static void Configure(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, services, cfg) =>
        {
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .Enrich.FromLogContext()
               .Enrich.WithProperty("Application", "accounts-api")
               .MinimumLevel.Information()
               .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
               .WriteTo.Console(outputTemplate:
                   "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} firm={FirmId} user={UserId} {Message:lj}{NewLine}{Exception}")
               .WriteTo.Seq(ctx.Configuration["Seq:Url"] ?? "http://localhost:5341");
        });
    }

    public static IApplicationBuilder UseTenantLogContext(this IApplicationBuilder app) =>
        app.Use(async (ctx, next) =>
        {
            var firmCtx = ctx.RequestServices.GetService<IFirmContext>();
            using (LogContext.PushProperty("FirmId", firmCtx?.FirmId?.Value ?? Guid.Empty))
            using (LogContext.PushProperty("UserId", firmCtx?.UserId?.Value ?? Guid.Empty))
            using (LogContext.PushProperty("CorrelationId", ctx.TraceIdentifier))
            {
                await next();
            }
        });
}
```

- [ ] **Step 3: Wire into `Program.cs`**

After `var builder = WebApplication.CreateBuilder(args);` add:
```csharp
Accounts.Web.Observability.SerilogConfig.Configure(builder);
```

After `app.UseAuthorization();` add:
```csharp
app.UseSerilogRequestLogging();
app.UseTenantLogContext();
```

- [ ] **Step 4: Add `Seq` config in `appsettings.json`**

```json
{
  "Seq": { "Url": "http://localhost:5341" }
}
```

- [ ] **Step 5: Run and observe**

```powershell
dotnet run --project src/Web/Accounts.Web/Accounts.Web.csproj --urls http://localhost:5080 &
Start-Sleep -Seconds 3
Invoke-RestMethod http://localhost:5080/health
```
Open Seq at http://localhost:5341 in a browser — request logs with `FirmId`, `UserId`, `CorrelationId` properties should appear (FirmId/UserId empty for the anonymous health probe; that's correct).

Stop:
```powershell
Get-Process -Name "Accounts.Web" -ErrorAction SilentlyContinue | Stop-Process
```

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat(observability): Serilog with FirmId/UserId/CorrelationId enrichers and Seq sink"
```

---

### Task 30: OpenTelemetry tracing

**Files:**
- Create: `src/Web/Accounts.Web/Observability/OpenTelemetryConfig.cs`
- Modify: `src/Web/Accounts.Web/Program.cs`

- [ ] **Step 1: Add OTel packages**

```powershell
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package OpenTelemetry.Extensions.Hosting
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package OpenTelemetry.Instrumentation.AspNetCore
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package OpenTelemetry.Instrumentation.EntityFrameworkCore --prerelease
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package OpenTelemetry.Instrumentation.Http
dotnet add src/Web/Accounts.Web/Accounts.Web.csproj package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

- [ ] **Step 2: Write OTel bootstrap**

```csharp
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Accounts.Web.Observability;

public static class OpenTelemetryConfig
{
    public static void AddTracing(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(
                serviceName: "accounts-api",
                serviceVersion: typeof(OpenTelemetryConfig).Assembly.GetName().Version?.ToString()))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation();
                t.AddHttpClientInstrumentation();
                t.AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true);

                var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                else
                    t.AddConsoleExporter();
            });
    }
}
```

- [ ] **Step 3: Wire in `Program.cs`**

After Serilog setup:
```csharp
Accounts.Web.Observability.OpenTelemetryConfig.AddTracing(builder);
```

- [ ] **Step 4: Build + run**

```powershell
dotnet build Accounts.sln
dotnet run --project src/Web/Accounts.Web/Accounts.Web.csproj --urls http://localhost:5080 &
Start-Sleep -Seconds 3
Invoke-RestMethod http://localhost:5080/health
Get-Process -Name "Accounts.Web" -ErrorAction SilentlyContinue | Stop-Process
```
Expected: trace spans printed to console (or to OTLP endpoint if configured).

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(observability): OpenTelemetry tracing for AspNetCore, EF Core, and HttpClient"
```

---

### Task 31: Correlation-ID middleware

**Files:**
- Create: `src/Web/Accounts.Web/Middleware/CorrelationIdMiddleware.cs`
- Modify: `src/Web/Accounts.Web/Program.cs`

- [ ] **Step 1: Middleware**

```csharp
namespace Accounts.Web.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        var id = ctx.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString("N");
        ctx.TraceIdentifier = id;
        ctx.Response.Headers[HeaderName] = id;
        await _next(ctx);
    }
}
```

- [ ] **Step 2: Use middleware (before `UseTenantLogContext`)**

```csharp
app.UseMiddleware<Accounts.Web.Middleware.CorrelationIdMiddleware>();
```

- [ ] **Step 3: Build + commit**

```powershell
dotnet build Accounts.sln
git add -A
git commit -m "feat(observability): correlation-id middleware, header round-trip"
```

---

## Phase 10: React + TypeScript frontend shell

### Task 32: Vite + React + TS scaffold with Tailwind

**Files:**
- Create: `client/accounts-web/package.json` etc. (via scaffold)

- [ ] **Step 1: Scaffold**

```powershell
cd C:/Repos/Claude/Accounts/client
npm create vite@latest accounts-web -- --template react-ts
cd accounts-web
npm install
```

- [ ] **Step 2: Add libraries**

```powershell
npm install react-router-dom @tanstack/react-query axios zod react-hook-form @hookform/resolvers
npm install -D tailwindcss postcss autoprefixer @types/node vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event jsdom
```

- [ ] **Step 3: Initialise Tailwind**

```powershell
npx tailwindcss init -p
```

Write `tailwind.config.ts`:
```ts
import type { Config } from 'tailwindcss';
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: { extend: {} },
  plugins: [],
} satisfies Config;
```

Write `src/styles/globals.css`:
```css
@tailwind base;
@tailwind components;
@tailwind utilities;

html, body, #root { height: 100%; }
body { @apply bg-slate-50 text-slate-900 antialiased; }
```

Import in `src/main.tsx`:
```tsx
import './styles/globals.css';
```

- [ ] **Step 4: Configure Vite proxy to API**

`vite.config.ts`:
```ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'node:path';

export default defineConfig({
  plugins: [react()],
  resolve: { alias: { '@': path.resolve(__dirname, './src') } },
  server: {
    port: 5173,
    proxy: { '/api': 'http://localhost:5080' },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./tests/setup.ts'],
    globals: true,
  },
});
```

`tests/setup.ts`:
```ts
import '@testing-library/jest-dom';
```

- [ ] **Step 5: Build + commit**

```powershell
npm run build
cd C:/Repos/Claude/Accounts
git add -A
git commit -m "feat(web-ui): Vite + React + TS + Tailwind scaffold with API proxy"
```

---

### Task 33: API client + auth context

**Files:**
- Create: `client/accounts-web/src/lib/api.ts`
- Create: `client/accounts-web/src/lib/auth.tsx`

- [ ] **Step 1: API client**

`src/lib/api.ts`:
```ts
import axios, { AxiosInstance } from 'axios';

let bearerToken: string | null = null;

export function setAuthToken(token: string | null) { bearerToken = token; }
export function getAuthToken(): string | null { return bearerToken; }

export const api: AxiosInstance = axios.create({ baseURL: '/api' });

api.interceptors.request.use(config => {
  if (bearerToken) {
    config.headers.Authorization = `Bearer ${bearerToken}`;
  }
  return config;
});

export interface RegisterFirmRequest {
  firmName: string;
  firmSlug: string;
  ownerEmail: string;
  ownerPassword: string;
}
export interface RegisterFirmResponse { firmId: string; ownerUserId: string; }

export interface SignInRequest { email: string; password: string; totpCode?: string | null; }
export interface SignInResponse {
  accessToken: string;
  expiresAt: string;
  totpRequired: boolean;
}

export const firms = {
  register: (req: RegisterFirmRequest) =>
    api.post<RegisterFirmResponse>('/firms/register', req).then(r => r.data),
};

export const auth = {
  signIn: (req: SignInRequest) =>
    api.post<SignInResponse>('/auth/sign-in', req).then(r => r.data),
  enrollTotp: () =>
    api.post<{ secret: string; otpAuthUri: string }>('/auth/enroll-totp', {}).then(r => r.data),
  me: () => api.get<{ firmId: string; userId: string; isAuthenticated: boolean }>('/admin/me').then(r => r.data),
};
```

- [ ] **Step 2: Auth context**

`src/lib/auth.tsx`:
```tsx
import { createContext, useContext, useEffect, useMemo, useState, ReactNode } from 'react';
import { setAuthToken } from './api';

interface AuthState {
  token: string | null;
  expiresAt: Date | null;
}

interface AuthContextValue extends AuthState {
  signIn: (token: string, expiresAt: string) => void;
  signOut: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const STORAGE_KEY = 'accounts.auth';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() => {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) return { token: null, expiresAt: null };
      const parsed = JSON.parse(raw) as { token: string; expiresAt: string };
      const exp = new Date(parsed.expiresAt);
      if (exp.getTime() <= Date.now()) return { token: null, expiresAt: null };
      return { token: parsed.token, expiresAt: exp };
    } catch { return { token: null, expiresAt: null }; }
  });

  useEffect(() => { setAuthToken(state.token); }, [state.token]);

  const value = useMemo<AuthContextValue>(() => ({
    ...state,
    signIn: (token, expiresAt) => {
      const exp = new Date(expiresAt);
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ token, expiresAt }));
      setState({ token, expiresAt: exp });
    },
    signOut: () => {
      sessionStorage.removeItem(STORAGE_KEY);
      setState({ token: null, expiresAt: null });
    },
  }), [state]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
```

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "feat(web-ui): api client + auth context with sessionStorage persistence"
```

---

### Task 34: Routing, protected route, top nav

**Files:**
- Create: `client/accounts-web/src/lib/routes.ts`
- Create: `client/accounts-web/src/components/ProtectedRoute.tsx`
- Create: `client/accounts-web/src/components/TopNav.tsx`
- Create: `client/accounts-web/src/App.tsx` (overwrite)
- Create: `client/accounts-web/src/main.tsx` (overwrite)

- [ ] **Step 1: Routes constants**

```ts
export const routes = {
  root: '/',
  register: '/register',
  signIn: '/sign-in',
  enrollTotp: '/enroll-totp',
  dashboard: '/dashboard',
} as const;
```

- [ ] **Step 2: Protected route**

```tsx
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/lib/auth';
import { routes } from '@/lib/routes';
import type { ReactNode } from 'react';

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { token } = useAuth();
  if (!token) return <Navigate to={routes.signIn} replace />;
  return <>{children}</>;
}
```

- [ ] **Step 3: Top nav**

```tsx
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/lib/auth';
import { routes } from '@/lib/routes';

export function TopNav() {
  const { token, signOut } = useAuth();
  const navigate = useNavigate();
  return (
    <header className="border-b bg-white">
      <nav className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
        <Link to={routes.root} className="text-lg font-semibold">Accounts</Link>
        <div className="flex items-center gap-4 text-sm">
          {token ? (
            <>
              <Link to={routes.dashboard}>Dashboard</Link>
              <button
                className="rounded border px-3 py-1 hover:bg-slate-100"
                onClick={() => { signOut(); navigate(routes.signIn); }}>
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link to={routes.signIn}>Sign in</Link>
              <Link to={routes.register} className="rounded bg-slate-900 px-3 py-1 text-white">
                Register firm
              </Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
}
```

- [ ] **Step 4: App + main**

`src/main.tsx`:
```tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './lib/auth';
import App from './App';
import './styles/globals.css';

const qc = new QueryClient();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <QueryClientProvider client={qc}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </QueryClientProvider>
    </BrowserRouter>
  </StrictMode>
);
```

`src/App.tsx`:
```tsx
import { Routes, Route, Navigate } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { RegisterPage } from '@/pages/RegisterPage';
import { SignInPage } from '@/pages/SignInPage';
import { EnrollTotpPage } from '@/pages/EnrollTotpPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { routes } from '@/lib/routes';

export default function App() {
  return (
    <>
      <TopNav />
      <main className="mx-auto max-w-6xl p-6">
        <Routes>
          <Route path={routes.root} element={<Navigate to={routes.signIn} replace />} />
          <Route path={routes.register} element={<RegisterPage />} />
          <Route path={routes.signIn} element={<SignInPage />} />
          <Route path={routes.enrollTotp}
            element={<ProtectedRoute><EnrollTotpPage /></ProtectedRoute>} />
          <Route path={routes.dashboard}
            element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
        </Routes>
      </main>
    </>
  );
}
```

- [ ] **Step 5: Commit**

```powershell
git add -A
git commit -m "feat(web-ui): routing, ProtectedRoute, TopNav, App composition"
```

---

### Task 35: Register, Sign-in, EnrollTotp, Dashboard pages

**Files:**
- Create: `client/accounts-web/src/pages/RegisterPage.tsx`
- Create: `client/accounts-web/src/pages/SignInPage.tsx`
- Create: `client/accounts-web/src/pages/EnrollTotpPage.tsx`
- Create: `client/accounts-web/src/pages/DashboardPage.tsx`

- [ ] **Step 1: Register page**

```tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { firms } from '@/lib/api';
import { routes } from '@/lib/routes';

export function RegisterPage() {
  const [firmName, setFirmName] = useState('');
  const [firmSlug, setFirmSlug] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await firms.register({
        firmName, firmSlug, ownerEmail: email, ownerPassword: password,
      });
      navigate(routes.signIn);
    } catch (err: any) {
      setError(err?.response?.data?.error ?? 'Registration failed');
    }
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Register your firm</h1>
      <form className="space-y-4" onSubmit={submit}>
        <Field label="Firm name" value={firmName} onChange={setFirmName} />
        <Field label="Firm slug (kebab-case)" value={firmSlug} onChange={setFirmSlug} />
        <Field label="Owner email" value={email} onChange={setEmail} type="email" />
        <Field label="Password (min 12 chars)" value={password} onChange={setPassword} type="password" />
        {error && <p className="text-sm text-red-700">{error}</p>}
        <button type="submit" className="w-full rounded bg-slate-900 px-4 py-2 text-white">
          Register
        </button>
      </form>
    </div>
  );
}

function Field({ label, value, onChange, type = 'text' }: {
  label: string; value: string; onChange: (v: string) => void; type?: string;
}) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm">{label}</span>
      <input
        className="w-full rounded border px-3 py-2"
        type={type} value={value}
        onChange={e => onChange(e.target.value)}
        required />
    </label>
  );
}
```

- [ ] **Step 2: Sign-in page**

```tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { auth } from '@/lib/api';
import { useAuth } from '@/lib/auth';
import { routes } from '@/lib/routes';

export function SignInPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [totpCode, setTotpCode] = useState('');
  const [needsTotp, setNeedsTotp] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { signIn } = useAuth();
  const navigate = useNavigate();

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      const result = await auth.signIn({ email, password, totpCode: needsTotp ? totpCode : null });
      if (result.totpRequired) { setNeedsTotp(true); return; }
      signIn(result.accessToken, result.expiresAt);
      navigate(routes.dashboard);
    } catch (err: any) {
      setError(err?.response?.status === 401 ? 'Invalid credentials.' : 'Sign-in failed.');
    }
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Sign in</h1>
      <form className="space-y-4" onSubmit={submit}>
        <label className="block">
          <span className="mb-1 block text-sm">Email</span>
          <input className="w-full rounded border px-3 py-2" type="email"
            value={email} onChange={e => setEmail(e.target.value)} required />
        </label>
        <label className="block">
          <span className="mb-1 block text-sm">Password</span>
          <input className="w-full rounded border px-3 py-2" type="password"
            value={password} onChange={e => setPassword(e.target.value)} required />
        </label>
        {needsTotp && (
          <label className="block">
            <span className="mb-1 block text-sm">6-digit authenticator code</span>
            <input className="w-full rounded border px-3 py-2 font-mono tracking-widest"
              inputMode="numeric" maxLength={6}
              value={totpCode} onChange={e => setTotpCode(e.target.value.replace(/\D/g, ''))} />
          </label>
        )}
        {error && <p className="text-sm text-red-700">{error}</p>}
        <button type="submit" className="w-full rounded bg-slate-900 px-4 py-2 text-white">
          {needsTotp ? 'Verify and sign in' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}
```

- [ ] **Step 3: Enroll TOTP page**

```tsx
import { useEffect, useState } from 'react';
import { auth } from '@/lib/api';

export function EnrollTotpPage() {
  const [secret, setSecret] = useState<string | null>(null);
  const [uri, setUri] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    auth.enrollTotp()
      .then(r => { setSecret(r.secret); setUri(r.otpAuthUri); })
      .catch(() => setError('Could not enroll TOTP.'));
  }, []);

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-4 text-2xl font-semibold">Enrol TOTP MFA</h1>
      {error && <p className="text-sm text-red-700">{error}</p>}
      {secret && (
        <div className="space-y-3">
          <p className="text-sm">
            Scan this URI in your authenticator app (Microsoft Authenticator, 1Password, etc.),
            or enter the secret manually.
          </p>
          <pre className="rounded border bg-slate-50 p-3 text-xs break-all">{uri}</pre>
          <p className="text-sm">Secret (manual entry): <span className="font-mono">{secret}</span></p>
          <p className="text-sm text-slate-600">
            On your next sign-in, you'll be prompted for a 6-digit code from your app.
          </p>
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 4: Dashboard page (calls `/api/admin/me`)**

```tsx
import { useQuery } from '@tanstack/react-query';
import { auth } from '@/lib/api';
import { Link } from 'react-router-dom';
import { routes } from '@/lib/routes';

export function DashboardPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['me'], queryFn: auth.me,
  });

  return (
    <div>
      <h1 className="mb-4 text-2xl font-semibold">Dashboard</h1>
      {isLoading && <p>Loading…</p>}
      {error && <p className="text-red-700">Could not load profile.</p>}
      {data && (
        <dl className="grid grid-cols-2 gap-x-4 gap-y-2 text-sm">
          <dt className="font-semibold">Firm ID</dt><dd className="font-mono">{data.firmId}</dd>
          <dt className="font-semibold">User ID</dt><dd className="font-mono">{data.userId}</dd>
        </dl>
      )}
      <p className="mt-6">
        <Link className="underline" to={routes.enrollTotp}>Enrol MFA</Link>
      </p>
    </div>
  );
}
```

- [ ] **Step 5: Manually verify the full flow**

```powershell
# In one terminal: backend
dotnet run --project src/Web/Accounts.Web/Accounts.Web.csproj --urls http://localhost:5080

# In another: frontend
cd client/accounts-web; npm run dev
```
Open http://localhost:5173 — register a firm, sign in, land on Dashboard, enrol TOTP, sign out, sign in again with TOTP. All flows should work.

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "feat(web-ui): Register / SignIn / EnrollTotp / Dashboard pages"
```

---

## Phase 11: Containers and CI

### Task 36: Dockerfile for the API

**Files:**
- Create: `src/Web/Accounts.Web/Dockerfile`
- Create: `.dockerignore`

- [ ] **Step 1: `.dockerignore` at repo root**

```
**/bin/
**/obj/
**/node_modules/
**/dist/
**/.vs/
**/.idea/
**/TestResults/
.git/
docker/
client/
```

- [ ] **Step 2: API Dockerfile**

`src/Web/Accounts.Web/Dockerfile`:
```dockerfile
# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY src/ ./src/
RUN dotnet restore src/Web/Accounts.Web/Accounts.Web.csproj
RUN dotnet publish src/Web/Accounts.Web/Accounts.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Accounts.Web.dll"]
```

- [ ] **Step 3: Build the image**

```powershell
docker build -f src/Web/Accounts.Web/Dockerfile -t accounts-api:dev .
```
Expected: image built successfully.

- [ ] **Step 4: Commit**

```powershell
git add -A
git commit -m "chore(docker): API Dockerfile and .dockerignore"
```

---

### Task 37: Dockerfile for the frontend

**Files:**
- Create: `client/accounts-web/Dockerfile`

- [ ] **Step 1: Frontend Dockerfile (multi-stage with nginx)**

```dockerfile
# syntax=docker/dockerfile:1.7

FROM node:22-alpine AS build
WORKDIR /app
COPY client/accounts-web/package*.json ./
RUN npm ci
COPY client/accounts-web/ ./
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY client/accounts-web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

`client/accounts-web/nginx.conf`:
```nginx
server {
  listen 80;
  root /usr/share/nginx/html;
  index index.html;
  location / { try_files $uri /index.html; }
  location /api/ { proxy_pass http://accounts-api:8080/api/; }
}
```

- [ ] **Step 2: Build**

```powershell
docker build -f client/accounts-web/Dockerfile -t accounts-web:dev .
```

- [ ] **Step 3: Commit**

```powershell
git add -A
git commit -m "chore(docker): frontend Dockerfile + nginx reverse proxy"
```

---

### Task 38: GitHub Actions — backend build + test

**Files:**
- Create: `.github/workflows/backend.yml`

- [ ] **Step 1: Workflow**

```yaml
name: backend
on:
  push:
    branches: [main]
    paths: ['src/**', '.github/workflows/backend.yml', 'Directory.*.props', 'Accounts.sln']
  pull_request:
    paths: ['src/**', '.github/workflows/backend.yml', 'Directory.*.props', 'Accounts.sln']

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    timeout-minutes: 30
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.0.x' }
      - run: dotnet restore Accounts.sln
      - run: dotnet build Accounts.sln --no-restore --configuration Release
      - run: dotnet test Accounts.sln --no-build --configuration Release --logger "trx;LogFileName=test.trx"
      - uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results
          path: '**/TestResults/*.trx'
```

> Testcontainers needs Docker; `ubuntu-latest` runners have Docker preinstalled.

- [ ] **Step 2: Commit**

```powershell
git add -A
git commit -m "chore(ci): backend GitHub Actions workflow"
```

---

### Task 39: GitHub Actions — frontend build + lint + test

**Files:**
- Create: `.github/workflows/frontend.yml`
- Create: `client/accounts-web/eslint.config.js` (Vite scaffold ships one; confirm)

- [ ] **Step 1: Workflow**

```yaml
name: frontend
on:
  push:
    branches: [main]
    paths: ['client/**', '.github/workflows/frontend.yml']
  pull_request:
    paths: ['client/**', '.github/workflows/frontend.yml']

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    timeout-minutes: 15
    defaults: { run: { working-directory: client/accounts-web } }
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '22', cache: 'npm', cache-dependency-path: client/accounts-web/package-lock.json }
      - run: npm ci
      - run: npm run lint
      - run: npm run test --if-present -- --run
      - run: npm run build
```

Add an `lint` script if Vite didn't already; check `package.json` has:
```json
"scripts": {
  "dev": "vite",
  "build": "tsc -b && vite build",
  "lint": "eslint .",
  "preview": "vite preview",
  "test": "vitest"
}
```

- [ ] **Step 2: Commit**

```powershell
git add -A
git commit -m "chore(ci): frontend GitHub Actions workflow"
```

---

## Phase 12: End-to-end happy path

### Task 40: Playwright test — register, sign in, land on dashboard

**Files:**
- Create: `client/accounts-web/playwright.config.ts`
- Create: `client/accounts-web/e2e/signin.spec.ts`
- Create: `.github/workflows/e2e.yml`

- [ ] **Step 1: Install Playwright**

```powershell
cd client/accounts-web
npm install -D @playwright/test
npx playwright install --with-deps chromium
```

- [ ] **Step 2: Config**

`client/accounts-web/playwright.config.ts`:
```ts
import { defineConfig, devices } from '@playwright/test';
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  timeout: 30_000,
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
```

- [ ] **Step 3: E2E spec**

`client/accounts-web/e2e/signin.spec.ts`:
```ts
import { test, expect } from '@playwright/test';

test('register firm, sign in, land on dashboard', async ({ page }) => {
  const slug = `e2e-${Math.random().toString(36).slice(2, 10)}`;
  const email = `owner-${Math.random().toString(36).slice(2, 10)}@example.com`;
  const password = 'long-enough-password-1';

  await page.goto('/register');
  await page.getByLabel('Firm name').fill('E2E Co');
  await page.getByLabel('Firm slug (kebab-case)').fill(slug);
  await page.getByLabel('Owner email').fill(email);
  await page.getByLabel('Password (min 12 chars)').fill(password);
  await page.getByRole('button', { name: 'Register' }).click();

  await page.waitForURL('**/sign-in');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();

  await page.waitForURL('**/dashboard');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  await expect(page.getByText(/Firm ID/)).toBeVisible();
});
```

- [ ] **Step 4: Run locally**

In two background terminals: the API (`dotnet run --project ...Web --urls http://localhost:5080`) and the frontend (`npm run dev` on port 5173).

```powershell
cd client/accounts-web
npx playwright test
```
Expected: test passes.

- [ ] **Step 5: CI workflow**

`.github/workflows/e2e.yml`:
```yaml
name: e2e
on:
  push: { branches: [main] }
  pull_request:

jobs:
  e2e:
    runs-on: ubuntu-latest
    timeout-minutes: 30
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_USER: accounts
          POSTGRES_PASSWORD: accounts_dev_password
          POSTGRES_DB: accounts_dev
        ports: ['5432:5432']
        options: >-
          --health-cmd "pg_isready -U accounts -d accounts_dev"
          --health-interval 5s --health-timeout 5s --health-retries 10
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.0.x' }
      - uses: actions/setup-node@v4
        with: { node-version: '22', cache: 'npm', cache-dependency-path: client/accounts-web/package-lock.json }
      - run: dotnet restore Accounts.sln
      - run: dotnet build src/Web/Accounts.Web/Accounts.Web.csproj -c Release
      - name: Start API
        run: |
          dotnet run --project src/Web/Accounts.Web/Accounts.Web.csproj -c Release \
            --urls http://localhost:5080 &
          npx wait-on http://localhost:5080/health
      - name: Install & start frontend
        working-directory: client/accounts-web
        run: |
          npm ci
          npm run build
          npm install -g serve
          serve -s dist -l 5173 &
          npx wait-on http://localhost:5173
      - name: Playwright
        working-directory: client/accounts-web
        run: |
          npx playwright install --with-deps chromium
          npx playwright test
      - uses: actions/upload-artifact@v4
        if: always()
        with:
          name: playwright-report
          path: client/accounts-web/playwright-report/
```

- [ ] **Step 6: Commit**

```powershell
git add -A
git commit -m "test(e2e): playwright register→signin→dashboard happy path + CI"
```

---

## Self-Review

This plan was checked against `PROJECT-STATE.md` "Foundation Core" scope. Coverage summary:

| Foundation Core requirement | Covered by |
|---|---|
| .NET 10 solution scaffold with Clean Architecture per bounded context | Task 1, 2 |
| Multi-tenant data isolation (`FirmId` row-level partitioning) | Task 9, 10, 11 |
| Tenant lifecycle (signup → trial → paid skeleton) | Task 17 (Firm.Register → Trial; Firm.Activate → Active) |
| Tenant offboarding + 30-day retention | **Deferred** to Sub-plan 1b alongside data-class/retention framework |
| Email/password + TOTP authentication | Task 21, 24, 25, 27 |
| MFA mandatory for staff | Step-up in Task 27; **strict enforcement** (deny sign-in if not enrolled after grace period) belongs in Sub-plan 1b — flagged below |
| Role model (FirmOwner / Partner / Manager / FeeEarner / PracticeAdmin) | Task 16, 28 |
| MLRO / DPO overlays | **Deferred** to Sub-plan 3 (when AML lifecycle needs them) |
| Append-only immutable audit log | Task 12, 13, 14 |
| PostgreSQL + EF Core | Task 3, 5, 8 |
| Observability: Serilog + OpenTelemetry + per-tenant log context | Task 29, 30, 31 |
| CI/CD scaffold | Task 38, 39, 40 |
| React + TypeScript admin shell | Task 32–35 |
| Azure deployment target | **Deferred** to Sub-plan 1b (paired with SSO+integration once the deployment surface is bigger) |
| Cyber Essentials Plus baseline (MFA, encryption at rest/in transit, backup) | MFA: Task 27; in-transit (HTTPS): default ASP.NET Core; at-rest: Postgres TDE / cloud-provided; backup: belongs in operational runbook |
| End-to-end proof: register → MFA sign-in → tenant-scoped page | Task 40 |

**Items flagged out for Sub-plan 1b** (transparently moved, not lost):
1. Strict MFA enforcement (deny non-MFA staff sign-in after grace period)
2. Firm offboarding + 30-day retention
3. Azure-specific deployment workflow (Container Apps or App Service)
4. SSO (M365 + Google) — was always Sub-plan 1b
5. Integration-framework skeleton — was always Sub-plan 1b
6. Data-class tagging + retention policy framework — was always Sub-plan 1b

**Placeholder scan:** None found. Every code block contains the actual implementation; every command shows expected output; cross-task type names (`FirmId`, `UserId`, `IFirmContext`, `IAuditWriter`, `RegisterFirmCommand`, `SignInCommand`, etc.) are consistent.

**Type consistency check passed:**
- `FirmId.Value` is `Guid` (Task 4) — consumed as `Guid` in `JwtIssuer` claims (Task 26), `RegisterFirmResult` (Task 22), `/api/admin/me` (Task 28). ✓
- `IFirmContext.FirmId` / `UserId` return nullable strongly-typed structs (Task 9) — used consistently in `EfAuditWriter`, `EnrollTotpHandler`, `AdminEndpoints`. ✓
- `AuditingBehavior` reads `IAuditedCommand` (Task 14) — only `EnrollTotpCommand` (Task 25) currently implements it; deliberately, `SignInCommand` does **not** because failures must also be audited (note recorded in Task 27).

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-11-foundation-core.md`. Two execution options:

**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration. Best for the size of this plan (40 tasks) because context stays clean and review is per-task.

**2. Inline Execution** — Execute tasks in this session using `superpowers:executing-plans`, batch execution with checkpoints for review.

Which approach?

