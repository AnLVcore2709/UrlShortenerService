using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Data;
using UrlShortenerService.Services;

namespace UrlShortenerService.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UrlService _urlService;

        public UrlsController(AppDbContext context, UrlService urlService)
        {
            _context = context;
            _urlService = urlService;
        }

        // Create a short URL via POST request
        [HttpPost("api/urls")]
        public async Task<IActionResult> Shorten([FromBody] UrlRequest model)
        {
            // Validate URL format (only HTTP and HTTPS are allowed)
            if (!Uri.TryCreate(model.Url, UriKind.Absolute, out var uriResult)
            || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest("Invalid URL. Only HTTP and HTTPS are supported.");
            }

            // Generate short code and build the complete short URL string
            var code = await _urlService.CreateShortUrl(model.Url);
            var shortUrl = $"{Request.Scheme}://{Request.Host}/{code}";

            // Return 201 Created status with the location of the new resource
            return CreatedAtAction(nameof(GetUrlInfo), new { code = code }, new { shortUrl, code });
        }

        // Redirect to the original long URL using the short code identifier
        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectTo(string code)
        {
            // Use service to retrieve original URL (includes caching logic)
            var originalUrl = await _urlService.GetOriginalUrl(code);

            if (originalUrl == null)
            {
                return NotFound(new { message = "URL not found." });
            }

            // Increment click count in the background to avoid slowing down redirect
            await _urlService.IncrementClickCount(code);

            // Perform a 302 Found redirect to the original destination
            return Redirect(originalUrl);
        }

        // Get detailed information and analytics about a short URL
        [HttpGet("api/urls/{code}")]
        public async Task<IActionResult> GetUrlInfo(string code)
        {
            // Query the database directly for the entity details
            var urlEntry = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            if (urlEntry == null)
                return NotFound(new { message = "URL not found." });

            return Ok(urlEntry);
        }
    }

    // Simple Data Transfer Object (DTO) for incoming URL requests
    public record UrlRequest(string Url);
}