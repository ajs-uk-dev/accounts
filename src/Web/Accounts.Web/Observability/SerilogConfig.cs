using System.Globalization;
using Accounts.PracticeOperations.Application.Abstractions;
using Serilog;
using Serilog.AspNetCore;
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
            var firmId = firmCtx?.FirmId?.Value ?? Guid.Empty;
            var userId = firmCtx?.UserId?.Value ?? Guid.Empty;
            var correlationId = ctx.TraceIdentifier;

            // Push to LogContext for all log events emitted from within this request.
            using (LogContext.PushProperty("FirmId", firmId))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                // Also set on IDiagnosticContext so the Serilog request-completion event
                // (emitted by UseSerilogRequestLogging after the inner middleware unwinds)
                // captures FirmId, UserId and CorrelationId even though LogContext using-blocks
                // have been disposed by the time that emit fires.
                var diagCtx = ctx.RequestServices.GetService<IDiagnosticContext>();
                if (diagCtx is not null)
                {
                    diagCtx.Set("FirmId", firmId);
                    diagCtx.Set("UserId", userId);
                    diagCtx.Set("CorrelationId", correlationId);
                }

                await next();
            }
        });
}
