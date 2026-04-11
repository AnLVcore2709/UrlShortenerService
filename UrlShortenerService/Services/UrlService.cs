using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UrlShortenerService.Data;
using UrlShortenerService.Models;

namespace UrlShortenerService.Services
{
    // Main service handling URL shortening logic, cache management, and analytics.
    public class UrlService
    {
        private readonly AppDbContext _context;
        // Use in-memory cache to optimize performance and reduce database load.
        private readonly IMemoryCache _cache;

        public UrlService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Generate a random 6-character alphanumeric code for the short URL.
        public string GenerateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        // Retrieve original URL using the short code.
        // Flow: Check Cache -> If miss, query Database -> Store in Cache for future use.
        public async Task<string?> GetOriginalUrl(string code)
        {
            // 1. Try to fetch from cache first to improve response time.
            if (!_cache.TryGetValue(code, out string? originalUrl))
            {
                // 2. Cache miss: Query the database for the mapping.
                var urlEntry = await _context.ShortUrls
                    .FirstOrDefaultAsync(u => u.ShortCode == code);

                if (urlEntry == null) return null;

                originalUrl = urlEntry.OriginalUrl;

                // 3. Update cache with a 60-minute sliding expiration.
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60));

                _cache.Set(code, originalUrl, cacheOptions);
            }

            return originalUrl;
        }

        // Create a new shortened URL entry and persist it to the database.
        public async Task<string> CreateShortUrl(string originalUrl)
        {
            string code;

            // Loop to ensure the generated code is unique (handling collisions).
            do
            {
                code = GenerateCode();
            }
            while (await _context.ShortUrls.AnyAsync(u => u.ShortCode == code));

            var urlEntry = new ShortUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = code
            };

            // Add the new entity and save changes.
            _context.ShortUrls.Add(urlEntry);
            await _context.SaveChangesAsync();

            return code;
        }

        // Increment the access count for a specific short code.
        public async Task IncrementClickCount(string code)
        {
            // Find the record by its short code identifier.
            var urlEntry = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            if (urlEntry != null)
            {
                urlEntry.ClickCount++;
                // Commit the updated click count to the database.
                await _context.SaveChangesAsync();
            }
        }
    }
}