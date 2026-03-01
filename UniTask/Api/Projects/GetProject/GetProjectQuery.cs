using MediatR;

namespace UniTask.Api.Projects.GetProject;

public class GetProjectQuery : IRequest<ProjectDto?>
{
    public Guid Id { get; set; }
}
