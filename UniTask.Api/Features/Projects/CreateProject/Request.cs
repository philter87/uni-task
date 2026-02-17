using MediatR;

namespace UniTask.Api.Features.Projects.CreateProject;

public class Request : IRequest<Response>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
