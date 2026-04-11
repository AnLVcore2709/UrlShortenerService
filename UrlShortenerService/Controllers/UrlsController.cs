using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Data;
using UrlShortenerService.Services;

namespace UrlShortenerService.Controllers
{
    // Controller handling HTTP requests for URL shortening, redirection, and analytics.
    [ApiController]
    [Route("")]
    public class UrlsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UrlService _urlService;
        private readonly IConfiguration _configuration;

        public UrlsController(AppDbContext context, UrlService urlService, IConfiguration configuration)
        {
            _context = context;
            _urlService = urlService;
            _configuration = configuration;
        }

        // API POST: Shorten a long URL into a small code.
        [HttpPost("api/urls")]
        public async Task<IActionResult> Shorten([FromBody] UrlRequest model)
        {
            // Validate URL format (must be absolute with HTTP/HTTPS schemes).
            if (!Uri.TryCreate(model.Url, UriKind.Absolute, out var uriResult)
                || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest("Invalid URL. Only HTTP and HTTPS schemes are supported.");
            }

            // Generate short code and save entry via service.
            var code = await _urlService.CreateShortUrl(model.Url);

            // Determine Base URL for the full shortened link.
            // Priority: Environment Variable -> AppSettings Config -> Current Request Host.
            var baseUrl = Environment.GetEnvironmentVariable("BASE_URL")
                          ?? _configuration["AppSettings:BaseUrl"]
                          ?? $"{Request.Scheme}://{Request.Host}";

            var shortUrl = $"{baseUrl}/{code}";

            // Return created status with the shortened link information.
            return CreatedAtAction(nameof(GetUrlInfo), new { code = code }, new { shortUrl, code });
        }

        // API GET: Redirect user from the short code to the original destination.
        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectTo(string code)
        {
            // Retrieve mapping for the given code.
            var originalUrl = await _urlService.GetOriginalUrl(code);

            if (originalUrl == null)
            {
                return NotFound(new { message = "Short URL not found." });
            }

            // Increment access count (can be executed as background task).
            await _urlService.IncrementClickCount(code);

            // Perform 302 Found redirection.
            return Redirect(originalUrl);
        }

        // API GET: Retrieve detailed analytics and data for a short code.
        [HttpGet("api/urls/{code}")]
        public async Task<IActionResult> GetUrlInfo(string code)
        {
            // Direct query from database for full entity details.
            var urlEntry = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            if (urlEntry == null)
                return NotFound(new { message = "Short URL not found." });

            return Ok(urlEntry);
        }
    }

    // Data Transfer Object for incoming shorten requests.
    public record UrlRequest(string Url);
}