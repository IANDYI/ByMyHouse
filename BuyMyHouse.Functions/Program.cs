using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Repositories;
using BuyMyHouse.Functions;
using BuyMyHouse.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register services
var storageConnectionString = builder.Configuration["AzureStorageConnectionString"] 
    ?? "UseDevelopmentStorage=true";

builder.Services.AddSingleton<IMortgageRepository>(
    new MortgageRepository(storageConnectionString));

builder.Services.AddScoped<IMortgageOfferService, MortgageOfferService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register functions for dependency injection (for manual testing)
builder.Services.AddScoped<ProcessMortgageApplicationsFunction>();
builder.Services.AddScoped<SendMortgageOffersFunction>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
