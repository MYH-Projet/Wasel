using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Wasel.Api.Shared.Database;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Users.Services;
var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// Services Configuration
// ──────────────────────────────────────────────

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// PostgreSQL with EF Core
builder.Services.AddDbContext<WaselDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Module Users
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Controllers
builder.Services.AddControllers();

// CORS — permissive for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Automatically apply pending EF Core migrations on startup ONLY in Development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
        dbContext.Database.Migrate();
    }
}

// ──────────────────────────────────────────────
// Middleware Pipeline
// ──────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.MapControllers();

// ──────────────────────────────────────────────
// Health Check Endpoint
// ──────────────────────────────────────────────
app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "Wasel.Api",
    Timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health");

app.Run();
