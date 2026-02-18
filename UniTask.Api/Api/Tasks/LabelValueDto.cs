namespace UniTask.Api.Tasks;

public class LabelValueDto
{
    public int Id { get; set; }
    public required string Value { get; set; }
    public int LabelId { get; set; }
}
