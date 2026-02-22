using MediatR;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.ChangeStatus;

public class ChangeTaskStatusCommand : IRequest<TaskStatusChangedEvent>
{
    public int TaskId { get; set; }
    public int StatusId { get; set; }
}
