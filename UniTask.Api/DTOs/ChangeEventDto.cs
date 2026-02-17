namespace UniTask.Api.DTOs;

public class ChangeEventDto
{
    public int Id { get; set; }
    public required string EventId { get; set; }
    public int? ProjectId { get; set; }
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public required string Operation { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? ActorUserId { get; set; }
    public int Version { get; set; }
    public string? Payload { get; set; }
}
