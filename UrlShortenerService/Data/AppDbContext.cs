using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Models;

namespace UrlShortenerService.Data
{
    // Database context class for the application.
    public class AppDbContext : DbContext
    {
        // Constructor to initialize DbContext with options (connection string, provider, etc.).
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ShortUrls table mapping for the database.
        public DbSet<ShortUrl> ShortUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure ShortUrl entity mapping.
            modelBuilder.Entity<ShortUrl>()
                        // Create an index on ShortCode for faster lookup.
                        .HasIndex(u => u.ShortCode)
                        // Ensure ShortCode is unique across the database.
                        .IsUnique();
        }
    }
}