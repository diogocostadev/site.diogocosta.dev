using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Dados;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;
using site.diogocosta.dev.ViewModels;

namespace site.diogocosta.dev.Controllers;

public class CursosController : Controller
{
    private readonly INewsletterService _newsletterService;
    public CursosController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }
    
    public IActionResult Index()
    {
        var viewModel = new TestimonialViewModel
        {
            Testimonials = TestimonialsData.GetTestimonials(),
            RatingSummary = new TestimonialViewModel.CourseRatingSummary
            {
                AverageRating = 4.8M,
                TotalStudents = 800,
                StudentAvatars = new List<string> { /* urls dos avatars */ }
            }
        };
    
        return View(viewModel);
    }
    
    private readonly List<Curso> _cursos = new List<Curso>
    {
        new Curso
        {
            Id = "saia-do-zero",
            Titulo = "Saia do Zero",
            Descricao = "Construa do zero uma página HTML e apresente-a para o mundo. Aprenda a criar domínio, contratar servidor, configurar segurança HTTPS e muito mais.",
            Preco = 0M, // Preço será definido no Hotmart
            Avaliacao = 5.0,
            TotalAlunos = 100,
            Topicos = new List<string>
            {
                "Criar uma página HTML completa do zero",
                "Registrar domínio e contratar servidor",
                "Configurar servidor e fazer deploy",
                "Implementar segurança HTTPS",
                "Sessão de dúvidas como bônus"
            }
        },
        new Curso
        {
            Id = "dc360-programador",
            Titulo = "DC360: Torne-se um Programador",
            Descricao = "Aprenda os fundamentos de programação com C# e desenvolva sua primeira aplicação ASP.NET do zero, sem experiência prévia, em apenas 4 semanas.",
            Preco = 0M, // Preço será definido no Hotmart
            Avaliacao = 5.0,
            TotalAlunos = 200,
            Topicos = new List<string>
            {
                "Fundamentos de programação com C#",
                "ASP.NET do zero, sem experiência prévia",
                "Aplicação funcional completa do zero",
                "Preparação para tecnologias avançadas",
                "Conceitos fundamentais de desenvolvimento"
            }
        },
        new Curso
        {
            Id = "comunidade-didaticos",
            Titulo = "Comunidade #Didáticos",
            Descricao = "A comunidade que apoia programadores que desejam se tornar empreendedores e conquistar clientes. Transforme programadores iniciantes nos desenvolvedores mais disputados no mercado de trabalho.",
            Preco = 0M, // Preço será definido no Hotmart
            Avaliacao = 4.0,
            TotalAlunos = 50,
            Topicos = new List<string>
            {
                "Comunidade ativa de programadores empreendedores",
                "Networking e troca de experiências",
                "Suporte para desenvolvedores de sistemas e aplicações",
                "Recursos e conteúdos exclusivos da comunidade",
                "Ambiente colaborativo de aprendizado"
            }
        },
        new Curso
        {
            Id = "mentoria-elite-backend",
            Titulo = "Mentoria Elite: Backend com .NET",
            Descricao = "Experiência personalizada voltada para desenvolvedores que desejam elevar suas habilidades no desenvolvimento backend utilizando o ecossistema .NET. Domine conceitos avançados como DDD, CQRS e Arquitetura Hexagonal.",
            Preco = 0M, // Preço será definido no Hotmart
            Avaliacao = 5.0,
            TotalAlunos = 25,
            Topicos = new List<string>
            {
                "Domain-Driven Design (DDD)",
                "CQRS - Command Query Responsibility Segregation",
                "Arquitetura Hexagonal e Clean Architecture",
                "Test-Driven Development (TDD)",
                "Segurança em APIs com JWT e OAuth",
                "DevOps e práticas modernas de deployment",
                "Encontros online exclusivos e acompanhamento contínuo"
            }
        }
    };

    public IActionResult DetalhesCurso(string id)
    {
        var curso = _cursos.FirstOrDefault(c => c.Id == id);
        if (curso == null)
            return NotFound();

        // Redirecionar para os links do Hotmart baseado no ID do curso
        return id switch
        {
            "saia-do-zero" => Redirect("https://hotmart.com/pt-br/marketplace/produtos/saia-do-zero-3/U51428792E"),
            "dc360-programador" => Redirect("https://go.hotmart.com/H98063764C"),
            "comunidade-didaticos" => Redirect("https://hotmart.com/pt-br/marketplace/produtos/comunidade-didatica/S45776492D"),
            "mentoria-elite-backend" => Redirect("https://hotmart.com/pt-br/marketplace/produtos/mentoria-elite-backend-com-net/O95796577O"),
            _ => NotFound()
        };
    }

    [HttpPost]
    public async Task<IActionResult> EntrarListaEspera(string cursoId, DetalheCursoViewModel viewModel)
    {
        // Limpar erros de validação relacionados a Curso
        foreach (var key in ModelState.Keys.ToList())
        {
            if (key.StartsWith("Curso."))
            {
                ModelState.Remove(key);
            }
        }
        
        // Remover validação do curso
        ModelState.Remove("Curso");

        var curso = _cursos.FirstOrDefault(c => c.Id == cursoId);
        if (curso == null)
        {
            TempData["Error"] = "Curso inválido.";
            return RedirectToAction("Index");
        }
        
        viewModel.Curso = curso;

        if (!ModelState.IsValid)
        {
            return View("DetalhesCurso", viewModel);
        }

        try
        {
            var usuarioNewsletter = new UsuarioNewsletter
            {
                Email = viewModel.ListaEspera.Email,
                Nome = viewModel.ListaEspera.Nome
            };

            if (await _newsletterService.CadastrarUsuarioAsync(usuarioNewsletter))
            {
                TempData["MensagemSucesso"] = "Seu cadastro na lista de espera foi criado com sucesso!";
                return RedirectToAction("DetalhesCurso", new { id = cursoId });
            }
            else
            {
                ModelState.AddModelError("", "Houve um erro ao realizar seu cadastro na lista de espera.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar lista de espera: {ex.Message}");
            ModelState.AddModelError("", "Ocorreu um erro interno ao processar sua inscrição. Por favor, tente novamente mais tarde.");
        }
        
        return View("DetalhesCurso", viewModel);
    }
}