using UniTask.Api.Projects;

namespace UniTask.Api.Tasks;

public static class TaskItemMapper
{
    public static TaskItemDto MapToDto(TaskItem task)
    {
        return new TaskItemDto
        {
            Id = task.Id,
            ExternalId = task.ExternalId,
            Title = task.Title,
            Description = task.Description,
            ProjectId = task.ProjectId,
            TaskTypeId = task.TaskTypeId,
            StatusId = task.StatusId,
            BoardId = task.BoardId,
            ParentId = task.ParentId,
            Priority = task.Priority,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate,
            AssignedTo = task.AssignedTo,
            AssignedToUserId = task.AssignedToUserId,
            Source = task.Source?.ToString(),
            DurationHours = task.DurationHours,
            DurationRemainingHours = task.DurationRemainingHours,
            Project = task.Project == null ? null : new ProjectDto
            {
                Id = task.Project.Id,
                ExternalId = task.Project.ExternalId,
                Name = task.Project.Name,
                Description = task.Project.Description,
                CreatedAt = task.Project.CreatedAt,
                UpdatedAt = task.Project.UpdatedAt
            },
            TaskType = task.TaskType == null ? null : new TaskTypeDto
            {
                Id = task.TaskType.Id,
                ExternalId = task.TaskType.ExternalId,
                Name = task.TaskType.Name,
                Description = task.TaskType.Description,
                ProjectId = task.TaskType.ProjectId
            },
            Status = task.Status == null ? null : new StatusDto
            {
                Id = task.Status.Id,
                ExternalId = task.Status.ExternalId,
                Name = task.Status.Name,
                Description = task.Status.Description,
                Order = task.Status.Order,
                TaskTypeId = task.Status.TaskTypeId
            },
            Board = task.Board == null ? null : new BoardDto
            {
                Id = task.Board.Id,
                ExternalId = task.Board.ExternalId,
                Name = task.Board.Name,
                Goal = task.Board.Goal,
                StartDate = task.Board.StartDate,
                EndDate = task.Board.EndDate,
                ProjectId = task.Board.ProjectId
            },
            Comments = task.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                TaskItemId = c.TaskItemId,
                Content = c.Content,
                UserId = c.UserId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            Labels = task.Labels.Select(l => new LabelDto
            {
                Id = l.Id,
                Name = l.Name,
                TypeId = l.TypeId,
                LabelType = l.LabelType == null ? null : new LabelTypeDto
                {
                    Id = l.LabelType.Id,
                    Name = l.LabelType.Name,
                    Color = l.LabelType.Color
                }
            }).ToList(),
            Tags = task.Tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
    }
}
