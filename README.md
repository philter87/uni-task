# uni-task
A unified TaskManager for Azure DevOps boards and GitHub issues

## Architecture

This project uses **CQRS with MediatR** and **Domain-Driven Design (DDD)** to provide a clean, maintainable, and extensible codebase.

### 1. CQRS + Domain-Driven Design

The application uses **Command Query Responsibility Segregation (CQRS)** to separate read operations (queries) from write operations (commands). This is implemented using **MediatR**, a simple mediator implementation in .NET, combined with **Domain Events** pattern for loose coupling.

#### Commands, Queries, and Domain Events

Every controller in the app should either create a **Command** or a **Query**:

- **Commands**: Represent actions that change system state. They are named as imperative orders:
  - `CreateProject`, `CreateTask`, `ChangeTaskStatus`
  
- **Queries**: Retrieve data without modifying state:
  - `GetAllProjects`, `GetTaskById`, `GetTasksByStatus`

- **Domain Events**: Represent things that have already happened (past tense). They are stored in the entity's `DomainEvents` collection, published before `SaveChanges`, and handled by EventHandlers for side effects and external integrations:
  - `ProjectCreatedEvent`, `TaskCreatedEvent`, `TaskStatusChangedEvent`

#### Flow

```
Controller → Command → Handler → Entity Factory (adds DomainEvents) → Publish DomainEvents → SaveChanges
                                                                             ↓
                                                                        EventHandler (external integrations)
```

1. **Controllers** receive HTTP requests and dispatch Commands or Queries via MediatR
2. **Handlers** process the Commands/Queries:
   - **CommandHandlers** call entity factory methods (e.g., `Project.Create()`), publish domain events using `_publisher.PublishAll(entity.DomainEvents)`, then save to `TaskDbContext`
   - **QueryHandlers** read from `TaskDbContext` directly and return DTOs
3. **Domain Events** are published by handlers before `SaveChangesAsync` and handled by **EventHandlers** for side effects (logging, notifications, external API calls)

**Example: CreateProject Flow**

```
ProjectsController 
    ↓ sends CreateProjectCommand
CreateProjectCommandHandler
    ↓ calls Project.Create(command)
Project (entity)
    ↓ adds ProjectCreatedEvent to DomainEvents collection
CreateProjectCommandHandler
    ↓ publishes entity.DomainEvents via _publisher.PublishAll()
    ↓ saves to TaskDbContext
ProjectCreatedEventHandler
    ↓ performs additional actions (logging, notifications, external sync, etc.)
```

**Key Pattern:**
- Entities have `[NotMapped] public List<INotification> DomainEvents { get; private set; } = new();`
- Static factory methods (e.g., `Project.Create(command)`) create the entity and add events to `DomainEvents`
- Handlers publish events BEFORE `SaveChangesAsync` using `await _publisher.PublishAll(entity.DomainEvents, cancellationToken)`

### 2. External Integrations (GitHub, Azure DevOps)

External provider logic (calling GitHub Issues API, Azure DevOps Boards API, etc.) belongs in **EventHandlers**. This keeps the core command/query flow clean and makes integrations opt-in side effects.

**Example:** To sync a newly created task to GitHub Issues:
- `CreateTaskCommandHandler` saves to the DB and publishes `TaskCreatedEvent`
- A `GitHubTaskSyncEventHandler` (implementing `INotificationHandler<TaskCreatedEvent>`) calls the GitHub API

#### GitHub Concept Mapping

| UniTask concept | GitHub equivalent |
|-----------------|-------------------|
| **Project**     | **Repository** — A UniTask project points to a single GitHub repository. Configure it via `ExternalId` (e.g. `owner/repo`). |
| **Task**        | **Issue** — Each UniTask task synced to GitHub maps to a GitHub issue. |

> **Important:** A *GitHub Project* (the GitHub planning board feature) is **not** the same as a UniTask project. GitHub projects are a kanban/sprint planning tool, whereas UniTask projects map to repositories. For this reason, creating a UniTask project does **not** create any corresponding entity in GitHub — the repository is configured separately.

## Folder Structure

The project follows a **feature-based** organization combined with **model co-location**, where related code is grouped by feature rather than by technical layer.

### Api Folder Organization

```
UniTask.Api/
└── Api/
    ├── Projects/          # Project feature
    │   ├── Commands/
    │   │   └── Create/    # CreateProjectCommand, Handler
    │   ├── Queries/
    │   │   ├── GetProject/
    │   │   └── GetProjects/
    │   ├── Models/        # DB entity models
    │   │   ├── Project.cs
    │   │   ├── Organisation.cs
    │   │   ├── Board.cs
    │   │   └── ...
    │   ├── Events/        # Events and EventHandlers
    │   │   ├── ProjectCreatedEvent.cs
    │   │   └── ProjectCreatedEventHandler.cs
    │   ├── ProjectDto.cs  # Project DTO
    │   └── ProjectsController.cs
    │
    ├── Tasks/             # Task feature
    │   ├── Commands/
    │   │   ├── Create/, Update/, Delete/
    │   │   ├── ChangeStatus/, AddLabel/, RemoveLabel/
    │   │   └── AssignMember/
    │   ├── Queries/
    │   │   ├── GetTask/, GetTasks/
    │   ├── Models/        # DB entity models
    │   │   ├── TaskItem.cs
    │   │   ├── Comment.cs
    │   │   ├── Label.cs
    │   │   └── ...
    │   ├── Events/        # Events and EventHandlers
    │   │   ├── TaskCreatedEvent.cs
    │   │   ├── TaskCreatedEventHandler.cs
    │   │   └── ...
    │   ├── TaskItemDto.cs # TaskItem DTO
    │   ├── TaskItemMapper.cs # Shared entity → DTO mapping
    │   └── TasksController.cs
    │
    └── Shared/            # Shared components across features
        ├── TaskDbContext.cs
        ├── TaskProvider.cs  # TaskProvider enum
        └── [Status, TaskType, Label, Comment models/DTOs]
```

### Feature Folders (Projects, Tasks)

Each feature folder contains:

- **Models/** (`Models/*.cs`): DB entity models - classes mapped to database tables
  - Example: `Project.cs`, `TaskItem.cs`
  
- **DTO** (`*Dto.cs`): Data Transfer Objects for API communication
  - Example: `ProjectDto.cs`, `TaskItemDto.cs`
  
- **Controller** (`*Controller.cs`): API endpoints for the feature
  - Example: `ProjectsController.cs`, `TasksController.cs`
  
- **Commands/** and **Queries/**: CQRS sub-folders (without events)
  - Example: `Projects/Commands/Create/` contains:
    - `CreateProjectCommand.cs` - The command
    - `CreateProjectCommandHandler.cs` - Command handler (injects `TaskDbContext`)

- **Events/**: Events and EventHandlers for the feature
  - Example: `Projects/Events/` contains:
    - `ProjectCreatedEvent.cs` - Event raised after creation
    - `ProjectCreatedEventHandler.cs` - Event handler (side effects / external integrations)

## Testing with the Any Class

### Integration Testing Approach

This project uses **integration tests** with `CustomWebApplicationFactory` to test the complete CQRS flow including MediatR, handlers, and database operations.

**Pattern:**
```csharp
public class CreateProjectHandlerTests
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly IMediator _mediator;
    private readonly TaskDbContext _dbContext;

    public CreateProjectHandlerTests()
    {
        var scope = _factory.Services.CreateScope();
        _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        _dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    }
    
    [Fact]
    public async Task Should_CreateProjectInDatabase_WhenCommandIsExecuted()
    {
        // Arrange
        var command = Any.CreateProjectCommand();

        // Act
        await _mediator.Send(command);
        
        // Assert
        Assert.Single(_dbContext.Projects);
    }
}
```

### Test Data Generation with Any

This project uses the `Any` class to generate random test data, making tests more maintainable and less brittle.

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

// Create commands with random data
var command = Any.CreateProjectCommand();
var commandWithOverrides = Any.CreateProjectCommand(name: "Specific Name");

// Create complex objects with random data
var project = Any.Project();              // All fields randomized
var task = Any.TaskItem(
    title: "Specific Title",              // Override specific fields
    priority: 8.5                         // Keep others random
);
```

### Available Methods

**Primitive Types:**
- `Any.String(length)` - Random string
- `Any.Int(min, max)` - Random integer
- `Any.Email()` - Random email address
- `Any.Enum<T>()` - Random enum value
- `Any.DateTime(minDaysAgo, maxDaysAgo)` - Random DateTime

**Commands:**
- `Any.CreateProjectCommand(...)` - Random CreateProjectCommand with optional overrides

**Entity Models:**
- `Any.Project(...)` - Random Project with optional overrides
- `Any.TaskItem(...)` - Random TaskItem with optional overrides
- `Any.TaskType(...)` - Random TaskType with optional overrides
- `Any.Status(...)` - Random Status with optional overrides

**Important**: When adding new commands, always add a corresponding factory method to the `Any` class.

We strongly encourage using the `Any` class in all test cases to keep tests clean, focused, and maintainable.

