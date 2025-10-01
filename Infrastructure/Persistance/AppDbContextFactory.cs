using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        EnvLoader.LoadRootEnv();

        var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");
        Console.WriteLine($"[DEBUG] - MYSQL_CONNECTION_STRING being used : {connectionString}");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("MYSQL_CONNECTION_STRING is missing - Check .env path, or if .env is still there.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));

        return new AppDbContext(optionsBuilder.Options);
    }
}