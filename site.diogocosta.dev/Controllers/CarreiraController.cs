using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using site.diogocosta.dev.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting; // Para IWebHostEnvironment
using System.IO; // Para Path
using System.Linq; // Para OrderByDescending
using System.Collections.Generic; // Para List<T>
using System.Threading.Tasks; // Para Task<T>
using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Servicos.Interfaces;
using System; // Para Exception

namespace site.diogocosta.dev.Controllers
{
    public class CarreiraController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INewsletterService _newsletterService;

        public CarreiraController(IWebHostEnvironment hostingEnvironment, INewsletterService newsletterService)
        {
            _hostingEnvironment = hostingEnvironment;
            _newsletterService = newsletterService;
        }

        public async Task<IActionResult> Index()
        {
            var videos = await LoadVideosFromJsonAsync();
            var videosOrdenados = videos.OrderByDescending(v => v.DataPublicacao).ToList();
            return View(videosOrdenados);
        }

        [HttpPost]
        public async Task<IActionResult> Newsletter([FromBody] NewsletterSubscription model)
        {
            bool success = false;
            string message = "";
            
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

        private async Task<List<VideoCarreira>> LoadVideosFromJsonAsync()
        {
            // Constrói o caminho absoluto para o arquivo JSON dentro de wwwroot
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "dados", "videosCarreira.json");

            if (!System.IO.File.Exists(filePath))
            {
                // Retorna lista vazia ou lança exceção se o arquivo não for encontrado
                return new List<VideoCarreira>(); 
            }

            var jsonString = await System.IO.File.ReadAllTextAsync(filePath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Facilita a desserialização
            };

            var videos = JsonSerializer.Deserialize<List<VideoCarreira>>(jsonString, options);

            return videos ?? new List<VideoCarreira>(); // Retorna lista vazia se a desserialização falhar
        }
    }
} 