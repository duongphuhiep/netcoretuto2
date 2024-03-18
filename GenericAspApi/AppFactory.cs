using Elastic.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var builder = WebApplication.CreateBuilder();

        var config = builder.Configuration;

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

        #region Get Logging Servers config (Elastic & Seq)

        var elasticShipToOptions = config?.GetSection("Logging:Elastic").Get<Elastic.Extensions.Logging.Options.ShipToOptions>();
        var seqConfig = config?.GetSection("Logging:Seq");
        var seqServerUrlRaw = seqConfig?["ServerUrl"];
        var seqServerUrl = string.IsNullOrEmpty(seqServerUrlRaw) ? null : new Uri(seqServerUrlRaw);

        #endregion

        #region OpenTelemetry

        var otlpExporterUri = GetOtelExporterGrpcEndpoint(config);

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
                .AddConsoleExporter()
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

        //var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        //if (useOtlpExporter)
        //{
        //    builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        //    builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
        //    builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        //}


        #endregion

        builder.Services.AddControllers();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on service discovery
            //http.UseServiceDiscovery();
        });
        builder.Services
            .AddHttpContextAccessor()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
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

        var app = builder.Build();

        //log all request and response
        app.UseHttpLogging();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
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
