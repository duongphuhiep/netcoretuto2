using Elastic.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Text;
namespace GenericAspApi;

public static class AppFactory
{
    //static bool ShouldCollectTrace(PathString path) =>
    //        !path.HasValue ||
    //        (!path.Value.StartsWith("/metrics", StringComparison.InvariantCultureIgnoreCase)
    //        && !path.Value.StartsWith("/health", StringComparison.InvariantCultureIgnoreCase)
    //        && !path.Value.StartsWith("/swagger", StringComparison.InvariantCultureIgnoreCase)
    //        && !path.Value.StartsWith("/_framework", StringComparison.InvariantCultureIgnoreCase)
    //        && !path.Value.StartsWith("/_vs", StringComparison.InvariantCultureIgnoreCase)
    //        && !path.Value.StartsWith("/version", StringComparison.InvariantCultureIgnoreCase));

    static bool ShouldCollectTrace(PathString path) => path.HasValue && path.Value.StartsWith("/api", StringComparison.InvariantCultureIgnoreCase);

    public static WebApplication Create()
    {
        var builder = WebApplication.CreateBuilder();

        var config = builder.Configuration;

        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        var applicationName = config?["ApplicationName"]
            ?? assemblyName.Name
            ?? "Noname";
        var applicationVersion = config?["ApplicationVersion"]
            ?? assemblyName.Version?.ToString()
            ?? "0.0.0.0";

        builder.Environment.ApplicationName = applicationName;

        var otel = builder.Services.AddOpenTelemetry();
        otel.ConfigureResource(resource => resource
            .AddService(serviceName: builder.Environment.ApplicationName, serviceVersion: applicationVersion));
        otel.WithTracing(tracing =>
        {
            tracing
                .AddSource(applicationName)
                .AddAspNetCoreInstrumentation(istOptions =>
                {
                    istOptions.Filter = httpContext => ShouldCollectTrace(httpContext.Request.Path);
                })
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(config?["JaegerGrpcUrl"] ?? "http://localhost:4317");
                    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    //otlpOptions.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
                });
        });

        builder.Services.AddControllers();

        builder.Services
            .AddHttpContextAccessor()
            .AddHttpClient()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddSeq(config?.GetRequiredSection("Logging:Seq"));
                loggingBuilder.AddElasticsearch(elasticConf =>
                {
                    elasticConf.ShipTo = config?.GetRequiredSection("Logging:Elastic").Get<Elastic.Extensions.Logging.Options.ShipToOptions>()!;
                });
            })
            .AddHttpLogging(opts =>
            {
                opts.CombineLogs = true;
                opts.LoggingFields = HttpLoggingFields.All;

                //log the body as text for the following types of requests
                opts.MediaTypeOptions.AddText("application/json", Encoding.UTF8);
                opts.MediaTypeOptions.AddText("text/json", Encoding.UTF8);
                opts.MediaTypeOptions.AddText("text/plain", Encoding.UTF8);
                opts.MediaTypeOptions.AddText("application/xml", Encoding.UTF8);
                opts.MediaTypeOptions.AddText("application/x-www-form-urlencoded", Encoding.UTF8);
                opts.MediaTypeOptions.AddText("multipart/form-data", Encoding.UTF8);
                opts.RequestBodyLogLimit = 4096;
                opts.ResponseBodyLogLimit = 4096;
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //log all request and response
        app.UseHttpLogging();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
