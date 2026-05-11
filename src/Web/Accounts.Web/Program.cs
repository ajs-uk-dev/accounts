using System.Security.Claims;
using System.Text;
using Accounts.PracticeOperations.Infrastructure;
using Accounts.PracticeOperations.Infrastructure.Endpoints;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.Web.Auth;
using Accounts.Web.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Fail-fast on missing / empty / short JWT configuration.
// HMAC-SHA256 requires a key of at least 256 bits (32 bytes).
ValidateJwtConfig(builder.Configuration);

SerilogConfig.Configure(builder);
OpenTelemetryConfig.AddTracing(builder);

builder.Services.AddPracticeOperations(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Accounts.PracticeOperations.Application.Abstractions.IFirmContext,
                          Accounts.Web.Auth.FirmContextAccessor>();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<PracticeOperationsDbContext>("practice-operations-db");

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

var app = builder.Build();

app.UseMiddleware<Accounts.Web.Middleware.CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantLogContext();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "accounts", status = "ok" }));

app.MapPracticeOperations();

app.Run();

static void ValidateJwtConfig(IConfiguration config)
{
    static void Require(IConfiguration cfg, string key)
    {
        var value = cfg[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Required configuration key '{key}' is missing or empty.");
    }

    Require(config, "Jwt:Issuer");
    Require(config, "Jwt:Audience");
    Require(config, "Jwt:Secret");

    var secret = config["Jwt:Secret"]!;
    if (Encoding.UTF8.GetByteCount(secret) < 32)
        throw new InvalidOperationException(
            $"'Jwt:Secret' is too short: HMAC-SHA256 requires a key of at least 32 bytes (256 bits).");
}

public partial class Program { }   // for WebApplicationFactory<Program>
