using Elastic.Extensions.Logging;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleApi;
using System.Text;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = Telemetry.ApplicationName,
});
var config = builder.Configuration;

var otelBuilder = builder.Services.AddOpenTelemetry();
otelBuilder.ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName));
otelBuilder.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource(builder.Environment.ApplicationName);
    var tracingOtlpEndpoint = config["JaegerGrpcUrl"];
    if (tracingOtlpEndpoint is not null)
    {
        tracing.AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
            otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
    }
    else
    {
        tracing.AddConsoleExporter();
    }
});


builder.Services.AddControllers();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddSeq(config.GetRequiredSection("Logging:Seq"));
        loggingBuilder.AddElasticsearch(elasticConf =>
        {
            elasticConf.ShipTo = config.GetRequiredSection("Logging:Elastic").Get<Elastic.Extensions.Logging.Options.ShipToOptions>()!;
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
app.MapPost("/hello", (Person p) => new { greet = $"hello, {p.Name}", young = p.Age < 50 });
app.MapPost("/hello-failed", (Person p) => { throw new InvalidOperationException("something wrong"); });
app.Run();