using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Controllers;
using UrlShortenerService.Data;
using UrlShortenerService.Services;
using Microsoft.Extensions.Caching.Memory;
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
    }
}