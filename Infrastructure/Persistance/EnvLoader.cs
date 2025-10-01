using DotNetEnv;

namespace Infrastructure.Persistance;

public static class EnvLoader
{
    public static void LoadRootEnv()
    {
        // Look for .env file in the solution root directory
        var currentDirectory = Directory.GetCurrentDirectory();
        var solutionRoot = FindSolutionRoot(currentDirectory);
        
        if (solutionRoot != null)
        {
            var envPath = Path.Combine(solutionRoot, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                Console.WriteLine($"[DEBUG] - Loaded .env from: {envPath}");
            }
            else
            {
                Console.WriteLine($"[WARNING] - .env file not found at: {envPath}");
            }
        }
        else
        {
            Console.WriteLine("[WARNING] - Could not find solution root directory");
        }
    }
    
    private static string? FindSolutionRoot(string currentPath)
    {
        var directory = new DirectoryInfo(currentPath);
        
        while (directory != null)
        {
            if (directory.GetFiles("*.sln").Any() || 
                directory.GetFiles("docker-compose.yml").Any())
            {
                return directory.FullName;
            }
            
            directory = directory.Parent;
        }
        
        return null;
    }
}
