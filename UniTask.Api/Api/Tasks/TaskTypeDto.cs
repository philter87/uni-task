namespace UniTask.Api.Tasks;

public class TaskTypeDto
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ProjectId { get; set; }
}
