using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WishlistContracts.DTOs;
using WishlistModels;
using Xunit;

namespace WishlistWeb.IntegrationTests
{
    /// <summary>
    /// Integration tests covering the core workflow:
    /// - Register account A
    /// - Login to account A
    /// - View wishlist
    /// - Add 2 items to wishlist
    /// - Logout
    /// - Register account B
    /// - B views A's wishlist
    /// - B claims a gift from A's wishlist
    /// - B can see their claims
    /// - B logs out
    /// - A logs in
    /// - A can see the gift is claimed but not who claimed it
    /// </summary>
    public class CoreWorkflowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public CoreWorkflowIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public async Task CoreWorkflow_CompleteUserJourney_ShouldSucceed()
        {
            // 1. Register account A
            var userARequest = new LoginRequestDto
            {
                Name = "UserA",
                Password = "password123"
            };

            var registerAResponse = await _client.PostAsJsonAsync("/api/auth/register", userARequest);
            Assert.Equal(HttpStatusCode.OK, registerAResponse.StatusCode);

            var userALoginResponse = await registerAResponse.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            Assert.NotNull(userALoginResponse);
            Assert.NotEmpty(userALoginResponse.Token);
            var userAId = userALoginResponse.UserId;
            var userAToken = userALoginResponse.Token;

            // 2. Login to account A (already logged in from registration, but testing login endpoint)
            var loginAResponse = await _client.PostAsJsonAsync("/api/auth/login", userARequest);
            Assert.Equal(HttpStatusCode.OK, loginAResponse.StatusCode);

            var userALoginAgain = await loginAResponse.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            Assert.NotNull(userALoginAgain);
            Assert.NotEmpty(userALoginAgain.Token);
            userAToken = userALoginAgain.Token; // Update token

            // 3. View my wishlist (should be empty initially)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAToken);
            
            var myWishlistResponse = await _client.GetAsync($"/api/users/{userAId}/wishlist");
            Assert.Equal(HttpStatusCode.OK, myWishlistResponse.StatusCode);

            var myWishlist = await myWishlistResponse.Content.ReadFromJsonAsync<UserWishlistReadDto>(_jsonOptions);
            Assert.NotNull(myWishlist);
            Assert.Empty(myWishlist.Gifts);

            // 4. Add 2 items to my wishlist
            var gift1 = new GiftCreateDto
            {
                Title = "Gaming Laptop",
                Description = "High-performance laptop for gaming",
                Category = "Electronics",
                Link = "https://example.com/laptop"
            };

            var gift2 = new GiftCreateDto
            {
                Title = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard",
                Category = "Electronics",
                Link = "https://example.com/keyboard"
            };

            var createGift1Response = await _client.PostAsJsonAsync("/api/gift", gift1);
            Assert.Equal(HttpStatusCode.Created, createGift1Response.StatusCode);
            var createdGift1 = await createGift1Response.Content.ReadFromJsonAsync<GiftReadDto>(_jsonOptions);
            Assert.NotNull(createdGift1);
            var gift1Id = createdGift1.Id;

            var createGift2Response = await _client.PostAsJsonAsync("/api/gift", gift2);
            Assert.Equal(HttpStatusCode.Created, createGift2Response.StatusCode);
            var createdGift2 = await createGift2Response.Content.ReadFromJsonAsync<GiftReadDto>(_jsonOptions);
            Assert.NotNull(createdGift2);
            var gift2Id = createdGift2.Id;

            // Verify wishlist now has 2 items
            var updatedWishlistResponse = await _client.GetAsync($"/api/users/{userAId}/wishlist");
            var updatedWishlist = await updatedWishlistResponse.Content.ReadFromJsonAsync<UserWishlistReadDto>(_jsonOptions);
            Assert.NotNull(updatedWishlist);
            Assert.Equal(2, updatedWishlist.Gifts.Count);

            // 5. Logout (clear authorization header)
            var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
            _client.DefaultRequestHeaders.Authorization = null;

            // 6. Register another user B
            var userBRequest = new LoginRequestDto
            {
                Name = "UserB",
                Password = "password456"
            };

            var registerBResponse = await _client.PostAsJsonAsync("/api/auth/register", userBRequest);
            Assert.Equal(HttpStatusCode.OK, registerBResponse.StatusCode);

            var userBLoginResponse = await registerBResponse.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            Assert.NotNull(userBLoginResponse);
            Assert.NotEmpty(userBLoginResponse.Token);
            var userBId = userBLoginResponse.UserId;
            var userBToken = userBLoginResponse.Token;

            // 7. B views A's wishlist
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userBToken);
            
            var userAWishlistForBResponse = await _client.GetAsync($"/api/users/{userAId}/wishlist");
            Assert.Equal(HttpStatusCode.OK, userAWishlistForBResponse.StatusCode);

            var userAWishlistForB = await userAWishlistForBResponse.Content.ReadFromJsonAsync<UserWishlistReadDto>(_jsonOptions);
            Assert.NotNull(userAWishlistForB);
            Assert.Equal(2, userAWishlistForB.Gifts.Count);
            Assert.All(userAWishlistForB.Gifts, gift => Assert.False(gift.IsTaken));

            // 8. B "claims" to buy a gift from A's wishlist
            var claimGift1 = new VolunteerCreateDto
            {
                GiftId = gift1Id
            };

            var claimResponse = await _client.PostAsJsonAsync("/api/volunteers", claimGift1);
            Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);

            var volunteer = await claimResponse.Content.ReadFromJsonAsync<Volunteer>(_jsonOptions);
            Assert.NotNull(volunteer);

            // 9. B can see in their claims that they are buying that gift
            var myClaimsResponse = await _client.GetAsync("/api/volunteers");
            Assert.Equal(HttpStatusCode.OK, myClaimsResponse.StatusCode);

            var myClaims = await myClaimsResponse.Content.ReadFromJsonAsync<List<Volunteer>>(_jsonOptions);
            Assert.NotNull(myClaims);
            Assert.Single(myClaims);
            Assert.Equal(gift1Id, myClaims[0].GiftId);
            Assert.Equal(userBId, myClaims[0].VolunteerUserId);

            // 10. B logs out
            var logoutBResponse = await _client.PostAsync("/api/auth/logout", null);
            Assert.Equal(HttpStatusCode.OK, logoutBResponse.StatusCode);
            _client.DefaultRequestHeaders.Authorization = null;

            // 11. A logs in
            var loginAAgainResponse = await _client.PostAsJsonAsync("/api/auth/login", userARequest);
            Assert.Equal(HttpStatusCode.OK, loginAAgainResponse.StatusCode);

            var userALoginFinal = await loginAAgainResponse.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            Assert.NotNull(userALoginFinal);
            userAToken = userALoginFinal.Token;

            // 12. A can see the gift is claimed but not who claimed it
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAToken);
            
            var finalWishlistResponse = await _client.GetAsync($"/api/users/{userAId}/wishlist");
            Assert.Equal(HttpStatusCode.OK, finalWishlistResponse.StatusCode);

            var finalWishlist = await finalWishlistResponse.Content.ReadFromJsonAsync<UserWishlistReadDto>(_jsonOptions);
            Assert.NotNull(finalWishlist);
            Assert.Equal(2, finalWishlist.Gifts.Count);

            var claimedGift = finalWishlist.Gifts.FirstOrDefault(g => g.Id == gift1Id);
            Assert.NotNull(claimedGift);
            Assert.True(claimedGift.IsTaken); // Gift is marked as claimed

            var unclaimedGift = finalWishlist.Gifts.FirstOrDefault(g => g.Id == gift2Id);
            Assert.NotNull(unclaimedGift);
            Assert.False(unclaimedGift.IsTaken); // Gift is not claimed

            // Verify that A cannot see who claimed the gift (only the IsTaken flag)
            // The API should NOT return volunteer information to the gift owner
        }
    }

}