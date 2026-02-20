using MediatR;
using UniTask.Api.Tasks.Adapters;

namespace UniTask.Api.Tasks.Queries.GetTask;

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TaskItemDto?>
{
    private readonly ITasksAdapter _adapter;

    public GetTaskQueryHandler(ITasksAdapter adapter)
    {
        _adapter = adapter;
    }

    public Task<TaskItemDto?> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        return _adapter.Handle(request);
    }
}
