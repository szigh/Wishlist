using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WishlistContracts.DTOs;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class GiftDaysControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public GiftDaysControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetGiftDays_ReturnsAllGiftDays()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("testuser", "password123");
            await _client.CreateGiftDayForUser(token, "Birthday", 15, 5);

            // Act
            var response = await _client.GetAsync("/api/giftdays");

            // Assert
            response.EnsureSuccessStatusCode();
            var giftDays = await response.Content.ReadFromJsonAsync<List<GiftDaysReadDto>>();
            Assert.NotNull(giftDays);
            Assert.NotEmpty(giftDays);
        }

        [Fact]
        public async Task PostGiftDay_UserCanCreateTheirOwnGiftDay()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("testuser2", "password123");
            var giftDayDto = new GiftDaysCreateDto 
            { 
                Title = "Anniversary", 
                Day = 20,
                Month = 6
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/giftdays", giftDayDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var giftDay = await response.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            Assert.NotNull(giftDay);
            Assert.Equal("Anniversary", giftDay.Title);
            Assert.Equal(20, giftDay.Day);
            Assert.Equal(6, giftDay.Month);
            Assert.Equal(userId, giftDay.UserId);
        }

        [Fact]
        public async Task PostGiftDay_UserCanCreateGiftDayForAnotherUser()
        {
            // Arrange
            var (token1, userId1) = await _client.RegisterAndLoginUser("creator", "password123");
            var (token2, userId2) = await _client.RegisterAndLoginUser("recipient", "password123");
            
            var giftDayDto = new GiftDaysCreateDto 
            { 
                Title = "Birthday Gift Day",
                Day = 10,
                Month = 7,
                UserId = userId2
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

            // Act
            var response = await _client.PostAsJsonAsync("/api/giftdays", giftDayDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var giftDay = await response.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            Assert.NotNull(giftDay);
            Assert.Equal("Birthday Gift Day", giftDay.Title);
            Assert.Equal(userId2, giftDay.UserId);
        }

        [Fact]
        public async Task PostGiftDay_UserCanCreateProtectedGiftDayForThemselves()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("protecteduser", "password123");
            var giftDayDto = new GiftDaysCreateDto 
            { 
                Title = "My Protected Birthday",
                Day = 15,
                Month = 8,
                Protected = true
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/giftdays", giftDayDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var giftDay = await response.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            Assert.NotNull(giftDay);
            Assert.True(giftDay.Protected);
            Assert.Equal(userId, giftDay.UserId);
        }

        [Fact]
        public async Task PostGiftDay_NonAdminUserCannotCreateProtectedGiftDayForAnotherUser()
        {
            // Arrange
            var (token1, userId1) = await _client.RegisterAndLoginUser("creator2", "password123");
            var (token2, userId2) = await _client.RegisterAndLoginUser("recipient2", "password123");
            
            var giftDayDto = new GiftDaysCreateDto 
            { 
                Title = "Protected Birthday",
                Day = 25,
                Month = 12,
                Protected = true,
                UserId = userId2  // Trying to create protected gift day for another user
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

            // Act
            var response = await _client.PostAsJsonAsync("/api/giftdays", giftDayDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PostGiftDay_WithLeapYearDate_ShouldSucceed()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("leapyearuser", "password123");
            var giftDayDto = new GiftDaysCreateDto 
            { 
                Title = "Leap Year Birthday",
                Day = 29,
                Month = 2
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/giftdays", giftDayDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var giftDay = await response.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            Assert.NotNull(giftDay);
            Assert.Equal(29, giftDay.Day);
            Assert.Equal(2, giftDay.Month);
        }

        [Fact]
        public async Task PostGiftDay_WithInvalidDate_ShouldFail()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("invaliduser", "password123");
            var giftDayDto = new GiftDaysCreateDto 
            { 
                Title = "Invalid Date",
                Day = 31,
                Month = 2  // February doesn't have 31 days
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/giftdays", giftDayDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetGiftDaysByUser_ReturnsUserSpecificGiftDays()
        {
            // Arrange
            var (token1, userId1) = await _client.RegisterAndLoginUser("user1", "password123");
            var (token2, userId2) = await _client.RegisterAndLoginUser("user2", "password123");
            
            await _client.CreateGiftDayForUser(token1, "User1 Birthday", 1, 1);
            await _client.CreateGiftDayForUser(token2, "User2 Birthday", 2, 2);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

            // Act
            var response = await _client.GetAsync($"/api/giftdays/user/{userId1}");

            // Assert
            response.EnsureSuccessStatusCode();
            var giftDays = await response.Content.ReadFromJsonAsync<List<GiftDaysReadDto>>();
            Assert.NotNull(giftDays);
            Assert.All(giftDays, gd => Assert.Equal(userId1, gd.UserId));
        }

        [Fact]
        public async Task DeleteGiftDay_UserCanDeleteOwnGiftDay()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("deleteuser", "password123");
            var giftDayId = await _client.CreateGiftDayForUser(token, "Test Day", 3, 3);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/giftdays/{giftDayId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteGiftDay_UserCannotDeleteOthersGiftDay()
        {
            // Arrange
            var (token1, userId1) = await _client.RegisterAndLoginUser("owner", "password123");
            var (token2, userId2) = await _client.RegisterAndLoginUser("nonowner", "password123");
            
            var giftDayId = await _client.CreateGiftDayForUser(token1, "Owner's Day", 4, 4);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/giftdays/{giftDayId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task PutGiftDay_UserCanUpdateOwnGiftDay()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("updateuser", "password123");
            var giftDayId = await _client.CreateGiftDayForUser(token, "Original Title", 5, 5);

            var updateDto = new GiftDaysUpdateDto 
            { 
                Title = "Updated Title", 
                Day = 6,
                Month = 6
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsJsonAsync($"/api/giftdays/{giftDayId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/giftdays/{giftDayId}");
            var giftDay = await getResponse.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            Assert.Equal("Updated Title", giftDay!.Title);
            Assert.Equal(6, giftDay.Day);
            Assert.Equal(6, giftDay.Month);
        }

        [Fact]
        public async Task PutGiftDay_UserCanUpdateAnotherUsersNonProtectedGiftDay()
        {
            // Arrange
            var (token1, userId1) = await _client.RegisterAndLoginUser("owner2", "password123");
            var (token2, userId2) = await _client.RegisterAndLoginUser("updater", "password123");
            
            var giftDayId = await _client.CreateGiftDayForUser(token1, "Original", 7, 7, isProtected: false);

            var updateDto = new GiftDaysUpdateDto 
            { 
                Title = "Updated by Another User", 
                Day = 8,
                Month = 8
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            // Act
            var response = await _client.PutAsJsonAsync($"/api/giftdays/{giftDayId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/giftdays/{giftDayId}");
            var giftDay = await getResponse.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            Assert.Equal("Updated by Another User", giftDay!.Title);
            Assert.Equal(8, giftDay.Day);
            Assert.Equal(8, giftDay.Month);
            Assert.Equal(userId1, giftDay.UserId);
        }

        [Fact]
        public async Task PutGiftDay_UserCannotUpdateAnotherUsersProtectedGiftDay()
        {
            // Arrange
            var (token1, userId1) = await _client.RegisterAndLoginUser("owner3", "password123");
            var (token2, userId2) = await _client.RegisterAndLoginUser("updater2", "password123");
            
            var giftDayId = await _client.CreateGiftDayForUser(token1, "Protected Day", 9, 9, isProtected: true);

            var updateDto = new GiftDaysUpdateDto 
            { 
                Title = "Attempting Update", 
                Day = 10,
                Month = 10
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            // Act
            var response = await _client.PutAsJsonAsync($"/api/giftdays/{giftDayId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PutGiftDay_OwnerCanUpdateTheirOwnProtectedGiftDay()
        {
            // Arrange
            var (token, userId) = await _client.RegisterAndLoginUser("protectedowner", "password123");
            var giftDayId = await _client.CreateGiftDayForUser(token, "My Protected Day", 11, 11, isProtected: true);

            var updateDto = new GiftDaysUpdateDto 
            { 
                Title = "Updated Protected Day", 
                Day = 12,
                Month = 12,
                Protected = true
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PutAsJsonAsync($"/api/giftdays/{giftDayId}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the update
            var getResponse = await _client.GetAsync($"/api/giftdays/{giftDayId}");
            var giftDay = await getResponse.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            Assert.Equal("Updated Protected Day", giftDay!.Title);
            Assert.True(giftDay.Protected);
        }

        [Fact]
        public async Task DeleteGiftDay_UserCannotDeleteAnotherUsersProtectedGiftDay()
        {
            // Arrange
            var (token1, userId1) = await _client.RegisterAndLoginUser("owner4", "password123");
            var (token2, userId2) = await _client.RegisterAndLoginUser("deleter", "password123");
            
            var giftDayId = await _client.CreateGiftDayForUser(token1, "Protected Day to Delete", 13, 1, isProtected: true);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/giftdays/{giftDayId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }
    }
}
