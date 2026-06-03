using AutoMapper;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace Tests
{
    public sealed class UsersServiceTests : IDisposable
    {
        private readonly TodoDbContext context;
        private readonly Mock<ILogger<UsersService>> logger;
        private readonly Mock<IPasswordService> passwordService;
        private readonly Mock<ITokensService> tokensService;
        private readonly IMapper mapper;
        private readonly UsersService usersService;

        public UsersServiceTests()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            context = new TodoDbContext(options);
            logger = new Mock<ILogger<UsersService>>();
            passwordService = new Mock<IPasswordService>();
            tokensService = new Mock<ITokensService>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutomapperProfile>(), new LoggerFactory());
            mapper = config.CreateMapper();

            usersService = new UsersService(
                context,
                mapper,
                logger.Object,
                passwordService.Object,
                tokensService.Object
            );
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
            GC.SuppressFinalize(this);
        }

        #region RegisterAsync
        [Fact]
        public async Task RegisterAsync_ThrowsIfEmailAlreadyExists()
        {
            // Arrange
            var existingEmail = "user@example.com";
            var existingUser = new UserEntity()
            {
                Id = Guid.NewGuid(),
                Email = existingEmail,
                PasswordHash = "some_hashed_password"
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var userAddRequest = new UserAddRequest()
            {
                Email = existingEmail,
                Password = "Password123"
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await usersService.RegisterAsync(userAddRequest);
            });
        }

        [Theory]
        [InlineData("newuser@example.com", "SecurePass123")]
        public async Task RegisterAsync_RegistersUserIfValidData(string email, string password)
        {
            // Arrange
            var expectedHash = "generated_hash_value";
            passwordService.Setup(p => p.Generate(password)).Returns(expectedHash);

            var userAddRequest = new UserAddRequest()
            {
                Email = email,
                Password = password
            };

            // Action
            await usersService.RegisterAsync(userAddRequest);

            // Assert
            var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(dbUser);
            Assert.Equal(expectedHash, dbUser.PasswordHash);
            Assert.NotEqual(Guid.Empty, dbUser.Id);

            passwordService.Verify(p => p.Generate(password), Times.Once);
        }
        #endregion

        #region LoginAsync
        [Fact]
        public async Task LoginAsync_ThrowsIfUserDoesNotExist()
        {
            // Arrange
            var userLoginRequest = new UserLoginRequest()
            {
                Email = "nonexistent@example.com",
                Password = "SomePassword"
            };

            // Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                // Action
                await usersService.LoginAsync(userLoginRequest);
            });
        }

        [Fact]
        public async Task LoginAsync_ThrowsIfPasswordIsInvalid()
        {
            // Arrange
            var email = "user@example.com";
            var correctHash = "correct_hash";
            var wrongPassword = "WrongPassword";

            var user = new UserEntity() { Id = Guid.NewGuid(), Email = email, PasswordHash = correctHash };
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            passwordService.Setup(p => p.Verify(wrongPassword, correctHash)).Returns(false);

            var userLoginRequest = new UserLoginRequest()
            {
                Email = email,
                Password = wrongPassword
            };

            // Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                // Action
                await usersService.LoginAsync(userLoginRequest);
            });

            passwordService.Verify(p => p.Verify(wrongPassword, correctHash), Times.Once);
        }

        [Theory]
        [InlineData("validuser@example.com", "CorrectPassword123")]
        public async Task LoginAsync_ReturnsTokenIfCredentialsAreValid(string email, string password)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var passwordHash = "valid_hash";
            var expectedToken = "mocked_jwt_token_string";

            var user = new UserEntity() { Id = userId, Email = email, PasswordHash = passwordHash };
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            passwordService.Setup(p => p.Verify(password, passwordHash)).Returns(true);
            tokensService.Setup(t => t.GenerateToken(userId)).Returns(expectedToken);

            var userLoginRequest = new UserLoginRequest()
            {
                Email = email,
                Password = password
            };

            // Action
            var token = await usersService.LoginAsync(userLoginRequest);

            // Assert
            Assert.NotNull(token);
            Assert.Equal(expectedToken, token);

            passwordService.Verify(p => p.Verify(password, passwordHash), Times.Once);
            tokensService.Verify(t => t.GenerateToken(userId), Times.Once);
        }
        #endregion

        #region UpdatePasswordAsync
        [Fact]
        public async Task UpdatePasswordAsync_ThrowsIfCurrentAndNewPasswordsAreSame()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var samePassword = "SamePassword123";

            var userUpdateRequest = new UserUpdateRequest()
            {
                CurrentPassword = samePassword,
                NewPassword = samePassword
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await usersService.UpdatePasswordAsync(userId, userUpdateRequest);
            });
        }

        [Fact]
        public async Task UpdatePasswordAsync_ThrowsIfUserDoesNotExist()
        {
            // Arrange
            var invalidUserId = Guid.NewGuid();
            var userUpdateRequest = new UserUpdateRequest()
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123"
            };

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await usersService.UpdatePasswordAsync(invalidUserId, userUpdateRequest);
            });
        }

        [Fact]
        public async Task UpdatePasswordAsync_ThrowsIfCurrentPasswordIsInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var storedHash = "stored_hash";
            var wrongCurrentPassword = "WrongCurrentPassword";

            var user = new UserEntity() { Id = userId, Email = "user@example.com", PasswordHash = storedHash };
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            passwordService.Setup(p => p.Verify(wrongCurrentPassword, storedHash)).Returns(false);

            var userUpdateRequest = new UserUpdateRequest()
            {
                CurrentPassword = wrongCurrentPassword,
                NewPassword = "BrandNewPassword123"
            };

            // Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                // Action
                await usersService.UpdatePasswordAsync(userId, userUpdateRequest);
            });

            passwordService.Verify(p => p.Verify(wrongCurrentPassword, storedHash), Times.Once);
        }

        [Theory]
        [InlineData("OldPass123", "NewPass123", "new_generated_hash")]
        public async Task UpdatePasswordAsync_UpdatesPasswordIfValidData(string currentPassword, string newPassword, string newHash)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldHash = "old_stored_hash";

            var user = new UserEntity() { Id = userId, Email = "user@example.com", PasswordHash = oldHash };
            context.Users.Add(user);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            passwordService.Setup(p => p.Verify(currentPassword, oldHash)).Returns(true);
            passwordService.Setup(p => p.Generate(newPassword)).Returns(newHash);

            var userUpdateRequest = new UserUpdateRequest()
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            // Action
            await usersService.UpdatePasswordAsync(userId, userUpdateRequest);

            // Assert
            var dbUser = await context.Users.FindAsync([userId], TestContext.Current.CancellationToken);
            Assert.NotNull(dbUser);
            Assert.Equal(newHash, dbUser.PasswordHash);

            passwordService.Verify(p => p.Verify(currentPassword, oldHash), Times.Once);
            passwordService.Verify(p => p.Generate(newPassword), Times.Once);
        }
        #endregion
    }
}