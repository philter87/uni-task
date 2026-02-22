using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskStatusChangedEventHandler : INotificationHandler<TaskStatusChangedEvent>
{
    public Task Handle(TaskStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
