namespace UniTask.Api.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? PersonalOrganisationId { get; set; }
}
