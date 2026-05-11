using Accounts.PracticeOperations.Infrastructure;
using Accounts.PracticeOperations.Infrastructure.Endpoints;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.Web.Auth;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPracticeOperations(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Accounts.PracticeOperations.Application.Abstractions.IFirmContext,
                          Accounts.Web.Auth.FirmContextAccessor>();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<PracticeOperationsDbContext>("practice-operations-db");

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "accounts", status = "ok" }));

app.MapPracticeOperations();

app.Run();

public partial class Program { }   // for WebApplicationFactory<Program>
