using MediatR;
using UniTask.Api.Organisations.Models;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Create;

public class CreateOrganisationCommandHandler : IRequestHandler<CreateOrganisationCommand, Guid>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public CreateOrganisationCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(CreateOrganisationCommand request, CancellationToken cancellationToken)
    {
        var organisation = Organisation.Create(request);
        _context.Organisations.Add(organisation);

        await _publisher.PublishAll(organisation.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return organisation.Id;
    }
}
