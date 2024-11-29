using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Models;

namespace site.diogocosta.dev.Controllers;


public class SobreController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}