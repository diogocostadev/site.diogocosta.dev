using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Controllers;

public class SobreController : Controller
{
    private readonly INewsletterService _newsletterService;
    private readonly IAntiSpamService _antiSpamService;
    
    public SobreController(INewsletterService newsletterService, IAntiSpamService antiSpamService)
    {
        _newsletterService = newsletterService;
        _antiSpamService = antiSpamService;
    }
    
    public IActionResult Index()
    {
        return View(new NewsletterSubscription());
    }

    [HttpPost]
    public async Task<IActionResult> Newsletter(NewsletterSubscription model)
    {
        bool success = false;
        string message = "";

        // Verificação de honeypot (anti-bot protection)
        var honeypotName = Request.Form["name"].ToString();
        var honeypotEmailConfirm = Request.Form["email_confirm"].ToString();
        var honeypotWebsite = Request.Form["website"].ToString();
        
        if (!string.IsNullOrWhiteSpace(honeypotName) || 
            !string.IsNullOrWhiteSpace(honeypotEmailConfirm) || 
            !string.IsNullOrWhiteSpace(honeypotWebsite))
        {
            // Bot detectado - retorna erro genérico
            message = "Houve um erro ao processar sua inscrição. Tente novamente mais tarde.";
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = message });
            }
            
            TempData["Error"] = message;
            return RedirectToAction("Index");
        }
        
        if (!ModelState.IsValid)
        {
            message = "Por favor, insira um email válido.";
            
            // Verifica se é uma requisição AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = message });
            }
            
            TempData["Error"] = message;
            return RedirectToAction("Index");
        }

        // Verificação anti-spam
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();
        
        // Verificar IP blacklistado
        if (await _antiSpamService.IsBlacklistedIpAsync(clientIp))
        {
            message = "Houve um erro ao processar sua inscrição. Tente novamente mais tarde.";
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = message });
            }
            
            TempData["Error"] = message;
            return RedirectToAction("Index");
        }

        // Verificar email suspeito
        if (await _antiSpamService.IsSuspiciousEmailAsync(model.Email))
        {
            message = "Email inválido ou suspeito. Por favor, use um email válido.";
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = message });
            }
            
            TempData["Error"] = message;
            return RedirectToAction("Index");
        }

        // Verificar User-Agent suspeito
        if (await _antiSpamService.IsSuspiciousUserAgentAsync(userAgent))
        {
            message = "Houve um erro ao processar sua inscrição. Tente novamente mais tarde.";
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = message });
            }
            
            TempData["Error"] = message;
            return RedirectToAction("Index");
        }

        try
        {
            var usuarioNewsletter = new UsuarioNewsletter
            {
                Email = model.Email,
                Nome = ""
            };

            if (await _newsletterService.CadastrarUsuarioAsync(usuarioNewsletter))
            {
                success = true;
                message = "Seu cadastro foi realizado com sucesso!";
            }
            else
            {
                message = "Houve um erro ao realizar seu cadastro.";
            }
        }
        catch (Exception)
        {
            message = "Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.";
        }
        
        // Define a mensagem no TempData com base no resultado
        if (success)
        {
            TempData["Success"] = message;
        }
        else
        {
            TempData["Error"] = message;
        }
        
        // Verifica se é uma requisição AJAX
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = success, message = message });
        }
        
        return RedirectToAction("Index");
    }

}