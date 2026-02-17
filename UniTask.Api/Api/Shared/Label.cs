using UniTask.Api.Tasks;

namespace UniTask.Api.Shared;

public class Label
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }

    // Navigation properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
