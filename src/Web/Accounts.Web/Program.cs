using System.Security.Claims;
using System.Text;
using Accounts.PracticeOperations.Infrastructure;
using Accounts.PracticeOperations.Infrastructure.Endpoints;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.Web.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "accounts", status = "ok" }));

app.MapPracticeOperations();

app.Run();

public partial class Program { }   // for WebApplicationFactory<Program>
