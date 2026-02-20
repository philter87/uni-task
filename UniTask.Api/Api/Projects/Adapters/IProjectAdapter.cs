using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Queries.GetProject;
using UniTask.Api.Projects.Queries.GetProjects;

namespace UniTask.Api.Projects.Adapters;

public interface IProjectAdapter
{
    Task<ProjectCreatedEvent> Handle(CreateProjectCommand command);
    Task<ProjectDto?> Handle(GetProjectQuery query);
    Task<IEnumerable<ProjectDto>> Handle(GetProjectsQuery query);
}
