using System.Security.Claims;
using ToDoApp.Extensions;

namespace Tests
{
    public sealed class ClaimsPrincipalExtensionsTests
    {
        #region GetUserId
        [Fact]
        public void GetUserId_ReturnsGuid_WhenClaimIsValid()
        {
            // Arrange
            var expectedUserId = Guid.NewGuid();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Action
            var result = principal.GetUserId();

            // Assert
            Assert.Equal(expectedUserId, result);
        }

        [Fact]
        public void GetUserId_ThrowsUnauthorizedAccessException_WhenClaimIsMissing()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                // Action
                principal.GetUserId();
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-valid-guid-string")]
        public void GetUserId_ThrowsUnauthorizedAccessException_WhenClaimIsInvalidGuid(string invalidGuidString)
        {
            // Arrange
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, invalidGuidString)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            // Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                // Action
                principal.GetUserId();
            });
        }
        #endregion
    }
}