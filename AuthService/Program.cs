using GenericAspApi;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

var app = AppFactory.Create();

var appActivitySource = new ActivitySource(app.Environment.ApplicationName);

app.MapPost("/api/checkAllow", async ([FromServices] ILogger<Program> logger, [FromServices] HttpClient httpClient) =>
{
    logger.LogInformation("start checkAllow");
    logger.LogInformation("checking permission");
    //await httpClient.GetStringAsync("https://httpstat.us/200?sleep=1000");
    //await httpClient.GetStringAsync("https://httpstat.us/301"); //this will make 2 https requests to return the response because the target URI returns a redirection HTTP 301
    logger.LogInformation("end checkAllow");
    return "OK";
});

app.Run();
