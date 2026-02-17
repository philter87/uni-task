using UniTask.Api.Tasks;

namespace UniTask.Api.Shared;

public class Comment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public required string Content { get; set; }
    public required string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public TaskItem TaskItem { get; set; } = null!;
}
