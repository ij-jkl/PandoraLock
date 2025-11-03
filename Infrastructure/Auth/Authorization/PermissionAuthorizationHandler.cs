using System.Security.Claims;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Auth.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        
        if (roleClaim != null && roleClaim.Value == AppRoles.Admin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var permissionClaim = context.User.FindFirst(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (permissionClaim != null)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
