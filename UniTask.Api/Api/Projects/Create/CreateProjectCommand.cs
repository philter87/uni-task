using MediatR;

namespace UniTask.Api.Projects.Create;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
