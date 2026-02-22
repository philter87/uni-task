using MediatR;
using UniTask.Api.Projects.Events;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Commands.Create;

public class CreateProjectCommand : IRequest<ProjectCreatedEvent>, IProviderEvent
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
