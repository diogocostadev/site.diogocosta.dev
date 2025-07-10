using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models;

public class DesbloqueioModel
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;
    
    // Honeypot fields - should always be empty
    public string? Website { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? EmailConfirm { get; set; } = string.Empty;
}
