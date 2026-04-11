using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using UrlShortenerService.Controllers;
using UrlShortenerService.Data;
using UrlShortenerService.Models;
using UrlShortenerService.Services;
using Xunit;

namespace UrlShortener.Tests
{
    // Main test class for UrlService and UrlsController functionality.
    public class UrlServiceTests
    {
        // Helper method to create an in-memory database context for isolated testing.
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Helper method to create a mock configuration with a base URL.
        private IConfiguration GetConfiguration()
        {
            var myConfiguration = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("AppSettings:BaseUrl", "http://localhost:5000")
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        // Test case: Verifies that GenerateCode produces a string exactly 6 characters long.
        [Fact]
        public void GenerateCode_ShouldReturn6Characters()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var code = service.GenerateCode();

            Assert.Equal(6, code.Length);
        }

        // Test case: Verifies that creating a short URL correctly saves the original mapping to the database.
        [Fact]
        public async Task CreateShortUrl_ShouldSaveToDatabase()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var longUrl = "https://google.com";

            var code = await service.CreateShortUrl(longUrl);

            var savedUrl = await db.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            Assert.NotNull(savedUrl);
            Assert.Equal(longUrl, savedUrl.OriginalUrl);
        }

        // Test case: Verifies that the controller returns a BadRequest when provided with an invalid URL format.
        [Fact]
        public async Task Shorten_InvalidUrl_ShouldReturnBadRequest()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var config = GetConfiguration();

            var controller = new UrlsController(db, service, config);
            var request = new UrlRequest("not-a-valid-url");

            var result = await controller.Shorten(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test case: Verifies that GetOriginalUrl stores the result in cache after the first database hit.
        [Fact]
        public async Task GetOriginalUrl_ShouldSetCacheAfterFirstCall()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var code = "cache1";
            var longUrl = "https://openai.com";
            db.ShortUrls.Add(new ShortUrl { ShortCode = code, OriginalUrl = longUrl });
            await db.SaveChangesAsync();

            await service.GetOriginalUrl(code);

            var isExistsInCache = cache.TryGetValue(code, out string? cachedUrl);
            Assert.True(isExistsInCache);
            Assert.Equal(longUrl, cachedUrl);
        }

        // Test case: Verifies that IncrementClickCount successfully updates the ClickCount property in the database.
        [Fact]
        public async Task IncrementClickCount_ShouldIncreaseDatabaseValue()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var code = "click1";
            var entry = new ShortUrl { ShortCode = code, OriginalUrl = "https://test.com", ClickCount = 0 };
            db.ShortUrls.Add(entry);
            await db.SaveChangesAsync();

            await service.IncrementClickCount(code);

            var updatedEntry = await db.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code);
            Assert.NotNull(updatedEntry);
            Assert.Equal(1, updatedEntry.ClickCount);
        }

        // Test case: Verifies that RedirectTo returns a 302 Found status to the correct long URL.
        [Fact]
        public async Task RedirectTo_ValidCode_ShouldReturn302Redirect()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var config = GetConfiguration();
            var controller = new UrlsController(db, service, config);

            var code = "test302";
            var longUrl = "https://facebook.com";
            db.ShortUrls.Add(new ShortUrl { ShortCode = code, OriginalUrl = longUrl });
            await db.SaveChangesAsync();

            var result = await controller.RedirectTo(code);

            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(longUrl, redirectResult.Url);
        }

        // Test case: Verifies that querying a non-existent code returns null for the original URL.
        [Fact]
        public async Task GetOriginalUrl_NonExistentCode_ShouldReturnNull()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);

            var result = await service.GetOriginalUrl("missing");

            Assert.Null(result);
        }

        // Test case: Verifies that Shorten returns a CreatedAtActionResult with the short URL data when successful.
        [Fact]
        public async Task Shorten_ValidUrl_ShouldReturnCreatedAtAction()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var config = GetConfiguration();
            var controller = new UrlsController(db, service, config);
            
            var request = new UrlRequest("https://google.com");

            var result = await controller.Shorten(request);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        // Test case: Verifies that unsupported URL schemes (like FTP) are rejected with a BadRequest result.
        [Fact]
        public async Task Shorten_UnsupportedScheme_ShouldReturnBadRequest()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var config = GetConfiguration();
            var controller = new UrlsController(db, service, config);
            
            var request = new UrlRequest("ftp://myserver.com");

            var result = await controller.Shorten(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test case: Verifies that trying to redirect with a non-existent code returns a NotFound result.
        [Fact]
        public async Task RedirectTo_NonExistentCode_ShouldReturnNotFound()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var config = GetConfiguration();
            var controller = new UrlsController(db, service, config);

            var result = await controller.RedirectTo("nonexistent");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // Test case: Verifies that GetUrlInfo returns an Ok result with the correct metadata for a valid code.
        [Fact]
        public async Task GetUrlInfo_ValidCode_ShouldReturnOk()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var config = GetConfiguration();
            var controller = new UrlsController(db, service, config);

            var code = "info1";
            db.ShortUrls.Add(new ShortUrl { ShortCode = code, OriginalUrl = "https://test.com" });
            await db.SaveChangesAsync();

            var result = await controller.GetUrlInfo(code);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<ShortUrl>(okResult.Value);
            Assert.Equal(code, model.ShortCode);
        }

        // Test case: Verifies that querying info for a non-existent code returns a NotFound result.
        [Fact]
        public async Task GetUrlInfo_NonExistentCode_ShouldReturnNotFound()
        {
            var db = GetDbContext();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new UrlService(db, cache);
            var config = GetConfiguration();
            var controller = new UrlsController(db, service, config);

            var result = await controller.GetUrlInfo("missinginfo");

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
