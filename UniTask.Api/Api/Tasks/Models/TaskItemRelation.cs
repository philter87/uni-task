namespace UniTask.Api.Tasks;

public class TaskItemRelation
{
    public Guid Id { get; set; }
    
    // Foreign Keys
    public Guid FromTaskId { get; set; }
    public Guid ToTaskId { get; set; }
    
    // Relation types describing both sides
    public TaskRelationType FromRelationType { get; set; }
    public TaskRelationType ToRelationType { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public TaskItem FromTask { get; set; } = null!;
    public TaskItem ToTask { get; set; } = null!;
}
