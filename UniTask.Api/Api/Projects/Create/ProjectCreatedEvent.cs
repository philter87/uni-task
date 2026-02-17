using MediatR;

namespace UniTask.Api.Projects.Create;

public class ProjectCreatedEvent : INotification
{
    public int ProjectId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
