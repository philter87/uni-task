using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using UniTask.Api.Projects.Models;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;
using UniTask.Tests.Utls;

namespace UniTask.Tests.Api.Shared.Providers;

public class GitHubHttpClientFactoryTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IGitHubHttpClientFactory _factory;

    public GitHubHttpClientFactoryTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<TaskDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddHttpClient();
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        var configuration = new ConfigurationBuilder().Build();
        _factory = new GitHubHttpClientFactory(
            _serviceProvider.GetRequiredService<IHttpClientFactory>(),
            configuration,
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<GitHubHttpClientFactory>.Instance);
    }

    [Fact]
    public async Task IsConfigured_Should_ReturnTrue_When_DbHasGitHubAppAuth()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var auth = Any.TaskProviderAuth(
                authenticationType: AuthenticationType.GitHubApp,
                secretValue: "db-token",
                organisationId: organisationId);
            dbContext.TaskProviderAuths.Add(auth);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var result = _factory.IsConfigured(organisationId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsConfigured_Should_ReturnFalse_When_NoAuthAndNoConfig()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        // Act
        var result = _factory.IsConfigured(organisationId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateClient_Should_UseDbToken_When_OrganisationHasGitHubAppAuth()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        const string expectedToken = "db-secret-token";
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var auth = Any.TaskProviderAuth(
                authenticationType: AuthenticationType.GitHubApp,
                secretValue: expectedToken,
                organisationId: organisationId);
            dbContext.TaskProviderAuths.Add(auth);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var client = _factory.CreateClient(organisationId);

        // Assert
        Assert.NotNull(client.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization!.Scheme);
        Assert.Equal(expectedToken, client.DefaultRequestHeaders.Authorization.Parameter);
    }

    [Fact]
    public void CreateClient_Should_NotSetAuthHeader_When_NoTokenAvailable()
    {
        // Arrange
        var organisationId = Guid.NewGuid();

        // Act
        var client = _factory.CreateClient(organisationId);

        // Assert
        Assert.Null(client.DefaultRequestHeaders.Authorization);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }
}

