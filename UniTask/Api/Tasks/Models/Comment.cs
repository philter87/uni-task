using UniTask.Api.Users;

namespace UniTask.Api.Tasks.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public required string Content { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public TaskItem TaskItem { get; set; } = null!;
    public UniUser? User { get; set; }
}
