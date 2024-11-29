using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models;

public class Testimonial
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(1000)]
    public string Content { get; set; }

    [Required]
    [StringLength(200)]
    public string ImageUrl { get; set; }

    // Opcional - pode ser útil para ordenação/filtro
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Propriedades adicionais que podem ser úteis
    public string? Role { get; set; }
    public string? Company { get; set; }
    public int Rating { get; set; } = 5; // Padrão 5 estrelas
}