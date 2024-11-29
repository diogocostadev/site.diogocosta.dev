using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models;

public class FormularioEsperaModel
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; }
}