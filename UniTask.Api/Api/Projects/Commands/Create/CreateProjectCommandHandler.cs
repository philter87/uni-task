using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Commands.Create;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectCreatedEvent>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public CreateProjectCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<ProjectCreatedEvent> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        var projectCreatedEvent = new ProjectCreatedEvent
        {
            ProjectId = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt
        };

        await _publisher.Publish(projectCreatedEvent, cancellationToken);

        return projectCreatedEvent;
    }
}
