using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove the existing DbContext registration from Program.cs
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<WishlistDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Also remove the DbContext itself to ensure clean replacement
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(WishlistDbContext));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

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
    }
}