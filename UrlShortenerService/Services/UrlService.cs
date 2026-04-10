using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UrlShortenerService.Data;
using UrlShortenerService.Models;

namespace UrlShortenerService.Services
{
    public class UrlService
    {
        private readonly AppDbContext _context;
        // Inject in-memory cache for performance optimization

        private readonly IMemoryCache _cache;

        public UrlService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Generate a random 6-character alphanumeric short code
        public string GenerateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        // Retrieve original URL by short code
        // Flow: Check Cache -> If not found, query Database -> Store in Cache
        public async Task<string?> GetOriginalUrl(string code)
        {
            // 1. Check cache first before hitting the database
            if (!_cache.TryGetValue(code, out string originalUrl))
            {
                // 2. Cache miss -> Query database for the mapping
                var urlEntry = await _context.ShortUrls
                    .FirstOrDefaultAsync(u => u.ShortCode == code);

                if (urlEntry == null) return null;

                originalUrl = urlEntry.OriginalUrl;

                // 3. Store result in cache with a sliding expiration for future requests
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60));

                _cache.Set(code, originalUrl, cacheOptions);
            }

            return originalUrl;
        }

        // Create a new short URL entry and save it to the database
        public async Task<string> CreateShortUrl(string originalUrl)
        {
            string code;

            // Ensure generated code is unique in the database (Collision handling)
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

            // Add new entity and persist changes asynchronously
            _context.ShortUrls.Add(urlEntry);
            await _context.SaveChangesAsync();

            return code;
        }
        // Increment click count when a short URL is accessed
        public async Task IncrementClickCount(string code)
        {
            // This logic can be executed as a background task to optimize response time
            var urlEntry = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            if (urlEntry != null)
            {
                urlEntry.ClickCount++;
                // Commit the update to the database
                await _context.SaveChangesAsync();
            }
        }
    }
}