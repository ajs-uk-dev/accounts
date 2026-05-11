using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Accounts.Web.Observability;

public static class OpenTelemetryConfig
{
    public static void AddTracing(WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(
                serviceName: "accounts-api",
                serviceVersion: typeof(OpenTelemetryConfig).Assembly.GetName().Version?.ToString()))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation();
                t.AddHttpClientInstrumentation();
                t.AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true);

                var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            });
    }
}
