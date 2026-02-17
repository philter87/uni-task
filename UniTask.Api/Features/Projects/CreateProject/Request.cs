using MediatR;

namespace UniTask.Api.Features.Projects.CreateProject;

public class CreateProjectCommand : IRequest<CreateProjectResponse>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
