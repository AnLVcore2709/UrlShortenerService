using System.ComponentModel.DataAnnotations;

namespace UrlShortenerService.Models
{
    public class ShortUrl
    {
        // Primary key: Unique identifier for each record in the database
        public int Id { get; set; }

        // Original long URL (required property)
        [Required]
        public string OriginalUrl { get; set; } = string.Empty;

        // Generated short code which must be unique for redirection
        [Required]
        public string ShortCode { get; set; } = string.Empty;

        // Timestamp when the short URL was created in UTC format
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Analytics: Track how many times the short URL has been accessed
        public int ClickCount { get; set; } = 0;
    }
}