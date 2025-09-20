var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(); 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("DefaultConnection"));

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