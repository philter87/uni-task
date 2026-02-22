using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class TaskStatusChangedEvent : INotification, IProviderEvent
{
    public int TaskId { get; set; }
    public int StatusId { get; set; }
    public DateTime ChangedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
