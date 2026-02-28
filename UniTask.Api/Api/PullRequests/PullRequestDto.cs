namespace UniTask.Api.PullRequests;

public class PullRequestDto
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public string? Repository { get; set; }
    public string? SourceBranch { get; set; }
    public string? TargetBranch { get; set; }
    public Guid? MergeStatusId { get; set; }
    public MergeStatusDto? MergeStatus { get; set; }
    public Guid TaskItemId { get; set; }
}
