using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Users.Repositories;

namespace Wasel.Api.Shared.Security;

public class ActiveUserRequirementHandler : AuthorizationHandler<ActiveUserRequirement>
{
    private readonly IUserRepository _userRepository;

    public ActiveUserRequirementHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ActiveUserRequirement requirement)
    {
        var keycloakId = context.User?.FindFirstValue("sub") 
                      ?? context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? context.User?.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;

        if (string.IsNullOrEmpty(keycloakId))
        {
            context.Fail();
            return;
        }

        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        
        if (user is null)
        {
            context.Fail();
            return;
        }

        if (user.Status == UserStatus.Active)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
