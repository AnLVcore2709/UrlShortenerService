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
            // Create an in-memory database for fast testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void GenerateCode_ShouldReturn6Characters()
        {
            // Arrange
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            // Act
            var code = service.GenerateCode();

            // Assert
            Assert.Equal(6, code.Length);
        }

        [Fact]
        public async Task CreateShortUrl_ShouldSaveToDatabase()
        {
            // Arrange
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var longUrl = "https://google.com";

            // Act
            var code = await service.CreateShortUrl(longUrl);

            // Assert
            var savedUrl = await db.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            Assert.NotNull(savedUrl);
            Assert.Equal(longUrl, savedUrl.OriginalUrl);
        }

        [Fact]
        public async Task Shorten_InvalidUrl_ShouldReturnBadRequest()
        {
            // Arrange
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var controller = new UrlsController(db, service);
            var request = new UrlRequest("not-a-valid-url");

            // Act
            var result = await controller.Shorten(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}