using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Minio;

using Wasel.Api.Shared.Database;
using Wasel.Api.Shared.Security;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Infrastructure.MinIO;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Users.Services;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Drivers.Services;
using Wasel.Api.Modules.Documents.Repositories;
using Wasel.Api.Modules.Documents.Services;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Tracking.Hubs;
using Wasel.Api.Modules.Tracking.Repositories;
using Wasel.Api.Modules.Tracking.Services;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Deliveries.Services;
using System.Text.Json.Serialization;
using Wasel.Api.Modules.Complaints.Repositories;
using Wasel.Api.Modules.Complaints.Services;
using Wasel.Api.Modules.Messaging.Hubs;
using Wasel.Api.Modules.Messaging.Repositories;
using Wasel.Api.Modules.Messaging.Services;
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

// Keycloak JWT Authentication
var keycloakOptions = builder.Configuration
    .GetSection(KeycloakOptions.SectionName)
    .Get<KeycloakOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakOptions.InternalAuthority;
        options.Audience = keycloakOptions.ClientId;
        options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                keycloakOptions.Authority,
                keycloakOptions.InternalAuthority,
                keycloakOptions.NginxAuthority
            }.Where(i => !string.IsNullOrEmpty(i)).ToArray(),
            ValidateAudience = false, // TODO: Audience validation can be reinforced later if needed
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/api/hubs/gps"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(KeycloakConstants.RoleAdmin));
    options.AddPolicy("DriverOnly", p => p.RequireRole(KeycloakConstants.RoleDriver));
    options.AddPolicy("ClientOnly", p => p.RequireRole(KeycloakConstants.RoleClient));
});

// Claims transformation (Keycloak realm_access.roles → ClaimTypes.Role)
builder.Services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformer>();

// Shared Auth Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// MinIO object storage
builder.Services.Configure<MinioOptions>(
    builder.Configuration.GetSection(MinioOptions.SectionName));
builder.Services.AddSingleton<IMinioClient>(serviceProvider =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<MinioOptions>>()
        .Value;

    return new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey)
        .WithSSL(options.UseSSL)
        .Build();
});
builder.Services.AddScoped<IStorageService, MinioStorageService>();

// Module Users
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Module Tracking
builder.Services.AddScoped<ITrackingRepository, TrackingRepository>();
builder.Services.AddScoped<ITrackingService, TrackingService>();

// Memory Cache & SignalR
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

// Module Drivers
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IDriverDossierRepository, DriverDossierRepository>();
builder.Services.AddScoped<IDriverDossierService, DriverDossierService>();

// Module Documents
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

//module deliveries
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();

//module reclamations
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();

//module message
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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

// Auth middlewares must be here, before MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GpsHub>("/api/hubs/gps");
app.MapHub<MessagingHub>("/hubs/messaging");
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

// Required for WebApplicationFactory in integration tests
public partial class Program { }
