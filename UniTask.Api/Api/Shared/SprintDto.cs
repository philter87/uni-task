namespace UniTask.Api.Shared;

public class SprintDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Goal { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ProjectId { get; set; }
}
