using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using WishlistContracts.DTOs;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class HealthControllerTests(CustomWebApplicationFactory factory) 
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory = factory;

        [Fact]
        public async Task Get_ShouldReturnOkStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_ShouldReturnHealthyStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/health");
            var content = await response.Content.ReadFromJsonAsync<HealthResponseDto>();

            // Assert
            Assert.NotNull(content);
            Assert.Equal("healthy", content.Status);
        }

        [Fact]
        public async Task Get_ShouldReturnTimestamp()
        {
            // Arrange
            var client = _factory.CreateClient();
            var beforeRequest = DateTime.UtcNow;

            // Act
            var response = await client.GetAsync("/api/health");
            var afterRequest = DateTime.UtcNow.AddSeconds(1); // Add 1 second tolerance
            var content = await response.Content.ReadFromJsonAsync<HealthResponseDto>();

            // Assert
            Assert.NotNull(content);
            Assert.True(content.Timestamp >= beforeRequest, "Timestamp should be after request started");
            Assert.True(content.Timestamp <= afterRequest, "Timestamp should be before request completed (with tolerance)");
        }

        [Fact]
        public async Task Get_ShouldBeAccessibleWithoutAuthentication()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Note: Not setting any authentication headers

            // Act
            var response = await client.GetAsync("/api/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ConfigCheck_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/health/config-check");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ConfigCheck_WithAuthentication_ShouldReturnConfigurationStatus()
        {
            // Arrange
            var client = _factory.CreateClient();
            
            // Register and login to get a token
            var registerRequest = new LoginRequestDto
            {
                Name = "ConfigCheckUser",
                Password = "password123"
            };
            var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var loginResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            
            // Set the authorization header
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Act
            var response = await client.GetAsync("/api/health/config-check");
            var content = await response.Content.ReadFromJsonAsync<JsonNode>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(content!["jwtKeyConfigured"]!.GetValue<bool>());
            Assert.True(content["jwtKeyLength"]!.GetValue<int>() > 0);
            Assert.True(content["connectionStringsConfigured"]!.GetValue<bool>());
        }
    }
}
