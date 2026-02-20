using MediatR;

namespace UniTask.Api.Tasks.Commands.Delete;

public class TaskDeletedEvent : INotification
{
    public int TaskId { get; set; }
    public DateTime DeletedAt { get; set; }
}
