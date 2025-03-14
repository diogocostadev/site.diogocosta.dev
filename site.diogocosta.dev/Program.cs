using Serilog;
using site.diogocosta.dev.Extentions;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Servicos.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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