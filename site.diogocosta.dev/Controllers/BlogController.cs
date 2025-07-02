using Microsoft.AspNetCore.Mvc;
using Markdig;

namespace site.diogocosta.dev.Controllers;

public class BlogPost
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string DateFormatted => Date.ToString("dd/MM/yyyy");
}

public class BlogIndexViewModel
{
    public List<BlogPost> Posts { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalPosts { get; set; }
    public int PostsPerPage { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class BlogController : Controller
{
    private const int POSTS_PER_PAGE = 6; // Quantidade de posts por página
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<BlogController> _logger;

    public BlogController(IWebHostEnvironment environment, ILogger<BlogController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public IActionResult Index(int page = 1)
    {
        var blogPath = Path.Combine(_environment.ContentRootPath, "Blog");
        
        _logger.LogInformation("Blog path: {BlogPath}", blogPath);
        _logger.LogInformation("Directory exists: {DirectoryExists}", Directory.Exists(blogPath));
        
        if (!Directory.Exists(blogPath))
        {
            _logger.LogWarning("Blog directory not found at: {BlogPath}", blogPath);
            
            // Tentar criar a pasta se ela não existir
            try 
            {
                Directory.CreateDirectory(blogPath);
                _logger.LogInformation("Created blog directory at: {BlogPath}", blogPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create blog directory at: {BlogPath}", blogPath);
                return View(new BlogIndexViewModel());
            }
            
            // Verificar novamente se agora existe
            if (!Directory.Exists(blogPath))
            {
                _logger.LogError("Blog directory still doesn't exist after creation attempt: {BlogPath}", blogPath);
                return View(new BlogIndexViewModel());
            }
        }

        var allPosts = new List<BlogPost>();
        var markdownFiles = Directory.GetFiles(blogPath, "*.md");
        
        _logger.LogInformation("Found {FileCount} markdown files in blog directory", markdownFiles.Length);

        foreach (var file in markdownFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var content = System.IO.File.ReadAllText(file);
            
            // Extrair título (primeira linha que começa com #)
            var lines = content.Split('\n');
            var title = lines.FirstOrDefault(l => l.StartsWith("# "))?.Substring(2).Trim() ?? fileName;
            
            // Extrair resumo (primeiras 2-3 linhas após o título)
            var contentLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Take(3);
            var excerpt = string.Join(" ", contentLines).Trim();
            if (excerpt.Length > 150)
                excerpt = excerpt.Substring(0, 150) + "...";

            // Extrair data do nome do arquivo (formato: 2024-06-12-titulo)
            var dateParts = fileName.Split('-');
            DateTime postDate;
            
            if (dateParts.Length >= 3 && 
                int.TryParse(dateParts[0], out int year) &&
                int.TryParse(dateParts[1], out int month) &&
                int.TryParse(dateParts[2], out int day))
            {
                try
                {
                    postDate = new DateTime(year, month, day);
                }
                catch
                {
                    postDate = System.IO.File.GetCreationTime(file);
                }
            }
            else
            {
                postDate = System.IO.File.GetCreationTime(file);
            }

                allPosts.Add(new BlogPost
                {
                    Slug = fileName,
                    Title = title,
                    Excerpt = excerpt,
                    Date = postDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing blog file: {FileName}", file);
                // Continue processing other files
            }
        }

        // Ordenar por data (mais recente primeiro)
        allPosts = allPosts.OrderByDescending(p => p.Date).ToList();

        // Calcular paginação
        var totalPosts = allPosts.Count;
        var totalPages = (int)Math.Ceiling((double)totalPosts / POSTS_PER_PAGE);
        
        // Garantir que a página está dentro dos limites
        page = Math.Max(1, Math.Min(page, totalPages));
        
        // Obter posts da página atual
        var postsForPage = allPosts
            .Skip((page - 1) * POSTS_PER_PAGE)
            .Take(POSTS_PER_PAGE)
            .ToList();

        var viewModel = new BlogIndexViewModel
        {
            Posts = postsForPage,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalPosts = totalPosts,
            PostsPerPage = POSTS_PER_PAGE
        };

        return View(viewModel);
    }

    public IActionResult Post(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var path = Path.Combine(_environment.ContentRootPath, "Blog", $"{slug}.md");

        if (!System.IO.File.Exists(path))
            return NotFound();

        var markdown = System.IO.File.ReadAllText(path);
        var html = Markdown.ToHtml(markdown);

        ViewData["Content"] = html;
        ViewData["Title"] = slug.Replace("-", " ").ToUpperInvariant();

        return View("Post");
    }

    // Action de diagnóstico - REMOVER EM PRODUÇÃO DEPOIS DE TESTAR
    [HttpGet("blog/debug")]
    public IActionResult Debug()
    {
        var blogPath = Path.Combine(_environment.ContentRootPath, "Blog");
        var info = new
        {
            BlogPath = blogPath,
            DirectoryExists = Directory.Exists(blogPath),
            ContentRootPath = _environment.ContentRootPath,
            WebRootPath = _environment.WebRootPath,
            Files = Directory.Exists(blogPath) ? Directory.GetFiles(blogPath, "*.md").Select(f => Path.GetFileName(f)).ToArray() : new string[0],
            AllDirectories = Directory.GetDirectories(_environment.ContentRootPath).Select(d => Path.GetFileName(d)).ToArray()
        };
        
        return Json(info);
    }
}
