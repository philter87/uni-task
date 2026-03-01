using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using UniTask.Api.Projects.Models;

namespace UniTask.Api.Shared.TaskProviderClients;

public interface IGitHubHttpClientFactory
{
    HttpClient CreateClient(Guid organisationId);
    string GetOwner();
    string GetRepo();
    bool IsConfigured(Guid organisationId);
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
        var token = GetToken(organisationId);
        
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
    
    public bool IsConfigured(Guid organisationId)
    {
        return !string.IsNullOrEmpty(GetToken(organisationId));
    }

    private string? GetToken(Guid organisationId)
    {
        if (organisationId != Guid.Empty)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
            var auth = context.TaskProviderAuths
                .FirstOrDefault(a => a.OrganisationId == organisationId
                    && a.AuthenticationType == AuthenticationType.GitHubApp);
            if (auth != null)
                return auth.SecretValue;
        }

        return _configuration["GitHub:Token"];
    }
}

