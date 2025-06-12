using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models;

public class NewsletterSubscription
{
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = default!;
}