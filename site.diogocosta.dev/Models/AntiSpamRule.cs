using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AntiSpam.Core.Models;

namespace site.diogocosta.dev.Models
{
    [Table("antispam_rules")]
    public class AntiSpamRule : IAntiSpamRule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("rule_type")]
        public string RuleType { get; set; } = string.Empty;

        [Required]
        [Column("rule_value")]
        public string RuleValue { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("severity")]
        public string Severity { get; set; } = "medium";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_regex")]
        public bool IsRegex { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("detection_count")]
        public int DetectionCount { get; set; } = 0;

        [Column("last_detection")]
        public DateTime? LastDetection { get; set; }
    }

    // Enum para tipos de regras
    public static class AntiSpamRuleTypes
    {
        public const string IP = "ip";
        public const string Domain = "domain";
        public const string EmailPattern = "email_pattern";
        public const string NamePattern = "name_pattern";
        public const string UserAgent = "user_agent";
    }

    // Enum para severidade
    public static class AntiSpamSeverity
    {
        public const string Low = "low";
        public const string Medium = "medium";
        public const string High = "high";
        public const string Critical = "critical";
    }

    // DTOs para API
    public class CreateAntiSpamRuleRequest
    {
        [Required]
        [MaxLength(50)]
        public string RuleType { get; set; } = string.Empty;

        [Required]
        public string RuleValue { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(20)]
        public string Severity { get; set; } = AntiSpamSeverity.Medium;

        public bool IsActive { get; set; } = true;

        public bool IsRegex { get; set; } = false;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }

    public class UpdateAntiSpamRuleRequest
    {
        public string? RuleValue { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsRegex { get; set; }
    }

    public class AntiSpamRuleResponse
    {
        public int Id { get; set; }
        public string RuleType { get; set; } = string.Empty;
        public string RuleValue { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsRegex { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public int DetectionCount { get; set; }
        public DateTime? LastDetection { get; set; }
    }
}
