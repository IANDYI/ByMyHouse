

using BuyMyHouse.Core.Repositories;
using BuyMyHouse.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var storageConnectionString = builder.Configuration["AzureStorageConnectionString"] 
    ?? "UseDevelopmentStorage=true";

builder.Services.AddSingleton<IMortgageRepository>(
    new MortgageRepository(storageConnectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for local testing
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
