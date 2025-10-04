using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth.JWT
{
    public class TokenService : ITokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secret;
        private readonly int _accessTokenLifetimeMinutes;

        public TokenService(IConfiguration configuration)
        {
            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new ArgumentNullException("JWT_ISSUER", "JWT_ISSUER environment variable is missing");
            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new ArgumentNullException("JWT_AUDIENCE", "JWT_AUDIENCE environment variable is missing");
            _secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new ArgumentNullException("JWT_SECRET", "JWT_SECRET environment variable is missing");
            _accessTokenLifetimeMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFETIME_MINUTES"), out var minutes) ? minutes : 20;
        }

        public string CreateAccessToken(UserEntity user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateAccessToken(string token, bool validateLifetime = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero
            };
            try
            {
                // Here we check the correct validity of the token, and if so we return the ClaimsPrincipal (It has sub,email,role,etc)
                var principal = tokenHandler.ValidateToken(token, parameters, out _);
                return principal;
            }
            catch
            {
                // Token isnt valid
                return null;
            }
        }
    }
}
