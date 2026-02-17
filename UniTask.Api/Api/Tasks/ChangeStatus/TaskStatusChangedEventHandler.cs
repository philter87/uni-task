using MediatR;

namespace UniTask.Api.Tasks.ChangeStatus;

public class TaskStatusChangedEventHandler : INotificationHandler<TaskStatusChangedEvent>
{
    public Task Handle(TaskStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // Log or handle the event as needed
        // For now, just return completed task
        return Task.CompletedTask;
    }
}
