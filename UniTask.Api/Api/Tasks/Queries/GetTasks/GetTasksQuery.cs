using MediatR;

namespace UniTask.Api.Tasks.Queries.GetTasks;

public class GetTasksQuery : IRequest<IEnumerable<TaskItemDto>>
{
}
