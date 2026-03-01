using MediatR;

namespace UniTask.Api.Organisations.Events;

public class OrganisationCreatedEventHandler : INotificationHandler<OrganisationCreatedEvent>
{
    private readonly ILogger<OrganisationCreatedEventHandler> _logger;

    public OrganisationCreatedEventHandler(ILogger<OrganisationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrganisationCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Organisation created: {OrganisationId} with name {Name}",
            notification.OrganisationId,
            notification.Name);

        return Task.CompletedTask;
    }
}
