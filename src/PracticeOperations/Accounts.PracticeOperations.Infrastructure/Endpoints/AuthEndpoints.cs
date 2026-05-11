using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Application.Users.EnrollTotp;
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

        // Sign-in lives in Task 27.
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
