using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Models;

namespace UrlShortenerService.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor to pass database configuration settings to the base DbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Table representing ShortUrl entities in the database
        public DbSet<ShortUrl> ShortUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Create an index on ShortCode to improve lookup performance
            // and enforce uniqueness (no duplicate short codes allowed)
            modelBuilder.Entity<ShortUrl>()
                        .HasIndex(u => u.ShortCode)
                        .IsUnique();
        }
    }
}