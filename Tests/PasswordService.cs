using Services;

namespace Tests
{
    public sealed class PasswordServiceTests
    {
        private readonly PasswordService passwordService;

        public PasswordServiceTests()
        {
            passwordService = new PasswordService();
        }

        #region Generate
        [Theory]
        [InlineData("Password123")]
        [InlineData("MySecureP@ss!")]
        [InlineData("admin")]
        public void Generate_ReturnsHashedPassword_WhenPlainPasswordIsProvided(string plainPassword)
        {
            // Action
            var hash = passwordService.Generate(plainPassword);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.NotEqual(plainPassword, hash);
        }
        #endregion

        #region Verify
        [Fact]
        public void Verify_ReturnsTrue_WhenPasswordMatchesHash()
        {
            // Arrange
            var password = "CorrectPassword123";
            var hash = passwordService.Generate(password);

            // Action
            var result = passwordService.Verify(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Verify_ReturnsFalse_WhenPasswordDoesNotMatchHash()
        {
            // Arrange
            var password = "CorrectPassword123";
            var wrongPassword = "WrongPassword123";
            var hash = passwordService.Generate(password);

            // Action
            var result = passwordService.Verify(wrongPassword, hash);

            // Assert
            Assert.False(result);
        }
        #endregion
    }
}