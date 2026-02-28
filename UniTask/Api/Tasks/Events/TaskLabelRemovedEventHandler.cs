using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskLabelRemovedEventHandler : INotificationHandler<TaskLabelRemovedEvent>
{
    public Task Handle(TaskLabelRemovedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
