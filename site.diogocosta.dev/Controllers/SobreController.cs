using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;

namespace site.diogocosta.dev.Controllers;


public class SobreController : Controller
{
    private readonly IMauticService _mauticService;

    public SobreController(IMauticService mauticService)
    {
        _mauticService = mauticService;
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
            var mauticContact = new MauticContact
            {
                Email = model.Email,
                FirstName = "",
                LastName = ""
            };

//            var (success, message) = await _mauticService.AdicionarContatoAsync(mauticContact);
            var (success, message) = await _mauticService.AdicionarContatoAsync(mauticContact, 1);

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