using UniTask.Api.Tasks;

namespace UniTask.Api.Shared;

public class TaskItemRelation
{
    public int Id { get; set; }
    
    // Foreign Keys
    public int FromTaskId { get; set; }
    public int ToTaskId { get; set; }
    
    // Relation types describing both sides
    public TaskRelationType FromRelationType { get; set; }
    public TaskRelationType ToRelationType { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public TaskItem FromTask { get; set; } = null!;
    public TaskItem ToTask { get; set; } = null!;
}
