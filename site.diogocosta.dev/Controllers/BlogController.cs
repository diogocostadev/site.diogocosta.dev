using Microsoft.AspNetCore.Mvc;
using Markdig;

namespace site.diogocosta.dev.Controllers;

public class BlogController : Controller
{
    public IActionResult Post(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var path = Path.Combine(Directory.GetCurrentDirectory(), "Blog", $"{slug}.md");

        if (!System.IO.File.Exists(path))
            return NotFound();

        var markdown = System.IO.File.ReadAllText(path);
        var html = Markdown.ToHtml(markdown);

        ViewData["Content"] = html;
        ViewData["Title"] = slug.Replace("-", " ").ToUpperInvariant();

        return View("Post");
    }
}
