using UniTask.Api.Tasks.Models;
using UniTask.Api.Users;

namespace UniTask.Api.PullRequests;

public class PullRequest
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Repository { get; set; }
    public string? SourceBranch { get; set; }
    public string? TargetBranch { get; set; }
    
    // Foreign Keys
    public Guid? MergeStatusId { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    
    // Navigation properties
    public MergeStatus? MergeStatus { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public UniUser? CreatedByUser { get; set; }
    public UniUser? UpdatedByUser { get; set; }
}
