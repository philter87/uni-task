using MediatR;
using UniTask.Api.Organisations.Models;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Commands.Create;

public class CreateOrganisationCommandHandler : IRequestHandler<CreateOrganisationCommand>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public CreateOrganisationCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(CreateOrganisationCommand request, CancellationToken cancellationToken)
    {
        var organisation = Organisation.Create(request);
        _context.Organisations.Add(organisation);

        await _publisher.PublishAll(organisation.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
