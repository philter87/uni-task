using MediatR;
using UniTask.Api.Projects.Models;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.SyncTasks;

namespace UniTask.Api.Projects.Create;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;
    private readonly IMediator _mediator;

    public CreateProjectCommandHandler(TaskDbContext context, IPublisher publisher, IMediator mediator)
    {
        _context = context;
        _publisher = publisher;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = Project.Create(request);
        _context.Projects.Add(project);

        await _publisher.PublishAll(project.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.TriggerSync && request.Provider.HasValue && request.Provider != UniTask.Api.Shared.TaskProvider.Internal)
        {
            await _mediator.Send(new SyncTasksCommand { ProjectId = project.Id }, cancellationToken);
        }

        return project.Id;
    }
}
