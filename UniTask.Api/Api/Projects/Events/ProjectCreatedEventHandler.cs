using MediatR;

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

        return Task.CompletedTask;
    }
}
