using MediatR;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.AddLabel;

public class AddTaskLabelCommand : IRequest<TaskLabelAddedEvent>
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
}
