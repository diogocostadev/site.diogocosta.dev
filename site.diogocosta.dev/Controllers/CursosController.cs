using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Dados;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.ViewModels;

namespace site.diogocosta.dev.Controllers;


public class CursosController : Controller
{
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
    public IActionResult EntrarListaEspera(string cursoId, FormularioEsperaModel modelo)
    {
        if (!ModelState.IsValid)
        {
            var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);
            return View("DetalhesCurso", new DetalheCursoViewModel 
            { 
                Curso = curso, 
                ListaEspera = modelo 
            });
        }

        // Aqui você implementaria a lógica para salvar na lista de espera
        TempData["MensagemSucesso"] = "Inscrição realizada com sucesso! Você será notificado quando novas vagas forem abertas.";
        
        return RedirectToAction("DetalhesCurso", new { id = cursoId });
    }

}