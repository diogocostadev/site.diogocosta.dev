namespace AntiSpam.Core.Models;

/// <summary>
/// Interface base para regras anti-spam
/// </summary>
public interface IAntiSpamRule
{
    int Id { get; set; }
    string RuleType { get; set; }
    string RuleValue { get; set; }
    string Description { get; set; }
    string Severity { get; set; }
    bool IsActive { get; set; }
    bool IsRegex { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    int DetectionCount { get; set; }
    DateTime? LastDetection { get; set; }
}

/// <summary>
/// Interface base para interações de leads/usuários
/// </summary>
public interface ILeadInteraction
{
    int Id { get; set; }
    DateTime CreatedAt { get; set; }
    object? IpAddress { get; set; }
    string? UserAgent { get; set; }
    object? Dados { get; set; }
}

/// <summary>
/// Tipos de regras anti-spam
/// </summary>
public static class AntiSpamRuleTypes
{
    public const string IP = "ip";
    public const string UserAgent = "user_agent";
    public const string Domain = "domain";
    public const string EmailPattern = "email_pattern";
    public const string NamePattern = "name_pattern";
}

/// <summary>
/// Níveis de severidade
/// </summary>
public static class SeverityLevels
{
    public const string Low = "low";
    public const string Medium = "medium";
    public const string High = "high";
    public const string Critical = "critical";
}

/// <summary>
/// Configurações do detector de bots
/// </summary>
public class BotDetectorConfig
{
    public int IpThreshold { get; set; } = 10;
    public int UserAgentThreshold { get; set; } = 15;
    public int EmailDomainThreshold { get; set; } = 5;
    public int NameThreshold { get; set; } = 3;
    public int AnalysisIntervalMinutes { get; set; } = 5;
    public int CacheCleanupHours { get; set; } = 24;
}

/// <summary>
/// Configurações de rate limiting
/// </summary>
public class RateLimitConfig
{
    public int RequestsPerMinute { get; set; } = 5;
    public int BurstSize { get; set; } = 10;
    public int WindowSizeMinutes { get; set; } = 1;
}
