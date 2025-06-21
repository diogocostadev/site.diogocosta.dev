using Microsoft.AspNetCore.DataProtection;
using Serilog;
using site.diogocosta.dev.Extentions;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Servicos.Interfaces;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//--------------------------------------------
// Substituir Redis por armazenamento em arquivo
var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");

// Garantir que o diretório existe
if (!Directory.Exists(keysDirectory))
{
    Directory.CreateDirectory(keysDirectory);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetApplicationName("Site Diogo Costa Dev");
//--------------------------------------------

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
    name: "blog-post",
    pattern: "blog/{slug}",
    defaults: new { controller = "Blog", action = "Post" });
app.MapControllerRoute(
    name: "blog-index",
    pattern: "blog",
    defaults: new { controller = "Blog", action = "Index" });
app.MapControllerRoute(
    name: "desbloqueio",
    pattern: "desbloqueio",
    defaults: new { controller = "Desbloqueio", action = "Index" });
app.MapControllerRoute(
    name: "obrigado-desbloqueio",
    pattern: "obrigado-desbloqueio",
    defaults: new { controller = "Desbloqueio", action = "Obrigado" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Log.Information($"Sistema iniciado em {DateTime.Now}");

app.Run();