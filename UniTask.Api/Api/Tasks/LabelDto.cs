namespace UniTask.Api.Tasks;

public class LabelDto
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }
}
