using Microsoft.AspNetCore.DataProtection;
using Serilog;
using site.diogocosta.dev.Extentions;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Servicos.Interfaces;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Improved Redis connection configuration
var redisConfigOptions = new ConfigurationOptions
{
    EndPoints = { "185.182.186.116:6379" },
    Password = "Did@40csbr", 
    DefaultDatabase = 0,
    AbortOnConnectFail = false,
    ConnectTimeout = 10000,          // Increase timeout to 10 seconds
    SyncTimeout = 10000,             // Increase sync timeout
    ConnectRetry = 5,                // Retry connection 5 times
    ReconnectRetryPolicy = new ExponentialRetry(5000), // Exponential backoff
    KeepAlive = 60                   // Send keepalive every 60 seconds
};

// Create Redis connection with error handling
ConnectionMultiplexer redis;
try
{
    redis = ConnectionMultiplexer.Connect(redisConfigOptions);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    
    // Configure data protection with Redis
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
        .SetApplicationName("Site Diogo Costa Dev");
        
    Log.Information("Successfully connected to Redis for data protection");
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to connect to Redis. Falling back to file system for data protection");
    
    // Fallback to file system persistence if Redis connection fails
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
        .SetApplicationName("Site Diogo Costa Dev");
}


builder.Services.AddControllersWithViews();
builder.ConfigureSerilog();

builder.Services.AddHttpClient<INewsletterService, N8nNewsletterService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NewsletterService");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<NewsletterSettings>(
    builder.Configuration.GetSection("NewsletterSettings"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Log.Information($"Sistema iniciado em {DateTime.Now}");

app.Run();