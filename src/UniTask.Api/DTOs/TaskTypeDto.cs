namespace UniTask.Api.DTOs;

public class TaskTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
}
