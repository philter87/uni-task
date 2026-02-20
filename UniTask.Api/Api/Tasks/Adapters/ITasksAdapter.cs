using UniTask.Api.Tasks.Commands.AddLabel;
using UniTask.Api.Tasks.Commands.AssignMember;
using UniTask.Api.Tasks.Commands.ChangeStatus;
using UniTask.Api.Tasks.Commands.Create;
using UniTask.Api.Tasks.Commands.Delete;
using UniTask.Api.Tasks.Commands.RemoveLabel;
using UniTask.Api.Tasks.Commands.Update;
using UniTask.Api.Tasks.Queries.GetTask;
using UniTask.Api.Tasks.Queries.GetTasks;

namespace UniTask.Api.Tasks.Adapters;

public interface ITasksAdapter
{
    Task<TaskCreatedEvent> Handle(CreateTaskCommand command);
    Task<TaskUpdatedEvent> Handle(UpdateTaskCommand command);
    Task<TaskDeletedEvent> Handle(DeleteTaskCommand command);
    Task<TaskStatusChangedEvent> Handle(ChangeTaskStatusCommand command);
    Task<MemberAssignedToTaskEvent> Handle(AssignMemberToTaskCommand command);
    Task<TaskLabelAddedEvent> Handle(AddTaskLabelCommand command);
    Task<TaskLabelRemovedEvent> Handle(RemoveTaskLabelCommand command);
    Task<IEnumerable<TaskItemDto>> Handle(GetTasksQuery query);
    Task<TaskItemDto?> Handle(GetTaskQuery query);
}
