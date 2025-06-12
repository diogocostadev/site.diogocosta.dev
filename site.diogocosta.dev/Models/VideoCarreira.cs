using System;
using System.ComponentModel.DataAnnotations;

namespace site.diogocosta.dev.Models
{
    public class VideoCarreira
    {
        public int Id { get; set; } // Pode ser útil futuramente

        [Required(ErrorMessage = "O título do vídeo é obrigatório.")]
        [StringLength(200, ErrorMessage = "O título não pode exceder 200 caracteres.")]
        public string Titulo { get; set; } = default!;

        [StringLength(1000, ErrorMessage = "A descrição não pode exceder 1000 caracteres.")]
        public string Descricao { get; set; } = default!;

        [Required(ErrorMessage = "A URL do vídeo é obrigatória.")]
        [Url(ErrorMessage = "Por favor, insira uma URL válida.")]
        [StringLength(500, ErrorMessage = "A URL não pode exceder 500 caracteres.")]
        public string UrlVideo { get; set; } = default!; // URL do embed do YouTube ou link direto

        [Required(ErrorMessage = "A data de publicação é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime DataPublicacao { get; set; }
    }
} 