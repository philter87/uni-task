using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskLabelAddedEventHandler : INotificationHandler<TaskLabelAddedEvent>
{
    public Task Handle(TaskLabelAddedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
