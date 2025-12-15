using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WishlistModels;
using log4net;

namespace WishlistWeb.Services
{
    public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
    {
        private readonly IConfiguration _configuration = configuration;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JwtTokenService));

        /// <summary>
        /// Generates a JWT (JSON Web Token) for authenticating the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the authentication token.</param>
        /// <returns>A signed JWT token string that can be used for bearer authentication.</returns>
        /// <remarks>
        /// This method creates a JWT token with the following security characteristics:
        /// <list type="bullet">
        /// <item><description>Signed using HMAC-SHA256 algorithm with the configured secret key</description></item>
        /// <item><description>Includes standard claims: Subject (user ID), Name, Role, and JTI (unique token identifier)</description></item>
        /// <item><description>Token expiration is controlled by the Jwt:ExpirationMinutes configuration setting</description></item>
        /// <item><description>The JTI claim enables token revocation via the blacklist service</description></item>
        /// </list>
        /// Security considerations:
        /// <list type="bullet">
        /// <item><description>Tokens are stateless and cannot be revoked until they expire, unless blacklisted via the logout endpoint</description></item>
        /// <item><description>The signing key must be kept secure and should be stored in a secure configuration (e.g., user secrets, Azure Key Vault)</description></item>
        /// <item><description>Token expiration should be set appropriately to balance security and user experience</description></item>
        /// </list>
        /// </remarks>
        public string GenerateToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var jwtExpirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                _logger.Error("JWT key is missing or empty in configuration (Jwt:Key).");
                throw new InvalidOperationException("Jwt:Key configuration is required to generate JWT tokens.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
                signingCredentials: credentials
            );

            _logger.Debug($"Generated JWT token for user: {user.Name} (ID: {user.Id})");
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}