using Microsoft.AspNetCore.Mvc;

namespace site.diogocosta.dev.Controllers;

public class ResultadosController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
