using MediatR;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.AssignMember;

public class AssignMemberToTaskCommand : IRequest<MemberAssignedToTaskEvent>
{
    public int TaskId { get; set; }
    public required string AssignedTo { get; set; }
}
