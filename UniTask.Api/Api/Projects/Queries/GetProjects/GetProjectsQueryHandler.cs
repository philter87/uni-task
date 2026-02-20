using MediatR;
using UniTask.Api.Projects.Adapters;

namespace UniTask.Api.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly IProjectAdapter _adapter;

    public GetProjectsQueryHandler(IProjectAdapter adapter)
    {
        _adapter = adapter;
    }

    public Task<IEnumerable<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        return _adapter.Handle(request);
    }
}
