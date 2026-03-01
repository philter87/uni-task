using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;

namespace UniTask.Tests.Utls;

public class AppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove all existing DbContext-related registrations
            var descriptors = Enumerable.Where<ServiceDescriptor>(
                services,
                d => 
                d.ServiceType == typeof(DbContextOptions<TaskDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType.Name.Contains("DbContext")).ToList();
            
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<TaskDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Remove existing IGitHubHttpClientFactory registration
            var githubFactoryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IGitHubHttpClientFactory));
            if (githubFactoryDescriptor != null)
            {
                services.Remove(githubFactoryDescriptor);
            }

            // Register MockGitHubHttpClientFactory for tests
            services.AddSingleton<IGitHubHttpClientFactory, MockGitHubHttpClientFactory>();

            // Register GitHubTaskProviderClient for tests
            services.AddSingleton<GitHubTaskProviderClient>();
        });

        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Ensure database is created
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            db.Database.EnsureCreated();
        }

        return host;
    }
}