using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Data;
using UrlShortenerService.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Register core services: API controllers, Swagger, and Memory Cache
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Configure database connection for both Local and Cloud environments
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                        ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register application services using Scoped lifetime
builder.Services.AddScoped<UrlService>();

// Configure CORS to allow frontend applications (Vue/React) to access the API
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    policy => policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()));

var app = builder.Build();

// Configure middleware pipeline: Enable Swagger UI in the request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors("AllowAll");

app.MapControllers();

// Apply database migrations automatically at startup (useful for Docker deployment)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try 
    {
        // Applies any pending migrations
        dbContext.Database.Migrate();
    }
    catch (Exception)
    {
        // If migration fails (e.g. table already exists), ensure DB is created
        dbContext.Database.EnsureCreated();
    }
}

// Start the application and begin listening for requests
app.Run();