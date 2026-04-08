using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Models;

namespace UrlShortenerService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ShortUrl> ShortUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Create an index for ShortCode to improve lookup performance and ensure uniqueness
            modelBuilder.Entity<ShortUrl>().HasIndex(u => u.ShortCode).IsUnique();
        }
    }
}