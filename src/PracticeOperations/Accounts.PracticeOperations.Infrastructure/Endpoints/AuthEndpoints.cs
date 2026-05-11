using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Application.Users.EnrollTotp;
using Accounts.PracticeOperations.Application.Users.SignIn;
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

        group.MapPost("/sign-in", async (SignInCommand cmd, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(cmd);
                return result.TotpRequired
                    ? Results.Json(new { totpRequired = true }, statusCode: StatusCodes.Status200OK)
                    : Results.Ok(result);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return Results.ValidationProblem(ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        }).AllowAnonymous();

        return app;
    }
}
