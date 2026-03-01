using MediatR;

namespace UniTask.Api.Projects.GetProjects;

public class GetProjectsQuery : IRequest<IEnumerable<ProjectDto>>
{
    public Guid? OrganisationId { get; set; }
}
