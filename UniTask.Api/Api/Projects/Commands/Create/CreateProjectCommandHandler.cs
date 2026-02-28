using MediatR;
using UniTask.Api.Projects.Events;
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
        var project = Project.Create(request);

        _context.Projects.Add(project);

        foreach (var domainEvent in project.DomainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return project.DomainEvents.OfType<ProjectCreatedEvent>().First();
    }
}
