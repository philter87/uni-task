using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.AssignMember;

public class AssignMemberToTaskCommand : IRequest<MemberAssignedToTaskEvent>, IProviderEvent
{
    public Guid TaskId { get; set; }
    public required string AssignedTo { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
