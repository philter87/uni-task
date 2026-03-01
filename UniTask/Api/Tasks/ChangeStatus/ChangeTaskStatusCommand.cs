using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.ChangeStatus;

public class ChangeTaskStatusCommand : IRequest<TaskStatusChangedEvent>, IProviderEvent
{
    public Guid TaskId { get; set; }
    public Guid StatusId { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
