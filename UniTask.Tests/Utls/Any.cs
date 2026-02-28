using UniTask.Api.Projects;
using UniTask.Api.Projects.Commands.Create;
using UniTask.Api.Projects.Models;
using UniTask.Api.Shared;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Models;
using UniTask.Api.Users;

namespace UniTask.Tests.Utls;

public static class Any
{
    public static string String(int length = 10)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }

    public static int Int(int min = 1, int max = 1000)
    {
        return Random.Shared.Next(min, max);
    }

    public static Guid GuidRan()
    {
        return Guid.NewGuid();
    }

    public static string Email()
    {
        return $"{String(8)}@{String(6)}.{String(3)}";
    }

    public static T Enum<T>() where T : struct, System.Enum
    {
        var values = System.Enum.GetValues<T>();
        return values[Random.Shared.Next(values.Length)];
    }

    public static DateTime DateTime(int minDaysAgo = 0, int maxDaysAgo = 365)
    {
        var daysAgo = Random.Shared.Next(minDaysAgo, maxDaysAgo);
        return System.DateTime.UtcNow.AddDays(-daysAgo);
    }

    public static string Color()
    {
        return $"#{Int(0, 16777215):X6}";
    }

    public static CreateProjectCommand CreateProjectCommand(
        Guid? id = null,
        string? name = null,
        string? description = null,
        ChangeOrigin? origin = null,
        TaskProvider? taskProvider = null)
    {
        return new CreateProjectCommand()
        {
            Id = id ?? GuidRan(),
            Name = name ?? String(),
            Description = description ?? String(),
            Origin = origin ?? ChangeOrigin.Internal,
            TaskProvider = taskProvider ?? TaskProvider.GitHub,
        };
    }

    public static Project Project(
        string? name = null,
        string? description = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        var created = createdAt ?? DateTime();
        return new Project
        {
            Name = name ?? $"Project {String(8)}",
            Description = description ?? String(20),
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created.AddDays(Random.Shared.Next(0, 30))
        };
    }

    public static TaskItem TaskItem(
        string? title = null,
        string? description = null,
        double? priority = null,
        Guid? projectId = null,
        Guid? taskTypeId = null,
        Guid? statusId = null,
        Guid? boardId = null)
    {
        return new TaskItem
        {
            Title = title ?? $"Task {String(10)}",
            Description = description ?? String(30),
            Priority = priority ?? Random.Shared.NextDouble() * 10,
            ProjectId = projectId,
            TaskTypeId = taskTypeId,
            StatusId = statusId,
            BoardId = boardId,
            CreatedAt = DateTime(),
            UpdatedAt = DateTime()
        };
    }

    public static TaskType TaskType(
        string? name = null,
        string? description = null,
        Guid? projectId = null)
    {
        return new TaskType
        {
            Name = name ?? $"TaskType {String(6)}",
            Description = description ?? String(20),
            ProjectId = projectId
        };
    }

    public static Board Board(
        string? name = null,
        string? goal = null,
        Guid? projectId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime(0, 60);
        return new Board
        {
            Name = name ?? $"Board {String(5)}",
            Goal = goal ?? String(25),
            ProjectId = projectId ?? Guid.NewGuid(),
            StartDate = start,
            EndDate = endDate ?? start.AddDays(Random.Shared.Next(7, 21))
        };
    }

    public static Status Status(
        string? name = null,
        string? description = null,
        int? order = null,
        Guid? taskTypeId = null)
    {
        return new Status
        {
            Name = name ?? $"Status {String(6)}",
            Description = description ?? String(15),
            Order = order ?? Int(0, 10),
            TaskTypeId = taskTypeId
        };
    }

    public static Label Label(
        string? name = null,
        Guid? typeId = null)
    {
        return new Label
        {
            Name = name ?? $"Label {String(6)}",
            TypeId = typeId
        };
    }

    public static LabelType LabelType(
        string? name = null,
        string? color = null)
    {
        return new LabelType
        {
            Name = name ?? $"LabelType {String(6)}",
            Color = color ?? Color()
        };
    }

    public static Tag Tag(
        string? name = null)
    {
        return new Tag
        {
            Name = name ?? $"Tag {String(6)}"
        };
    }

    public static Attachment Attachment(
        string? name = null,
        string? url = null,
        string? internalName = null,
        string? fileType = null,
        string? externalId = null,
        Guid? taskItemId = null)
    {
        return new Attachment
        {
            Name = name ?? $"File {String(8)}.pdf",
            Url = url ?? $"https://example.com/files/{String(16)}",
            InternalName = internalName ?? $"{String(32)}.pdf",
            FileType = fileType ?? "application/pdf",
            ExternalId = externalId,
            TaskItemId = taskItemId ?? Guid.NewGuid()
        };
    }

    public static ProjectDto ProjectDto(
        Guid id = default,
        string? name = null,
        string? description = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        var created = createdAt ?? DateTime();
        return new ProjectDto
        {
            Id = id,
            Name = name ?? $"Project {String(8)}",
            Description = description ?? String(20),
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created.AddDays(Random.Shared.Next(0, 30))
        };
    }

    public static StatusDto StatusDto(
        Guid id = default,
        string? name = null,
        string? description = null,
        int? order = null,
        Guid? taskTypeId = null)
    {
        return new StatusDto
        {
            Id = id,
            Name = name ?? $"Status {String(6)}",
            Description = description ?? String(15),
            Order = order ?? Int(0, 10),
            TaskTypeId = taskTypeId
        };
    }

    public static TaskTypeDto TaskTypeDto(
        Guid id = default,
        string? name = null,
        string? description = null,
        Guid? projectId = null)
    {
        return new TaskTypeDto
        {
            Id = id,
            Name = name ?? $"TaskType {String(6)}",
            Description = description ?? String(20),
            ProjectId = projectId
        };
    }

    public static BoardDto BoardDto(
        Guid id = default,
        string? name = null,
        string? goal = null,
        Guid? projectId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime(0, 60);
        return new BoardDto
        {
            Id = id,
            Name = name ?? $"Board {String(5)}",
            Goal = goal ?? String(25),
            ProjectId = projectId ?? Guid.NewGuid(),
            StartDate = start,
            EndDate = endDate ?? start.AddDays(Random.Shared.Next(7, 21))
        };
    }

    public static CommentDto CommentDto(
        Guid id = default,
        Guid? taskItemId = null,
        string? content = null,
        Guid? userId = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new CommentDto
        {
            Id = id,
            TaskItemId = taskItemId ?? Guid.NewGuid(),
            Content = content ?? String(50),
            UserId = userId,
            CreatedAt = createdAt ?? DateTime(),
            UpdatedAt = updatedAt
        };
    }

    public static LabelDto LabelDto(
        Guid id = default,
        string? name = null,
        Guid? typeId = null)
    {
        return new LabelDto
        {
            Id = id,
            Name = name ?? $"Label {String(6)}",
            TypeId = typeId
        };
    }

    public static TagDto TagDto(
        Guid id = default,
        string? name = null)
    {
        return new TagDto
        {
            Id = id,
            Name = name ?? $"Tag {String(6)}"
        };
    }

    public static AttachmentDto AttachmentDto(
        Guid id = default,
        string? name = null,
        string? url = null,
        string? internalName = null,
        string? fileType = null,
        string? externalId = null)
    {
        return new AttachmentDto
        {
            Id = id,
            Name = name ?? $"File {String(8)}.pdf",
            Url = url ?? $"https://example.com/files/{String(16)}",
            InternalName = internalName ?? $"{String(32)}.pdf",
            FileType = fileType ?? "application/pdf",
            ExternalId = externalId
        };
    }

    public static UniUser User(
        string? email = null,
        string? username = null,
        string? displayName = null,
        string? externalId = null)
    {
        return new UniUser
        {
            Email = email ?? Email(),
            UserName = username ?? $"user_{String(8)}",
            DisplayName = displayName ?? String(12),
            ExternalId = externalId
        };
    }

    public static Organisation Organisation(
        string? name = null,
        string? externalId = null)
    {
        return new Organisation
        {
            Name = name ?? $"Org {String(8)}",
            ExternalId = externalId
        };
    }

    public static TaskProviderAuth TaskProviderAuth(
        AuthenticationType? authenticationType = null,
        string? authTypeId = null,
        string? secretValue = null,
        Guid organisationId = default)
    {
        return new TaskProviderAuth
        {
            AuthenticationType = authenticationType ?? Enum<AuthenticationType>(),
            AuthTypeId = authTypeId ?? String(20),
            SecretValue = secretValue ?? String(40),
            OrganisationId = organisationId
        };
    }

    public static TaskItemDto TaskItemDto(
        Guid id = default,
        string? title = null,
        string? description = null,
        double? priority = null,
        Guid? projectId = null,
        Guid? taskTypeId = null,
        Guid? statusId = null,
        Guid? boardId = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? dueDate = null,
        string? assignedTo = null,
        Guid? assignedToUserId = null,
        TaskProvider? provider = null,
        string? externalId = null,
        double? durationHours = null,
        double? durationRemainingHours = null)
    {
        var created = createdAt ?? DateTime();
        return new TaskItemDto
        {
            Id = id,
            Title = title ?? $"Task {String(10)}",
            Description = description ?? String(30),
            Priority = priority ?? Random.Shared.NextDouble() * 10,
            ProjectId = projectId,
            TaskTypeId = taskTypeId,
            StatusId = statusId,
            BoardId = boardId,
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created,
            DueDate = dueDate,
            AssignedTo = assignedTo,
            AssignedToUserId = assignedToUserId,
            Provider = provider,
            ExternalId = externalId,
            DurationHours = durationHours,
            DurationRemainingHours = durationRemainingHours
        };
    }
}

