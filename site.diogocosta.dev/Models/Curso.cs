using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models;

public class Curso
{
    public string Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public double Avaliacao { get; set; }
    public int TotalAlunos { get; set; }
    public List<string> Topicos { get; set; }
}