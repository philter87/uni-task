using MediatR;

namespace UniTask.Api.Tasks.Commands.Update;

public class TaskUpdatedEvent : INotification
{
    public int TaskId { get; set; }
    public required string Title { get; set; }
    public DateTime UpdatedAt { get; set; }
}
