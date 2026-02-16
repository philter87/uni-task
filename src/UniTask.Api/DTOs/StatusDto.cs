namespace UniTask.Api.DTOs;

public class StatusDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public int? ProjectId { get; set; }
}
