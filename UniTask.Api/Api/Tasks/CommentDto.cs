namespace UniTask.Api.Tasks;

public class CommentDto
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public required string Content { get; set; }
    public required string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
