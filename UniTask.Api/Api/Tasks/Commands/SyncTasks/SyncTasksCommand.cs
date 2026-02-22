using MediatR;

namespace UniTask.Api.Tasks.Commands.SyncTasks;

public class SyncTasksCommand : IRequest<IEnumerable<TaskItemDto>>
{
    public required int ProjectId { get; set; }
}
