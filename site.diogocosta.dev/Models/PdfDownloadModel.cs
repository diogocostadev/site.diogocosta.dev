using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace site.diogocosta.dev.Models
{
    [Table("pdf_downloads", Schema = "leads_system")]
    public class PdfDownloadModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("arquivo_nome")]
        [MaxLength(255)]
        public string ArquivoNome { get; set; } = string.Empty;

        [Column("email")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [Column("ip_address")]
        [MaxLength(45)] // IPv6 suporte
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [Column("referer")]
        [MaxLength(500)]
        public string? Referer { get; set; }

        [Column("origem")]
        [MaxLength(100)]
        public string? Origem { get; set; } // Ex: "download_direto_obrigado", "email_link", etc.

        [Column("pais")]
        [MaxLength(2)]
        public string? Pais { get; set; }

        [Column("cidade")]
        [MaxLength(100)]
        public string? Cidade { get; set; }

        [Column("dispositivo")]
        [MaxLength(50)]
        public string? Dispositivo { get; set; } // mobile, desktop, tablet

        [Column("navegador")]
        [MaxLength(50)]
        public string? Navegador { get; set; }

        [Column("sistema_operacional")]
        [MaxLength(50)]
        public string? SistemaOperacional { get; set; }

        [Column("sucesso")]
        public bool Sucesso { get; set; } = true;

        [Column("tamanho_arquivo")]
        public long? TamanhoArquivo { get; set; }

        [Column("tempo_download_ms")]
        public int? TempoDownloadMs { get; set; }

        [Column("dados_extra", TypeName = "jsonb")]
        public string? DadosExtra { get; set; } // JSON para dados adicionais

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("lead_id")]
        public int? LeadId { get; set; }

        // Relacionamento com Lead (opcional)
        [ForeignKey("LeadId")]
        public virtual LeadModel? Lead { get; set; }
    }

    // DTO para estatísticas de downloads
    public class PdfDownloadStats
    {
        public int TotalDownloads { get; set; }
        public int DownloadsHoje { get; set; }
        public int DownloadsUltimaSemana { get; set; }
        public int DownloadsUltimoMes { get; set; }
        public string? ArquivoMaisBaixado { get; set; }
        public string? OrigemMaisComum { get; set; }
        public string? DispositivoMaisComum { get; set; }
        public List<DownloadPorDia> DownloadsPorDia { get; set; } = new();
        public List<DownloadPorOrigem> DownloadsPorOrigem { get; set; } = new();
        public List<DownloadPorPais> DownloadsPorPais { get; set; } = new();
    }

    public class DownloadPorDia
    {
        public DateTime Data { get; set; }
        public int Quantidade { get; set; }
    }

    public class DownloadPorOrigem
    {
        public string Origem { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }

    public class DownloadPorPais
    {
        public string? Pais { get; set; }
        public int Quantidade { get; set; }
    }
} 