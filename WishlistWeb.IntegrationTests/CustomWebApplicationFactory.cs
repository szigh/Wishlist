using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WishlistModels;

namespace WishlistWeb.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Static field to ensure the same database name is used across all requests
        private static readonly string _databaseName = $"InMemoryTestDb_{Guid.NewGuid()}";
        
        public CustomWebApplicationFactory()
        {
            // Set environment variable to skip SQLite registration in Program.cs
            Environment.SetEnvironmentVariable("INTEGRATION_TEST", "true");
            
            // Set JWT configuration through environment variables
            Environment.SetEnvironmentVariable("Jwt__Key", "TestKeyForIntegrationTestsThatIsLongEnough123456");
            Environment.SetEnvironmentVariable("Jwt__Issuer", "WishlistTestApi");
            Environment.SetEnvironmentVariable("Jwt__Audience", "WishlistTestClient");
            Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Use the same database name for all requests in this factory instance
                services.AddDbContext<WishlistDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Initialize the database after the host is created
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WishlistDbContext>();
            db.Database.EnsureCreated();

            return host;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up environment variables
                Environment.SetEnvironmentVariable("INTEGRATION_TEST", null);
                Environment.SetEnvironmentVariable("Jwt__Key", null);
                Environment.SetEnvironmentVariable("Jwt__Issuer", null);
                Environment.SetEnvironmentVariable("Jwt__Audience", null);
                Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", null);
            }
            base.Dispose(disposing);
        }
    }
}