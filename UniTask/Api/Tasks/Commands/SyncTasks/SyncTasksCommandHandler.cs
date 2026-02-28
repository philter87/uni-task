using MediatR;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Shared;
using UniTask.Api.Shared.TaskProviderClients;
using UniTask.Api.Tasks.Models;
using UniTask.Api.Tasks.Queries.GetTasks;

namespace UniTask.Api.Tasks.Commands.SyncTasks;

public class SyncTasksCommandHandler : IRequestHandler<SyncTasksCommand, IEnumerable<TaskItemDto>>
{
    private readonly TaskDbContext _context;
    private readonly ITaskProviderClient _taskProviderClient;

    public SyncTasksCommandHandler(TaskDbContext context, ITaskProviderClient taskProviderClient)
    {
        _context = context;
        _taskProviderClient = taskProviderClient;
    }

    public async Task<IEnumerable<TaskItemDto>> Handle(SyncTasksCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects.FindAsync([request.ProjectId], cancellationToken);

        var query = new GetTasksQuery
        {
            ProjectId = request.ProjectId,
            ExternalProjectId = project?.ExternalId
        };

        var tasks = await _taskProviderClient.GetTasks(query);
        var taskList = tasks.ToList();

        foreach (var taskDto in taskList)
        {
            TaskItem? existing = null;
            if (!string.IsNullOrEmpty(taskDto.ExternalId))
                existing = await _context.Tasks.FirstOrDefaultAsync(t => t.ExternalId == taskDto.ExternalId && t.ProjectId == request.ProjectId, cancellationToken);

            if (existing == null)
            {
                _context.Tasks.Add(new TaskItem
                {
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    ProjectId = request.ProjectId,
                    ExternalId = taskDto.ExternalId,
                    Priority = taskDto.Priority,
                    AssignedTo = taskDto.AssignedTo,
                    Provider = taskDto.Provider,
                    DueDate = taskDto.DueDate,
                    CreatedAt = taskDto.CreatedAt != default ? taskDto.CreatedAt : DateTime.UtcNow,
                    UpdatedAt = taskDto.UpdatedAt != default ? taskDto.UpdatedAt : DateTime.UtcNow
                });
            }
            else
            {
                existing.Title = taskDto.Title;
                existing.Description = taskDto.Description;
                existing.AssignedTo = taskDto.AssignedTo;
                existing.Priority = taskDto.Priority;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return taskList;
    }
}
