using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Events;

public class AuthTaskProviderCreatedEventHandler : INotificationHandler<AuthTaskProviderCreatedEvent>
{
    private readonly ILogger<AuthTaskProviderCreatedEventHandler> _logger;

    public AuthTaskProviderCreatedEventHandler(ILogger<AuthTaskProviderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AuthTaskProviderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auth task provider created: {AuthTaskProviderId} for organisation {OrganisationId}",
            notification.AuthTaskProviderId,
            notification.OrganisationId);

        return Task.CompletedTask;
    }
}
