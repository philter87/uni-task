namespace UniTask.Api.PullRequests;

public class MergeStatusDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
