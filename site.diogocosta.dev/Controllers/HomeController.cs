using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Controllers;

public class HomeController : Controller
{
    private readonly INewsletterService _newsletterService;
    public HomeController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }
    public IActionResult Index()
    {
        return View(new NewsletterSubscription());
    }
    
    public IActionResult Cursos()
    {
        return View();
    }

    public IActionResult Redes()
    {
        return View();
    }

    public IActionResult Termo()
    {
        return View();
    }
    
    public IActionResult Privacy()
    {
        return View();
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