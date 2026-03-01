using Microsoft.AspNetCore.Identity;

namespace UniTask.Api.Users;

public class UniUser : IdentityUser<Guid>
{
    public string? ExternalId { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? PersonalOrganisationId { get; set; }
}
