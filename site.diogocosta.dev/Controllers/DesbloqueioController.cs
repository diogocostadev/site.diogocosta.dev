using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Controllers;

public class DesbloqueioController : Controller
{
    private readonly INewsletterService _newsletterService;
    public DesbloqueioController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
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
            var usuario = new UsuarioNewsletter
            {
                Email = model.Email,
                Nome = model.Nome
            };

            await _newsletterService.CadastrarUsuarioAsync(usuario);
        }
        catch
        {
            // ignorar erros nesta fase
        }

        return RedirectToAction("Obrigado");
    }

    [Route("obrigado-desbloqueio")]
    public IActionResult Obrigado()
    {
        return View();
    }
}
