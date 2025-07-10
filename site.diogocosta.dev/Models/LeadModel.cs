using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json;
using AntiSpam.Core.Models;

namespace site.diogocosta.dev.Models
{
    [Table("leads", Schema = "leads_system")]
    public class LeadModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string Email { get; set; } = string.Empty;

        [ForeignKey("LeadSource")]
        public int? SourceId { get; set; }

        [Required]
        [StringLength(100)]
        public string DesafioSlug { get; set; } = string.Empty;

        public IPAddress? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? UtmSource { get; set; }
        public string? UtmMedium { get; set; }
        public string? UtmCampaign { get; set; }
        public string? UtmContent { get; set; }
        public string? UtmTerm { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "novo";

        public string[]? Tags { get; set; }
        public string? Notas { get; set; }
        public bool OptIn { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual LeadSourceModel? LeadSource { get; set; }
        public virtual ICollection<LeadInteractionModel> Interactions { get; set; } = new List<LeadInteractionModel>();
        public virtual ICollection<EmailLogModel> EmailLogs { get; set; } = new List<EmailLogModel>();
    }

    [Table("lead_sources", Schema = "leads_system")]
    public class LeadSourceModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<LeadModel> Leads { get; set; } = new List<LeadModel>();
    }

    [Table("lead_interactions", Schema = "leads_system")]
    public class LeadInteractionModel : ILeadInteraction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Lead")]
        public int LeadId { get; set; }

        [Required]
        [StringLength(100)]
        public string Tipo { get; set; } = string.Empty; // 'email_sent', 'email_opened', 'form_submitted', etc

        public string? Descricao { get; set; }
        public JsonDocument? Dados { get; set; } // JSON data
        public IPAddress? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual LeadModel Lead { get; set; } = null!;

        // Implementação da interface ILeadInteraction
        object? ILeadInteraction.IpAddress 
        { 
            get => IpAddress; 
            set => IpAddress = value as IPAddress; 
        }
        
        object? ILeadInteraction.Dados 
        { 
            get => Dados; 
            set => Dados = value as JsonDocument; 
        }
    }

    [Table("email_templates", Schema = "leads_system")]
    public class EmailTemplateModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Assunto { get; set; } = string.Empty;

        [Required]
        public string CorpoHtml { get; set; } = string.Empty;

        public string? CorpoTexto { get; set; }
        public string[]? Variaveis { get; set; }
        public bool Ativo { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<EmailLogModel> EmailLogs { get; set; } = new List<EmailLogModel>();
    }

    [Table("email_logs", Schema = "leads_system")]
    public class EmailLogModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Lead")]
        public int? LeadId { get; set; }

        [ForeignKey("EmailTemplate")]
        public int? TemplateId { get; set; }

        [Required]
        [StringLength(320)]
        public string EmailTo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Assunto { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // 'enviado', 'falhou', 'bounce', 'spam'

        public string? ErroMsg { get; set; }

        public DateTime EnviadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AbertoEm { get; set; }
        public DateTime? ClicadoEm { get; set; }
        public DateTime? BounceEm { get; set; }
        public DateTime? SpamEm { get; set; }

        // Navigation properties
        public virtual LeadModel? Lead { get; set; }
        public virtual EmailTemplateModel? EmailTemplate { get; set; }
    }

    // DTOs para requests
    public class CriarLeadRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(320, ErrorMessage = "Email deve ter no máximo 320 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Desafio é obrigatório")]
        public string DesafioSlug { get; set; } = string.Empty;

        public string? UtmSource { get; set; }
        public string? UtmMedium { get; set; }
        public string? UtmCampaign { get; set; }
        public string? UtmContent { get; set; }
        public string? UtmTerm { get; set; }
    }

    public class LeadStats
    {
        public string DesafioSlug { get; set; } = string.Empty;
        public string FonteNome { get; set; } = string.Empty;
        public int TotalLeads { get; set; }
        public int OptInLeads { get; set; }
        public int LeadsNovos { get; set; }
        public int LeadsQualificados { get; set; }
        public int LeadsHoje { get; set; }
        public int LeadsUltimaSemana { get; set; }
    }
} 