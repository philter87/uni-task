using System.ComponentModel.DataAnnotations.Schema;
using MediatR;
using UniTask.Api.Organisations.CreateTaskProviderAuth;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Models;

public class TaskProviderAuth
{
    [NotMapped]
    public List<INotification> DomainEvents { get; private set; } = new();

    public Guid Id { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public required string AuthTypeId { get; set; }
    public required string SecretValue { get; set; }

    // Navigation properties (many-to-many)
    public ICollection<Organisation> Organisations { get; set; } = new List<Organisation>();

    public static TaskProviderAuth Create(CreateTaskProviderAuthCommand command)
    {
        var auth = new TaskProviderAuth
        {
            Id = command.Id,
            AuthenticationType = command.AuthenticationType,
            AuthTypeId = command.AuthTypeId,
            SecretValue = command.SecretValue
        };

        auth.DomainEvents.Add(new TaskProviderAuthCreatedEvent
        {
            TaskProviderAuthId = auth.Id,
            OrganisationId = command.OrganisationId,
            AuthenticationType = auth.AuthenticationType,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });

        return auth;
    }
}
