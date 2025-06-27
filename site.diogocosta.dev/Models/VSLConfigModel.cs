using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace site.diogocosta.dev.Models
{
    public class VSLConfigModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Titulo { get; set; } = string.Empty;
        
        public string? Subtitulo { get; set; }
        public string? Descricao { get; set; }
        
        public int? VideoId { get; set; }
        public int? VideoIdTeste { get; set; }
        
        [Range(0, 999999.99)]
        public decimal? PrecoOriginal { get; set; }
        
        [Range(0, 999999.99)]
        public decimal? PrecoPromocional { get; set; }
        
        public string? CheckoutUrl { get; set; }
        public bool Ativo { get; set; } = true;
        
        [StringLength(20)]
        public string AmbienteAtivo { get; set; } = "producao"; // 'teste', 'producao'
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Propriedades navegacionais (populated from view)
        [NotMapped]
        public string? VideoUrlProducao { get; set; }
        
        [NotMapped]
        public string? VideoUrlTeste { get; set; }
        
        [NotMapped]
        public string? VideoUrlAtivo { get; set; }
        
        [NotMapped]
        public string? ThumbnailUrlProducao { get; set; }
        
        [NotMapped]
        public string? ThumbnailUrlTeste { get; set; }
        
        [NotMapped]
        public int? DuracaoProducao { get; set; }
        
        [NotMapped]
        public int? DuracaoTeste { get; set; }
    }

    public class VSLVideoModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;
        
        public string? Descricao { get; set; }
        
        [Required]
        public string VideoUrl { get; set; } = string.Empty;
        
        public string? ThumbnailUrl { get; set; }
        public int? DuracaoSegundos { get; set; }
        
        [StringLength(50)]
        public string Formato { get; set; } = "hls";
        
        [StringLength(50)]
        public string Qualidade { get; set; } = "1080p";
        
        public long? TamanhoBytes { get; set; }
        public bool Ativo { get; set; } = true;
        
        [StringLength(20)]
        public string Ambiente { get; set; } = "producao"; // 'teste', 'homologacao', 'producao'
        
        public string? Observacoes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 