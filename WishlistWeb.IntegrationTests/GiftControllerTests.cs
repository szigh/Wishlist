using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WishlistContracts.DTOs;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class GiftControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public GiftControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateGift_WithValidData_ShouldSucceed()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("GiftUser1", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var giftDto = new GiftCreateDto
            {
                Title = "Test Gift",
                Description = "Test Description",
                Category = "Electronics"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/gift", giftDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdGift = await response.Content.ReadFromJsonAsync<GiftReadDto>();
            Assert.NotNull(createdGift);
            Assert.Equal("Test Gift", createdGift.Title);
            Assert.Equal(userId, createdGift.UserId);
        }

        [Fact]
        public async Task GetGift_WithValidId_ShouldReturnGift()
        {
            // Arrange
            var (token, _) = await _client.RegisterAndLoginUser("GiftUser2", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var giftDto = new GiftCreateDto { Title = "Retrievable Gift" };
            var createResponse = await _client.PostAsJsonAsync("/api/gift", giftDto);
            var createdGift = await createResponse.Content.ReadFromJsonAsync<GiftReadDto>();

            // Act
            var response = await _client.GetAsync($"/api/gift/{createdGift!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var retrievedGift = await response.Content.ReadFromJsonAsync<GiftReadDto>();
            Assert.NotNull(retrievedGift);
            Assert.Equal("Retrievable Gift", retrievedGift.Title);
        }

        [Fact]
        public async Task UpdateGift_AsOwner_ShouldSucceed()
        {
            // Arrange
            var (token, _) = await _client.RegisterAndLoginUser("GiftUser3", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var giftDto = new GiftCreateDto { Title = "Original Title" };
            var createResponse = await _client.PostAsJsonAsync("/api/gift", giftDto);
            var createdGift = await createResponse.Content.ReadFromJsonAsync<GiftReadDto>();

            var updateDto = new GiftUpdateDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                IsTaken = false
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/gift/{createdGift!.Id}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/gift/{createdGift.Id}");
            var updatedGift = await getResponse.Content.ReadFromJsonAsync<GiftReadDto>();
            Assert.Equal("Updated Title", updatedGift!.Title);
            Assert.Equal("Updated Description", updatedGift.Description);
        }

        [Fact]
        public async Task UpdateGift_AsNonOwner_ShouldReturnNotFound()
        {
            // Arrange
            var (token1, _) = await _client.RegisterAndLoginUser("GiftOwner", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

            var giftDto = new GiftCreateDto { Title = "Owner's Gift" };
            var createResponse = await _client.PostAsJsonAsync("/api/gift", giftDto);
            var createdGift = await createResponse.Content.ReadFromJsonAsync<GiftReadDto>();

            // Login as different user
            var (token2, _) = await _client.RegisterAndLoginUser("NotOwner", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            var updateDto = new GiftUpdateDto { Title = "Hacked Title", IsTaken = false };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/gift/{createdGift!.Id}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteGift_AsOwner_ShouldSucceed()
        {
            // Arrange
            var (token, _) = await _client.RegisterAndLoginUser("GiftUser4", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var giftDto = new GiftCreateDto { Title = "Gift to Delete" };
            var createResponse = await _client.PostAsJsonAsync("/api/gift", giftDto);
            var createdGift = await createResponse.Content.ReadFromJsonAsync<GiftReadDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/gift/{createdGift!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/gift/{createdGift.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteGift_AsNonOwner_ShouldReturnForbidden()
        {
            // Arrange
            var (token1, _) = await _client.RegisterAndLoginUser("Owner2", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

            var giftDto = new GiftCreateDto { Title = "Protected Gift" };
            var createResponse = await _client.PostAsJsonAsync("/api/gift", giftDto);
            var createdGift = await createResponse.Content.ReadFromJsonAsync<GiftReadDto>();

            // Login as different user
            var (token2, _) = await _client.RegisterAndLoginUser("NotOwner2", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            // Act
            var response = await _client.DeleteAsync($"/api/gift/{createdGift!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateGift_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;
            var giftDto = new GiftCreateDto { Title = "Unauthorized Gift" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/gift", giftDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}