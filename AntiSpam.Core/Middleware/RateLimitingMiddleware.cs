using AntiSpam.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace AntiSpam.Core.Middleware;

/// <summary>
/// Middleware de rate limiting reutilizável
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitConfig _config;

    // Cache de tentativas por IP
    private static readonly ConcurrentDictionary<string, List<DateTime>> _requestTimes = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        
        _config = new RateLimitConfig();
        configuration.GetSection("RateLimit").Bind(_config);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var path = context.Request.Path.ToString();

        if (IsRateLimited(clientIp, path))
        {
            _logger.LogWarning("🚫 Rate limit excedido para IP: {IP} na rota: {Path}", clientIp, path);
            
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", _config.WindowSizeMinutes.ToString());
            
            await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
            return;
        }

        await _next(context);
    }

    private bool IsRateLimited(string clientIp, string path)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-_config.WindowSizeMinutes);

        _requestTimes.AddOrUpdate(clientIp, new List<DateTime> { now }, (key, times) =>
        {
            // Remover tentativas antigas
            times.RemoveAll(t => t < windowStart);
            times.Add(now);
            return times;
        });

        var currentCount = _requestTimes[clientIp].Count;
        return currentCount > _config.RequestsPerMinute;
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Tentar obter IP real através de headers de proxy
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            return ipAddress.Split(',')[0].Trim();
        }

        ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            return ipAddress;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
