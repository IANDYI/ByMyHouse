using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BuyMyHouse.Core.Data;
using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Repositories;
using BuyMyHouse.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Use SQLite on Mac/Linux, SQL Server on Windows
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddDbContext<HouseDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("HouseDatabase")));
}
else
{
    // Use SQLite for Mac/Linux
    builder.Services.AddDbContext<HouseDbContext>(options =>
        options.UseSqlite("Data Source=buy_my_house.db"));
}

builder.Services.AddScoped<IHouseRepository, HouseRepository>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

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

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HouseDbContext>();
    db.Database.EnsureCreated();
}

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
