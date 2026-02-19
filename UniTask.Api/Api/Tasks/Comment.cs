namespace UniTask.Api.Tasks;

public class Comment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public required string Content { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public TaskItem TaskItem { get; set; } = null!;
    public UniTask.Api.Users.UniUser? User { get; set; }
}
