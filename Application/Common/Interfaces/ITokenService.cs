using System.Security.Claims;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(UserEntity user);
    ClaimsPrincipal? ValidateAccessToken(string token, bool validateLifetime = true);
}
