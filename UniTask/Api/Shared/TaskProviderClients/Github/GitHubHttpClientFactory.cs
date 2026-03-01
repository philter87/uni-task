using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Organisations.Models;

namespace UniTask.Api.Shared.TaskProviderClients;

public interface IGitHubHttpClientFactory
{
    HttpClient CreateClient(Guid organisationId, Guid? projectId = null);
    string GetOwner();
    string GetRepo();
    bool IsConfigured(Guid organisationId, Guid? projectId = null);
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

    public HttpClient CreateClient(Guid organisationId, Guid? projectId = null)
    {
        var token = GetToken(organisationId, projectId);
        
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

    public string GetOwner() => _configuration["GitHub:Owner"] ?? string.Empty;
    
    public string GetRepo() => _configuration["GitHub:Repo"] ?? string.Empty;
    
    public bool IsConfigured(Guid organisationId, Guid? projectId = null)
    {
        return !string.IsNullOrEmpty(GetToken(organisationId, projectId));
    }

    private string? GetToken(Guid organisationId, Guid? projectId = null)
    {
        if (projectId.HasValue && projectId != Guid.Empty)
        {
            using var projectScope = _scopeFactory.CreateScope();
            var projectContext = projectScope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var secretValue = projectContext.Projects
                .Where(p => p.Id == projectId.Value)
                .Select(p => p.TaskProviderAuth != null ? p.TaskProviderAuth.SecretValue : null)
                .FirstOrDefault();
            if (secretValue != null)
                return secretValue;
        }

        if (organisationId != Guid.Empty)
        {
            using var orgScope = _scopeFactory.CreateScope();
            var orgContext = orgScope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var auth = orgContext.TaskProviderAuths
                .Include(a => a.Organisations)
                .FirstOrDefault(a => a.Organisations.Any(o => o.Id == organisationId)
                    && a.AuthenticationType == AuthenticationType.GitHubApp);
            if (auth != null)
                return auth.SecretValue;
        }

        return _configuration["GitHub:Token"];
    }
}

