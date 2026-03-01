using MediatR;

namespace UniTask.Api.Organisations.Events;

public class TaskProviderAuthCreatedEventHandler : INotificationHandler<TaskProviderAuthCreatedEvent>
{
    private readonly ILogger<TaskProviderAuthCreatedEventHandler> _logger;

    public TaskProviderAuthCreatedEventHandler(ILogger<TaskProviderAuthCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TaskProviderAuthCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Task provider auth created: {TaskProviderAuthId} for organisation {OrganisationId}",
            notification.TaskProviderAuthId,
            notification.OrganisationId);

        return Task.CompletedTask;
    }
}
