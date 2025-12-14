using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class HealthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public HealthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

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
            var content = await response.Content.ReadFromJsonAsync<HealthResponse>();

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
            var content = await response.Content.ReadFromJsonAsync<HealthResponse>();

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

        private class HealthResponse
        {
            public string Status { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }
    }
}
