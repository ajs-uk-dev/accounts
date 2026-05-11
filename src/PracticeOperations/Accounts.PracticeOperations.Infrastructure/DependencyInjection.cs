using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Application.Behaviors;
using Accounts.PracticeOperations.Infrastructure.Audit;
using Accounts.PracticeOperations.Infrastructure.Auth;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.Infrastructure.Persistence.Repositories;
using Accounts.SharedKernel.Time;
using MediatR;
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
                    npgsql.MigrationsHistoryTable("__ef_migrations", "practice_operations"))
                .UseSnakeCaseNamingConvention());

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditWriter, EfAuditWriter>();
        services.AddScoped<IFirmRepository, EfFirmRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IFirmContext).Assembly); // Application asm
        });
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditingBehavior<,>));

        return services;
    }
}
