using MediatR;
using UniTask.Api.Projects.Adapters;

namespace UniTask.Api.Projects.Commands.Create;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectCreatedEvent>
{
    private readonly IProjectAdapter _adapter;
    private readonly IPublisher _publisher;

    public CreateProjectCommandHandler(IProjectAdapter adapter, IPublisher publisher)
    {
        _adapter = adapter;
        _publisher = publisher;
    }

    public async Task<ProjectCreatedEvent> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var projectCreatedEvent = await _adapter.Handle(request);

        await _publisher.Publish(projectCreatedEvent, cancellationToken);

        return projectCreatedEvent;
    }
}
