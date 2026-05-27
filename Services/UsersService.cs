using AutoMapper;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;
using Services.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{
    public class UsersService(TodoDbContext context, IMapper mapper, ILogger<UsersService> logger, IOptions<JwtOptions> jwtOptions) : IUsersService
    {
        public async Task RegisterAsync(UserAddRequest userAddRequest)
        {
            if (await context.Users.AnyAsync(u => u.Email == userAddRequest.Email))
            {
                logger.LogWarning("User add failed: Email {UserEmail} already exists.", userAddRequest.Email);
                throw new ArgumentException("User with given email already exists.");
            }

            var user = mapper.Map<UserEntity>(userAddRequest);
            user.Id = Guid.NewGuid();
            user.PasswordHash = PasswordHashHelper.Generate(userAddRequest.Password!);

            context.Users.Add(user);

            await context.SaveChangesAsync();
        }

        public async Task<string> LoginAsync(UserLoginRequest userLoginRequest)
        {
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userLoginRequest.Email);

            if (user == null)
            {
                logger.LogWarning("User not found: User with email {Email} doesn't exist.", userLoginRequest.Email);
                throw new UnauthorizedAccessException("User with given email doesn't exist.");
            }

            if (!PasswordHashHelper.Verify(userLoginRequest.Password!, user.PasswordHash!))
            {
                logger.LogWarning("Invalid password provided.");
                throw new UnauthorizedAccessException("Invalid password.");
            }

            return GenerateToken(user.Id);
        }

        public async Task UpdatePasswordAsync(Guid userId, UserUpdateRequest userUpdateRequest)
        {
            if (userUpdateRequest.CurrentPassword == userUpdateRequest.NewPassword)
            {
                logger.LogWarning("Same password provided two times.");
                throw new ArgumentException("Same password provided two times.");
            }

            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                logger.LogWarning("User not found: User with id {UserId} doesn't exist.", userId);
                throw new KeyNotFoundException("User with given id doesn't exist.");
            }

            if (!PasswordHashHelper.Verify(userUpdateRequest.CurrentPassword!, user.PasswordHash!))
            {
                logger.LogWarning("Invalid password provided.");
                throw new UnauthorizedAccessException("Invalid password.");
            }

            user.PasswordHash = PasswordHashHelper.Generate(userUpdateRequest.NewPassword!);

            await context.SaveChangesAsync();
        }

        private string GenerateToken(Guid userId)
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
