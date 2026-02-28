using Microsoft.AspNetCore.Identity;

namespace UniTask.Api.Users;

public class UniUser : IdentityUser<Guid>
{
    public string? ExternalId { get; set; }
    public string? DisplayName { get; set; }
}
