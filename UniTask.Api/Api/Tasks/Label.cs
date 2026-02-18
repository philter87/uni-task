namespace UniTask.Api.Tasks;

public class Label
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }

    // Navigation properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<LabelValue> Values { get; set; } = new List<LabelValue>();
}
