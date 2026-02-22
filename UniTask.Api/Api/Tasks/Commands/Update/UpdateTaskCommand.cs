using MediatR;
using System.ComponentModel.DataAnnotations;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.Update;

public class UpdateTaskCommand : IRequest<TaskUpdatedEvent>, IProviderEvent
{
    public int TaskId { get; set; }
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
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
