using MediatR;
using UniTask.Api.Projects.Models;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Commands.Create;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public CreateProjectCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = Project.Create(request);
        _context.Projects.Add(project);

        await _publisher.PublishAll(project.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
