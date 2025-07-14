using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace site.diogocosta.dev.Servicos;

/// <summary>
/// Implementação do serviço anti-spam usando a biblioteca AntiSpam.Core
/// </summary>
public class AntiSpamServiceCore : IAntiSpamService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AntiSpamServiceCore> _logger;

    public AntiSpamServiceCore(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<AntiSpamServiceCore> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> CheckAsync(string? email, string? userAgent, string? ipAddress, object? additionalData)
    {
        try
        {
            var activeRules = await GetActiveRulesFromCacheAsync();

            // Verificar IP
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var ipRules = activeRules.Where(r => r.RuleType == AntiSpamRuleTypes.IP);
                if (CheckRules(ipAddress, ipRules))
                {
                    _logger.LogWarning("IP {IpAddress} bloqueado por regra anti-spam", ipAddress);
                    return false;
                }
            }

            // Verificar email
            if (!string.IsNullOrEmpty(email))
            {
                var emailRules = activeRules.Where(r => r.RuleType == AntiSpamRuleTypes.Domain || r.RuleType == AntiSpamRuleTypes.EmailPattern);
                if (CheckRules(email, emailRules))
                {
                    _logger.LogWarning("Email {Email} bloqueado por regra anti-spam", email);
                    return false;
                }
            }

            // Verificar User Agent
            if (!string.IsNullOrEmpty(userAgent))
            {
                var userAgentRules = activeRules.Where(r => r.RuleType == AntiSpamRuleTypes.UserAgent);
                if (CheckRules(userAgent, userAgentRules))
                {
                    _logger.LogWarning("User Agent {UserAgent} bloqueado por regra anti-spam", userAgent);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar regras anti-spam");
            return true; // Em caso de erro, permita o acesso
        }
    }

    public async Task<bool> IsBlacklistedIpAsync(string ipAddress)
    {
        var rules = await GetActiveRulesFromCacheAsync();
        var ipRules = rules.Where(r => r.RuleType == AntiSpamRuleTypes.IP);
        return CheckRules(ipAddress, ipRules);
    }

    public async Task<bool> IsDisposableEmailAsync(string email)
    {
        var domain = email.Split('@').LastOrDefault();
        if (string.IsNullOrEmpty(domain)) return false;

        var rules = await GetActiveRulesFromCacheAsync();
        var domainRules = rules.Where(r => r.RuleType == AntiSpamRuleTypes.Domain);
        return CheckRules(domain, domainRules);
    }

    public async Task<bool> IsSuspiciousEmailAsync(string email)
    {
        var rules = await GetActiveRulesFromCacheAsync();
        var emailRules = rules.Where(r => r.RuleType == AntiSpamRuleTypes.EmailPattern);
        return CheckRules(email, emailRules);
    }

    public async Task<bool> IsSuspiciousUserAgentAsync(string userAgent)
    {
        var rules = await GetActiveRulesFromCacheAsync();
        var userAgentRules = rules.Where(r => r.RuleType == AntiSpamRuleTypes.UserAgent);
        return CheckRules(userAgent, userAgentRules);
    }

    public async Task<bool> IsSuspiciousNameAsync(string name)
    {
        var rules = await GetActiveRulesFromCacheAsync();
        var nameRules = rules.Where(r => r.RuleType == AntiSpamRuleTypes.NamePattern);
        return CheckRules(name, nameRules);
    }

    public async Task AddRuleAsync(string ruleType, string ruleValue, string description, string severity, bool isRegex, string? createdBy)
    {
        var rule = new AntiSpamRule
        {
            RuleType = ruleType,
            RuleValue = ruleValue,
            Description = description,
            Severity = severity,
            IsRegex = isRegex,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AntiSpamRules.Add(rule);
        await _context.SaveChangesAsync();

        // Limpar cache
        _cache.Remove("active_antispam_rules");

        _logger.LogInformation("Nova regra anti-spam criada: {RuleType} - {RuleValue}", ruleType, ruleValue);
    }

    public async Task<bool> UpdateRuleAsync(int ruleId, bool? isActive, string? description)
    {
        var rule = await _context.AntiSpamRules.FindAsync(ruleId);
        if (rule == null) return false;

        if (isActive.HasValue) rule.IsActive = isActive.Value;
        if (!string.IsNullOrEmpty(description)) rule.Description = description;
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Limpar cache
        _cache.Remove("active_antispam_rules");

        return true;
    }

    public async Task<bool> DeleteRuleAsync(int ruleId)
    {
        var rule = await _context.AntiSpamRules.FindAsync(ruleId);
        if (rule == null) return false;

        _context.AntiSpamRules.Remove(rule);
        await _context.SaveChangesAsync();

        // Limpar cache
        _cache.Remove("active_antispam_rules");

        return true;
    }

    public async Task<IEnumerable<AntiSpamRule>> GetActiveRulesAsync()
    {
        return await _context.AntiSpamRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.RuleType)
            .ThenBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task IncrementDetectionCountAsync(int ruleId)
    {
        var rule = await _context.AntiSpamRules.FindAsync(ruleId);
        if (rule != null)
        {
            rule.DetectionCount++;
            rule.LastDetection = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    // Métodos síncronos (implementações simples)
    public bool IsBlacklistedIp(string ipAddress) => IsBlacklistedIpAsync(ipAddress).Result;
    public bool IsDisposableEmail(string email) => IsDisposableEmailAsync(email).Result;
    public bool IsSuspiciousEmail(string email) => IsSuspiciousEmailAsync(email).Result;
    public bool IsSuspiciousUserAgent(string userAgent) => IsSuspiciousUserAgentAsync(userAgent).Result;
    public bool IsSuspiciousName(string name) => IsSuspiciousNameAsync(name).Result;
    public void AddToBlacklist(string type, string value) => AddRuleAsync(type, value, "Auto-generated rule", "medium", false, "system").Wait();

    private async Task<List<AntiSpamRule>> GetActiveRulesFromCacheAsync()
    {
        const string cacheKey = "active_antispam_rules";
        
        if (_cache.TryGetValue(cacheKey, out List<AntiSpamRule>? cachedRules) && cachedRules != null)
        {
            return cachedRules;
        }

        var rules = await _context.AntiSpamRules
            .Where(r => r.IsActive)
            .ToListAsync();

        _cache.Set(cacheKey, rules, TimeSpan.FromMinutes(10));
        return rules;
    }

    private bool CheckRules(string value, IEnumerable<AntiSpamRule> rules)
    {
        foreach (var rule in rules)
        {
            bool isMatch = rule.IsRegex 
                ? Regex.IsMatch(value, rule.RuleValue, RegexOptions.IgnoreCase)
                : value.Contains(rule.RuleValue, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                // Incrementar contador de detecção em background
                _ = Task.Run(async () => await IncrementDetectionCountAsync(rule.Id));
                return true;
            }
        }

        return false;
    }
}
