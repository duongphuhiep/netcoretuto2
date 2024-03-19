using GenericAspApi;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

var app = AppFactory.Create();

var appActivitySource = new ActivitySource(app.Environment.ApplicationName);

app.MapPost("/api/createCardPayment", async ([FromServices] ILogger<Program> logger, [FromServices] HttpClient httpClient) =>
{
    logger.LogInformation("start createCardPayment");
    var r = await httpClient.PostAsJsonAsync("https://transactionService/api/CreateTransaction", "{}");
    logger.LogInformation("success call transaction service {result}", r);
    logger.LogInformation("end createCardPayment");
    return "OK";
});

app.Run();
