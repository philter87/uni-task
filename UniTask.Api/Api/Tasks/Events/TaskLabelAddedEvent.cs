using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskLabelAddedEvent : INotification
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
    public DateTime AddedAt { get; set; }
}
