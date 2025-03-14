using Microsoft.AspNetCore.DataProtection;
using Serilog;
using site.diogocosta.dev.Extentions;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Servicos.Interfaces;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { "185.182.186.116:6379" },
    Password = "Did@40csbr", 
    DefaultDatabase = 0,
    AbortOnConnectFail = false,
    AllowAdmin = true
});

builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("Site Código Central");


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