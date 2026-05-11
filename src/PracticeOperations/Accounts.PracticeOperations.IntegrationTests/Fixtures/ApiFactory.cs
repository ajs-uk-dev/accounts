using System;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable EF1001 // Internal EF API — ReplaceService is the supported hook for customising EF's internal SP in tests

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
        // Apply via host configuration so the value is visible to WebApplication.CreateBuilder
        // in Program.cs. ConfigureAppConfiguration alone does not always win in minimal-hosting
        // when the SUT uses appsettings.json — host configuration runs first and feeds the
        // builder's configuration chain.
        builder.UseSetting("ConnectionStrings:PracticeOperations", _connectionString);
        // JWT config for the Testing environment (Task 26). Production deploys must override
        // these via env vars; the Development overlay carries the dev values; tests get a
        // self-contained set so JwtBearer can construct its signing key without choking.
        builder.UseSetting("Jwt:Issuer", "accounts-test");
        builder.UseSetting("Jwt:Audience", "accounts-test");
        builder.UseSetting("Jwt:Secret", "TEST-SECRET-do-not-use-in-prod-min-32-chars-xx");
        builder.UseSetting("Jwt:LifetimeMinutes", "60");
        builder.ConfigureServices(services =>
        {
            // Re-register the DbContext with:
            //   (a) ReplaceService<IModelCustomizer, TestModelCustomizer>() — tells EF's
            //       internal service provider to use our customizer, which adds the
            //       TenantTestRow test-only entity to the model.
            //   (b) ConfigureWarnings(Ignore PendingModelChangesWarning) — the test model
            //       intentionally includes TenantTestRow which is absent from the production
            //       migration snapshot, so we suppress the warning that would otherwise throw.
            services.RemoveAll<DbContextOptions<PracticeOperationsDbContext>>();
            services.AddDbContext<PracticeOperationsDbContext>(opts =>
            {
                opts.UseNpgsql(_connectionString, npgsql =>
                        npgsql.MigrationsHistoryTable("__ef_migrations", "practice_operations"))
                    .UseSnakeCaseNamingConvention()
                    .ReplaceService<IModelCustomizer, TestModelCustomizer>()
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);

            _configureServices?.Invoke(services);
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Database.Migrate();

            // The production migrations drop tenant_test_rows (it was removed from production
            // schema in DropTenantTestRowFromProductionSchema).  Re-create it for tests so that
            // TenantIsolationTests can use it to exercise the query filter.
            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS practice_operations.tenant_test_rows (
                    id uuid NOT NULL,
                    firm_id uuid NOT NULL,
                    label character varying(200) NOT NULL,
                    CONSTRAINT pk_tenant_test_rows PRIMARY KEY (id)
                );
                CREATE INDEX IF NOT EXISTS ix_tenant_test_rows_firm_id
                    ON practice_operations.tenant_test_rows (firm_id);
            ");
        });
    }
}
