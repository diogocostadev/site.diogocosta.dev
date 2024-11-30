using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;

namespace site.diogocosta.dev.Controllers;


public class HomeController : Controller
{
    private readonly IMauticService _mauticService;
    public HomeController(IMauticService mauticService)
    {
        _mauticService = mauticService;
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
            var mauticContact = new MauticContact
            {
                Email = model.Email,
                FirstName = "",
                LastName = ""
            };

//            var (success, message) =  await _mauticService.AdicionarContatoAsync(mauticContact);
            var (success, message) =  await _mauticService.AdicionarContatoAsync(mauticContact, 1);

            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.";
        }

        return RedirectToAction("Index");
    }
    
    
    
}