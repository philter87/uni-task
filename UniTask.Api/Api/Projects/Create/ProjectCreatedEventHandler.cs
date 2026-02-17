using MediatR;

namespace UniTask.Api.Projects.Create;

public class ProjectCreatedEventHandler : INotificationHandler<ProjectCreatedEvent>
{
    private readonly ILogger<ProjectCreatedEventHandler> _logger;

    public ProjectCreatedEventHandler(ILogger<ProjectCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Log the event
        _logger.LogInformation(
            "Project created: {ProjectId} - {Name} at {CreatedAt}",
            notification.ProjectId,
            notification.Name,
            notification.CreatedAt);

        // Additional event handling logic can go here
        // - Send notifications
        // - Update analytics
        // - Trigger workflows
        // etc.

        return Task.CompletedTask;
    }
}
