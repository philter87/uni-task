using UniTask.Api.Tasks;

namespace UniTask.Api.Shared;

public class TaskChange
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public required string Field { get; set; }
    public string? Value { get; set; }
    public DateTime Date { get; set; }
    public required string UserId { get; set; }

    // Navigation properties
    public TaskItem TaskItem { get; set; } = null!;
}
