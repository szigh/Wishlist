using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WishlistModels;
using WishlistWeb.Services;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class JwtTokenServiceTests
    {
        private const string ValidJwtKey = "TestKeyForIntegrationTestsThatIsLongEnough123456";
        private const string JwtIssuer = "WishlistTestApi";
        private const string JwtAudience = "WishlistTestClient";
        private const int ExpirationMinutes = 60;

        private static IConfiguration CreateConfiguration(string? jwtKey = null, string? issuer = null, string? audience = null, int? expirationMinutes = null, bool includeKey = true)
        {
            var configData = new Dictionary<string, string?>();
            
            if (includeKey)
            {
                configData["Jwt:Key"] = jwtKey ?? ValidJwtKey;
            }
            else if (jwtKey != null)
            {
                configData["Jwt:Key"] = jwtKey;
            }
            
            configData["Jwt:Issuer"] = issuer ?? JwtIssuer;
            configData["Jwt:Audience"] = audience ?? JwtAudience;
            configData["Jwt:ExpirationMinutes"] = (expirationMinutes ?? ExpirationMinutes).ToString();

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        private static User CreateTestUser(int id = 1, string name = "TestUser", string role = "user")
        {
            return new User
            {
                Id = id,
                Name = name,
                Role = role,
                PasswordHash = "dummy-hash"
            };
        }

        [Fact]
        public void GenerateToken_WithValidConfiguration_ShouldReturnValidToken()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act
            var token = service.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // Verify it's a valid JWT structure
            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(token));
        }

        [Fact]
        public void GenerateToken_ShouldIncludeUserIdInSubjectClaim()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser(id: 42);

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            
            Assert.NotNull(subClaim);
            Assert.Equal("42", subClaim.Value);
        }

        [Fact]
        public void GenerateToken_ShouldIncludeUserNameClaim()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser(name: "JohnDoe");

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);
            
            Assert.NotNull(nameClaim);
            Assert.Equal("JohnDoe", nameClaim.Value);
        }

        [Fact]
        public void GenerateToken_ShouldIncludeRoleClaim()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser(role: "admin");

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            
            Assert.NotNull(roleClaim);
            Assert.Equal("admin", roleClaim.Value);
        }

        [Fact]
        public void GenerateToken_ShouldIncludeUniqueJtiClaim()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act
            var token1 = service.GenerateToken(user);
            var token2 = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadJwtToken(token1);
            var jwtToken2 = handler.ReadJwtToken(token2);
            
            var jti1 = jwtToken1.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var jti2 = jwtToken2.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            
            Assert.NotNull(jti1);
            Assert.NotNull(jti2);
            Assert.NotEqual(jti1, jti2);
        }

        [Fact]
        public void GenerateToken_ShouldSetCorrectIssuer()
        {
            // Arrange
            var config = CreateConfiguration(issuer: "CustomIssuer");
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            Assert.Equal("CustomIssuer", jwtToken.Issuer);
        }

        [Fact]
        public void GenerateToken_ShouldSetCorrectAudience()
        {
            // Arrange
            var config = CreateConfiguration(audience: "CustomAudience");
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            Assert.Contains("CustomAudience", jwtToken.Audiences);
        }

        [Fact]
        public void GenerateToken_ShouldSetExpirationTime()
        {
            // Arrange
            var expirationMinutes = 30;
            var config = CreateConfiguration(expirationMinutes: expirationMinutes);
            var service = new JwtTokenService(config);
            var user = CreateTestUser();
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            // Token should expire approximately expirationMinutes from now (with 1 minute tolerance)
            var expectedExpiration = beforeGeneration.AddMinutes(expirationMinutes);
            var timeDifference = Math.Abs((jwtToken.ValidTo - expectedExpiration).TotalMinutes);
            Assert.True(timeDifference < 1, $"Token expiration is off by {timeDifference} minutes");
        }

        [Fact]
        public void GenerateToken_WithMissingJwtKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var config = CreateConfiguration(includeKey: false);
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => service.GenerateToken(user));
            Assert.Contains("Jwt:Key", exception.Message);
        }

        [Fact]
        public void GenerateToken_WithEmptyJwtKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var config = CreateConfiguration(jwtKey: "");
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => service.GenerateToken(user));
            Assert.Contains("Jwt:Key", exception.Message);
        }

        [Fact]
        public void GenerateToken_WithWhitespaceJwtKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var config = CreateConfiguration(jwtKey: "   ");
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => service.GenerateToken(user));
            Assert.Contains("Jwt:Key", exception.Message);
        }

        [Fact]
        public void GenerateToken_ShouldUseHmacSha256Algorithm()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act
            var token = service.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            Assert.Equal(SecurityAlgorithms.HmacSha256, jwtToken.SignatureAlgorithm);
        }

        [Fact]
        public void GenerateToken_TokenShouldBeValidatableWithCorrectKey()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act
            var token = service.GenerateToken(user);

            // Assert - Validate the token
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = JwtIssuer,
                ValidAudience = JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(ValidJwtKey))
            };

            // This should not throw
            var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
        }

        [Fact]
        public void GenerateToken_DifferentUsers_ShouldGenerateDifferentTokens()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user1 = CreateTestUser(id: 1, name: "User1");
            var user2 = CreateTestUser(id: 2, name: "User2");

            // Act
            var token1 = service.GenerateToken(user1);
            var token2 = service.GenerateToken(user2);

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateToken_SameUser_CalledMultipleTimes_ShouldGenerateDifferentTokens()
        {
            // Arrange
            var config = CreateConfiguration();
            var service = new JwtTokenService(config);
            var user = CreateTestUser();

            // Act
            var token1 = service.GenerateToken(user);
            System.Threading.Thread.Sleep(10); // Small delay to ensure different timestamp
            var token2 = service.GenerateToken(user);

            // Assert
            Assert.NotEqual(token1, token2);
        }
    }
}
