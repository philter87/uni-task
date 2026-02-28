namespace UniTask.Api.Tasks;

public class LabelDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid? TypeId { get; set; }
    public LabelTypeDto? LabelType { get; set; }
}
