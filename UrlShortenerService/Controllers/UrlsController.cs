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

        [HttpPost("api/urls")]
        public async Task<IActionResult> Shorten([FromBody] UrlRequest model)
        {
            if (!Uri.TryCreate(model.Url, UriKind.Absolute, out var uriResult)
            || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest("Invalid URL. Only HTTP and HTTPS are supported.");
            }

            var code = await _urlService.CreateShortUrl(model.Url);
            var shortUrl = $"{Request.Scheme}://{Request.Host}/{code}";

            return CreatedAtAction(nameof(GetUrlInfo),
                new { code = code },
                new { shortUrl, code });
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectTo(string code)
        {
            // Get original URL via service (service handles caching)
            var originalUrl = await _urlService.GetOriginalUrl(code);

            if (originalUrl == null)
            {
                return NotFound(new { message = "URL not found." });
            }

            // Increase click count in background
            _ = Task.Run(() => _urlService.IncrementClickCount(code));

            return Redirect(originalUrl);
        }

        [HttpGet("api/urls/{code}")]
        public async Task<IActionResult> GetUrlInfo(string code)
        {
            var urlEntry = await _context.ShortUrls
                .FirstOrDefaultAsync(u => u.ShortCode == code);

            if (urlEntry == null)
                return NotFound(new { message = "URL not found." });

            return Ok(urlEntry);
        }
    }

    public record UrlRequest(string Url);
}