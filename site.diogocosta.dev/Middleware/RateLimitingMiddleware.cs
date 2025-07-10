using System.Collections.Concurrent;
using System.Net;

namespace site.diogocosta.dev.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
        private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(5);
        private readonly int _maxRequests = 3; // Máximo 3 leads por IP a cada 5 minutos

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Aplicar rate limiting apenas para formulários de lead
            if (IsLeadFormRequest(context))
            {
                var clientIp = GetClientIpAddress(context);
                
                if (IsRateLimited(clientIp))
                {
                    _logger.LogWarning("🚫 Rate limit excedido para IP: {IP} na rota: {Path}", 
                        clientIp, context.Request.Path);
                    
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Muitas tentativas. Tente novamente em alguns minutos.");
                    return;
                }

                RecordRequest(clientIp);
            }

            await _next(context);
        }

        private bool IsLeadFormRequest(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            return context.Request.Method == "POST" && 
                   (path?.Contains("/desbloqueio/cadastrar") == true ||
                    path?.Contains("/desafios/cadastrar") == true ||
                    path?.Contains("/api/leads") == true);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var ip = forwarded.Split(',')[0].Trim();
                if (IPAddress.TryParse(ip, out _))
                    return ip;
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp) && IPAddress.TryParse(realIp, out _))
                return realIp;

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsRateLimited(string clientIp)
        {
            if (!_requests.TryGetValue(clientIp, out var timestamps))
                return false;

            var cutoff = DateTime.UtcNow - _timeWindow;
            var recentRequests = timestamps.Where(t => t > cutoff).ToList();
            
            // Atualizar a lista com apenas requests recentes
            _requests[clientIp] = recentRequests;

            return recentRequests.Count >= _maxRequests;
        }

        private void RecordRequest(string clientIp)
        {
            _requests.AddOrUpdate(clientIp, 
                new List<DateTime> { DateTime.UtcNow },
                (key, existing) => 
                {
                    existing.Add(DateTime.UtcNow);
                    return existing;
                });
        }
    }
}
