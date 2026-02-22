using MediatR;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.Delete;

public class DeleteTaskCommand : IRequest<TaskDeletedEvent>
{
    public int TaskId { get; set; }
}
