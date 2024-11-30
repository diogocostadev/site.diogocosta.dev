using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Dados;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.ViewModels;

namespace site.diogocosta.dev.Controllers;


public class CursosController : Controller
{
    private readonly IMauticService _mauticService;
    public CursosController(IMauticService mauticService)
    {
        _mauticService = mauticService;
    }
    
    public IActionResult Index()
    {
        var viewModel = new TestimonialViewModel
        {
            Testimonials = TestimonialsData.GetTestimonials(),
            RatingSummary = new TestimonialViewModel.CourseRatingSummary
            {
                AverageRating = 4.97M,
                TotalStudents = 30000,
                StudentAvatars = new List<string> { /* urls dos avatars */ }
            }
        };
    
        return View(viewModel);
    }
    
    private readonly List<Curso> _cursos = new List<Curso>
    {
        new Curso
        {
            Id = "001",
            Titulo = "A Jornada do Desenvolvedor Completo",
            Descricao = "Domine as habilidades essenciais para se tornar um desenvolvedor completo. Aprenda C#, crie aplicações robustas com banco de dados integrado, e implemente DevOps para garantir entregas ágeis e escaláveis.",
            Preco = 299.00M,
            Avaliacao = 4.96,
            TotalAlunos = 1000,
            Topicos = new List<string>
            {
                "C# e boas práticas de programação",
                "Integração com bancos de dados",
                "Pipelines de DevOps",
                "APIs e arquitetura de software"
            }
        },
        new Curso
        {
            Id = "002",
            Titulo = "Transforme suas habilidades em um negócio lucrativo",
            Descricao = "Descubra como transformar suas habilidades técnicas em um negócio rentável e escalável. Com um método prático e estratégico, você aprenderá a criar, validar e lançar suas próprias soluções no mercado.",
            Preco = 399.00M,
            Avaliacao = 4.98,
            TotalAlunos = 1000,
            Topicos = new List<string>
            {
                "Validação de ideias de negócio",
                "Criação de protótipos e MVPs",
                "Monetização de produtos digitais",
                "Escalabilidade de operações"
            }
        }
    };

    public IActionResult DetalhesCurso(string id)
    {
        var curso = _cursos.FirstOrDefault(c => c.Id == id);
        if (curso == null)
            return NotFound();

        var viewModel = new DetalheCursoViewModel
        {
            Curso = curso,
            ListaEspera = new FormularioEsperaModel()
        };
        return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> EntrarListaEspera(DetalheCursoViewModel viewModel)
    {
        foreach (var key in ModelState.Keys.ToList())
        {
            if (key.StartsWith("Curso."))
            {
                ModelState.Remove(key);
            }
        }

        if (!ModelState.IsValid)
        {
            var curso = _cursos.FirstOrDefault(c => c.Id == viewModel.Curso.Id);
            if (curso != null)
            {
                viewModel.Curso = curso;
            }
            return View("DetalhesCurso", viewModel);
        }
        
        if (!ModelState.IsValid)
        {
            // Recarrega os dados do curso
            var curso = _cursos.FirstOrDefault(c => c.Id == viewModel.Curso.Id);
            if (curso != null)
            {
                viewModel.Curso = curso;
            }

            return View("DetalhesCurso", viewModel);
        }

        try
        {
            var contact = new MauticContact
            {
                Email = viewModel.ListaEspera.Email,
                FirstName = viewModel.ListaEspera.Nome.Split(' ').FirstOrDefault() ?? "",
                LastName = string.Join(" ", viewModel.ListaEspera.Nome.Split(' ').Skip(1)) ?? ""
            };

            // Sempre adiciona à newsletter (segmento 1)
            var (success, message) = await _mauticService.AdicionarContatoAsync(contact, 1);

            // Se o primeiro foi bem sucedido, adiciona ao segmento específico do curso
            if (success)
            {
                if (viewModel.Curso.Id == "001") // Jornada do Desenvolvedor
                {
                    (success, message) = await _mauticService.AdicionarContatoAsync(contact, 2);
                }
                else if (viewModel.Curso.Id == "002") // Transforme
                {
                    (success, message) = await _mauticService.AdicionarContatoAsync(contact, 3);
                }
            }

            if (success)
            {
                TempData["MensagemSucesso"] = message;
                return RedirectToAction("DetalhesCurso", new { id = viewModel.Curso.Id });
            }

            ModelState.AddModelError("", message);
            viewModel.Curso = _cursos.FirstOrDefault(c => c.Id == viewModel.Curso.Id);
            return View("DetalhesCurso", viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("",
                "Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.");
            viewModel.Curso = _cursos.FirstOrDefault(c => c.Id == viewModel.Curso.Id);
            return View("DetalhesCurso", viewModel);
        }
    }
    
    
    // [HttpPost]
    // public async Task<IActionResult> EntrarListaEspera(string cursoId, FormularioEsperaModel modelo)
    // {
    //     // Validação do ModelState
    //     if (!ModelState.IsValid)
    //     {
    //         var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);
    //         return View("DetalhesCurso", new DetalheCursoViewModel
    //         {
    //             Curso = curso,
    //             ListaEspera = modelo
    //         });
    //     }
    //
    //     try
    //     {
    //         // Aqui você tinha razão, os dados estão vindo, vamos debugar
    //         Console.WriteLine($"Email recebido: {modelo.Email}");
    //         Console.WriteLine($"Nome recebido: {modelo.Nome}");
    //
    //         var contact = new MauticContact
    //         {
    //             Email = modelo.Email,
    //             FirstName = modelo.Nome.Split(' ').FirstOrDefault() ?? "",
    //             LastName = string.Join(" ", modelo.Nome.Split(' ').Skip(1)) ?? ""
    //         };
    //
    //         // Sempre adiciona à newsletter (segmento 1)
    //         var (success, message) = await _mauticService.AdicionarContatoAsync(contact, 1);
    //
    //         // Se o primeiro foi bem sucedido, adiciona ao segmento específico do curso
    //         if (success)
    //         {
    //             if (cursoId == "001") // Jornada do Desenvolvedor
    //             {
    //                 (success, message) = await _mauticService.AdicionarContatoAsync(contact, 2);
    //             }
    //             else if (cursoId == "002") // Transforme
    //             {
    //                 (success, message) = await _mauticService.AdicionarContatoAsync(contact, 3);
    //             }
    //         }
    //
    //         if (success)
    //         {
    //             TempData["MensagemSucesso"] = message;
    //         }
    //         else
    //         {
    //             ModelState.AddModelError("", message);
    //             var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);
    //             return View("DetalhesCurso", new DetalheCursoViewModel
    //             {
    //                 Curso = curso,
    //                 ListaEspera = modelo
    //             });
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log do erro
    //         Console.WriteLine($"Erro: {ex.Message}");
    //         ModelState.AddModelError("",
    //             "Ocorreu um erro ao processar sua inscrição. Por favor, tente novamente mais tarde.");
    //         var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);
    //         return View("DetalhesCurso", new DetalheCursoViewModel
    //         {
    //             Curso = curso,
    //             ListaEspera = modelo
    //         });
    //     }
    //
    //     return RedirectToAction("DetalhesCurso", new { id = cursoId });
    // }

   

}