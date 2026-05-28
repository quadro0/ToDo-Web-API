using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts;
using Services.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{
    public class TokensService(IOptions<JwtOptions> jwtOptions) : ITokensService
    {
        public string GenerateToken(Guid userId)
        {
            var options = jwtOptions.Value;

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(options.SecretKey!));

            var jwtToken = new JwtSecurityToken(
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
                claims: [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                expires: DateTime.UtcNow.AddHours(options.ExpiresInHours)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}
