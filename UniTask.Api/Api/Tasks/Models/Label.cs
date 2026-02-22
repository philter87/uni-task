namespace UniTask.Api.Tasks;

public class Label
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Foreign Key
    public int? TypeId { get; set; }

    // Navigation properties
    public LabelType? LabelType { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
