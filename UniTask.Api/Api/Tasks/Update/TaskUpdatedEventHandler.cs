using MediatR;

namespace UniTask.Api.Tasks.Update;

public class TaskUpdatedEventHandler : INotificationHandler<TaskUpdatedEvent>
{
    public Task Handle(TaskUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // Log or handle the event as needed
        // For now, just return completed task
        return Task.CompletedTask;
    }
}
