using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;
using site.diogocosta.dev.Servicos;

namespace site.diogocosta.dev.Controllers;

public class DesbloqueioController : Controller
{
    private readonly INewsletterService _newsletterService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<DesbloqueioController> _logger;
    private readonly IPdfDownloadService _pdfDownloadService;
    private readonly ILeadService _leadService;

    public DesbloqueioController(
        INewsletterService newsletterService, 
        IEmailService emailService,
        IWebHostEnvironment hostingEnvironment,
        ILogger<DesbloqueioController> logger,
        IPdfDownloadService pdfDownloadService,
        ILeadService leadService)
    {
        _newsletterService = newsletterService;
        _emailService = emailService;
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
        _pdfDownloadService = pdfDownloadService;
        _leadService = leadService;
    }

    public IActionResult Index()
    {
        return View(new DesbloqueioModel());
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(DesbloqueioModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        try
        {
            // Capturar dados do usuário para o lead
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            // Capturar UTM parameters
            var utmSource = Request.Query["utm_source"];
            var utmMedium = Request.Query["utm_medium"];
            var utmCampaign = Request.Query["utm_campaign"];
            var utmContent = Request.Query["utm_content"];
            var utmTerm = Request.Query["utm_term"];

            // Criar lead no sistema de leads
            var leadRequest = new CriarLeadRequest
            {
                Nome = model.Nome,
                Email = model.Email,
                DesafioSlug = "manual-primeira-virada", // Identificador único para o manual
                UtmSource = utmSource,
                UtmMedium = utmMedium,
                UtmCampaign = utmCampaign,
                UtmContent = utmContent,
                UtmTerm = utmTerm
            };

            var lead = await _leadService.CriarLeadAsync(leadRequest, ipAddress, userAgent);

            if (lead != null)
            {
                _logger.LogInformation("💰 Lead criado com sucesso para Manual da Primeira Virada: {Email} (ID: {LeadId})", 
                    model.Email, lead.Id);
            }
            else
            {
                _logger.LogWarning("⚠️ Falha ao criar lead para {Email}, mas continuando com o processo", model.Email);
            }

            // Cadastra na newsletter (mantendo funcionalidade existente)
            var usuario = new UsuarioNewsletter
            {
                Email = model.Email,
                Nome = model.Nome
            };

            await _newsletterService.CadastrarUsuarioAsync(usuario);

            // Envia email com link de download
            var emailEnviado = await _emailService.EnviarEmailDownloadAsync(model.Email, model.Nome);
            
            if (!emailEnviado)
            {
                _logger.LogWarning("Falha ao enviar email de download para {Email}", model.Email);
            }

            // Armazenar email na sessão para usar na página de obrigado
            TempData["UserEmail"] = model.Email;
            TempData["UserName"] = model.Nome;
            
            // LOG DETALHADO PARA DEBUG
            _logger.LogInformation("🔍 DEBUG CADASTRO - Email salvo no TempData: '{Email}'", model.Email);
            _logger.LogInformation("🔍 DEBUG CADASTRO - Nome salvo no TempData: '{Nome}'", model.Nome);

            _logger.LogInformation("Usuário {Nome} ({Email}) se cadastrou para receber o Manual da Primeira Virada", 
                model.Nome, model.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar cadastro para {Email}", model.Email);
        }

        return RedirectToAction("Obrigado");
    }

    private string GetClientIpAddress()
    {
        try
        {
            // Verificar headers de proxy
            var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = Request.Headers["X-Real-IP"].ToString();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback para RemoteIpAddress
            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    [Route("obrigado-desbloqueio")]
    public IActionResult Obrigado()
    {
        // Tentar recuperar email da sessão
        var userEmail = TempData["UserEmail"] as string;
        var userName = TempData["UserName"] as string;
        
        // LOG DETALHADO PARA DEBUG
        _logger.LogInformation("🔍 DEBUG OBRIGADO - Email recuperado do TempData: '{Email}'", userEmail ?? "NULL");
        _logger.LogInformation("🔍 DEBUG OBRIGADO - Nome recuperado do TempData: '{Nome}'", userName ?? "NULL");
        
        ViewBag.UserEmail = userEmail;
        ViewBag.UserName = userName;
        
        return View();
    }

    [Route("desbloqueio/download-pdf")]
    public async Task<IActionResult> DownloadPdf(string? email = null)
    {
        try
        {
            // LOG DETALHADO PARA DEBUG
            _logger.LogInformation("🔍 DEBUG DOWNLOAD - Email recebido: '{Email}'", email ?? "NULL");
            _logger.LogInformation("🔍 DEBUG DOWNLOAD - Query string completa: '{QueryString}'", Request.QueryString.Value);
            _logger.LogInformation("🔍 DEBUG DOWNLOAD - Parâmetros recebidos: {Parameters}", 
                string.Join(", ", Request.Query.Select(q => $"{q.Key}={q.Value}")));

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "pdfs", "manual_primeira_virada.pdf");
            
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Arquivo PDF não encontrado em {FilePath}", filePath);
                return NotFound("Arquivo não encontrado.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = "Manual_da_Primeira_Virada_Diogo_Costa.pdf";
            
            // Determinar origem do download
            var origem = "download_direto";
            var referer = Request.Headers["Referer"].ToString();
            
            if (!string.IsNullOrEmpty(referer))
            {
                if (referer.Contains("obrigado-desbloqueio"))
                    origem = "download_direto_obrigado";
                else if (referer.Contains("gmail.com") || referer.Contains("outlook.com") || referer.Contains("yahoo.com"))
                    origem = "email_link";
                else if (referer.Contains("diogocosta.dev"))
                    origem = "site_interno";
            }
            
            // Se o email não foi passado como parâmetro, tentar detectar origem
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("⚠️ PROBLEMA: Download sem email especificado. Referer: {Referer}", referer);
                origem = origem + "_sem_email";
            }
            else
            {
                _logger.LogInformation("✅ Email recebido para download: {Email}", email);
            }

            // CHAMAR O SERVIÇO COM LOG ANTES E DEPOIS
            _logger.LogInformation("🔄 Chamando PdfDownloadService.RegistrarDownloadAsync com email: '{Email}'", email ?? "NULL");
            
            var downloadRegistrado = await _pdfDownloadService.RegistrarDownloadAsync(
                fileName, 
                HttpContext, 
                origem, 
                email);
                
            _logger.LogInformation("✅ Download registrado no banco - ID: {Id}, Email gravado: '{EmailGravado}'", 
                downloadRegistrado.Id, downloadRegistrado.Email ?? "NULL");
            
            // Manter log tradicional também (backup)
            var userInfo = new
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Referer = referer,
                Email = email ?? "não_informado",
                Origem = origem,
                Timestamp = DateTime.UtcNow,
                FileName = fileName
            };
            
            _logger.LogInformation("📥 DOWNLOAD PDF REALIZADO: {UserInfo}", 
                System.Text.Json.JsonSerializer.Serialize(userInfo));
            
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 ERRO ao processar download do PDF");
            return StatusCode(500, "Erro interno do servidor.");
        }
    }

    // Endpoint para ver estatísticas de downloads do banco (apenas em desenvolvimento)
    [Route("desbloqueio/downloads-stats")]
    public async Task<IActionResult> DownloadStats()
    {
        try
        {
            // Verificar se estamos em desenvolvimento
            if (!_hostingEnvironment.IsDevelopment())
            {
                return NotFound(); // Esconder em produção por segurança
            }

            // Buscar estatísticas do banco de dados
            var stats = await _pdfDownloadService.ObterEstatisticasAsync();
            var downloadsRecentes = await _pdfDownloadService.ObterDownloadsRecentesAsync(20);

            return Json(new
            {
                Message = "📊 Estatísticas de Downloads de PDF - Banco de Dados",
                DataSource = "PostgreSQL Database",
                Estatisticas = new
                {
                    TotalDownloads = stats.TotalDownloads,
                    DownloadsHoje = stats.DownloadsHoje,
                    DownloadsUltimaSemana = stats.DownloadsUltimaSemana,
                    DownloadsUltimoMes = stats.DownloadsUltimoMes,
                    ArquivoMaisBaixado = stats.ArquivoMaisBaixado,
                    OrigemMaisComum = stats.OrigemMaisComum,
                    DispositivoMaisComum = stats.DispositivoMaisComum
                },
                DownloadsPorDia = stats.DownloadsPorDia.Take(30), // Últimos 30 dias
                DownloadsPorOrigem = stats.DownloadsPorOrigem,
                DownloadsRecentes = downloadsRecentes.Select(d => new
                {
                    Id = d.Id,
                    ArquivoNome = d.ArquivoNome,
                    Email = d.Email,
                    IpAddress = d.IpAddress,
                    Dispositivo = d.Dispositivo,
                    Navegador = d.Navegador,
                    SistemaOperacional = d.SistemaOperacional,
                    Origem = d.Origem,
                    CreatedAt = d.CreatedAt,
                    Sucesso = d.Sucesso,
                    LeadId = d.LeadId
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de downloads do banco");
            return Json(new { Error = "Erro interno", Message = ex.Message });
        }
    }

    // Endpoint backup para ver estatísticas de downloads dos logs (apenas em desenvolvimento)
    [Route("desbloqueio/downloads-stats-logs")]
    public IActionResult DownloadStatsLogs()
    {
        try
        {
            // Verificar se estamos em desenvolvimento
            if (!_hostingEnvironment.IsDevelopment())
            {
                return NotFound(); // Esconder em produção por segurança
            }

            var logsPath = Path.Combine(_hostingEnvironment.ContentRootPath, "logs");
            var downloads = new List<object>();

            if (Directory.Exists(logsPath))
            {
                var logFiles = Directory.GetFiles(logsPath, "app-*.txt");
                
                foreach (var logFile in logFiles.OrderByDescending(f => f))
                {
                    var lines = System.IO.File.ReadAllLines(logFile);
                    
                    foreach (var line in lines)
                    {
                        if (line.Contains("DOWNLOAD PDF REALIZADO"))
                        {
                            try
                            {
                                var jsonStart = line.IndexOf("{");
                                if (jsonStart > -1)
                                {
                                    var jsonPart = line.Substring(jsonStart);
                                    var downloadInfo = System.Text.Json.JsonSerializer.Deserialize<object>(jsonPart);
                                    downloads.Add(new
                                    {
                                        LogFile = Path.GetFileName(logFile),
                                        DateTime = line.Substring(0, 23), // Pegar timestamp do log
                                        Info = downloadInfo
                                    });
                                }
                            }
                            catch
                            {
                                // Ignorar erros de parsing
                            }
                        }
                    }
                }
            }

            return Json(new
            {
                Message = "📁 Estatísticas de Downloads de PDF - Arquivos de Log",
                DataSource = "Log Files",
                TotalDownloads = downloads.Count,
                Downloads = downloads.Take(50), // Últimos 50 downloads
                LogsPath = logsPath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de downloads dos logs");
            return Json(new { Error = "Erro interno" });
        }
    }
}
