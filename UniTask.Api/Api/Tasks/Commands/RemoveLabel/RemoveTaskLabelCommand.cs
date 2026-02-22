using MediatR;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.RemoveLabel;

public class RemoveTaskLabelCommand : IRequest<TaskLabelRemovedEvent>
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
}
