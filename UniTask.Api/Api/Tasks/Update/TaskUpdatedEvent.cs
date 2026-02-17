using MediatR;

namespace UniTask.Api.Tasks.Update;

public class TaskUpdatedEvent : INotification
{
    public int TaskId { get; set; }
    public required string Title { get; set; }
    public DateTime UpdatedAt { get; set; }
}
