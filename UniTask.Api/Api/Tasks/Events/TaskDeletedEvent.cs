using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class TaskDeletedEvent : INotification, IProviderEvent
{
    public int TaskId { get; set; }
    public DateTime DeletedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
