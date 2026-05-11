using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Infrastructure.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.SharedKernel.Time;
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
