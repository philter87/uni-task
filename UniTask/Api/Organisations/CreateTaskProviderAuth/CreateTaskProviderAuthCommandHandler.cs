using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Organisations.Models;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.CreateTaskProviderAuth;

public class CreateTaskProviderAuthCommandHandler : IRequestHandler<CreateTaskProviderAuthCommand>
{
    private readonly TaskDbContext _context;
    private readonly IPublisher _publisher;

    public CreateTaskProviderAuthCommandHandler(TaskDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(CreateTaskProviderAuthCommand request, CancellationToken cancellationToken)
    {
        var organisation = await _context.Organisations
            .Include(o => o.Auths)
            .FirstOrDefaultAsync(o => o.Id == request.OrganisationId, cancellationToken);

        var auth = TaskProviderAuth.Create(request);
        _context.TaskProviderAuths.Add(auth);

        if (organisation != null)
        {
            organisation.Auths.Add(auth);
        }

        await _publisher.PublishAll(auth.DomainEvents, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
