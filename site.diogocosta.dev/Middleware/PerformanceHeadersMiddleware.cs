using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace site.diogocosta.dev.Middleware
{
    public class PerformanceHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public PerformanceHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Adicionar headers de performance e segurança
            var response = context.Response;
            var headers = response.Headers;

            // Headers de segurança que também melhoram performance
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "SAMEORIGIN";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Headers para melhorar performance
            headers["X-DNS-Prefetch-Control"] = "on";
            
            // Server timing para debugging (somente em desenvolvimento)
            if (context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
            {
                var startTime = DateTime.UtcNow;
                
                await _next(context);
                
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;
                headers["Server-Timing"] = $"total;dur={duration:F2}";
            }
            else
            {
                await _next(context);
            }

            // Headers específicos para recursos estáticos
            if (context.Request.Path.StartsWithSegments("/img") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/css"))
            {
                // Permitir que recursos sejam carregados de qualquer origem
                headers["Access-Control-Allow-Origin"] = "*";
                headers["Cross-Origin-Resource-Policy"] = "cross-origin";
            }
        }
    }

    public static class PerformanceHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceHeadersMiddleware>();
        }
    }
}
