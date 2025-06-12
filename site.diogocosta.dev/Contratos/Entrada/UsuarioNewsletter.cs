using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Contratos.Entrada;

public class UsuarioNewsletter
{
    public string? Nome { get; set; }
    
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;
}