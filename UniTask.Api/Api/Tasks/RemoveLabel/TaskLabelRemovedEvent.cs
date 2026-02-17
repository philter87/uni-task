using MediatR;

namespace UniTask.Api.Tasks.RemoveLabel;

public class TaskLabelRemovedEvent : INotification
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
    public DateTime RemovedAt { get; set; }
}
