using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using System.Net;

namespace site.diogocosta.dev.Controllers
{
    public class LivesController : Controller
    {
        private readonly IInteressadoLiveService _interessadoLiveService;
        private readonly ILogger<LivesController> _logger;

        public LivesController(
            IInteressadoLiveService interessadoLiveService,
            ILogger<LivesController> logger)
        {
            _interessadoLiveService = interessadoLiveService;
            _logger = logger;
        }

        // GET: /lives
        public IActionResult Index()
        {
            var model = new CadastroInteressadoLiveRequest();
            return View(model);
        }

        // POST: /lives/cadastrar
        [HttpPost]
        [Route("lives/cadastrar")]
        public async Task<IActionResult> Cadastrar(CadastroInteressadoLiveRequest model)
        {
            try
            {
                // Validação customizada
                if (!model.IsValid(out string errorMessage))
                {
                    ModelState.AddModelError("", errorMessage);
                    return View("Index", model);
                }

                // Capturar dados do request
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // Definir origem baseada no parâmetro ou referrer
                if (string.IsNullOrWhiteSpace(model.Origem))
                {
                    var referer = Request.Headers["Referer"].ToString();
                    model.Origem = GetOrigemFromReferer(referer);
                }

                // Cadastrar interessado
                var interessado = await _interessadoLiveService.CadastrarInteressadoAsync(model, ipAddress, userAgent);

                if (interessado != null)
                {
                    // Salvar dados na sessão para página de sucesso
                    TempData["NomeInteressado"] = interessado.Nome;
                    TempData["EmailInteressado"] = interessado.Email;
                    TempData["WhatsAppInteressado"] = interessado.WhatsAppFormatado;
                    TempData["CadastroNovo"] = true;

                    _logger.LogInformation("Interessado cadastrado com sucesso: {Nome} - {Email} - {WhatsApp}", 
                        interessado.Nome, interessado.Email ?? "N/A", interessado.WhatsAppFormatado ?? "N/A");

                    return RedirectToAction("Sucesso");
                }
                else
                {
                    ModelState.AddModelError("", "Erro ao processar cadastro. Tente novamente.");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar cadastro de interessado");
                ModelState.AddModelError("", "Erro interno. Tente novamente em alguns minutos.");
                return View("Index", model);
            }
        }

        // GET: /lives/sucesso
        [Route("lives/sucesso")]
        public IActionResult Sucesso()
        {
            var nome = TempData["NomeInteressado"] as string;
            var email = TempData["EmailInteressado"] as string;
            var whatsapp = TempData["WhatsAppInteressado"] as string;
            var cadastroNovo = TempData["CadastroNovo"] as bool? ?? false;

            // Se não há dados, redirecionar para cadastro
            if (string.IsNullOrWhiteSpace(nome))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Nome = nome;
            ViewBag.Email = email;
            ViewBag.WhatsApp = whatsapp;
            ViewBag.CadastroNovo = cadastroNovo;

            return View();
        }

        // GET: /lives/sair
        [Route("lives/sair")]
        public IActionResult Sair()
        {
            var model = new DescadastroInteressadoLiveRequest();
            return View(model);
        }

        // POST: /lives/sair
        [HttpPost]
        [Route("lives/sair")]
        public async Task<IActionResult> Sair(DescadastroInteressadoLiveRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var sucesso = await _interessadoLiveService.DescadastrarInteressadoAsync(model.Email);

                if (sucesso)
                {
                    TempData["EmailDescadastrado"] = model.Email;
                    TempData["DescadastroSucesso"] = true;
                    
                    _logger.LogInformation("Interessado descadastrado: {Email}", model.Email);

                    return RedirectToAction("DescadastroSucesso");
                }
                else
                {
                    ModelState.AddModelError("", "Email não encontrado ou já estava inativo.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar descadastro: {Email}", model.Email);
                ModelState.AddModelError("", "Erro interno. Tente novamente em alguns minutos.");
                return View(model);
            }
        }

        // GET: /lives/descadastro-sucesso
        [Route("lives/descadastro-sucesso")]
        public IActionResult DescadastroSucesso()
        {
            var email = TempData["EmailDescadastrado"] as string;
            var sucesso = TempData["DescadastroSucesso"] as bool? ?? false;

            ViewBag.Email = email;
            ViewBag.Sucesso = sucesso;

            return View();
        }

        // API: /api/lives/stats (para admin)
        [Route("api/lives/stats")]
        public async Task<IActionResult> Stats()
        {
            try
            {
                // Verificar se está em desenvolvimento (remover em produção)
                if (!HttpContext.RequestServices.GetService<IWebHostEnvironment>()!.IsDevelopment())
                {
                    return NotFound();
                }

                var stats = await _interessadoLiveService.ObterEstatisticasAsync();
                
                return Json(new
                {
                    success = true,
                    data = stats,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de interessados");
                return Json(new
                {
                    success = false,
                    error = "Erro interno",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // API: /api/lives/lista-ativos (para admin)
        [Route("api/lives/lista-ativos")]
        public async Task<IActionResult> ListaAtivos()
        {
            try
            {
                // Verificar se está em desenvolvimento (remover em produção)
                if (!HttpContext.RequestServices.GetService<IWebHostEnvironment>()!.IsDevelopment())
                {
                    return NotFound();
                }

                var interessados = await _interessadoLiveService.ListarInteressadosAtivosAsync();
                
                return Json(new
                {
                    success = true,
                    data = interessados.Select(i => new
                    {
                        id = i.Id,
                        nome = i.Nome,
                        email = i.Email,
                        whatsapp = i.WhatsAppFormatado,
                        origem = i.Origem,
                        dataCadastro = i.DataCadastro,
                        boasVindasEmail = i.BoasVindasEmailEnviado,
                        boasVindasWhatsApp = i.BoasVindasWhatsAppEnviado
                    }),
                    count = interessados.Count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar interessados ativos");
                return Json(new
                {
                    success = false,
                    error = "Erro interno",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // Métodos auxiliares
        private string GetClientIpAddress()
        {
            var xForwardedFor = Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrWhiteSpace(xForwardedFor))
            {
                return xForwardedFor.ToString().Split(',')[0].Trim();
            }

            var xRealIp = Request.Headers["X-Real-IP"];
            if (!string.IsNullOrWhiteSpace(xRealIp))
            {
                return xRealIp.ToString();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private string GetOrigemFromReferer(string referer)
        {
            if (string.IsNullOrWhiteSpace(referer))
                return "site";

            var refererLower = referer.ToLower();
            
            if (refererLower.Contains("youtube.com") || refererLower.Contains("youtu.be"))
                return "youtube";
            
            if (refererLower.Contains("twitch.tv"))
                return "twitch";
            
            if (refererLower.Contains("diogocosta.dev"))
                return "site";

            return "externo";
        }

        // API: /api/lives/teste-whatsapp (para desenvolvimento/teste)
        [Route("api/lives/teste-whatsapp")]
        public async Task<IActionResult> TesteWhatsApp()
        {
            try
            {
                // Verificar se está em desenvolvimento (remover em produção se necessário)
                if (!HttpContext.RequestServices.GetService<IWebHostEnvironment>()!.IsDevelopment())
                {
                    return NotFound();
                }

                var whatsAppService = HttpContext.RequestServices.GetService<IWhatsAppService>();
                if (whatsAppService == null)
                {
                    return Json(new
                    {
                        success = false,
                        error = "Serviço de WhatsApp não disponível",
                        timestamp = DateTime.UtcNow
                    });
                }

                var resultado = await whatsAppService.TestarEnvioAsync();
                
                return Json(new
                {
                    success = resultado,
                    message = resultado ? "Teste de WhatsApp enviado com sucesso!" : "Falha no envio do teste",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar WhatsApp");
                return Json(new
                {
                    success = false,
                    error = "Erro interno",
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
} 