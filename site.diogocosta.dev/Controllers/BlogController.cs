using Microsoft.AspNetCore.Mvc;
using Markdig;

namespace site.diogocosta.dev.Controllers;

public class BlogController : Controller
{
    public IActionResult Post(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Blog");
        var fullPath = Path.Combine(basePath, $"{slug}.md");

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var markdown = System.IO.File.ReadAllText(fullPath);
        var html = Markdown.ToHtml(markdown);

        ViewData["Content"] = html;
        ViewData["Title"] = slug.Replace("-", " ").ToUpperInvariant();

        return View("Post");
    }
}
