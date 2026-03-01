using System.ComponentModel.DataAnnotations.Schema;
using MediatR;
using UniTask.Api.Organisations.Create;
using UniTask.Api.Projects.Models;
using UniTask.Api.Shared;

namespace UniTask.Api.Organisations.Models;

public class Organisation
{
    [NotMapped]
    public List<INotification> DomainEvents { get; private set; } = new();

    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public TaskProvider? Provider { get; set; }
    public bool IsPersonal { get; set; }

    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<OrganisationMember> Members { get; set; } = new List<OrganisationMember>();
    public ICollection<TaskProviderAuth> Auths { get; set; } = new List<TaskProviderAuth>();

    public static Organisation Create(CreateOrganisationCommand command)
    {
        var organisation = new Organisation
        {
            Id = command.Id,
            Name = command.Name,
            ExternalId = command.ExternalId,
            IsPersonal = command.IsPersonal,
        };

        organisation.DomainEvents.Add(new OrganisationCreatedEvent
        {
            OrganisationId = organisation.Id,
            Name = organisation.Name,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });

        return organisation;
    }
}
