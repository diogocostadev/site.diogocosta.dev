using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Controllers;

public class DesbloqueioController : Controller
{
    private readonly INewsletterService _newsletterService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<DesbloqueioController> _logger;

    public DesbloqueioController(
        INewsletterService newsletterService, 
        IEmailService emailService,
        IWebHostEnvironment hostingEnvironment,
        ILogger<DesbloqueioController> logger)
    {
        _newsletterService = newsletterService;
        _emailService = emailService;
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
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
            // Cadastra na newsletter
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

            _logger.LogInformation("Usuário {Nome} ({Email}) se cadastrou para receber o Manual da Primeira Virada", 
                model.Nome, model.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar cadastro para {Email}", model.Email);
        }

        return RedirectToAction("Obrigado");
    }

    [Route("obrigado-desbloqueio")]
    public IActionResult Obrigado()
    {
        return View();
    }

    [Route("desbloqueio/download-pdf")]
    public IActionResult DownloadPdf()
    {
        try
        {
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "pdfs", "manual_primeira_virada.pdf");
            
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Arquivo PDF não encontrado em {FilePath}", filePath);
                return NotFound("Arquivo não encontrado.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = "Manual_da_Primeira_Virada_Diogo_Costa.pdf";
            
            _logger.LogInformation("Download do PDF realizado - arquivo: {FileName}", fileName);
            
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar download do PDF");
            return StatusCode(500, "Erro interno do servidor.");
        }
    }
}
