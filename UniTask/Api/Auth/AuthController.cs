using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniTask.Api.Organisations.Create;
using UniTask.Api.Organisations.Models;
using UniTask.Api.Shared;
using UniTask.Api.Users;

namespace UniTask.Api.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<UniUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly IMediator _mediator;
    private readonly TaskDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<UniUser> userManager,
        JwtService jwtService,
        IMediator mediator,
        TaskDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _mediator = mediator;
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("login/{provider}")]
    [AllowAnonymous]
    public IActionResult Login(string provider)
    {
        var scheme = NormalizeScheme(provider);
        var redirectUrl = Url.Action(nameof(Callback), new { provider });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, scheme);
    }

    [HttpGet("callback/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(string provider)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";

        var result = await HttpContext.AuthenticateAsync(NormalizeScheme(provider));
        if (!result.Succeeded)
            return Redirect($"{frontendUrl}/login?error=auth_failed");

        var principal = result.Principal!;
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.FindFirstValue(ClaimTypes.Name);
        var externalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        // GitHub sets a custom claim; Google uses "picture"
        var avatarUrl = principal.FindFirstValue("urn:github:avatar")
                     ?? principal.FindFirstValue("picture");

        if (string.IsNullOrEmpty(email))
            return Redirect($"{frontendUrl}/login?error=no_email");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new UniUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = name,
                ExternalId = externalId,
                AvatarUrl = avatarUrl,
            };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return Redirect($"{frontendUrl}/login?error=create_failed");
        }
        else
        {
            user.AvatarUrl = avatarUrl ?? user.AvatarUrl;
            user.DisplayName = name ?? user.DisplayName;
            await _userManager.UpdateAsync(user);
        }

        // Auto-create personal organisation on first login
        if (user.PersonalOrganisationId == null)
        {
            var orgId = await _mediator.Send(new CreateOrganisationCommand
            {
                Name = $"{user.DisplayName ?? user.Email}'s Workspace",
                IsPersonal = true,
            });

            _context.OrganisationMembers.Add(new OrganisationMember
            {
                OrganisationId = orgId,
                UserId = user.Id,
                Role = "Owner",
            });
            await _context.SaveChangesAsync();

            user.PersonalOrganisationId = orgId;
            await _userManager.UpdateAsync(user);
        }

        var token = _jwtService.CreateToken(user);
        return Redirect($"{frontendUrl}/auth/callback?token={token}");
    }

    private static string NormalizeScheme(string provider) =>
        provider.ToLower() switch
        {
            "github" => "GitHub",
            "google" => "Google",
            _ => provider,
        };

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return Unauthorized();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            PersonalOrganisationId = user.PersonalOrganisationId,
        });
    }
}
