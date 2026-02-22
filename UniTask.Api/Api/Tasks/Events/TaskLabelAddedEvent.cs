using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Events;

public class TaskLabelAddedEvent : INotification, IProviderEvent
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
    public DateTime AddedAt { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
