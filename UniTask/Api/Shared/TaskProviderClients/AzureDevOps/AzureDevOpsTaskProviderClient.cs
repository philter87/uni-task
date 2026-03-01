using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using UniTask.Api.Projects;
using UniTask.Api.Projects.Create;
using UniTask.Api.Projects.GetProjects;
using UniTask.Api.Tasks;
using UniTask.Api.Tasks.Create;
using UniTask.Api.Tasks.GetTasks;

namespace UniTask.Api.Shared.TaskProviderClients.AzureDevOps;

public class AzureDevOpsTaskProviderClient : ITaskProviderClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TaskDbContext _context;

    public TaskProvider Provider => TaskProvider.AzureDevOps;

    public AzureDevOpsTaskProviderClient(IHttpClientFactory httpClientFactory, TaskDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
    }

    public Task CreateProject(ProjectCreatedEvent projectCreated) => Task.CompletedTask;

    public Task<IEnumerable<ProjectDto>> GetProjects(GetProjectsQuery getProjects) =>
        Task.FromResult(Enumerable.Empty<ProjectDto>());

    public Task CreateTask(TaskCreatedEvent taskCreated) => Task.CompletedTask;

    public async Task<IEnumerable<TaskItemDto>> GetTasks(GetTasksQuery getTasks)
    {
        if (!getTasks.ProjectId.HasValue)
            return Enumerable.Empty<TaskItemDto>();

        var project = await _context.Projects
            .Include(p => p.TaskProviderAuth)
            .FirstOrDefaultAsync(p => p.Id == getTasks.ProjectId.Value);

        if (project?.TaskProviderAuth == null || string.IsNullOrEmpty(project.ExternalId))
            return Enumerable.Empty<TaskItemDto>();

        var auth = project.TaskProviderAuth;
        // AuthTypeId = ADO org URL (e.g. https://dev.azure.com/myorg)
        // ExternalId = ADO project name
        var orgUrl = auth.AuthTypeId.TrimEnd('/');
        var pat = auth.SecretValue;
        var projectName = project.ExternalId;

        var httpClient = CreateClient(orgUrl, pat);

        // Step 1: WIQL query to get work item IDs
        var wiqlUrl = $"{orgUrl}/{Uri.EscapeDataString(projectName)}/_apis/wit/wiql?api-version=7.1";
        var wiqlBody = new { query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project ORDER BY [System.ChangedDate] DESC" };
        var wiqlJson = JsonSerializer.Serialize(wiqlBody);
        var wiqlResponse = await httpClient.PostAsync(wiqlUrl,
            new StringContent(wiqlJson, Encoding.UTF8, "application/json"));

        if (!wiqlResponse.IsSuccessStatusCode)
            return Enumerable.Empty<TaskItemDto>();

        var wiqlContent = await wiqlResponse.Content.ReadAsStringAsync();
        var wiqlResult = JsonSerializer.Deserialize<WiqlResult>(wiqlContent);
        var ids = wiqlResult?.WorkItems?.Select(w => w.Id).Take(200).ToList();

        if (ids == null || ids.Count == 0)
            return Enumerable.Empty<TaskItemDto>();

        // Step 2: Batch fetch work item details
        var idsParam = string.Join(",", ids);
        var fields = "System.Id,System.Title,System.Description,System.State,System.AssignedTo,System.WorkItemType,System.CreatedDate,System.ChangedDate";
        var detailsUrl = $"{orgUrl}/_apis/wit/workitems?ids={idsParam}&fields={Uri.EscapeDataString(fields)}&api-version=7.1";
        var detailsResponse = await httpClient.GetAsync(detailsUrl);

        if (!detailsResponse.IsSuccessStatusCode)
            return Enumerable.Empty<TaskItemDto>();

        var detailsContent = await detailsResponse.Content.ReadAsStringAsync();
        var detailsResult = JsonSerializer.Deserialize<WorkItemsResult>(detailsContent);

        return detailsResult?.Value?.Select(wi => new TaskItemDto
        {
            Id = default,
            ExternalId = wi.Id.ToString(),
            Title = wi.Fields?.Title ?? $"Work Item #{wi.Id}",
            Description = wi.Fields?.Description,
            ProjectId = getTasks.ProjectId,
            Provider = TaskProvider.AzureDevOps,
            AssignedTo = wi.Fields?.AssignedTo?.DisplayName,
            CreatedAt = wi.Fields?.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = wi.Fields?.ChangedDate ?? DateTime.UtcNow,
        }) ?? Enumerable.Empty<TaskItemDto>();
    }

    private HttpClient CreateClient(string orgUrl, string pat)
    {
        var client = _httpClientFactory.CreateClient();
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($":{pat}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private class WiqlResult
    {
        [JsonPropertyName("workItems")]
        public List<WorkItemRef>? WorkItems { get; set; }
    }

    private class WorkItemRef
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    private class WorkItemsResult
    {
        [JsonPropertyName("value")]
        public List<WorkItemDetail>? Value { get; set; }
    }

    private class WorkItemDetail
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fields")]
        public WorkItemFields? Fields { get; set; }
    }

    private class WorkItemFields
    {
        [JsonPropertyName("System.Title")]
        public string? Title { get; set; }

        [JsonPropertyName("System.Description")]
        public string? Description { get; set; }

        [JsonPropertyName("System.State")]
        public string? State { get; set; }

        [JsonPropertyName("System.AssignedTo")]
        public AdoIdentity? AssignedTo { get; set; }

        [JsonPropertyName("System.WorkItemType")]
        public string? WorkItemType { get; set; }

        [JsonPropertyName("System.CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [JsonPropertyName("System.ChangedDate")]
        public DateTime? ChangedDate { get; set; }
    }

    private class AdoIdentity
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }
}
