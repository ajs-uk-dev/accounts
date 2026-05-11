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
