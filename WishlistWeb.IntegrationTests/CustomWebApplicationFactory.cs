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
                // Remove all existing DbContext registrations from Program.cs
                services.RemoveAll(typeof(DbContextOptions<WishlistDbContext>));
                services.RemoveAll(typeof(WishlistDbContext));

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