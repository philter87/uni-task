namespace UniTask.Api.Shared;

public class TaskItemRelationDto
{
    public int Id { get; set; }
    public int FromTaskId { get; set; }
    public int ToTaskId { get; set; }
    public string FromRelationType { get; set; } = "Undefined";
    public string ToRelationType { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
}
