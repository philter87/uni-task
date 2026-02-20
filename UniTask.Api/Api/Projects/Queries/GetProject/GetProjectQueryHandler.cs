using MediatR;
using UniTask.Api.Projects.Adapters;

namespace UniTask.Api.Projects.Queries.GetProject;

public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, ProjectDto?>
{
    private readonly IProjectAdapter _adapter;

    public GetProjectQueryHandler(IProjectAdapter adapter)
    {
        _adapter = adapter;
    }

    public Task<ProjectDto?> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        return _adapter.Handle(request);
    }
}
