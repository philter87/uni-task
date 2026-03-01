using MediatR;

namespace UniTask.Api.Tasks.RemoveLabel;

public class TaskLabelRemovedEventHandler : INotificationHandler<TaskLabelRemovedEvent>
{
    public Task Handle(TaskLabelRemovedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
