using MediatR;

namespace UniTask.Api.Tasks.RemoveLabel;

public class TaskLabelRemovedEventHandler : INotificationHandler<TaskLabelRemovedEvent>
{
    public Task Handle(TaskLabelRemovedEvent notification, CancellationToken cancellationToken)
    {
        // Log or handle the event as needed
        // For now, just return completed task
        return Task.CompletedTask;
    }
}
