using System.Net.Http.Headers;
using RichardSzalay.MockHttp;
using UniTask.Api.Shared.TaskProviderClients;

namespace UniTask.Tests.Utls;

public class MockGitHubHttpClientFactory : IGitHubHttpClientFactory
{
    private readonly MockHttpMessageHandler _mockHandler;
    private const string GitHubApiBaseUrl = "https://api.github.com";

    public MockGitHubHttpClientFactory()
    {
        _mockHandler = new MockHttpMessageHandler();
        SetupMockResponses();
    }

    private void SetupMockResponses()
    {
        var issuesJsonPath = Path.Combine(AppContext.BaseDirectory, "HttpMocks", "repos", "org", "repo", "issues.json");
        
        if (!File.Exists(issuesJsonPath))
        {
            throw new FileNotFoundException($"Mock JSON file not found: {issuesJsonPath}");
        }

        var issuesJson = File.ReadAllText(issuesJsonPath);

        _mockHandler
            .When($"{GitHubApiBaseUrl}/repos/org/repo/issues*")
            .Respond("application/json", issuesJson);
    }

    public HttpClient CreateClient()
    {
        var httpClient = _mockHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri(GitHubApiBaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("UniTask", "1.0"));
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "fake-test-token");
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return httpClient;
    }

    public string GetOwner() => "org";

    public string GetRepo() => "repo";

    public bool IsConfigured() => true;
}

