using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UrlShortenerService.Data;
using UrlShortenerService.Models;

namespace UrlShortenerService.Services
{
    public class UrlService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache; // Add cache to the service
        private readonly Random _random = new();

        public UrlService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public string GenerateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        // URL retrieval flow: Check Cache first -> If not found, query Database
        public async Task<string?> GetOriginalUrl(string code)
        {
            // 1. Check cache first
            if (!_cache.TryGetValue(code, out string originalUrl))
            {
                // 2. Cache miss -> query database
                var urlEntry = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code);
                if (urlEntry == null) return null;

                originalUrl = urlEntry.OriginalUrl;

                // 3. Store result in cache for future requests
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(60));

                _cache.Set(code, originalUrl, cacheOptions);
            }

            return originalUrl;
        }

        public async Task<string> CreateShortUrl(string originalUrl)
        {
            string code;

            // Ensure generated code is unique in the database
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

            _context.ShortUrls.Add(urlEntry);
            await _context.SaveChangesAsync();

            return code;
        }

        public async Task IncrementClickCount(string code)
        {
            // Run this logic as a background task to avoid slowing down user redirect
            var urlEntry = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code);

            if (urlEntry != null)
            {
                urlEntry.ClickCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}