using MediatR;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.ChangeStatus;

public class ChangeTaskStatusCommand : IRequest<TaskStatusChangedEvent>, IProviderEvent
{
    public Guid TaskId { get; set; }
    public Guid StatusId { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
