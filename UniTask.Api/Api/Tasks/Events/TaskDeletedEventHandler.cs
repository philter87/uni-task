using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskDeletedEventHandler : INotificationHandler<TaskDeletedEvent>
{
    public Task Handle(TaskDeletedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
