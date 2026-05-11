using System;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
        // Apply via host configuration so the value is visible to WebApplication.CreateBuilder
        // in Program.cs. ConfigureAppConfiguration alone does not always win in minimal-hosting
        // when the SUT uses appsettings.json — host configuration runs first and feeds the
        // builder's configuration chain.
        builder.UseSetting("ConnectionStrings:PracticeOperations", _connectionString);
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
