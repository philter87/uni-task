using System.ComponentModel.DataAnnotations.Schema;
using MediatR;
using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Events;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Models;

namespace UniTask.Api.Projects.Models;

public class Project
{
    [NotMapped]
    public List<INotification> DomainEvents { get; private set; } = new();
    
    
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? OrganisationId { get; set; }
    public TaskProvider? Provider { get; set; }

    // Navigation properties
    public Organisation? Organisation { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<TaskType> TaskTypes { get; set; } = new List<TaskType>();

    public static Project Create(CreateProjectCommand command)
    {
        var project = new Project
        {
            Id = command.Id,
            Name = command.Name,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        project.DomainEvents.Add(new ProjectCreatedEvent
        {
            ProjectId = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });

        return project;
    }
}
