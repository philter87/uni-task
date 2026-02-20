using MediatR;
using System.ComponentModel.DataAnnotations;

namespace UniTask.Api.Tasks.Commands.Create;

public class CreateTaskCommand : IRequest<TaskCreatedEvent>
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskTypeId { get; set; }
    public int? StatusId { get; set; }
    public int? BoardId { get; set; }
    [Range(0, 10)]
    public double Priority { get; set; } = 5.0;
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Source { get; set; }
    public string? ExternalId { get; set; }
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
}
