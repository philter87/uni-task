using System.Net.Http.Json;
using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Events;

public class ProjectCreatedEventHandler : INotificationHandler<ProjectCreatedEvent>
{
    private const string GitHubApiBaseUrl = "https://api.github.com";

    private readonly ILogger<ProjectCreatedEventHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ProjectCreatedEventHandler(
        ILogger<ProjectCreatedEventHandler> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Project created: {ProjectId} - {Name} at {CreatedAt}",
            notification.ProjectId,
            notification.Name,
            notification.CreatedAt);

        if (notification.TaskProvider != TaskProvider.GitHub)
            return;

        var token = _configuration["GitHub:Token"];
        var owner = _configuration["GitHub:Owner"];
        var repo = _configuration["GitHub:Repo"];

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
        {
            _logger.LogWarning("GitHub configuration (Token, Owner, Repo) is missing. Skipping GitHub project creation.");
            return;
        }

        var httpClient = _httpClientFactory.CreateClient();

        var body = new
        {
            title = notification.Name,
            body = notification.Description
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/issues")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("Accept", "application/vnd.github+json");
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Headers.Add("User-Agent", "UniTask");

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to create GitHub issue for project {ProjectId}. Status: {StatusCode}. Response: {Content}",
                notification.ProjectId,
                response.StatusCode,
                content);
        }
        else
        {
            _logger.LogInformation(
                "Successfully created GitHub issue for project {ProjectId}",
                notification.ProjectId);
        }
    }
}
