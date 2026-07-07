using TxConcert.Application;
using TxConcert.Domain.Common.Settings;
using TxConcert.Infrastructure;
using TxConcert.Infrastructure.Persistence;
using Serilog;
using TxConcert.Worker.BackgroundJobs;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

    hostBuilder.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    hostBuilder.ConfigureServices((ctx, services) =>
    {
        IConfiguration configuration = ctx.Configuration;

        // Settings
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));

        // Layers
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddMessaging(configuration);

        // Background jobs
        services.AddBackgroundJobs(configuration);
    });

    IHost host = hostBuilder.Build();
    await ApplyDatabaseMigrationsAsync(host.Services);

    Log.Information("Worker starting...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task ApplyDatabaseMigrationsAsync(IServiceProvider services)
{
    using IServiceScope scope = services.CreateScope();
    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}
