using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.Delete;

public class DeleteTaskCommand : IRequest<TaskDeletedEvent>, IProviderEvent
{
    public Guid TaskId { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
