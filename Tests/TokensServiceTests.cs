using Microsoft.Extensions.Options;
using Moq;
using Services;
using Services.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Tests
{
    public sealed class TokensServiceTests
    {
        private readonly Mock<IOptions<JwtOptions>> jwtOptionsMock;

        public TokensServiceTests()
        {
            jwtOptionsMock = new Mock<IOptions<JwtOptions>>();
        }

        #region GenerateToken
        [Fact]
        public void GenerateToken_ReturnsValidTokenWithCorrectClaims_WhenUserIdIsProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var jwtOptions = new JwtOptions
            {
                SecretKey = "super_secret_key_that_is_long_enough_for_hmac_sha256",
                ExpiresInHours = 2
            };
            jwtOptionsMock.Setup(o => o.Value).Returns(jwtOptions);

            var tokensService = new TokensService(jwtOptionsMock.Object);

            // Action
            var tokenString = tokensService.GenerateToken(userId);

            // Assert
            Assert.NotNull(tokenString);
            Assert.NotEmpty(tokenString);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenString);

            var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Assert.NotNull(nameIdentifierClaim);
            Assert.Equal(userId.ToString(), nameIdentifierClaim.Value);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12)]
        [InlineData(24)]
        public void GenerateToken_SetsCorrectExpirationTime_BasedOnJwtOptions(double expiresInHours)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var jwtOptions = new JwtOptions
            {
                SecretKey = "super_secret_key_that_is_long_enough_for_hmac_sha256",
                ExpiresInHours = expiresInHours
            };
            jwtOptionsMock.Setup(o => o.Value).Returns(jwtOptions);

            var tokensService = new TokensService(jwtOptionsMock.Object);

            // Action
            var tokenString = tokensService.GenerateToken(userId);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenString);

            var expectedExpiration = DateTime.UtcNow.AddHours(expiresInHours);
            Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
            Assert.True((expectedExpiration - jwtToken.ValidTo).Duration() < TimeSpan.FromMinutes(1));
        }
        #endregion
    }
}