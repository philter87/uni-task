namespace UniTask.Api.Tasks;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public required string Content { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
