using MediatR;
using UniTask.Api.Projects.Events;

namespace UniTask.Api.Projects.Commands.Create;

public class CreateProjectCommand : IRequest<ProjectCreatedEvent>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
