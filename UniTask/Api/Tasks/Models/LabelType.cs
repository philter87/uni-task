namespace UniTask.Api.Tasks.Models;

public class LabelType
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }

    // Navigation properties
    public ICollection<Label> Labels { get; set; } = new List<Label>();
}
