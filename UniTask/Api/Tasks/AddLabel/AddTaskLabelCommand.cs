using MediatR;
using UniTask.Api.Shared;

namespace UniTask.Api.Tasks.AddLabel;

public class AddTaskLabelCommand : IRequest<TaskLabelAddedEvent>, IProviderEvent
{
    public Guid TaskId { get; set; }
    public Guid LabelId { get; set; }
    public ChangeOrigin Origin { get; set; } = ChangeOrigin.Internal;
    public TaskProvider TaskProvider { get; set; } = TaskProvider.Internal;
}
