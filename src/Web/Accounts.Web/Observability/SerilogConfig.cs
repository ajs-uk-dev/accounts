using System.Globalization;
using Accounts.PracticeOperations.Application.Abstractions;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Accounts.Web.Observability;

public static class SerilogConfig
{
    public static void Configure(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, services, cfg) =>
        {
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .Enrich.FromLogContext()
               .Enrich.WithProperty("Application", "accounts-api")
               .MinimumLevel.Information()
               .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
               .WriteTo.Console(
                   outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} firm={FirmId} user={UserId} {Message:lj}{NewLine}{Exception}",
                   formatProvider: CultureInfo.InvariantCulture)
               .WriteTo.Seq(
                   serverUrl: ctx.Configuration["Seq:Url"] ?? "http://localhost:5341",
                   formatProvider: CultureInfo.InvariantCulture);
        });
    }

    public static IApplicationBuilder UseTenantLogContext(this IApplicationBuilder app) =>
        app.Use(async (ctx, next) =>
        {
            var firmCtx = ctx.RequestServices.GetService<IFirmContext>();
            using (LogContext.PushProperty("FirmId", firmCtx?.FirmId?.Value ?? Guid.Empty))
            using (LogContext.PushProperty("UserId", firmCtx?.UserId?.Value ?? Guid.Empty))
            using (LogContext.PushProperty("CorrelationId", ctx.TraceIdentifier))
            {
                await next();
            }
        });
}
