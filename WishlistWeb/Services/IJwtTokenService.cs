using WishlistModels;

namespace WishlistWeb.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}