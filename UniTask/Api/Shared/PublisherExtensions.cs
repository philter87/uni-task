using MediatR;

namespace UniTask.Api.Shared;

public static class PublisherExtensions
{
    public static async Task PublishAll(this IPublisher publisher, List<INotification> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}