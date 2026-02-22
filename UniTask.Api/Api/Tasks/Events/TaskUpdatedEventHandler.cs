using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskUpdatedEventHandler : INotificationHandler<TaskUpdatedEvent>
{
    public Task Handle(TaskUpdatedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
