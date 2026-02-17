using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using UniTask.Api.Infrastructure.Persistence;
using UniTask.Api.DTOs;
using UniTask.Api.Models;
using Xunit;

namespace UniTask.Tests;

public class TasksControllerTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TasksControllerTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task GetTasks_ReturnsEmptyList_WhenNoTasks()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItemDto>>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task CreateTask_ReturnsCreatedTask()
    {
        // Arrange
        var newTask = new TaskItemDto
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = "Medium"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", newTask);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);
        Assert.Equal("Test Task", createdTask.Title);
        Assert.True(createdTask.Id > 0);
        Assert.True(createdTask.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task GetTask_ReturnsTask_WhenTaskExists()
    {
        // Arrange
        var newTask = new TaskItemDto
        {
            Title = "Get Task Test",
            Priority = "High"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{createdTask.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(task);
        Assert.Equal(createdTask.Id, task.Id);
        Assert.Equal("Get Task Test", task.Title);
    }

    [Fact]
    public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTask_UpdatesTask_WhenTaskExists()
    {
        // Arrange
        var newTask = new TaskItemDto
        {
            Title = "Original Title",
            Priority = "Low"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        createdTask.Title = "Updated Title";
        createdTask.Priority = "High";

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{createdTask.Id}", createdTask);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        var updatedTask = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("High", updatedTask.Priority);
    }

    [Fact]
    public async Task DeleteTask_DeletesTask_WhenTaskExists()
    {
        // Arrange
        var newTask = new TaskItemDto
        {
            Title = "Task to Delete",
            Priority = "Medium"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(createdTask);

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{createdTask.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove all existing DbContext-related registrations
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType.Name.Contains("DbContext")).ToList();
            
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing with unique name
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });

        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Ensure database is created
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        return host;
    }
}
