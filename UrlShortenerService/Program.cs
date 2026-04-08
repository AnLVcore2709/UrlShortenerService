using Microsoft.EntityFrameworkCore;
using UrlShortenerService.Data;
using UrlShortenerService.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Service Registration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Lab 8: Lấy Connection String từ môi trường Docker hoặc appsettings
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                        ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Lab 8: Chuyển sang dùng Npgsql cho PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<UrlService>();

builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// 2. Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.MapControllers();

// 3. Lab 8: Tự động Migration với Postgres (Retry logic)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    for (int i = 0; i < 10; i++)
    {
        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
            Console.WriteLine("Database migration successful!");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Waiting for Postgres... (Attempt {i + 1}/10)");
            Thread.Sleep(5000);
        }
    }
}

app.Run();