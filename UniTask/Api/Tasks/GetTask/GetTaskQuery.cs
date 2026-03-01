using MediatR;

namespace UniTask.Api.Tasks.GetTask;

public class GetTaskQuery : IRequest<TaskItemDto?>
{
    public Guid Id { get; set; }
}
