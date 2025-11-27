using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WishlistContracts.DTOs;
using WishlistModels;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    public class VolunteerControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public VolunteerControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ClaimGift_WithValidGift_ShouldSucceed()
        {
            // Arrange
            var (ownerToken, _) = await _client.RegisterAndLoginUser("GiftOwner1", "password123");
            var giftId = await _client.CreateGiftForUser(ownerToken, "Claimable Gift");

            var (volunteerToken, volunteerId) = await _client.RegisterAndLoginUser("Volunteer1", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

            var claimDto = new VolunteerCreateDto { GiftId = giftId };

            // Act
            var response = await _client.PostAsJsonAsync("/api/volunteers", claimDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var volunteer = await response.Content.ReadFromJsonAsync<Volunteer>();
            Assert.NotNull(volunteer);
            Assert.Equal(giftId, volunteer.GiftId);
            Assert.Equal(volunteerId, volunteer.VolunteerUserId);
        }

        [Fact]
        public async Task ClaimGift_AlreadyClaimed_ShouldReturnBadRequest()
        {
            // Arrange
            var (ownerToken, _) = await _client.RegisterAndLoginUser("GiftOwner2", "password123");
            var giftId = await _client.CreateGiftForUser(ownerToken, "Already Claimed Gift");

            // First volunteer claims
            var (volunteer1Token, _) = await _client.RegisterAndLoginUser("Volunteer2", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteer1Token);
            var claimDto = new VolunteerCreateDto { GiftId = giftId };
            await _client.PostAsJsonAsync("/api/volunteers", claimDto);

            // Second volunteer tries to claim
            var (volunteer2Token, _) = await _client.RegisterAndLoginUser("Volunteer3", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteer2Token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/volunteers", claimDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetVolunteers_ShouldReturnOnlyMyClaimedGifts()
        {
            // Arrange
            var (ownerToken, _) = await _client.RegisterAndLoginUser("GiftOwner3", "password123");
            var gift1Id = await _client.CreateGiftForUser(ownerToken, "Gift 1");
            var gift2Id = await _client.CreateGiftForUser(ownerToken, "Gift 2");

            // Volunteer claims gift1
            var (volunteerToken, _) = await _client.RegisterAndLoginUser("Volunteer4", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);
            await _client.PostAsJsonAsync("/api/volunteers", new VolunteerCreateDto { GiftId = gift1Id });

            // Another volunteer claims gift2
            var (volunteer2Token, _) = await _client.RegisterAndLoginUser("Volunteer5", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteer2Token);
            await _client.PostAsJsonAsync("/api/volunteers", new VolunteerCreateDto { GiftId = gift2Id });

            // Act - Get volunteers for first volunteer
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);
            var response = await _client.GetAsync("/api/volunteers");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var volunteers = await response.Content.ReadFromJsonAsync<List<Volunteer>>();
            Assert.NotNull(volunteers);
            Assert.Single(volunteers); // Should only see their own claim
            Assert.Equal(gift1Id, volunteers[0].GiftId);
        }

        [Fact]
        public async Task DeleteVolunteer_AsClaimOwner_ShouldSucceedAndResetGiftStatus()
        {
            // Arrange
            var (ownerToken, _) = await _client.RegisterAndLoginUser("GiftOwner4", "password123");
            var giftId = await _client.CreateGiftForUser(ownerToken, "Unclaim Test Gift");

            var (volunteerToken, _) = await _client.RegisterAndLoginUser("Volunteer6", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);
            
            var claimResponse = await _client.PostAsJsonAsync("/api/volunteers", new VolunteerCreateDto { GiftId = giftId });
            var volunteer = await claimResponse.Content.ReadFromJsonAsync<Volunteer>();

            // Act
            var response = await _client.DeleteAsync($"/api/volunteers/{volunteer!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify gift is no longer marked as taken
            var giftResponse = await _client.GetAsync($"/api/gift/{giftId}");
            var gift = await giftResponse.Content.ReadFromJsonAsync<GiftReadDto>();
            Assert.False(gift!.IsTaken);
        }

        [Fact]
        public async Task DeleteVolunteer_AsNonClaimOwner_ShouldReturnNotFound()
        {
            // Arrange
            var (ownerToken, _) = await _client.RegisterAndLoginUser("GiftOwner5", "password123");
            var giftId = await _client.CreateGiftForUser(ownerToken, "Gift for unauthorized unclaim");

            // First volunteer claims
            var (volunteer1Token, _) = await _client.RegisterAndLoginUser("Volunteer7", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteer1Token);
            var claimResponse = await _client.PostAsJsonAsync("/api/volunteers", new VolunteerCreateDto { GiftId = giftId });
            var volunteer = await claimResponse.Content.ReadFromJsonAsync<Volunteer>();

            // Different volunteer tries to unclaim
            var (volunteer2Token, _) = await _client.RegisterAndLoginUser("Volunteer8", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteer2Token);

            // Act
            var response = await _client.DeleteAsync($"/api/volunteers/{volunteer!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ClaimGift_NonExistentGift_ShouldReturnNotFound()
        {
            // Arrange
            var (volunteerToken, _) = await _client.RegisterAndLoginUser("Volunteer9", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", volunteerToken);

            var claimDto = new VolunteerCreateDto { GiftId = 99999 }; // Non-existent ID

            // Act
            var response = await _client.PostAsJsonAsync("/api/volunteers", claimDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}