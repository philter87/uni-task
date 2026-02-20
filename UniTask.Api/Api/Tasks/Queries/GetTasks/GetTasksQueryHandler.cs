using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Queries.GetTasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IEnumerable<TaskItemDto>>
{
    private readonly ITasksAdapter _adapter;

    public GetTasksQueryHandler(ITasksAdapter adapter)
    {
        _adapter = adapter;
    }

    public Task<IEnumerable<TaskItemDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        return _adapter.Handle(request);
    }
}
