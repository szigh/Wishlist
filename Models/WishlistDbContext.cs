using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class WishlistDbContext : DbContext
    {
        public WishlistDbContext(DbContextOptions<WishlistDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
    }

    public class WishlistDbContextFactory : IDesignTimeDbContextFactory<WishlistDbContext>
    {
        public WishlistDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WishlistDbContext>();
            optionsBuilder.UseSqlite("Data Source=wishlist.db");

            return new WishlistDbContext(optionsBuilder.Options);
        }
    }

}
