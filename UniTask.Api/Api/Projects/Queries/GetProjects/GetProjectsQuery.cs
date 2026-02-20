using MediatR;

namespace UniTask.Api.Projects.Queries.GetProjects;

public class GetProjectsQuery : IRequest<IEnumerable<ProjectDto>>
{
}
