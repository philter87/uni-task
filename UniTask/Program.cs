using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag.Generation.Processors.Security;
using UniTask.Api.Auth;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;
using UniTask.Api.Shared.TaskProviderClients.AzureDevOps;
using UniTask.Api.Users;

var builder = WebApplication.CreateBuilder(args);

// Persist Data Protection keys so OAuth state survives machine restarts (critical on Fly.io)
var dpKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrEmpty(dpKeysPath))
{
    var keysDir = new DirectoryInfo(dpKeysPath);
    keysDir.Create();
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(keysDir);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure DbContext with SQLite
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tasks.db"));

// Configure Identity (full Identity with SignInManager for OAuth)
builder.Services.AddIdentity<UniUser, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<TaskDbContext>()
.AddDefaultTokenProviders();

// Configure JWT + OAuth authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "REPLACE_WITH_SECURE_256BIT_KEY_IN_USER_SECRETS";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };
})
;

var googleClientId = builder.Configuration["Auth:Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]!;
            options.CallbackPath = "/api/auth/callback/google";
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });
}

var githubClientId = builder.Configuration["Auth:GitHub:ClientId"];
if (!string.IsNullOrEmpty(githubClientId))
{
    builder.Services.AddAuthentication()
        .AddGitHub(options =>
        {
            options.ClientId = githubClientId;
            options.ClientSecret = builder.Configuration["Auth:GitHub:ClientSecret"]!;
            options.CallbackPath = "/api/auth/callback/github";
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.ClaimActions.Add(new JsonKeyClaimAction("urn:github:avatar", "string", "avatar_url"));
            options.Events = new OAuthEvents
            {
                OnRemoteFailure = context =>
                {
                    var frontendUrl = context.HttpContext.RequestServices
                        .GetRequiredService<IConfiguration>()["FrontendUrl"] ?? "http://localhost:5173";
                    var error = Uri.EscapeDataString(context.Failure?.Message ?? "auth_failed");
                    context.Response.Redirect($"{frontendUrl}/login?error={error}");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });
}

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UniTask.Program).Assembly));

// Register HttpClient
builder.Services.AddHttpClient();

// Register GitHub HTTP Client Factory
builder.Services.AddSingleton<IGitHubHttpClientFactory, GitHubHttpClientFactory>();

// Register JWT service
builder.Services.AddScoped<JwtService>();

// Register task provider clients
builder.Services.AddScoped<GitHubTaskProviderClient>();
builder.Services.AddScoped<AzureDevOpsTaskProviderClient>();
builder.Services.AddScoped<MemoryTaskProviderClient>();
builder.Services.AddScoped<ITaskProviderClientFactory, TaskProviderClientFactory>();

// Configure OpenAPI with NSwag (JWT bearer scheme)
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "UniTask API";
    config.Description = "A unified task manager API for Azure DevOps and GitHub";
    config.Version = "v1";
    config.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token",
    });
    config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

// Configure CORS for frontend
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://localhost:5173",
        "http://localhost:5174", "https://localhost:5174"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.Path = "/swagger";
    });
}

// Always apply migrations so the DB is initialised on first boot
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Serve static files from wwwroot (for the React app)
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for testing
namespace UniTask
{
    public partial class Program { }
}
