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
    public SobreController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }
    
    public IActionResult Index()
    {
        return View(new NewsletterSubscription());
    }

    [HttpPost]
    public async Task<IActionResult> Newsletter(NewsletterSubscription model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Por favor, insira um email válido.";
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
                TempData["Success"] = "Seu cadastro foi realizado com sucesso!";
            }
            else
            {
                TempData["Error"] = "Houve um erro ao realizar seu cadastrar.";
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.";
        }

        return RedirectToAction("Index");
    }

}