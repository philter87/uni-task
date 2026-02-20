using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects.Adapters;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Adapters;
using UniTask.Api.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure DbContext with SQLite
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tasks.db"));

// Configure Identity
builder.Services.AddIdentityCore<UniUser>()
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<TaskDbContext>();

// Register adapters
builder.Services.AddScoped<IProjectAdapter, LocalProjectAdapter>();
builder.Services.AddScoped<ITasksAdapter, LocalTasksAdapter>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Configure OpenAPI with NSwag
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "UniTask API";
    config.Description = "A unified task manager API for Azure DevOps and GitHub";
    config.Version = "v1";
});

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "https://localhost:5173",
                               "http://localhost:5174", "https://localhost:5174")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    
    // Ensure database is created (only for SQLite)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        db.Database.EnsureCreated();
    }
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Serve static files from wwwroot (for the React app)
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for testing
public partial class Program { }
