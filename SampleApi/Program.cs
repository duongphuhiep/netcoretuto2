using Elastic.Extensions.Logging;
using Microsoft.AspNetCore.HttpLogging;
using SampleApi;
using System.Text;

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
var config = configBuilder.Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddSeq(config.GetSection("Logging:Seq"));
        loggingBuilder.AddElasticsearch(elasticConf =>
        {
            elasticConf.ShipTo = config.GetRequiredSection("Logging:Elastic").Get<Elastic.Extensions.Logging.Options.ShipToOptions>()!;
        });
    });
builder.Services.AddHttpLogging(opts =>
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