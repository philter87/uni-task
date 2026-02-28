namespace UniTask.Api.PullRequests;

public class MergeStatus
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<PullRequest> PullRequests { get; set; } = new List<PullRequest>();
}
