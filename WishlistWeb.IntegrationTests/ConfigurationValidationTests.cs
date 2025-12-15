using Microsoft.Extensions.Configuration;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class ConfigurationValidationTests
    {
        [Theory]
        [InlineData(null, false, 0)]
        [InlineData("", false, 0)]
        [InlineData("short", true, 5)]
        [InlineData("k37b493b1d75f4eac9d3f16b414b01157", true, 33)]
        [InlineData("TestKeyForIntegrationTestsThatIsLongEnough123456", true, 48)]
        public void JwtKey_Configuration_ValidatesCorrectly(string? jwtKey, bool expectedConfigured, int expectedLength)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            if (jwtKey != null)
            {
                configData["Jwt:Key"] = jwtKey;
            }
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var isConfigured = !string.IsNullOrEmpty(config["Jwt:Key"]);
            var keyLength = config["Jwt:Key"]?.Length ?? 0;

            // Assert
            Assert.Equal(expectedConfigured, isConfigured);
            Assert.Equal(expectedLength, keyLength);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Server=localhost;Database=TestDb", true)]
        public void ConnectionString_Configuration_ValidatesCorrectly(string? connectionString, bool expectedConfigured)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            if (connectionString != null)
            {
                configData["ConnectionStrings:DefaultConnection"] = connectionString;
            }
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var isConfigured = !string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection"));

            // Assert
            Assert.Equal(expectedConfigured, isConfigured);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("automapper-key-123", true)]
        public void AutomapperKey_Configuration_ValidatesCorrectly(string? automapperKey, bool expectedConfigured)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            if (automapperKey != null)
            {
                configData["AutomapperKey"] = automapperKey;
            }
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var isConfigured = !string.IsNullOrEmpty(config["AutomapperKey"]);

            // Assert
            Assert.Equal(expectedConfigured, isConfigured);
        }

        [Fact]
        public void AllConfiguration_WhenAllSet_ReturnsCorrectValues()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "k37b493b1d75f4eac9d3f16b414b01157",
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb",
                ["AutomapperKey"] = "automapper-key-123"
            };
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act & Assert
            Assert.True(!string.IsNullOrEmpty(config["Jwt:Key"]));
            Assert.Equal(33, config["Jwt:Key"]?.Length);
            Assert.True(!string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection")));
            Assert.True(!string.IsNullOrEmpty(config["AutomapperKey"]));
        }

        [Fact]
        public void AllConfiguration_WhenNoneSet_ReturnsCorrectValues()
        {
            // Arrange
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();

            // Act & Assert
            Assert.False(!string.IsNullOrEmpty(config["Jwt:Key"]));
            Assert.Equal(0, config["Jwt:Key"]?.Length ?? 0);
            Assert.False(!string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection")));
            Assert.False(!string.IsNullOrEmpty(config["AutomapperKey"]));
        }

        [Theory]
        [InlineData("k37b493b1d75f4eac9d3f16b414b01157", null, null)]
        [InlineData("k37b493b1d75f4eac9d3f16b414b01157", "Server=localhost", null)]
        [InlineData("k37b493b1d75f4eac9d3f16b414b01157", null, "automapper-key")]
        [InlineData("k37b493b1d75f4eac9d3f16b414b01157", "", "")]
        public void PartialConfiguration_ValidatesCorrectly(string? jwtKey, string? connectionString, string? automapperKey)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            if (jwtKey != null) configData["Jwt:Key"] = jwtKey;
            if (connectionString != null) configData["ConnectionStrings:DefaultConnection"] = connectionString;
            if (automapperKey != null) configData["AutomapperKey"] = automapperKey;
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act & Assert - JWT should always be configured in these tests
            Assert.True(!string.IsNullOrEmpty(config["Jwt:Key"]));
            Assert.Equal(!string.IsNullOrEmpty(connectionString), !string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection")));
            Assert.Equal(!string.IsNullOrEmpty(automapperKey), !string.IsNullOrEmpty(config["AutomapperKey"]));
        }

        [Theory]
        [InlineData(null, null, null, false, false, false)]
        [InlineData("", "", "", false, false, false)]
        [InlineData("WishlistApi", "WishlistClient", "60", true, true, true)]
        [InlineData("CustomIssuer", null, null, true, false, false)]
        [InlineData(null, "CustomAudience", "30", false, true, true)]
        public void JwtSettings_Configuration_ValidatesCorrectly(
            string? issuer, string? audience, string? expirationMinutes,
            bool expectedIssuerConfigured, bool expectedAudienceConfigured, bool expectedExpirationConfigured)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            if (issuer != null) configData["Jwt:Issuer"] = issuer;
            if (audience != null) configData["Jwt:Audience"] = audience;
            if (expirationMinutes != null) configData["Jwt:ExpirationMinutes"] = expirationMinutes;
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var isIssuerConfigured = !string.IsNullOrEmpty(config["Jwt:Issuer"]);
            var isAudienceConfigured = !string.IsNullOrEmpty(config["Jwt:Audience"]);
            var isExpirationConfigured = !string.IsNullOrEmpty(config["Jwt:ExpirationMinutes"]);

            // Assert
            Assert.Equal(expectedIssuerConfigured, isIssuerConfigured);
            Assert.Equal(expectedAudienceConfigured, isAudienceConfigured);
            Assert.Equal(expectedExpirationConfigured, isExpirationConfigured);
            
            if (isExpirationConfigured && int.TryParse(config["Jwt:ExpirationMinutes"], out var minutes))
            {
                Assert.True(minutes > 0);
            }
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        [InlineData("http://localhost:3000", 1)]
        [InlineData("http://localhost:3000;https://localhost:3000", 2)]
        [InlineData("http://localhost:3000;https://localhost:3000;http://localhost:5173;https://localhost:5173;http://localhost:5174;https://localhost:5174", 6)]
        public void CorsAllowedOrigins_Configuration_ValidatesCorrectly(string? origins, int expectedCount)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            
            if (origins != null && !string.IsNullOrEmpty(origins))
            {
                var originArray = origins.Split(';');
                for (int i = 0; i < originArray.Length; i++)
                {
                    configData[$"Cors:AllowedOrigins:{i}"] = originArray[i];
                }
            }
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();
            var actualCount = allowedOrigins?.Length ?? 0;

            // Assert
            Assert.Equal(expectedCount, actualCount);
            
            if (expectedCount > 0 && allowedOrigins != null)
            {
                Assert.All(allowedOrigins, origin => Assert.False(string.IsNullOrWhiteSpace(origin)));
            }
        }

        [Theory]
        [InlineData(null, false, false)]
        [InlineData("", false, false)]
        [InlineData("Development", true, true)]
        [InlineData("Production", true, false)]
        [InlineData("Staging", true, false)]
        [InlineData("development", true, true)]
        [InlineData("production", true, false)]
        public void AspNetCoreEnvironment_Configuration_ValidatesCorrectly(
            string? environment, bool expectedConfigured, bool expectedIsDevelopment)
        {
            // Arrange
            var configData = new Dictionary<string, string?>();
            if (environment != null)
            {
                configData["ASPNETCORE_ENVIRONMENT"] = environment;
            }
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var configuredEnvironment = config["ASPNETCORE_ENVIRONMENT"];
            var isConfigured = !string.IsNullOrEmpty(configuredEnvironment);
            var isDevelopment = string.Equals(configuredEnvironment, "Development", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(expectedConfigured, isConfigured);
            Assert.Equal(expectedIsDevelopment, isDevelopment);
        }

        [Fact]
        public void CompleteConfiguration_WithAllSettings_ValidatesCorrectly()
        {
            // Arrange
            var configData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "k37b493b1d75f4eac9d3f16b414b01157",
                ["Jwt:Issuer"] = "WishlistApi",
                ["Jwt:Audience"] = "WishlistClient",
                ["Jwt:ExpirationMinutes"] = "60",
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb",
                ["AutomapperKey"] = "automapper-key-123",
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
                ["Cors:AllowedOrigins:1"] = "https://localhost:3000",
                ["Cors:AllowedOrigins:2"] = "http://localhost:5173",
                ["Cors:AllowedOrigins:3"] = "https://localhost:5173",
                ["Cors:AllowedOrigins:4"] = "http://localhost:5174",
                ["Cors:AllowedOrigins:5"] = "https://localhost:5174",
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
            };
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act & Assert - JWT
            Assert.True(!string.IsNullOrEmpty(config["Jwt:Key"]));
            Assert.Equal("WishlistApi", config["Jwt:Issuer"]);
            Assert.Equal("WishlistClient", config["Jwt:Audience"]);
            Assert.Equal("60", config["Jwt:ExpirationMinutes"]);
            
            // Connection String
            Assert.True(!string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection")));
            
            // Automapper
            Assert.True(!string.IsNullOrEmpty(config["AutomapperKey"]));
            
            // CORS
            var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();
            Assert.NotNull(allowedOrigins);
            Assert.Equal(6, allowedOrigins.Length);
            Assert.Contains("http://localhost:3000", allowedOrigins);
            Assert.Contains("https://localhost:5174", allowedOrigins);
            
            // Environment
            Assert.Equal("Development", config["ASPNETCORE_ENVIRONMENT"]);
        }
    }
}
