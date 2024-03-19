var builder = DistributedApplication.CreateBuilder(args);

var authService = builder.AddProject<Projects.AuthService>("authService");
var transactionService = builder.AddProject<Projects.TransactionService>("transactionService")
    .WithReference(authService);
var cardService = builder.AddProject<Projects.CardService>("cardService")
    .WithReference(transactionService);

builder.Build().Run();
