using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Extensions;
using TaskFlow.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddApplicationServices();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
