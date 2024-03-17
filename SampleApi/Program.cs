using GenericAspApi;
using Microsoft.AspNetCore.Mvc;
using SampleApi;
using System.Diagnostics;

var app = AppFactory.Create();

var appActivitySource = new ActivitySource(app.Environment.ApplicationName);

app.MapPost("/api/hello", async (Person p, [FromServices] ILogger<Program> logger, [FromServices] HttpClient httpClient) =>
{
    logger.LogInformation("start my hello");
    await httpClient.GetStringAsync("https://httpstat.us/200?sleep=1000");
    await httpClient.GetStringAsync("https://httpstat.us/301"); //this will make 2 https requests to return the response because the target URI returns a redirection HTTP 301
    logger.LogInformation("end my hello");
    return new { greet = $"hello, {p.Name}", young = p.Age < 50 };
});

app.MapPost("/api/hello-failed", async (Person p, [FromServices] ILogger<Program> logger, [FromServices] HttpClient httpClient) =>
{
    logger.LogInformation("start my hello-failed");
    await httpClient.GetStringAsync("https://httpstat.us/200?sleep=1000");
    await httpClient.GetStringAsync("https://httpstat.us/301"); //this will make 2 https requests to return the response because the target URI returns a redirection HTTP 301
    logger.LogInformation("end my hello-failed");
    throw new InvalidOperationException("something wrong");
});
app.Run();
