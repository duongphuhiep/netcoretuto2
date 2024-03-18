var builder = DistributedApplication.CreateBuilder(args);

var transactionService = builder.AddProject<Projects.TransactionService>("transactionService");
var cardService = builder.AddProject<Projects.CardService>("cardService")
    .WithReference(transactionService);

builder.Build().Run();
