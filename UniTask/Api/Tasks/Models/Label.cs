namespace UniTask.Api.Tasks.Models;

public class Label
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    // Foreign Key
    public Guid? TypeId { get; set; }

    // Navigation properties
    public LabelType? LabelType { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
