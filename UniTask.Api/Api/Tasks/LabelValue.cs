namespace UniTask.Api.Tasks;

public class LabelValue
{
    public int Id { get; set; }
    public required string Value { get; set; }
    
    // Foreign Key
    public int LabelId { get; set; }
    
    // Navigation property
    public Label Label { get; set; } = null!;
}
