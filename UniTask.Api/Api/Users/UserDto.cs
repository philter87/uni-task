namespace UniTask.Api.Users;

public class UserDto
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public string? DisplayName { get; set; }
}
