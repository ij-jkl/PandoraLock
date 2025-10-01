using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

EnvLoader.LoadRootEnv();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("MYSQL_CONNECTION_STRING is not set in the environment variables (In Run Time).");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseAuthorization();
app.MapControllers();

app.Run();