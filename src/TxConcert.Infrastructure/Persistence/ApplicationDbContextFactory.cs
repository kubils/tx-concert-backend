using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TxConcert.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=tx_concert;Username=postgres;Password=postgres;Include Error Detail=true";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql =>
            {
                npgsql.UseNodaTime();
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.CommandTimeout(30);
            });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
