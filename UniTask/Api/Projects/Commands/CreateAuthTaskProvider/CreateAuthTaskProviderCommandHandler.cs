using MediatR;
using UniTask.Api.Projects.Models;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Commands.CreateAuthTaskProvider;

public class CreateAuthTaskProviderCommandHandler : IRequestHandler<CreateAuthTaskProviderCommand>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public CreateAuthTaskProviderCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(CreateAuthTaskProviderCommand request, CancellationToken cancellationToken)
    {
        var auth = TaskProviderAuth.Create(request);
        _context.TaskProviderAuths.Add(auth);

        await _publisher.PublishAll(auth.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
