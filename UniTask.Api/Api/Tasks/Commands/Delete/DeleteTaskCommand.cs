using MediatR;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.Delete;

public class DeleteTaskCommand : IRequest<TaskDeletedEvent>, IProviderEvent
{
    public int TaskId { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
