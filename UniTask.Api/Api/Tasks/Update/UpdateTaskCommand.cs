using MediatR;
using System.ComponentModel.DataAnnotations;

namespace UniTask.Api.Tasks.Update;

public class UpdateTaskCommand : IRequest<TaskItemDto>
{
    public int TaskId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskTypeId { get; set; }
    public int? StatusId { get; set; }
    public int? BoardId { get; set; }
    [Range(0, 10)]
    public double Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
}
