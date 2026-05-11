using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.SharedKernel.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Accounts.PracticeOperations.Infrastructure.Endpoints;

public static class FirmsEndpoints
{
    public static IEndpointRouteBuilder MapFirmsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/firms").WithTags("Firms");

        group.MapPost("/register", async (RegisterFirmCommand cmd, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(cmd);
                return Results.Created($"/api/firms/{result.FirmId}", result);
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(
                    ex.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
            }
            catch (ConflictException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .AllowAnonymous();

        return app;
    }
}
