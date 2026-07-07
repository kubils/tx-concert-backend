using TxConcert.Api.BackgroundTasks;
using TxConcert.Api.Auth;
using TxConcert.Api.Middleware;
using TxConcert.Api.RateLimit;
using TxConcert.Application;
using TxConcert.Domain.Common.Settings;
using TxConcert.Infrastructure;
using TxConcert.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .WriteTo.Console());

    // Settings
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(AppSettings.SectionName));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
    builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection(RateLimitSettings.SectionName));

    // Layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // In-process background work for quick manual API triggers
    builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
    builder.Services.AddHostedService<QueuedBackgroundTaskService>();

    // Controllers + JSON
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // Auth
    builder.Services.AddAuth(builder.Configuration);

    // Rate limiting
    builder.Services.AddAppRateLimiting(builder.Configuration);

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "TxConcert API", Version = "v1" });

        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "x-api-key",
            Description = "Enter your API key"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Exception handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Health checks
    RedisSettings redisSettings = builder.Configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>()
        ?? new RedisSettings();
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");
        //.AddRedis(redisSettings.ConnectionString);

    // Build
    WebApplication app = builder.Build();

    await ApplyDatabaseMigrationsAsync(app);

    // Middleware pipeline
    app.UseExceptionHandler();
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseStaticFiles();

    bool swaggerEnabled = app.Environment.IsDevelopment()
        || app.Configuration.GetValue<bool>("Swagger:Enabled");

    if (swaggerEnabled)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.DocumentTitle = "TxConcert API";
            c.InjectStylesheet("/swagger-dark.css");
        });
    }

    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    RateLimitSettings rateLimitSettings = builder.Configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>()
        ?? new RateLimitSettings();
    if (rateLimitSettings.Enabled)
        app.UseRateLimiter();

    app.MapControllers();
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();
    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    string[] pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
    if (pendingMigrations.Length == 0)
        return;

    Log.Information("Applying {MigrationCount} pending database migration(s): {Migrations}",
        pendingMigrations.Length,
        string.Join(", ", pendingMigrations));

    await dbContext.Database.MigrateAsync();
}

public partial class Program { }
