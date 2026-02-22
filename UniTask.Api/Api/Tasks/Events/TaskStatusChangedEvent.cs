using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskStatusChangedEvent : INotification
{
    public int TaskId { get; set; }
    public int StatusId { get; set; }
    public DateTime ChangedAt { get; set; }
}
