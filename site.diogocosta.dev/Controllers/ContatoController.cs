using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Dados;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.ViewModels;

namespace site.diogocosta.dev.Controllers;


public class ContatoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}