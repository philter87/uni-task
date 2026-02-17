using MediatR;

namespace UniTask.Api.Tasks.ChangeStatus;

public class ChangeTaskStatusCommand : IRequest<TaskItemDto>
{
    public int TaskId { get; set; }
    public int StatusId { get; set; }
}
