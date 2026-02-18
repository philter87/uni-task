namespace UniTask.Api.Tasks;

public class Attachment
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string InternalName { get; set; }
    public required string FileType { get; set; }
    
    // Foreign Key
    public int TaskItemId { get; set; }
    
    // Navigation property
    public TaskItem TaskItem { get; set; } = null!;
}
