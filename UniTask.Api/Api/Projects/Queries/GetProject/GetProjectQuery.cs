using MediatR;

namespace UniTask.Api.Projects.Queries.GetProject;

public class GetProjectQuery : IRequest<ProjectDto?>
{
    public int Id { get; set; }
}
