namespace UniTask.Api.Shared;

public class TaskTypeDto
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
}
