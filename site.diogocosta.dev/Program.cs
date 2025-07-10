using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Extentions;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Servicos.Interfaces;
using StackExchange.Redis;
using Npgsql;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

//--------------------------------------------
// Configurar Serilog com Seq
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Site.DiogoCosta.Dev");

    // Configurar Seq se disponível
    var seqUrl = context.Configuration["Seq:ServerUrl"];
    var seqApiKey = context.Configuration["Seq:ApiKey"];
    
    if (!string.IsNullOrEmpty(seqUrl))
    {
        configuration.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
    }
});
// Garantir que o diretório de logs existe
var logsDir = Path.Combine(builder.Environment.ContentRootPath, "logs");
if (!Directory.Exists(logsDir))
{
    Directory.CreateDirectory(logsDir);
}
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
// Configurar Entity Framework com PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("conexao-site");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), null);
    }).UseSnakeCaseNamingConvention();
    
    // Somente em desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
//--------------------------------------------

builder.Services.AddControllersWithViews();

// Configuração do serviço de newsletter
builder.Services.AddHttpClient<INewsletterService, N8nNewsletterService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NewsletterService");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<NewsletterSettings>(
    builder.Configuration.GetSection("NewsletterSettings"));

// Configuração do serviço de email
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Configuração da API de localização IP
builder.Services.Configure<IpLocalizationSettings>(
    builder.Configuration.GetSection("IpLocalizationApi"));

builder.Services.AddScoped<IEmailService, EmailService>();

// Configuração do serviço de leads
builder.Services.AddScoped<ILeadService, LeadService>();

// Configuração do serviço de VSL
builder.Services.AddScoped<IVSLService, VSLService>();

// Configuração do serviço de PDF Downloads
builder.Services.AddHttpClient(); // HttpClient genérico para injeção
builder.Services.AddScoped<IPdfDownloadService, PdfDownloadService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configurar arquivos estáticos com headers específicos para favicon
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // Cache para arquivos de favicon
        if (context.File.Name.Contains("favicon") || context.File.Name.Contains(".ico") || 
            context.File.Name.EndsWith(".png") && (context.File.PhysicalPath?.Contains("img") ?? false))
        {
            context.Context.Response.Headers["Cache-Control"] = "public, max-age=604800"; // 1 semana
            context.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddDays(7).ToString("R");
        }
    }
});

// Middleware específico para favicon na raiz - redireciona para PNG
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/favicon.ico")
    {
        context.Response.ContentType = "image/png";
        var faviconPath = Path.Combine(app.Environment.WebRootPath, "img", "D.png");
        if (File.Exists(faviconPath))
        {
            await context.Response.SendFileAsync(faviconPath);
            return;
        }
    }
    await next();
});

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
    name: "desafio-obrigado",
    pattern: "obrigado-{slug}",
    defaults: new { controller = "Desafios", action = "Obrigado" });
app.MapControllerRoute(
    name: "desafios",
    pattern: "{slug}",
    defaults: new { controller = "Desafios", action = "Index" },
    constraints: new { slug = @"^desafio-.*" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Log.Information($"Sistema iniciado em {DateTime.Now}");

app.Run();