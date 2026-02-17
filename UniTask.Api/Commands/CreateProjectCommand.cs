using MediatR;
using UniTask.Api.DTOs;

namespace UniTask.Api.Commands;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
