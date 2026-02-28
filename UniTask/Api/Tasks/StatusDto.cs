namespace UniTask.Api.Tasks;

public class StatusDto
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public Guid? TaskTypeId { get; set; }
}
