using System.Net.Http.Headers;
using System.Net.Http.Json;
using WishlistContracts.DTOs;

namespace WishlistWeb.IntegrationTests
{
    internal static class WishlistWebTestHelper
    {
        internal static async Task<(string token, int userId)> RegisterAndLoginUser(this HttpClient client, string username, string password)
        {
            var request = new LoginRequestDto { Name = username, Password = password };
            var response = await client.PostAsJsonAsync("/api/auth/register", request);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            return (loginResponse!.Token, loginResponse.UserId);
        }

        internal static async Task<int> CreateGiftForUser(this HttpClient client, string token, string giftTitle)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var giftDto = new GiftCreateDto { Title = giftTitle };
            var response = await client.PostAsJsonAsync("/api/gift", giftDto);
            var gift = await response.Content.ReadFromJsonAsync<GiftReadDto>();
            return gift!.Id;
        }

        internal static async Task<int> CreateGiftDayForUser(this HttpClient client, string token, string title, int day, int month, bool isProtected = false)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var giftDayDto = new GiftDaysCreateDto { Title = title, Day = day, Month = month, Protected = isProtected };
            var response = await client.PostAsJsonAsync("/api/giftdays", giftDayDto);
            var giftDay = await response.Content.ReadFromJsonAsync<GiftDaysReadDto>();
            return giftDay!.Id;
        }
    }
}
