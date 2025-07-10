using AntiSpam.Core.Middleware;
using AntiSpam.Core.Models;
using AntiSpam.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AntiSpam.Core.Extensions;

/// <summary>
/// Extensões para configuração dos serviços AntiSpam.Core
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona os serviços básicos do AntiSpam.Core
    /// </summary>
    public static IServiceCollection AddAntiSpamCore(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar configurações com valores padrão
        var botConfig = new BotDetectorConfig();
        configuration.GetSection("BotDetector").Bind(botConfig);
        services.Configure<BotDetectorConfig>(config =>
        {
            config.AnalysisIntervalMinutes = botConfig.AnalysisIntervalMinutes > 0 ? botConfig.AnalysisIntervalMinutes : 5;
            config.IpThreshold = botConfig.IpThreshold > 0 ? botConfig.IpThreshold : 10;
            config.UserAgentThreshold = botConfig.UserAgentThreshold > 0 ? botConfig.UserAgentThreshold : 15;
            config.EmailDomainThreshold = botConfig.EmailDomainThreshold > 0 ? botConfig.EmailDomainThreshold : 5;
            config.NameThreshold = botConfig.NameThreshold > 0 ? botConfig.NameThreshold : 3;
            config.CacheCleanupHours = botConfig.CacheCleanupHours > 0 ? botConfig.CacheCleanupHours : 24;
        });

        var rateLimitConfig = new RateLimitConfig();
        configuration.GetSection("RateLimit").Bind(rateLimitConfig);
        services.Configure<RateLimitConfig>(config =>
        {
            config.RequestsPerMinute = rateLimitConfig.RequestsPerMinute > 0 ? rateLimitConfig.RequestsPerMinute : 5;
            config.BurstSize = rateLimitConfig.BurstSize > 0 ? rateLimitConfig.BurstSize : 10;
            config.WindowSizeMinutes = rateLimitConfig.WindowSizeMinutes > 0 ? rateLimitConfig.WindowSizeMinutes : 1;
        });

        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Adiciona o serviço anti-spam específico
    /// </summary>
    public static IServiceCollection AddAntiSpamService<TService, TRule>(this IServiceCollection services)
        where TService : class, IAntiSpamService<TRule>
        where TRule : class, IAntiSpamRule
    {
        services.AddScoped<IAntiSpamService<TRule>, TService>();
        return services;
    }

    /// <summary>
    /// Adiciona o background service para detecção de bots
    /// </summary>
    public static IServiceCollection AddBotDetectorBackgroundService<TService>(this IServiceCollection services)
        where TService : class, IHostedService
    {
        services.AddHostedService<TService>();
        return services;
    }
}

/// <summary>
/// Extensões para configuração do pipeline de middleware
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adiciona o middleware de rate limiting
    /// </summary>
    public static IApplicationBuilder UseAntiSpamMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
