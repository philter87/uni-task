using MediatR;

namespace UniTask.Api.Tasks.Commands.Delete;

public class DeleteTaskCommand : IRequest<TaskDeletedEvent>
{
    public int TaskId { get; set; }
}
