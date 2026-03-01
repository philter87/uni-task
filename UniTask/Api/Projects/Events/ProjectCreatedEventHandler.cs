using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Events;

public class ProjectCreatedEventHandler : INotificationHandler<ProjectCreatedEvent>
{
    private readonly ILogger<ProjectCreatedEventHandler> _logger;

    public ProjectCreatedEventHandler(ILogger<ProjectCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Project created: {ProjectId} - {Name} at {CreatedAt}",
            notification.ProjectId,
            notification.Name,
            notification.CreatedAt);

        if (notification.TaskProvider == TaskProvider.GitHub)
        {
            // A UniTask project maps to a GitHub repository, not a GitHub project.
            // GitHub projects are a separate planning concept (like a kanban board or sprint)
            // and are unrelated to UniTask projects. No corresponding entity needs to be
            // created in GitHub when a UniTask project is created.
            _logger.LogInformation(
                "Skipping GitHub sync for project {ProjectId}: UniTask projects map to GitHub repositories, not GitHub projects.",
                notification.ProjectId);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
