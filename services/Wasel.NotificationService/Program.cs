using Microsoft.EntityFrameworkCore;
using Wasel.NotificationService.Consumers;
using Wasel.NotificationService.Data;
using Wasel.NotificationService.Options;
using Wasel.NotificationService.Repositories;
using Wasel.NotificationService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<FirebaseOptions>(
    builder.Configuration.GetSection(FirebaseOptions.SectionName));

// Database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
var firebaseEnabled = builder.Configuration.GetValue<bool>("Firebase:Enabled");
if (firebaseEnabled)
{
    builder.Services.AddSingleton<IPushNotificationSender, FirebasePushNotificationSender>();
}
else
{
    builder.Services.AddSingleton<IPushNotificationSender, NoopPushNotificationSender>();
}

builder.Services.AddSingleton<IEmailSender, NoopEmailSender>();
builder.Services.AddScoped<INotificationProcessor, NotificationProcessor>();

// Hosted Services
builder.Services.AddHostedService<NotificationRequestedConsumer>();

var host = builder.Build();
host.Run();
