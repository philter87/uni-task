using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class TaskLabelRemovedEvent : INotification, IProviderEvent
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
    public DateTime RemovedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
