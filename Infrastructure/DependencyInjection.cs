using Application.Common.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<ISharedFileAccessRepository, SharedFileAccessRepository>();
        services.AddSingleton<ITokenService, Auth.JWT.TokenService>();
        services.AddSingleton<IFileTypeValidator, FileTypeValidator>();
        services.AddSingleton<IFileSafetyAnalyzer, FileSafetyAnalyzer>();
        services.AddScoped<IEmailService, EmailService>();
        
        var fileStoragePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");
        services.AddSingleton<IFileStorageService>(new FileStorageService(fileStoragePath));
        
        return services;
    }
}
