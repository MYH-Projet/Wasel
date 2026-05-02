using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Wasel.Api.Shared.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? KeycloakId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string? FirstName => User?.FindFirstValue(ClaimTypes.GivenName);
    public string? LastName => User?.FindFirstValue(ClaimTypes.Surname);
    
    public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
