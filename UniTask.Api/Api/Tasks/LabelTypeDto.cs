namespace UniTask.Api.Tasks;

public class LabelTypeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }
}
