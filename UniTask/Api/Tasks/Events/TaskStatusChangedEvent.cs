using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class TaskStatusChangedEvent : INotification, IProviderEvent
{
    public Guid TaskId { get; set; }
    public Guid StatusId { get; set; }
    public DateTime ChangedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
