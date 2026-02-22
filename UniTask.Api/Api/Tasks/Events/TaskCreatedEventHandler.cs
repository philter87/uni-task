using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskCreatedEventHandler : INotificationHandler<TaskCreatedEvent>
{
    public Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
