using System.Security.Claims;
using Domain.Entities;

namespace Infrastructure.Auth.JWT.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(UserEntity user);    // short lived (20m)
    ClaimsPrincipal? ValidateAccessToken(string token, bool validateLifetime = true);
}
