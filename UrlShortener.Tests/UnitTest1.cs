using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UrlShortenerService.Controllers;
using UrlShortenerService.Data;
using UrlShortenerService.Models;
using UrlShortenerService.Services;
using Xunit;

namespace UrlShortener.Tests
{
    public class UrlServiceTests
    {
        private AppDbContext GetDbContext()
        {
            // Create an in-memory database for fast and isolated testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void GenerateCode_ShouldReturn6Characters()
        {
            // Arrange: Setup test data and dependencies
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions()); // Create in-memory cache
            var service = new UrlService(db, cache);

            // Act: Execute the method being tested
            var code = service.GenerateCode();

            // Assert: Verify the result matches expectations
            Assert.Equal(6, code.Length);
        }

        [Fact]
        public async Task CreateShortUrl_ShouldSaveToDatabase()
        {
            // Arrange: Prepare input data
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var longUrl = "https://google.com";

            // Act: Perform the creation of short URL
            var code = await service.CreateShortUrl(longUrl);

            // Assert: Check if the record exists in the database
            var savedUrl = await db.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            Assert.NotNull(savedUrl);
            Assert.Equal(longUrl, savedUrl.OriginalUrl);
        }

        // Test controller validation for invalid URL input
        [Fact]
        public async Task Shorten_InvalidUrl_ShouldReturnBadRequest()
        {
            // Arrange: Inject dependencies into the controller
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            // Controller requires both DbContext and Service
            var controller = new UrlsController(db, service);
            var request = new UrlRequest("not-a-valid-url");

            // Act: Call the API method with bad input data
            var result = await controller.Shorten(request);

            // Assert: Ensure the response is a 400 Bad Request
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task GetOriginalUrl_ShouldSetCacheAfterFirstCall()
        {
            // Arrange
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var code = "cache1";
            var longUrl = "https://openai.com";
            db.ShortUrls.Add(new ShortUrl { ShortCode = code, OriginalUrl = longUrl });
            await db.SaveChangesAsync();


            await service.GetOriginalUrl(code);

            var isExistsInCache = cache.TryGetValue(code, out string cachedUrl);
            Assert.True(isExistsInCache);
            Assert.Equal(longUrl, cachedUrl);
        }
        [Fact]
        public async Task IncrementClickCount_ShouldIncreaseDatabaseValue()
        {
            // Arrange
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var code = "click1";
            var entry = new ShortUrl { ShortCode = code, OriginalUrl = "https://test.com", ClickCount = 0 };
            db.ShortUrls.Add(entry);
            await db.SaveChangesAsync();

            // Act
            await service.IncrementClickCount(code);

            // Assert
            var updatedEntry = await db.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code);
            Assert.Equal(1, updatedEntry.ClickCount);
        }
        [Fact]
        public async Task RedirectTo_ValidCode_ShouldReturn302Redirect()
        {
            // Arrange
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var controller = new UrlsController(db, service);

            var code = "test302";
            var longUrl = "https://facebook.com";
            db.ShortUrls.Add(new ShortUrl { ShortCode = code, OriginalUrl = longUrl });
            await db.SaveChangesAsync();

            // Act
            var result = await controller.RedirectTo(code);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(longUrl, redirectResult.Url);
        }
    }
}