using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WishlistModels;
using WishlistWeb;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class WebApplicationBuilderExtensionsTests
    {
        private static WebApplicationBuilder CreateBuilder(Dictionary<string, string?> configData)
        {
            var args = Array.Empty<string>();
            var builder = WebApplication.CreateBuilder(args);

            // Clear existing configuration and add our test config
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddInMemoryCollection(configData);

            return builder;
        }

        #region InitializeDatabase Tests

        [Fact]
        public void InitializeDatabase_WithValidConnectionString_ShouldConfigureDbContext()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            };
            var builder = CreateBuilder(configData);

            // Act
            builder.InitializeDatabase();
            var app = builder.Build();

            // Assert
            var dbContext = app.Services.GetService<WishlistDbContext>();
            Assert.NotNull(dbContext);
        }

        [Fact]
        public void InitializeDatabase_WithValidConnectionString_ShouldUseSqlite()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db"
            };
            var builder = CreateBuilder(configData);

            // Act
            builder.InitializeDatabase();
            var app = builder.Build();

            // Assert
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WishlistDbContext>();
            Assert.True(dbContext.Database.IsSqlite());
        }

        [Fact]
        public void InitializeDatabase_WithMissingConnectionString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.InitializeDatabase());
            Assert.Contains("DefaultConnection", exception.Message);
            Assert.Contains("ConnectionStrings:DefaultConnection", exception.Message);
        }

        [Fact]
        public void InitializeDatabase_WithEmptyConnectionString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ""
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.InitializeDatabase());
            Assert.Contains("DefaultConnection", exception.Message);
        }

        [Fact]
        public void InitializeDatabase_WithWhitespaceConnectionString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "   "
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.InitializeDatabase());
            Assert.Contains("DefaultConnection", exception.Message);
        }

        #endregion

        #region ConfigureJwt Tests

        [Fact]
        public void ConfigureJwt_WithAllValidSettings_ShouldReturnConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Act
            var (jwtKey, jwtIssuer, jwtAudience) = builder.ConfigureJwt();

            // Assert
            Assert.Equal("test-key-that-is-long-enough-for-testing", jwtKey);
            Assert.Equal("TestIssuer", jwtIssuer);
            Assert.Equal("TestAudience", jwtAudience);
        }

        [Fact]
        public void ConfigureJwt_WithMissingKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Key", exception.Message);
            Assert.Contains("signing key", exception.Message.ToLower());
        }

        [Fact]
        public void ConfigureJwt_WithEmptyKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Key", exception.Message);
        }

        [Fact]
        public void ConfigureJwt_WithWhitespaceKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "   ",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Key", exception.Message);
        }

        [Fact]
        public void ConfigureJwt_WithMissingIssuer_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Issuer", exception.Message);
            Assert.Contains("issuer", exception.Message.ToLower());
        }

        [Fact]
        public void ConfigureJwt_WithEmptyIssuer_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Issuer"] = "",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Issuer", exception.Message);
        }

        [Fact]
        public void ConfigureJwt_WithWhitespaceIssuer_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Issuer"] = "   ",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Issuer", exception.Message);
        }

        [Fact]
        public void ConfigureJwt_WithMissingAudience_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Issuer"] = "TestIssuer"
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Audience", exception.Message);
            Assert.Contains("audience", exception.Message.ToLower());
        }

        [Fact]
        public void ConfigureJwt_WithEmptyAudience_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = ""
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Audience", exception.Message);
        }

        [Fact]
        public void ConfigureJwt_WithWhitespaceAudience_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "   "
            };
            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains("Jwt:Audience", exception.Message);
        }

        [Theory]
        [InlineData(null, "TestIssuer", "TestAudience", "Jwt:Key")]
        [InlineData("TestKey", null, "TestAudience", "Jwt:Issuer")]
        [InlineData("TestKey", "TestIssuer", null, "Jwt:Audience")]
        [InlineData("", "TestIssuer", "TestAudience", "Jwt:Key")]
        [InlineData("TestKey", "", "TestAudience", "Jwt:Issuer")]
        [InlineData("TestKey", "TestIssuer", "", "Jwt:Audience")]
        public void ConfigureJwt_WithInvalidConfiguration_ShouldThrowWithCorrectMessage(
            string? key, string? issuer, string? audience, string expectedInMessage)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            if (key != null) configData["Jwt:Key"] = key;
            if (issuer != null) configData["Jwt:Issuer"] = issuer;
            if (audience != null) configData["Jwt:Audience"] = audience;

            var builder = CreateBuilder(configData);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => builder.ConfigureJwt());
            Assert.Contains(expectedInMessage, exception.Message);
        }

        [Fact]
        public void ConfigureJwt_ShouldClearDefaultInboundClaimTypeMap()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-testing",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            };
            var builder = CreateBuilder(configData);

            // Store initial count
            var initialCount = System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Count;

            // Act
            builder.ConfigureJwt();

            // Assert
            var finalCount = System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Count;
            // If there were items initially, they should be cleared
            if (initialCount > 0)
            {
                Assert.True(finalCount < initialCount, "DefaultInboundClaimTypeMap should have been cleared");
            }
        }

        #endregion
    }
}
