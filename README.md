# uni-task
A unified TaskManager for Azure DevOps boards and GitHub issues

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

- `Any.String(length)` - Random string
- `Any.Int(min, max)` - Random integer
- `Any.Email()` - Random email address
- `Any.Enum<T>()` - Random enum value
- `Any.DateTime(minDaysAgo, maxDaysAgo)` - Random DateTime
- `Any.Project(...)` - Random Project with optional overrides
- `Any.TaskItem(...)` - Random TaskItem with optional overrides
- `Any.TaskType(...)` - Random TaskType with optional overrides
- `Any.Sprint(...)` - Random Sprint with optional overrides
- `Any.Status(...)` - Random Status with optional overrides

We strongly encourage using the `Any` class in all test cases to keep tests clean, focused, and maintainable.

## Note About This PR

This PR addresses the issue of cleaning up the git history. Due to automation limitations that prevent force-pushing, manual intervention is required to complete the history reset.

**Please see [HISTORY_RESET_INSTRUCTIONS.md](HISTORY_RESET_INSTRUCTIONS.md) for detailed instructions on how to reset the repository history to a single commit.**
