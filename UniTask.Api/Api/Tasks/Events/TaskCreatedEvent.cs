using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class TaskCreatedEvent : INotification, IProviderEvent
{
    public int TaskId { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
