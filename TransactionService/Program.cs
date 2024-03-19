using GenericAspApi;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

var app = AppFactory.Create();

var appActivitySource = new ActivitySource(app.Environment.ApplicationName);

app.MapPost("/api/createTransaction", async ([FromServices] ILogger<Program> logger, [FromServices] HttpClient httpClient) =>
{
    logger.LogInformation("start createTransaction");
    var authResult = await httpClient.PostAsJsonAsync("https://authService/api/checkAllow", "{}");
    logger.LogInformation("authorization result is {authResult}", authResult);
    await httpClient.GetStringAsync("https://httpstat.us/200?sleep=300");
    logger.LogInformation("end createTransaction");
    return "OK";
});

app.Run();
