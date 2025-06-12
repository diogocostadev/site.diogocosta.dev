using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models;

public class Curso
{
    public string Id { get; set; } = default!;
    public string Titulo { get; set; } = default!;
    public string Descricao { get; set; } = default!;
    public decimal Preco { get; set; }
    public double Avaliacao { get; set; }
    public int TotalAlunos { get; set; }
    public List<string> Topicos { get; set; } = new List<string>();
}