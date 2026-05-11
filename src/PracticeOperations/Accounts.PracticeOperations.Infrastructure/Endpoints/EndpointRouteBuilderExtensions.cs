using Microsoft.AspNetCore.Routing;

namespace Accounts.PracticeOperations.Infrastructure.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPracticeOperations(this IEndpointRouteBuilder app)
    {
        app.MapFirmsEndpoints();
        app.MapAuthEndpoints();
        app.MapAdminEndpoints();
        return app;
    }
}
