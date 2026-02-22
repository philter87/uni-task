# uni-task
A unified TaskManager for Azure DevOps boards and GitHub issues

## Architecture

This project uses **CQRS with MediatR** to provide a clean, maintainable, and extensible codebase.

### 1. CQRS Pattern with MediatR

The application uses **Command Query Responsibility Segregation (CQRS)** to separate read operations (queries) from write operations (commands). This is implemented using **MediatR**, a simple mediator implementation in .NET.

#### Commands, Queries, and Events

Every controller in the app should either create a **Command** or a **Query**:

- **Commands**: Represent actions that change system state. They are named as imperative orders:
  - `CreateProject`, `CreateTask`, `ChangeTaskStatus`
  
- **Queries**: Retrieve data without modifying state:
  - `GetAllProjects`, `GetTaskById`, `GetTasksByStatus`

- **Events**: Represent things that have already happened (past tense). They are published by CommandHandlers after successful operations, and handled by EventHandlers for side effects and external integrations:
  - `ProjectCreated`, `TaskCreated`, `TaskStatusChanged`

#### Flow

```
Controller → Command/Query → Handler → DbContext → Event (for commands)
                                                        ↓
                                                   EventHandler (external integrations)
```

1. **Controllers** receive HTTP requests and dispatch Commands or Queries via MediatR
2. **Handlers** process the Commands/Queries:
   - **CommandHandlers** write to `TaskDbContext` directly and publish Events when operations succeed
   - **QueryHandlers** read from `TaskDbContext` directly and return DTOs
3. **Events** are dispatched by MediatR and handled by **EventHandlers** for side effects (logging, notifications, external API calls)

**Example: CreateProject Flow**

```
ProjectsController 
    ↓ sends CreateProjectCommand
CreateProjectCommandHandler
    ↓ saves to TaskDbContext
    ↓ publishes ProjectCreatedEvent
ProjectCreatedEventHandler
    ↓ performs additional actions (logging, notifications, external sync, etc.)
```

### 2. External Integrations (GitHub, Azure DevOps)

External provider logic (calling GitHub Issues API, Azure DevOps Boards API, etc.) belongs in **EventHandlers**. This keeps the core command/query flow clean and makes integrations opt-in side effects.

**Example:** To sync a newly created task to GitHub Issues:
- `CreateTaskCommandHandler` saves to the DB and publishes `TaskCreatedEvent`
- A `GitHubTaskSyncEventHandler` (implementing `INotificationHandler<TaskCreatedEvent>`) calls the GitHub API

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

This project uses the `Any` class in test projects to generate random test data, making tests more maintainable and less brittle.

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

**Entity Models:**
- `Any.Project(...)` - Random Project with optional overrides
- `Any.TaskItem(...)` - Random TaskItem with optional overrides
- `Any.TaskType(...)` - Random TaskType with optional overrides
- `Any.Status(...)` - Random Status with optional overrides

We strongly encourage using the `Any` class in all test cases to keep tests clean, focused, and maintainable.

