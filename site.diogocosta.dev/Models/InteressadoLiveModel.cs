using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace site.diogocosta.dev.Models
{
    [Table("interessados_lives", Schema = "leads_system")]
    public class InteressadoLiveModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(320, ErrorMessage = "Email deve ter no máximo 320 caracteres")]
        [Column("email")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "WhatsApp deve ter no máximo 20 caracteres")]
        [Column("whatsapp")]
        public string? WhatsApp { get; set; }

        [StringLength(5, ErrorMessage = "Código do país deve ter no máximo 5 caracteres")]
        [Column("codigo_pais")]
        public string CodigoPais { get; set; } = "+55"; // Brasil por padrão

        [Column("data_cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        [Column("data_descadastro")]
        public DateTime? DataDescadastro { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; } = true;

        [Column("ip_address")]
        [StringLength(45)]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Column("origem")]
        [StringLength(50)]
        public string? Origem { get; set; } // youtube, twitch, site

        [Column("boas_vindas_email_enviado")]
        public bool BoasVindasEmailEnviado { get; set; } = false;

        [Column("boas_vindas_whatsapp_enviado")]
        public bool BoasVindasWhatsAppEnviado { get; set; } = false;

        [Column("data_boas_vindas_email")]
        public DateTime? DataBoasVindasEmail { get; set; }

        [Column("data_boas_vindas_whatsapp")]
        public DateTime? DataBoasVindasWhatsApp { get; set; }

        // Propriedades não mapeadas para validação
        [NotMapped]
        public bool TemContatoValido => !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(WhatsApp);

        [NotMapped]
        public string WhatsAppCompleto => !string.IsNullOrWhiteSpace(WhatsApp) ? $"{CodigoPais}{WhatsApp}" : string.Empty;

        [NotMapped]
        public string WhatsAppFormatado => !string.IsNullOrWhiteSpace(WhatsApp) ? $"{CodigoPais} {WhatsApp}" : string.Empty;
    }

    // DTO para cadastro
    public class CadastroInteressadoLiveRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(320, ErrorMessage = "Email deve ter no máximo 320 caracteres")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "WhatsApp deve ter no máximo 20 caracteres")]
        public string? WhatsApp { get; set; }

        [StringLength(5, ErrorMessage = "Código do país deve ter no máximo 5 caracteres")]
        public string CodigoPais { get; set; } = "+55";

        public string? Origem { get; set; }

        // Validação customizada
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Nome))
            {
                errorMessage = "Nome é obrigatório";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(WhatsApp))
            {
                errorMessage = "É necessário informar pelo menos um contato: email ou WhatsApp";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
            {
                errorMessage = "Email deve ter um formato válido";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(WhatsApp) && !IsValidWhatsApp(WhatsApp))
            {
                errorMessage = "WhatsApp deve conter apenas números (sem espaços ou caracteres especiais)";
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidWhatsApp(string whatsapp)
        {
            // Aceita apenas números, sem espaços ou caracteres especiais
            // Mínimo 8 dígitos, máximo 15 dígitos
            if (string.IsNullOrWhiteSpace(whatsapp))
                return false;

            // Remove espaços e caracteres especiais para validação
            var cleanNumber = System.Text.RegularExpressions.Regex.Replace(whatsapp, @"[^\d]", "");
            
            return cleanNumber.Length >= 8 && cleanNumber.Length <= 15;
        }
    }

    // DTO para descadastro
    public class DescadastroInteressadoLiveRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;
    }

    // DTO para estatísticas
    public class EstatisticasInteressadosLives
    {
        public int TotalInteressados { get; set; }
        public int TotalAtivos { get; set; }
        public int TotalDesativados { get; set; }
        public int ComEmail { get; set; }
        public int ComWhatsApp { get; set; }
        public int ComAmbos { get; set; }
        public int CadastrosHoje { get; set; }
        public int CadastrosUltimaSemana { get; set; }
        public int CadastrosUltimoMes { get; set; }
        public List<CadastrosPorDia> CadastrosPorDia { get; set; } = new();
        public List<CadastrosPorOrigem> CadastrosPorOrigem { get; set; } = new();
    }

    public class CadastrosPorDia
    {
        public DateTime Data { get; set; }
        public int Quantidade { get; set; }
    }

    public class CadastrosPorOrigem
    {
        public string Origem { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }

    // Países e códigos mais comuns
    public static class PaisesWhatsApp
    {
        public static readonly Dictionary<string, string> CodigosPaises = new()
        {
            { "+55", "🇧🇷 Brasil" },
            { "+1", "🇺🇸 Estados Unidos" },
            { "+54", "🇦🇷 Argentina" },
            { "+56", "🇨🇱 Chile" },
            { "+57", "🇨🇴 Colômbia" },
            { "+58", "🇻🇪 Venezuela" },
            { "+51", "🇵🇪 Peru" },
            { "+593", "🇪🇨 Equador" },
            { "+595", "🇵🇾 Paraguai" },
            { "+598", "🇺🇾 Uruguai" },
            { "+591", "🇧🇴 Bolívia" },
            { "+34", "🇪🇸 Espanha" },
            { "+351", "🇵🇹 Portugal" },
            { "+33", "🇫🇷 França" },
            { "+49", "🇩🇪 Alemanha" },
            { "+39", "🇮🇹 Itália" },
            { "+44", "🇬🇧 Reino Unido" },
            { "+81", "🇯🇵 Japão" },
            { "+86", "🇨🇳 China" },
            { "+91", "🇮🇳 Índia" },
            { "+61", "🇦🇺 Austrália" },
            { "+27", "🇿🇦 África do Sul" },
            { "+52", "🇲🇽 México" },
            { "+506", "🇨🇷 Costa Rica" },
            { "+507", "🇵🇦 Panamá" },
            { "+503", "🇸🇻 El Salvador" },
            { "+502", "🇬🇹 Guatemala" },
            { "+504", "🇭🇳 Honduras" },
            { "+505", "🇳🇮 Nicarágua" }
        };
    }
} 