using MediatR;

namespace UniTask.Api.Tasks.Commands.Create;

public class TaskCreatedEvent : INotification
{
    public int TaskId { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedAt { get; set; }
}
