using MediatR;

namespace UniTask.Api.Tasks.RemoveLabel;

public class RemoveTaskLabelCommand : IRequest<TaskItemDto>
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
}
