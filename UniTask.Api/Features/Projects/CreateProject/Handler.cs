using MediatR;
using UniTask.Api.Infrastructure.Adapters;
using UniTask.Api.DTOs;
using UniTask.Api.Events;

namespace UniTask.Api.Features.Projects.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly ITaskAdapter _adapter;
    private readonly IPublisher _publisher;

    public CreateProjectCommandHandler(ITaskAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<CreateProjectResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
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

        return new CreateProjectResponse
        {
            Id = createdProject.Id,
            Name = createdProject.Name,
            Description = createdProject.Description,
            CreatedAt = createdProject.CreatedAt,
            UpdatedAt = createdProject.UpdatedAt
        };
    }
}
