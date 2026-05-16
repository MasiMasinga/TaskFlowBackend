using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TaskFlow.Data;
using TaskFlow.Data.Configurations;
using TaskFlow.Extensions;
using TaskFlow.Middleware;
using TaskFlow.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());

        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlow API",
        Version = "v1",
        Description = "Project and Task management HTTP API."
    });
});
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddProblemDetails();
builder.Services.Configure<JwtConfiguration>(
    builder.Configuration.GetSection(JwtConfiguration.SectionName));

var jwtOptions = builder.Configuration
.GetSection(JwtConfiguration.SectionName)
.Get<JwtConfiguration>()
?? throw new InvalidOperationException("JWT options not configured");

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();


var app = builder.Build();

await ApplyMigrationsIfEnabledAsync(app);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow v1");
});

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuthForbiddenLoggingMiddleware>();
app.MapControllers();

await SeedAdminRoleAndUserAsync(app);

await app.RunAsync();

static async Task ApplyMigrationsIfEnabledAsync(WebApplication app)
{
    if (!app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        return;

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

static async Task SeedAdminRoleAndUserAsync(WebApplication app)
{
    const string adminRoleName = "Admin";

    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = adminRoleName
        });

        if (!roleResult.Succeeded)
        {
            logger.LogError(
                "Failed to create {Role} role: {Errors}",
                adminRoleName,
                string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            return;
        }
    }

    var adminEmail = config["Seed:AdminEmail"];
    var adminPassword = config["Seed:AdminPassword"];
    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
    {
        logger.LogWarning("Seed admin skipped: Seed:AdminEmail or Seed:AdminPassword is not configured.");
        return;
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Administrator",
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            logger.LogError(
                "Failed to create seed admin user: {Errors}",
                string.Join("; ", createResult.Errors.Select(e => e.Description)));
            return;
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
    {
        var assignResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);
        if (!assignResult.Succeeded)
        {
            logger.LogError(
                "Failed to assign {Role} role to seed admin: {Errors}",
                adminRoleName,
                string.Join("; ", assignResult.Errors.Select(e => e.Description)));
        }
    }
}
