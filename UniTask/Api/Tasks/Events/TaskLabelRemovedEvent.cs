using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class TaskLabelRemovedEvent : INotification, IProviderEvent
{
    public Guid TaskId { get; set; }
    public Guid LabelId { get; set; }
    public DateTime RemovedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
