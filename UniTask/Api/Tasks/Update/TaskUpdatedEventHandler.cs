using MediatR;

namespace UniTask.Api.Tasks.Update;

public class TaskUpdatedEventHandler : INotificationHandler<TaskUpdatedEvent>
{
    public Task Handle(TaskUpdatedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
