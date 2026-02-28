using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Commands.Create;

public class CreateProjectCommand : IRequest, IProviderEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
