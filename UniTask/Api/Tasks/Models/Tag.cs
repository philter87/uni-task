namespace UniTask.Api.Tasks.Models;

public class Tag
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    // Navigation properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
