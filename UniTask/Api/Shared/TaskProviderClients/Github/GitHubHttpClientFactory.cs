using System.Net.Http.Headers;

namespace UniTask.Api.Shared.TaskProviderClients;

public interface IGitHubHttpClientFactory
{
    HttpClient CreateClient();
    string GetOwner();
    string GetRepo();
    bool IsConfigured();
}

public class GitHubHttpClientFactory : IGitHubHttpClientFactory
{
    private const string GitHubApiBaseUrl = "https://api.github.com";
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitHubHttpClientFactory> _logger;

    public GitHubHttpClientFactory(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GitHubHttpClientFactory> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public HttpClient CreateClient()
    {
        var token = _configuration["GitHub:Token"];
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("GitHub Token is missing from configuration. GitHub API calls may fail.");
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
    
    public bool IsConfigured()
    {
        var token = _configuration["GitHub:Token"];
        var owner = _configuration["GitHub:Owner"];
        var repo = _configuration["GitHub:Repo"];
        
        return !string.IsNullOrEmpty(token) && 
               !string.IsNullOrEmpty(owner) && 
               !string.IsNullOrEmpty(repo);
    }
}

