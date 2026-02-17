using MediatR;
using UniTask.Api.Shared.Adapters;

namespace UniTask.Api.Projects.Create;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly ITaskAdapter _adapter;
    private readonly IPublisher _publisher;

    public CreateProjectCommandHandler(ITaskAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Create the project DTO
        var projectDto = new ProjectDto
        {
            Name = request.Name,
            Description = request.Description
        };

        // Use the adapter to create the project
        var createdProject = await _adapter.CreateProjectAsync(projectDto);

        // Publish the ProjectCreated event
        var projectCreatedEvent = new ProjectCreatedEvent
        {
            ProjectId = createdProject.Id,
            Name = createdProject.Name,
            Description = createdProject.Description,
            CreatedAt = createdProject.CreatedAt
        };

        await _publisher.Publish(projectCreatedEvent, cancellationToken);

        return createdProject;
    }
}
