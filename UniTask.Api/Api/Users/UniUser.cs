using Microsoft.AspNetCore.Identity;

namespace UniTask.Api.Users;

public class UniUser : IdentityUser<int>
{
    public string? ExternalId { get; set; }
    public string? DisplayName { get; set; }
}
