namespace UniTask.Api.Tasks;

public class TaskItemRelationDto
{
    public Guid Id { get; set; }
    public Guid FromTaskId { get; set; }
    public Guid ToTaskId { get; set; }
    public string FromRelationType { get; set; } = "Undefined";
    public string ToRelationType { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
}
