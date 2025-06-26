using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Servicos;

namespace site.diogocosta.dev.Controllers
{
    public class LeadsController : Controller
    {
        private readonly ILeadService _leadService;
        private readonly ILogger<LeadsController> _logger;

        public LeadsController(ILeadService leadService, ILogger<LeadsController> logger)
        {
            _leadService = leadService;
            _logger = logger;
        }

        // Dashboard interno simples - APENAS PARA DESENVOLVIMENTO/ADMIN
        // Em produção, implementar autenticação adequada
        public async Task<IActionResult> Stats()
        {
            try
            {
                // Verificar se estamos em desenvolvimento
                if (!HttpContext.RequestServices.GetService<IWebHostEnvironment>()!.IsDevelopment())
                {
                    return NotFound(); // Esconder em produção por segurança
                }

                var stats = await _leadService.ObterEstatisticasAsync();
                return Json(new
                {
                    success = true,
                    data = stats,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de leads");
                return Json(new
                {
                    success = false,
                    error = "Erro interno",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // API simples para verificar se sistema está funcionando
        [HttpGet("api/leads/health")]
        public IActionResult Health()
        {
            return Json(new
            {
                status = "healthy",
                service = "leads",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        // Endpoint para reenviar emails de boas-vindas (uso interno)
        [HttpPost("api/leads/resend-welcome-emails")]
        public async Task<IActionResult> ResendWelcomeEmails()
        {
            try
            {
                // Verificar se estamos em desenvolvimento
                if (!HttpContext.RequestServices.GetService<IWebHostEnvironment>()!.IsDevelopment())
                {
                    return Unauthorized();
                }

                var leadsSemEmail = await _leadService.BuscarLeadsSemEmailBoasVindasAsync();
                var count = leadsSemEmail.Count;

                _logger.LogInformation("🔄 Reenviando emails de boas-vindas para {Count} leads", count);

                // Reenviar emails em background
                _ = Task.Run(async () =>
                {
                    foreach (var lead in leadsSemEmail)
                    {
                        try
                        {
                            // Aqui você implementaria a lógica de reenvio
                            // Para este exemplo, apenas logamos
                            _logger.LogInformation("📧 Reenviando email para {Email}", lead.Email);
                            await Task.Delay(1000); // Evitar spam
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao reenviar email para {Email}", lead.Email);
                        }
                    }
                });

                return Json(new
                {
                    success = true,
                    message = $"Processo iniciado para {count} leads",
                    count = count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reenviar emails de boas-vindas");
                return Json(new
                {
                    success = false,
                    error = "Erro interno",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
} 