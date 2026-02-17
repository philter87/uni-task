using MediatR;

namespace UniTask.Api.Tasks.AddLabel;

public class TaskLabelAddedEventHandler : INotificationHandler<TaskLabelAddedEvent>
{
    public Task Handle(TaskLabelAddedEvent notification, CancellationToken cancellationToken)
    {
        // Log or handle the event as needed
        // For now, just return completed task
        return Task.CompletedTask;
    }
}
