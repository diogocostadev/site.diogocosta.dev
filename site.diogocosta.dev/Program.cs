using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Extentions;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Servicos.Interfaces;
using site.diogocosta.dev.Middleware;
using StackExchange.Redis;
using Npgsql;
using System.Net;
using AntiSpam.Core.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Configurar compressão de resposta
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "text/plain",
        "text/css",
        "text/javascript",
        "text/html",
        "application/javascript",
        "application/json",
        "application/xml",
        "text/xml",
        "image/svg+xml"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

//--------------------------------------------
// Configurar Serilog com Seq
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Site.DiogoCosta.Dev");

    // Logs em arquivo APENAS no ambiente de desenvolvimento
    // ou se explicitamente habilitado na configuração
    var enableFileLogging = context.Configuration.GetValue<bool>("Logging:EnableFileLogging");
    if (context.HostingEnvironment.IsDevelopment() || enableFileLogging)
    {
        configuration.WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day);
    }

    // Configurar Seq se disponível
    var seqUrl = context.Configuration["Seq:ServerUrl"];
    var seqApiKey = context.Configuration["Seq:ApiKey"];
    
    if (!string.IsNullOrEmpty(seqUrl))
    {
        configuration.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
    }
});

// Garantir que o diretório de logs existe APENAS quando necessário
var enableFileLoggingConfig = builder.Configuration.GetValue<bool>("Logging:EnableFileLogging");
if (builder.Environment.IsDevelopment() || enableFileLoggingConfig)
{
    var logsDir = Path.Combine(builder.Environment.ContentRootPath, "logs");
    if (!Directory.Exists(logsDir))
    {
        Directory.CreateDirectory(logsDir);
    }
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

// Configuração do serviço de WhatsApp
builder.Services.Configure<WhatsAppSettings>(
    builder.Configuration.GetSection("WhatsAppSettings"));
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "WhatsAppService");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configuração do serviço de interessados nas lives
builder.Services.AddScoped<IInteressadoLiveService, InteressadoLiveService>();

// Configuração do serviço anti-spam usando AntiSpam.Core (simplificado)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AntiSpamServiceCore>();
builder.Services.AddScoped<IAntiSpamService>(provider => provider.GetRequiredService<AntiSpamServiceCore>());

// Manter o serviço original também para compatibilidade
builder.Services.AddScoped<AntiSpamService>();

// Background services para detecção automática de bots
builder.Services.AddHostedService<BotDetectorBackgroundService>(); // Original
builder.Services.AddHostedService<BotDetectorBackgroundServiceCore>(); // Novo

// Configurar Response Caching para PageSpeed otimização
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.UseCaseSensitivePaths = false;
    options.SizeLimit = 100 * 1024 * 1024; // 100MB
});

// Configurar HTTP Cache Headers
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60 * 24 * 365; // 1 ano
        ctx.Context.Response.Headers.Append("Cache-Control", $"public,max-age={durationInSeconds}");
        ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddYears(1).ToString("R"));
    };
});

var app = builder.Build();

// Middleware de cache e otimização de performance
app.UseResponseCompression();
app.UseResponseCaching();

// Middleware personalizado para otimização de headers
app.Use(async (context, next) =>
{
    // Adicionar headers de segurança e performance
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    // Headers específicos para performance
    if (context.Request.Path.StartsWithSegments("/img") ||
        context.Request.Path.StartsWithSegments("/css") ||
        context.Request.Path.StartsWithSegments("/js"))
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        context.Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
        
        // Headers de performance para recursos estáticos
        context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        context.Response.Headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
    }
    
    // Cache policy para diferentes tipos de conteúdo
    if (context.Request.Path.Value?.EndsWith(".html") == true ||
        context.Request.Path == "/" ||
        context.Request.Path.StartsWithSegments("/blog"))
    {
        context.Response.Headers["Cache-Control"] = "public, max-age=300, stale-while-revalidate=60";
    }
    
    // Headers de preload para recursos críticos
    if (context.Request.Path == "/")
    {
        context.Response.Headers["Link"] = 
            "</css/site.css>; rel=preload; as=style, " +
            "</js/newsletter.js>; rel=preload; as=script, " +
            "<https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap>; rel=preload; as=style, " +
            "<https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css>; rel=preload; as=style";
    }
    
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configurar arquivos estáticos com cache otimizado
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var file = context.File;
        var response = context.Context.Response;
        var headers = response.Headers;
        
        // Cache agressivo para recursos de imagem
        if (file.Name.EndsWith(".webp") || file.Name.EndsWith(".png") || 
            file.Name.EndsWith(".jpg") || file.Name.EndsWith(".jpeg") ||
            file.Name.EndsWith(".gif") || file.Name.EndsWith(".svg"))
        {
            headers["Cache-Control"] = "public, max-age=31536000, immutable"; // 1 ano
            headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
        }
        // Cache para JavaScript e CSS
        else if (file.Name.EndsWith(".js") || file.Name.EndsWith(".css"))
        {
            headers["Cache-Control"] = "public, max-age=31536000, immutable"; // 1 ano
            headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
        }
        // Cache específico para favicons
        else if (file.Name.Contains("favicon") || file.Name.Contains(".ico") || 
                (file.Name.EndsWith(".png") && file.PhysicalPath?.Contains("img") == true))
        {
            headers["Cache-Control"] = "public, max-age=31536000, immutable"; // 1 ano
            headers["Expires"] = DateTime.UtcNow.AddYears(1).ToString("R");
        }
        // Cache padrão para outros recursos
        else
        {
            headers["Cache-Control"] = "public, max-age=86400"; // 1 dia
        }
        
        // Adicionar ETag para versionamento
        var etag = $"\"{file.LastModified:yyyyMMddHHmmss}\"";
        headers["ETag"] = etag;
        
        // Adicionar headers de compressão se não estiver comprimido
        if (!response.HasStarted && file.Length > 1024) // Arquivos maiores que 1KB
        {
            var acceptEncoding = context.Context.Request.Headers["Accept-Encoding"].ToString();
            if (acceptEncoding.Contains("gzip"))
            {
                headers["Vary"] = "Accept-Encoding";
            }
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

// Rate limiting middleware (manter o original por enquanto)
app.UseMiddleware<site.diogocosta.dev.Middleware.RateLimitingMiddleware>();

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