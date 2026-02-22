namespace UniTask.Api.Tasks;

public class Attachment
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string InternalName { get; set; }
    public required string FileType { get; set; }
    
    // Foreign Keys
    public int TaskItemId { get; set; }
    public int? UploadedById { get; set; }
    
    // Navigation properties
    public TaskItem TaskItem { get; set; } = null!;
    public UniTask.Api.Users.UniUser? UploadedBy { get; set; }
}
