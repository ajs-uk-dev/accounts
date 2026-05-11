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
