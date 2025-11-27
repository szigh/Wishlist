using System.Net;
using System.Net.Http.Json;
using WishlistContracts.DTOs;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_WithValidCredentials_ShouldReturnTokenAndUserId()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new LoginRequestDto
            {
                Name = "TestUser1",
                Password = "testpassword123"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.True(result.UserId > 0);
            Assert.Equal("TestUser1", result.Name);
            Assert.Equal("user", result.Role);
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new LoginRequestDto
            {
                Name = "DuplicateUser",
                Password = "password123"
            };

            // Register first time
            await client.PostAsJsonAsync("/api/auth/register", request);
            Thread.Sleep(1000);
            // Act - try to register again with same username
            var response = await client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var client = _factory.CreateClient();
            var registerRequest = new LoginRequestDto
            {
                Name = "LoginTestUser",
                Password = "password123"
            };
            await client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act - now login with same credentials
            var response = await client.PostAsJsonAsync("/api/auth/login", registerRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var registerRequest = new LoginRequestDto
            {
                Name = "PasswordTestUser",
                Password = "correctpassword"
            };
            await client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Act - try to login with wrong password
            var loginRequest = new LoginRequestDto
            {
                Name = "PasswordTestUser",
                Password = "wrongpassword"
            };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new LoginRequestDto
            {
                Name = "NonExistentUser",
                Password = "password123"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("", "password")]
        [InlineData("username", "")]
        [InlineData("", "")]
        public async Task Register_WithEmptyFields_ShouldReturnBadRequest(string username, string password)
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new LoginRequestDto
            {
                Name = username,
                Password = password
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}