using System.ComponentModel.DataAnnotations.Schema;
using MediatR;
using UniTask.Api.Projects.Commands.CreateAuthTaskProvider;
using UniTask.Api.Projects.Events;
using UniTask.Api.Shared;

namespace UniTask.Api.Projects.Models;

public class TaskProviderAuth
{
    [NotMapped]
    public List<INotification> DomainEvents { get; private set; } = new();

    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public required string AuthTypeId { get; set; }
    public required string SecretValue { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;

    public static TaskProviderAuth Create(CreateAuthTaskProviderCommand command)
    {
        var auth = new TaskProviderAuth
        {
            Id = command.Id,
            OrganisationId = command.OrganisationId,
            AuthenticationType = command.AuthenticationType,
            AuthTypeId = command.AuthTypeId,
            SecretValue = command.SecretValue
        };

        auth.DomainEvents.Add(new AuthTaskProviderCreatedEvent
        {
            AuthTaskProviderId = auth.Id,
            OrganisationId = auth.OrganisationId,
            AuthenticationType = auth.AuthenticationType,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });

        return auth;
    }
}
