using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskFlow.Interfaces;
using TaskFlow.Models;
using TaskFlow.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddSingleton<ISystemInfoService, SystemInfoService>();
builder.Services.AddSingleton<IHealthService, HealthService>();
builder.Services.AddSingleton<IGreetingService, GreetingsService>();
builder.Services.AddSingleton<IVersionService, VersionService>();
builder.Services.AddSingleton<IClockService, ClockService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
