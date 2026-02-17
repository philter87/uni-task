using MediatR;

namespace UniTask.Api.Tasks.AssignMember;

public class AssignMemberToTaskCommand : IRequest<TaskItemDto>
{
    public int TaskId { get; set; }
    public required string AssignedTo { get; set; }
}
