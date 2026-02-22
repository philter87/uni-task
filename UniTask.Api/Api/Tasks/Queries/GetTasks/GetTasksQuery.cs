using MediatR;

namespace UniTask.Api.Tasks.Queries.GetTasks;

public class GetTasksQuery : IRequest<IEnumerable<TaskItemDto>>
{
    public int? ProjectId { get; set; }
    public string? ExternalProjectId { get; set; }
}
