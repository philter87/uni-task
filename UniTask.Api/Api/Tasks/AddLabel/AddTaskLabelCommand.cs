using MediatR;

namespace UniTask.Api.Tasks.AddLabel;

public class AddTaskLabelCommand : IRequest<TaskItemDto>
{
    public int TaskId { get; set; }
    public int LabelId { get; set; }
}
