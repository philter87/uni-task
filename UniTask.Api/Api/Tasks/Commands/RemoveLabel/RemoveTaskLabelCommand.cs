using MediatR;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.Events;

namespace UniTask.Api.Tasks.Commands.RemoveLabel;

public class RemoveTaskLabelCommand : IRequest<TaskLabelRemovedEvent>, IProviderEvent
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
