using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Events;

public class ProjectCreatedNotifier(
    TaskDbContext context,
    ILogger<ProjectCreatedEventHandler> logger) 
    : INotificationHandler<ProjectCreatedEvent>
{
    public Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
    {
        var project = context.Projects.Find(notification.ProjectId);
        logger.LogInformation(
            "Project created: {ProjectId} - {Name} at {CreatedAt} and Found in DB: {Found}",
            notification.ProjectId,
            notification.Name,
            notification.CreatedAt, 
            project!.Id);
        
        
        return Task.CompletedTask;
    }
}