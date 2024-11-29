using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Models;

namespace site.diogocosta.dev.Controllers;


public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
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
    public IActionResult Newsletter(NewsletterSubscription subscription)
    {
        if (ModelState.IsValid)
        {
            // TODO: Implement newsletter subscription logic
            return RedirectToAction("Index", new { message = "Inscrição realizada com sucesso!" });
        }

        return View("Index");
    }
}