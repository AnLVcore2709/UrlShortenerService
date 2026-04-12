using System.ComponentModel.DataAnnotations;

namespace UrlShortenerService.Models
{
    // Entity representing a shortened link in the system.
    public class ShortUrl
    {
        // Primary key: Unique identifier for each database record.
        public int Id { get; set; }

        // The original long URL provided by the user.
        [Required]
        public string OriginalUrl { get; set; } = string.Empty;

        // Unique code used for redirection and identification.
        [Required]
        public string ShortCode { get; set; } = string.Empty;

        // Timestamp when the link was created (defaults to UTC now).
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Counter to track how many times the short URL has been accessed.
        public int ClickCount { get; set; } = 0;
    }
}