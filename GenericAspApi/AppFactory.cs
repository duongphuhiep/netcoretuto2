using Elastic.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Text;
namespace GenericAspApi;

public static class AppFactory
{
    public static WebApplication Create()
    {
        WebApplicationBuilder builder = ConfigureServices();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        #region default endpoint

        // Uncomment the following line to enable the Prometheus endpoint(requires the OpenTelemetry.Exporter.Prometheus.AspNetCore package)
        // app.MapPrometheusScrapingEndpoint();

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        #endregion

        app.UseHttpsRedirection();

        //log all request and response
        app.UseHttpLogging();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    private static WebApplicationBuilder ConfigureServices()
    {
        var builder = WebApplication.CreateBuilder();

        var config = builder.Configuration;

        #region Service Discovery

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            //http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.UseServiceDiscovery();
        });

        #endregion

        #region Get application name

        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        var applicationName = config?["ApplicationName"]
            ?? assemblyName.Name
            ?? "Noname";
        var applicationVersion = config?["ApplicationVersion"]
            ?? assemblyName.Version?.ToString()
            ?? "0.0.0.0";
        builder.Environment.ApplicationName = applicationName;

        #endregion

        #region Get Config Elastic & Seq

        var elasticShipToOptions = config?.GetSection("Logging:Elastic").Get<Elastic.Extensions.Logging.Options.ShipToOptions>();
        var seqConfig = config?.GetSection("Logging:Seq");
        var seqServerUrlRaw = seqConfig?["ServerUrl"];
        var seqServerUrl = string.IsNullOrEmpty(seqServerUrlRaw) ? null : new Uri(seqServerUrlRaw);

        #endregion

        #region Logging to Seq Elastic and Http Logging Middleware

        builder.Services
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                if (seqServerUrl is not null)
                {
                    loggingBuilder.AddSeq(seqConfig);
                }
                if (elasticShipToOptions is not null)
                {
                    loggingBuilder.AddElasticsearch(elasticConf =>
                    {
                        elasticConf.ShipTo = elasticShipToOptions;
                    });
                }
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

        #endregion

        #region OpenTelemetry

        var otlpExporterUri = GetOtelExporterGrpcEndpoint(config);
        var otel = builder.Services.AddOpenTelemetry();
        otel.ConfigureResource(resource => resource
            .AddService(serviceName: builder.Environment.ApplicationName, serviceVersion: applicationVersion));
        otel.WithTracing(tracing =>
        {
            tracing
                .AddSource(applicationName)
                .AddAspNetCoreInstrumentation(istOptions =>
                {
                    istOptions.Filter = httpContext => ShouldCollectIncomingRequestTrace(httpContext.Request.Path);
                })
                .AddHttpClientInstrumentation(istOptions =>
                {
                    istOptions.FilterHttpRequestMessage = httpRequestMessage => ShouldCollectOutgoingRequestTrace(httpRequestMessage.RequestUri, seqServerUrl, elasticShipToOptions);
                })
                //.AddConsoleExporter()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = otlpExporterUri;
                    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    //otlpOptions.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
                });
            if (builder.Environment.IsDevelopment())
            {
                // We want to view all traces in development
                tracing.SetSampler(new AlwaysOnSampler());
            }
        });
        otel.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddProcessInstrumentation()
                   .AddRuntimeInstrumentation();
        });

        //log the trace
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = otlpExporterUri;
                otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        });


        //var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        //if (useOtlpExporter)
        //{
        //    builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        //    builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
        //    builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        //}


        #endregion

        #region Health check

        builder.Services.AddHealthChecks()
           // Add a default liveness check to ensure app is responsive
           .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        #endregion

        builder.Services.AddControllers();

        builder.Services
            .AddHttpContextAccessor()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();

        return builder;
    }

    static bool ShouldCollectIncomingRequestTrace(PathString path) => path.HasValue && path.Value.StartsWith("/api", StringComparison.InvariantCultureIgnoreCase);

    static bool ShouldCollectOutgoingRequestTrace(Uri? outgoingRequest, Uri? seqServerUrl, Elastic.Extensions.Logging.Options.ShipToOptions? elasticShipToOptions)
    {
        if (outgoingRequest == null) return false;

        if (seqServerUrl is not null && IsCalling(outgoingRequest, seqServerUrl))
        {
            //the outgoing request is calling Seq
            return false;
        }

        if (elasticShipToOptions?.NodeUris is not null && Array.Exists(elasticShipToOptions.NodeUris, u => IsCalling(outgoingRequest, u)))
        {
            //the outgoing request is calling Elastic
            return false;
        }
        return true;
    }

    static bool IsCalling(Uri outgoingRequest, Uri serviceUri)
    {
        return
            outgoingRequest.Scheme == serviceUri.Scheme
            && outgoingRequest.Host == serviceUri.Host
            && outgoingRequest.Port == serviceUri.Port;
    }

    static Uri GetOtelExporterGrpcEndpoint(IConfiguration? config)
    {
        var endpoint = config?["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? config?["JaegerGrpcUrl"] ?? "http://localhost:4317";
        return new Uri(endpoint);
    }
}
