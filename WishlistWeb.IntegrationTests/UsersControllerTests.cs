using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WishlistContracts.DTOs;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UsersControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<(string token, int userId)> RegisterAndLoginUser(string username, string password)
        {
            var request = new LoginRequestDto { Name = username, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return (loginResponse!.Token, loginResponse.UserId);
        }

        [Fact]
        public async Task GetUsers_WithAuthentication_ShouldReturnUserList()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginUser("UserListTest1", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/users");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var users = await response.Content.ReadFromJsonAsync<List<UserReadDto>>();
            Assert.NotNull(users);
            Assert.NotEmpty(users);
        }

        [Fact]
        public async Task GetUser_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var (token, userId) = await RegisterAndLoginUser("UserGetTest", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = await response.Content.ReadFromJsonAsync<UserReadDto>();
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("UserGetTest", user.Name);
        }

        [Fact]
        public async Task GetUsersWishlist_WithValidUserId_ShouldReturnWishlist()
        {
            // Arrange
            var (token, userId) = await RegisterAndLoginUser("WishlistTest", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Create a gift
            var giftDto = new GiftCreateDto { Title = "Test Gift for Wishlist" };
            await _client.PostAsJsonAsync("/api/gift", giftDto);

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}/wishlist");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var wishlist = await response.Content.ReadFromJsonAsync<UserWishlistReadDto>();
            Assert.NotNull(wishlist);
            Assert.Equal(userId, wishlist.Id);
            Assert.Single(wishlist.Gifts);
            Assert.Equal("Test Gift for Wishlist", wishlist.Gifts[0].Title);
        }

        [Fact]
        public async Task GetUsersWishlist_ForAnotherUser_ShouldReturnTheirWishlist()
        {
            // Arrange - User A creates a wishlist
            var (tokenA, userAId) = await RegisterAndLoginUser("UserA_Wishlist", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            
            var giftDto = new GiftCreateDto { Title = "UserA's Gift" };
            await _client.PostAsJsonAsync("/api/gift", giftDto);

            // User B logs in and views User A's wishlist
            var (tokenB, _) = await RegisterAndLoginUser("UserB_Wishlist", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

            // Act
            var response = await _client.GetAsync($"/api/users/{userAId}/wishlist");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var wishlist = await response.Content.ReadFromJsonAsync<UserWishlistReadDto>();
            Assert.NotNull(wishlist);
            Assert.Equal(userAId, wishlist.Id);
            Assert.Single(wishlist.Gifts);
        }

        [Fact]
        public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var (token, _) = await RegisterAndLoginUser("UserNotFoundTest", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/users/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/users");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}