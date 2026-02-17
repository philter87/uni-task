using UniTask.Api.Models;
using Xunit;

namespace UniTask.Tests;

public class AnyTests
{
    [Fact]
    public void String_GeneratesStringOfSpecifiedLength()
    {
        // Act
        var result = Any.String(15);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(15, result.Length);
    }

    [Fact]
    public void Int_GeneratesIntegerInRange()
    {
        // Act
        var result = Any.Int(10, 20);

        // Assert
        Assert.InRange(result, 10, 19);
    }

    [Fact]
    public void Email_GeneratesValidEmailFormat()
    {
        // Act
        var result = Any.Email();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("@", result);
        Assert.Contains(".", result);
        var parts = result.Split('@');
        Assert.Equal(2, parts.Length);
    }

    [Fact]
    public void Enum_ReturnsValidEnumValue()
    {
        // Act
        var result = Any.Enum<TaskPriority>();

        // Assert
        Assert.IsType<TaskPriority>(result);
        Assert.True(System.Enum.IsDefined(typeof(TaskPriority), result));
    }

    [Fact]
    public void DateTime_GeneratesValidDateTime()
    {
        // Act
        var result = Any.DateTime();

        // Assert
        Assert.True(result <= System.DateTime.UtcNow);
        Assert.True(result >= System.DateTime.UtcNow.AddDays(-365));
    }

    [Fact]
    public void Project_GeneratesValidProject()
    {
        // Act
        var result = Any.Project();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        Assert.NotEmpty(result.Name);
        Assert.True(result.CreatedAt <= result.UpdatedAt);
    }

    [Fact]
    public void Project_AllowsOverridingProperties()
    {
        // Act
        var result = Any.Project(name: "Custom Name", description: "Custom Description");

        // Assert
        Assert.Equal("Custom Name", result.Name);
        Assert.Equal("Custom Description", result.Description);
    }

    [Fact]
    public void TaskItem_GeneratesValidTaskItem()
    {
        // Act
        var result = Any.TaskItem();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Title);
        Assert.NotEmpty(result.Title);
        Assert.True(System.Enum.IsDefined(typeof(TaskPriority), result.Priority));
    }

    [Fact]
    public void TaskItem_AllowsOverridingProperties()
    {
        // Act
        var result = Any.TaskItem(
            title: "Custom Title",
            priority: TaskPriority.Critical,
            projectId: 42);

        // Assert
        Assert.Equal("Custom Title", result.Title);
        Assert.Equal(TaskPriority.Critical, result.Priority);
        Assert.Equal(42, result.ProjectId);
    }

    [Fact]
    public void TaskType_GeneratesValidTaskType()
    {
        // Act
        var result = Any.TaskType();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        Assert.NotEmpty(result.Name);
    }

    [Fact]
    public void Sprint_GeneratesValidSprint()
    {
        // Act
        var result = Any.Sprint();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        Assert.True(result.StartDate <= result.EndDate);
    }

    [Fact]
    public void Status_GeneratesValidStatus()
    {
        // Act
        var result = Any.Status();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        Assert.InRange(result.Order, 0, 10);
    }
}
