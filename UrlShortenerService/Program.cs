using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Data;
using UrlShortenerService.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// 1. Register core services: API Controllers, Swagger, and Memory Cache
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// 2. Configure database connection (Prefer DB_CONNECTION_STRING environment variable for Docker/Cloud)
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                        ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Register custom application services (Dependency Injection)
builder.Services.AddScoped<UrlService>();

// 4. Configure CORS to allow frontend applications (Vue, React, etc.) to access the API
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    policy => policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()));

var app = builder.Build();

// 5. Configure HTTP Request Pipeline (Middleware)
// Enable Swagger UI for API testing
app.UseSwagger();
app.UseSwaggerUI();

// Handle forwarded headers when running behind a proxy (e.g., Nginx, Render)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors("AllowAll");

app.MapControllers();

// 6. Automatically apply pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        // Check and apply any missing schema changes
        dbContext.Database.Migrate();
    }
    catch (Exception)
    {
        // If migration fails, ensure the database is initialized
        dbContext.Database.EnsureCreated();
    }
}

// Start the application and begin listening for requests
app.Run();