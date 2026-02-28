using MediatR;

namespace UniTask.Api.Tasks.Commands.SyncTasks;

public class SyncTasksCommand : IRequest<IEnumerable<TaskItemDto>>
{
    public required Guid ProjectId { get; set; }
}
