using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using WishlistModels;

namespace WishlistWeb
{
    public static class WebApplicationBuilderExtensions
    {
        public static void InitializeDatabase(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string 'DefaultConnection' is not configured. " +
                    "Please set 'ConnectionStrings:DefaultConnection' in your configuration.");
            }

            builder.Services.AddDbContext<WishlistDbContext>(
                options => options.UseSqlite(connectionString));
        }

        public static (string jwtKey, string jwtIssuer, string jwtAudience) ConfigureJwt(this WebApplicationBuilder builder)
        {
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("JWT signing key is not configured. Please set 'Jwt:Key' in your configuration.");
            if (string.IsNullOrWhiteSpace(jwtIssuer))
                throw new InvalidOperationException("JWT issuer is not configured. Please set 'Jwt:Issuer' in your configuration.");
            if (string.IsNullOrWhiteSpace(jwtAudience))
                throw new InvalidOperationException("JWT audience is not configured. Please set 'Jwt:Audience' in your configuration.");

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            return (jwtKey, jwtIssuer, jwtAudience);
        }
    }
}
