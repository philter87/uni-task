using MediatR;

namespace UniTask.Api.Tasks.Events;

public class TaskDeletedEvent : INotification
{
    public int TaskId { get; set; }
    public DateTime DeletedAt { get; set; }
}
