using UniTask.Api.Users;

namespace UniTask.Api.Tasks.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string InternalName { get; set; }
    public required string FileType { get; set; }
    
    // Foreign Keys
    public Guid TaskItemId { get; set; }
    public Guid? UploadedById { get; set; }
    
    // Navigation properties
    public TaskItem TaskItem { get; set; } = null!;
    public UniUser? UploadedBy { get; set; }
}
