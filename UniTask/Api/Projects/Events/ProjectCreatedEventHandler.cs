using MediatR;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;

namespace UniTask.Api.Projects.Events;

public class ProjectCreatedEventHandler : INotificationHandler<ProjectCreatedEvent>
{
    private readonly ILogger<ProjectCreatedEventHandler> _logger;
    private readonly IGitHubHttpClientFactory _gitHubClientFactory;

    public ProjectCreatedEventHandler(
        ILogger<ProjectCreatedEventHandler> logger,
        IGitHubHttpClientFactory gitHubClientFactory)
    {
        _logger = logger;
        _gitHubClientFactory = gitHubClientFactory;
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

        if (!_gitHubClientFactory.IsConfigured())
        {
            _logger.LogWarning("GitHub configuration (Token, Owner, Repo) is missing. Skipping GitHub project creation.");
            return;
        }

        var httpClient = _gitHubClientFactory.CreateClient();
        var owner = _gitHubClientFactory.GetOwner();
        var repo = _gitHubClientFactory.GetRepo();

        var body = new
        {
            title = notification.Name,
            body = notification.Description
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"/repos/{owner}/{repo}/issues")
        {
            Content = JsonContent.Create(body)
        };

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
