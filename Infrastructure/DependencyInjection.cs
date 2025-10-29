using Application.Common.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<ISharedFileAccessRepository, SharedFileAccessRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddSingleton<ITokenService, Auth.JWT.TokenService>();
        services.AddSingleton<IFileTypeValidator, FileTypeValidator>();
        services.AddSingleton<IFileSafetyAnalyzer, FileSafetyAnalyzer>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();
        
        var fileStoragePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");
        services.AddSingleton<IFileStorageService>(new FileStorageService(fileStoragePath));

        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") 
            ?? throw new InvalidOperationException("REDIS_CONNECTION_STRING environment variable is not set");

        // Use lazy connection with AbortOnConnectFail=false to prevent startup blocking
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false; // Don't block startup if Redis is unavailable
            options.ConnectTimeout = 3000; // 3 second timeout
            options.SyncTimeout = 3000;
            return ConnectionMultiplexer.Connect(options);
        });

        var cacheExpirationMinutesStr = Environment.GetEnvironmentVariable("REDIS_CACHE_EXPIRATION_MINUTES") 
            ?? throw new InvalidOperationException("REDIS_CACHE_EXPIRATION_MINUTES environment variable is not set");
        var cacheExpirationMinutes = int.Parse(cacheExpirationMinutesStr);
        services.AddSingleton<ICacheService>(sp => 
        {
            var redisConnection = sp.GetRequiredService<IConnectionMultiplexer>();
            return new CacheService(redisConnection, TimeSpan.FromMinutes(cacheExpirationMinutes));
        });

        return services;
    }
}
