using System.ComponentModel.DataAnnotations.Schema;
using MediatR;
using UniTask.Api.Projects.Models;
using UniTask.Api.PullRequests;
using UniTask.Api.Shared;
using UniTask.Api.Tasks.AddLabel;
using UniTask.Api.Tasks.AssignMember;
using UniTask.Api.Tasks.ChangeStatus;
using UniTask.Api.Tasks.Create;
using UniTask.Api.Tasks.Delete;
using UniTask.Api.Tasks.RemoveLabel;
using UniTask.Api.Tasks.Update;
using UniTask.Api.Users;

namespace UniTask.Api.Tasks.Models;

public class TaskItem
{
    [NotMapped]
    public List<INotification> DomainEvents { get; private set; } = new();

    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    
    // Foreign Keys
    public Guid? ProjectId { get; set; }
    public Guid? TaskTypeId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? BoardId { get; set; }
    public Guid? ParentId { get; set; }
    
    public double Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public TaskProvider? Provider { get; set; }
    public string? ExternalId { get; set; }
    
    // New fields
    public double? DurationHours { get; set; }
    public double? DurationRemainingHours { get; set; }
    
    // Navigation properties
    public Project? Project { get; set; }
    public TaskType? TaskType { get; set; }
    public Status? Status { get; set; }
    public Board? Board { get; set; }
    public TaskItem? Parent { get; set; }
    public UniUser? AssignedToUser { get; set; }
    public ICollection<TaskItem> Children { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Label> Labels { get; set; } = new List<Label>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<TaskItemRelation> RelationsFrom { get; set; } = new List<TaskItemRelation>();
    public ICollection<TaskItemRelation> RelationsTo { get; set; } = new List<TaskItemRelation>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<PullRequest> PullRequests { get; set; } = new List<PullRequest>();

    public static TaskItem Create(CreateTaskCommand command)
    {
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            ProjectId = command.ProjectId,
            TaskTypeId = command.TaskTypeId,
            StatusId = command.StatusId,
            BoardId = command.BoardId,
            Priority = command.Priority,
            DueDate = command.DueDate,
            AssignedTo = command.AssignedTo,
            AssignedToUserId = command.AssignedToUserId,
            Provider = command.Provider,
            ExternalId = command.ExternalId,
            DurationHours = command.DurationHours,
            DurationRemainingHours = command.DurationRemainingHours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        taskItem.DomainEvents.Add(new TaskCreatedEvent
        {
            TaskId = taskItem.Id,
            Title = taskItem.Title,
            CreatedAt = taskItem.CreatedAt,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });

        return taskItem;
    }

    public void Update(UpdateTaskCommand command)
    {
        Title = command.Title;
        Description = command.Description;
        StatusId = command.StatusId;
        Priority = command.Priority;
        DueDate = command.DueDate;
        AssignedTo = command.AssignedTo;
        ProjectId = command.ProjectId;
        TaskTypeId = command.TaskTypeId;
        BoardId = command.BoardId;
        DurationHours = command.DurationHours;
        DurationRemainingHours = command.DurationRemainingHours;
        UpdatedAt = DateTime.UtcNow;

        DomainEvents.Add(new TaskUpdatedEvent
        {
            TaskId = Id,
            Title = Title,
            UpdatedAt = UpdatedAt,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });
    }

    public void ChangeStatus(ChangeTaskStatusCommand command)
    {
        StatusId = command.StatusId;
        UpdatedAt = DateTime.UtcNow;

        DomainEvents.Add(new TaskStatusChangedEvent
        {
            TaskId = Id,
            StatusId = command.StatusId,
            ChangedAt = UpdatedAt,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });
    }

    public void Delete(DeleteTaskCommand command)
    {
        DomainEvents.Add(new TaskDeletedEvent
        {
            TaskId = Id,
            DeletedAt = DateTime.UtcNow,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });
    }

    public void AddLabel(Label label, AddTaskLabelCommand command)
    {
        if (!Labels.Any(l => l.Id == label.Id))
        {
            Labels.Add(label);
            UpdatedAt = DateTime.UtcNow;
        }

        DomainEvents.Add(new TaskLabelAddedEvent
        {
            TaskId = Id,
            LabelId = label.Id,
            AddedAt = DateTime.UtcNow,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });
    }

    public void RemoveLabel(Guid labelId, RemoveTaskLabelCommand command)
    {
        var label = Labels.FirstOrDefault(l => l.Id == labelId);
        if (label != null)
        {
            Labels.Remove(label);
            UpdatedAt = DateTime.UtcNow;
        }

        DomainEvents.Add(new TaskLabelRemovedEvent
        {
            TaskId = Id,
            LabelId = labelId,
            RemovedAt = DateTime.UtcNow,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });
    }

    public void AssignMember(AssignMemberToTaskCommand command)
    {
        AssignedTo = command.AssignedTo;
        UpdatedAt = DateTime.UtcNow;

        DomainEvents.Add(new MemberAssignedToTaskEvent
        {
            TaskId = Id,
            AssignedTo = command.AssignedTo,
            AssignedAt = UpdatedAt,
            Origin = command.Origin,
            TaskProvider = command.TaskProvider
        });
    }
}
