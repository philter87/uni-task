using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Organisations.Models;

namespace UniTask.Api.Shared.TaskProviderClients;

public interface IGitHubHttpClientFactory
{
    HttpClient CreateClient(Guid organisationId);
    HttpClient CreateClientForProject(Guid projectId);
    string GetOwner();
    string GetRepo();
    bool IsConfigured(Guid organisationId);
    bool IsConfiguredForProject(Guid projectId);
}

public class GitHubHttpClientFactory : IGitHubHttpClientFactory
{
    private const string GitHubApiBaseUrl = "https://api.github.com";
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GitHubHttpClientFactory> _logger;

    public GitHubHttpClientFactory(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<GitHubHttpClientFactory> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public HttpClient CreateClient(Guid organisationId)
    {
        return BuildHttpClient(GetToken(organisationId));
    }

    public string GetOwner() => _configuration["GitHub:Owner"] ?? string.Empty;
    
    public string GetRepo() => _configuration["GitHub:Repo"] ?? string.Empty;
    
    public bool IsConfigured(Guid organisationId)
    {
        return !string.IsNullOrEmpty(GetToken(organisationId));
    }

    public HttpClient CreateClientForProject(Guid projectId)
    {
        return BuildHttpClient(GetTokenForProject(projectId));
    }

    public bool IsConfiguredForProject(Guid projectId)
    {
        return !string.IsNullOrEmpty(GetTokenForProject(projectId));
    }

    private HttpClient BuildHttpClient(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("GitHub Token is missing. GitHub API calls may fail.");
        }

        var httpClient = _httpClientFactory.CreateClient("GitHub");
        httpClient.BaseAddress = new Uri(GitHubApiBaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("UniTask", "1.0"));

        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return httpClient;
    }

    private string? GetToken(Guid organisationId)
    {
        if (organisationId != Guid.Empty)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var auth = context.TaskProviderAuths
                .Include(a => a.Organisations)
                .FirstOrDefault(a => a.Organisations.Any(o => o.Id == organisationId)
                    && a.AuthenticationType == AuthenticationType.GitHubApp);
            if (auth != null)
                return auth.SecretValue;
        }

        return _configuration["GitHub:Token"];
    }

    private string? GetTokenForProject(Guid projectId)
    {
        if (projectId != Guid.Empty)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var project = context.Projects
                .Include(p => p.TaskProviderAuth)
                .FirstOrDefault(p => p.Id == projectId);
            if (project?.TaskProviderAuth != null)
                return project.TaskProviderAuth.SecretValue;

            if (project?.OrganisationId != null)
                return GetToken(project.OrganisationId.Value);
        }

        return _configuration["GitHub:Token"];
    }
}

