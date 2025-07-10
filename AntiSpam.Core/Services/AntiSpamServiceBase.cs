using AntiSpam.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AntiSpam.Core.Services;

/// <summary>
/// Interface para o serviço anti-spam
/// </summary>
public interface IAntiSpamService<TRule> where TRule : class, IAntiSpamRule
{
    Task<bool> CheckAsync(string? email, string? nome, string? userAgent, IPAddress? ipAddress);
    Task<TRule> AddRuleAsync(string ruleType, string ruleValue, string description, string severity, bool isRegex, string? createdBy = null);
    Task<bool> UpdateRuleAsync(int id, bool? isActive = null, string? description = null);
    Task<bool> DeleteRuleAsync(int id);
    Task<IEnumerable<TRule>> GetActiveRulesAsync();
}

/// <summary>
/// Implementação base do serviço anti-spam
/// </summary>
public abstract class AntiSpamServiceBase<TRule, TContext> : IAntiSpamService<TRule>
    where TRule : class, IAntiSpamRule, new()
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly ILogger<AntiSpamServiceBase<TRule, TContext>> _logger;
    
    protected abstract DbSet<TRule> Rules { get; }

    protected AntiSpamServiceBase(TContext context, ILogger<AntiSpamServiceBase<TRule, TContext>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public virtual async Task<bool> CheckAsync(string? email, string? nome, string? userAgent, IPAddress? ipAddress)
    {
        try
        {
            var activeRules = await GetActiveRulesAsync();

            foreach (var rule in activeRules)
            {
                var isMatch = rule.RuleType switch
                {
                    AntiSpamRuleTypes.IP => CheckIpRule(rule, ipAddress),
                    AntiSpamRuleTypes.UserAgent => CheckUserAgentRule(rule, userAgent),
                    AntiSpamRuleTypes.Domain => CheckDomainRule(rule, email),
                    AntiSpamRuleTypes.EmailPattern => CheckEmailPatternRule(rule, email),
                    AntiSpamRuleTypes.NamePattern => CheckNamePatternRule(rule, nome),
                    _ => false
                };

                if (isMatch)
                {
                    await IncrementDetectionCountAsync(rule);
                    
                    _logger.LogWarning("🚫 Anti-spam rule triggered: {RuleType} = {RuleValue} - {Description}", 
                        rule.RuleType, rule.RuleValue, rule.Description);
                    
                    return true; // Spam detectado
                }
            }

            return false; // Não é spam
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao verificar regras anti-spam");
            return false; // Em caso de erro, não bloquear
        }
    }

    protected virtual bool CheckIpRule(TRule rule, IPAddress? ipAddress)
    {
        if (ipAddress == null) return false;
        
        var ipString = ipAddress.ToString();
        return rule.IsRegex 
            ? Regex.IsMatch(ipString, rule.RuleValue, RegexOptions.IgnoreCase)
            : ipString.Equals(rule.RuleValue, StringComparison.OrdinalIgnoreCase);
    }

    protected virtual bool CheckUserAgentRule(TRule rule, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return false;
        
        return rule.IsRegex
            ? Regex.IsMatch(userAgent, rule.RuleValue, RegexOptions.IgnoreCase)
            : userAgent.Contains(rule.RuleValue, StringComparison.OrdinalIgnoreCase);
    }

    protected virtual bool CheckDomainRule(TRule rule, string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        var domain = email.Split('@').LastOrDefault()?.ToLower();
        if (string.IsNullOrWhiteSpace(domain)) return false;
        
        return rule.IsRegex
            ? Regex.IsMatch(domain, rule.RuleValue, RegexOptions.IgnoreCase)
            : domain.Contains(rule.RuleValue, StringComparison.OrdinalIgnoreCase);
    }

    protected virtual bool CheckEmailPatternRule(TRule rule, string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        return rule.IsRegex
            ? Regex.IsMatch(email, rule.RuleValue, RegexOptions.IgnoreCase)
            : email.Contains(rule.RuleValue, StringComparison.OrdinalIgnoreCase);
    }

    protected virtual bool CheckNamePatternRule(TRule rule, string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) return false;
        
        return rule.IsRegex
            ? Regex.IsMatch(nome, rule.RuleValue, RegexOptions.IgnoreCase)
            : nome.Contains(rule.RuleValue, StringComparison.OrdinalIgnoreCase);
    }

    public virtual async Task<TRule> AddRuleAsync(string ruleType, string ruleValue, string description, string severity, bool isRegex, string? createdBy = null)
    {
        var rule = new TRule
        {
            RuleType = ruleType,
            RuleValue = ruleValue,
            Description = description,
            Severity = severity,
            IsRegex = isRegex,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            DetectionCount = 0
        };

        Rules.Add(rule);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Nova regra anti-spam criada: {RuleType} = {RuleValue}", ruleType, ruleValue);
        return rule;
    }

    public virtual async Task<bool> UpdateRuleAsync(int id, bool? isActive = null, string? description = null)
    {
        var rule = await Rules.FindAsync(id);
        if (rule == null) return false;

        if (isActive.HasValue) rule.IsActive = isActive.Value;
        if (description != null) rule.Description = description;
        rule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> DeleteRuleAsync(int id)
    {
        var rule = await Rules.FindAsync(id);
        if (rule == null) return false;

        Rules.Remove(rule);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<IEnumerable<TRule>> GetActiveRulesAsync()
    {
        return await Rules
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    protected virtual async Task IncrementDetectionCountAsync(TRule rule)
    {
        rule.DetectionCount++;
        rule.LastDetection = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
