# uni-task
A unified TaskManager for Azure DevOps boards and GitHub issues

## Architecture

This project combines two architectural patterns to provide a clean, maintainable, and extensible codebase:

### 1. CQRS Pattern with MediatR

The application uses **Command Query Responsibility Segregation (CQRS)** to separate read operations (queries) from write operations (commands). This is implemented using **MediatR**, a simple mediator implementation in .NET.

#### Commands, Queries, and Events

Every controller in the app should either create a **Command** or a **Query**:

- **Commands**: Represent actions that change system state. They are named as imperative orders:
  - `CreateProject`, `CreateTask`, `ChangeTaskStatus`
  
- **Queries**: Retrieve data without modifying state:
  - `GetAllProjects`, `GetTaskById`, `GetTasksByStatus`

- **Events**: Represent things that have already happened (past tense). They are created by CommandHandlers after successful operations:
  - `ProjectCreated`, `TaskCreated`, `TaskStatusChanged`

#### Flow

```
Controller → Command/Query → Handler → Adapter → Event (for commands)
                                           ↓
                                      Event Handler
```

1. **Controllers** receive HTTP requests and dispatch Commands or Queries via MediatR
2. **Handlers** process the Commands/Queries:
   - **CommandHandlers** interact with Adapters and create Events when operations succeed
   - **QueryHandlers** retrieve and return data
3. **Events** are dispatched by MediatR and can be handled by multiple **EventHandlers**
4. **Adapters** handle the actual data persistence or external API integration

**Example: CreateProject Flow**

```
ProjectsController 
    ↓ sends CreateProjectCommand
CreateProjectCommandHandler
    ↓ calls adapter
LocalAdapter
    ↓ saves to database
CreateProjectCommandHandler
    ↓ publishes ProjectCreatedEvent
ProjectCreatedEventHandler
    ↓ performs additional actions (logging, notifications, etc.)
```

### 2. Adapter Pattern

This project uses an **Adapter Pattern** to enable seamless integration with multiple task management backends (local database, GitHub Issues, Azure DevOps Boards, etc.) while maintaining a consistent API for the frontend.

### Why Adapters?

The adapter layer decouples the API controller from specific task management implementations, providing:

- **Backend Flexibility**: Switch between different task managers (local, GitHub, Azure DevOps) without changing the controller code
- **Consistent API**: Frontend always receives/sends DTOs regardless of the underlying backend
- **Easy Extension**: Add new task manager integrations by implementing the `ITaskAdapter` interface
- **Clean Separation**: Business logic stays in adapters, controllers remain thin and focused on HTTP concerns

#### How It Works

```
Frontend ←→ Controller ←→ Command/Query ←→ Handler ←→ ITaskAdapter ←→ [LocalAdapter | GithubAdapter | AzureDevOpsAdapter]
                                                                              ↓              ↓               ↓
                                                                        Local DB      GitHub API    Azure DevOps API
```

1. **DTOs (Data Transfer Objects)**: Plain objects that define the contract between frontend and backend
   - `TaskItemDto`, `ProjectDto`, `StatusDto`, `TaskTypeDto`, `SprintDto`, `CommentDto`, `LabelDto`
   
2. **ITaskAdapter Interface**: Defines the operations all task managers must support
   ```csharp
   public interface ITaskAdapter
   {
       Task<IEnumerable<TaskItemDto>> GetAllTasksAsync();
       Task<TaskItemDto?> GetTaskByIdAsync(int id);
       Task<TaskItemDto> CreateTaskAsync(TaskItemDto taskDto);
       Task<bool> UpdateTaskAsync(int id, TaskItemDto taskDto);
       Task<bool> DeleteTaskAsync(int id);
   }
   ```

3. **Adapters**: Concrete implementations that handle the mapping and communication
   - **LocalAdapter**: Maps DTOs to/from Entity Framework entities for local database storage
   - **GithubAdapter** *(planned)*: Maps DTOs to/from GitHub Issues API
   - **AzureDevOpsAdapter** *(planned)*: Maps DTOs to/from Azure DevOps Boards API

### Using the Adapter Pattern

The adapter is injected into controllers via dependency injection:

```csharp
public class TasksController : ControllerBase
{
    private readonly ITaskAdapter _adapter;

    public TasksController(ITaskAdapter adapter)
    {
        _adapter = adapter;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
    {
        var tasks = await _adapter.GetAllTasksAsync();
        return Ok(tasks);
    }
}
```

To switch backends, simply register a different adapter implementation in `Program.cs`:

```csharp
// Use local database
builder.Services.AddScoped<ITaskAdapter, LocalAdapter>();

// Or use GitHub (when implemented)
// builder.Services.AddScoped<ITaskAdapter, GithubAdapter>();
```

## Testing with the Any Class

This project uses the `Any` class in test projects to generate random test data, making tests more maintainable and less brittle. The `Any` class is a static helper that provides methods to create random primitive values and complex objects with randomized properties.

### Why Use Any?

- **Reduces Boilerplate**: Instead of manually creating test objects with hardcoded values, use `Any` methods to generate them quickly
- **Improves Test Maintainability**: Tests focus on the behavior being tested rather than specific test data
- **Prevents Test Coupling**: Random data helps ensure tests don't accidentally depend on specific values
- **Makes Intent Clear**: Optional parameters let you specify only the values that matter for each test

### Usage Examples

```csharp
// Generate random primitive values
var randomString = Any.String(10);        // Random string of length 10
var randomInt = Any.Int(1, 100);          // Random integer between 1 and 100
var randomEmail = Any.Email();            // Random email address
var randomPriority = Any.Enum<TaskPriority>(); // Random enum value

// Create complex objects with random data
var project = Any.Project();              // All fields randomized
var task = Any.TaskItem(
    title: "Specific Title",              // Override specific fields
    priority: TaskPriority.High           // Keep others random
);

// Adjust nested objects after creation
var project = Any.Project();
project.Tasks.Add(Any.TaskItem());
project.TaskTypes.Add(Any.TaskType(name: "Bug"));
```

### Available Methods

**Primitive Types:**
- `Any.String(length)` - Random string
- `Any.Int(min, max)` - Random integer
- `Any.Email()` - Random email address
- `Any.Enum<T>()` - Random enum value
- `Any.DateTime(minDaysAgo, maxDaysAgo)` - Random DateTime

**Entity Models:**
- `Any.Project(...)` - Random Project with optional overrides
- `Any.TaskItem(...)` - Random TaskItem with optional overrides
- `Any.TaskType(...)` - Random TaskType with optional overrides
- `Any.Sprint(...)` - Random Sprint with optional overrides
- `Any.Status(...)` - Random Status with optional overrides

**DTOs:**
- `Any.TaskItemDto(...)` - Random TaskItemDto with optional overrides
- `Any.ProjectDto(...)` - Random ProjectDto with optional overrides
- `Any.StatusDto(...)` - Random StatusDto with optional overrides
- `Any.TaskTypeDto(...)` - Random TaskTypeDto with optional overrides
- `Any.SprintDto(...)` - Random SprintDto with optional overrides
- `Any.CommentDto(...)` - Random CommentDto with optional overrides
- `Any.LabelDto(...)` - Random LabelDto with optional overrides

We strongly encourage using the `Any` class in all test cases to keep tests clean, focused, and maintainable.

## Note About This PR

This PR addresses the issue of cleaning up the git history. Due to automation limitations that prevent force-pushing, manual intervention is required to complete the history reset.

**Please see [HISTORY_RESET_INSTRUCTIONS.md](HISTORY_RESET_INSTRUCTIONS.md) for detailed instructions on how to reset the repository history to a single commit.**
