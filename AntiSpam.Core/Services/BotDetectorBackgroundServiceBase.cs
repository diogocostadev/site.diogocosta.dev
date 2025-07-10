using AntiSpam.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AntiSpam.Core.Services;

/// <summary>
/// Background service abstrato para detecção de bots
/// </summary>
public abstract class BotDetectorBackgroundServiceBase<TRule, TInteraction, TContext> : BackgroundService
    where TRule : class, IAntiSpamRule, new()
    where TInteraction : class, ILeadInteraction
    where TContext : DbContext
{
    protected readonly ILogger<BotDetectorBackgroundServiceBase<TRule, TInteraction, TContext>> _logger;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly BotDetectorConfig _config;

    // Cache em memória para contagem de tentativas
    protected readonly ConcurrentDictionary<string, int> _ipAttempts = new();
    protected readonly ConcurrentDictionary<string, int> _userAgentAttempts = new();
    protected readonly ConcurrentDictionary<string, int> _emailDomainAttempts = new();
    protected readonly ConcurrentDictionary<string, int> _suspiciousNameAttempts = new();
    protected readonly ConcurrentDictionary<string, DateTime> _lastSeen = new();

    protected abstract DbSet<TInteraction> GetInteractions(TContext context);
    protected abstract IAntiSpamService<TRule> GetAntiSpamService(IServiceProvider serviceProvider);

    protected BotDetectorBackgroundServiceBase(
        ILogger<BotDetectorBackgroundServiceBase<TRule, TInteraction, TContext>> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        // Carregar configurações
        _config = new BotDetectorConfig();
        configuration.GetSection("BotDetector").Bind(_config);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🤖 Bot Detector Background Service iniciado!");
        _logger.LogInformation("📊 Configurações: IP Threshold={IpThreshold}, UserAgent Threshold={UserAgentThreshold}, Análise a cada {AnalysisInterval}min", 
            _config.IpThreshold, _config.UserAgentThreshold, _config.AnalysisIntervalMinutes);

        var lastCleanup = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AnalyzeRecentLogsAsync();
                await AnalyzePatternsAndCreateRulesAsync();

                // Limpeza do cache
                if (DateTime.UtcNow - lastCleanup > TimeSpan.FromHours(_config.CacheCleanupHours))
                {
                    CleanupOldEntries();
                    lastCleanup = DateTime.UtcNow;
                }

                await Task.Delay(TimeSpan.FromMinutes(_config.AnalysisIntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no Bot Detector Background Service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("🛑 Bot Detector Background Service parado.");
    }

    protected virtual async Task AnalyzeRecentLogsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            var recentCutoff = DateTime.UtcNow.AddMinutes(-30);
            var interactions = GetInteractions(context);
            
            var recentInteractions = await interactions
                .Where(i => i.CreatedAt >= recentCutoff)
                .ToListAsync();

            foreach (var interaction in recentInteractions)
            {
                ProcessInteraction(interaction);
            }

            _logger.LogDebug("🔍 Analisadas {Count} interações recentes", recentInteractions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao analisar logs recentes");
        }
    }

    protected virtual void ProcessInteraction(TInteraction interaction)
    {
        try
        {
            var ipAddress = interaction.IpAddress?.ToString();
            var userAgent = interaction.UserAgent;
            
            // Extrair dados específicos (pode ser sobrescrito)
            var (email, name) = ExtractDataFromInteraction(interaction);

            // Contar tentativas por IP
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                _ipAttempts.AddOrUpdate(ipAddress, 1, (key, value) => value + 1);
                _lastSeen[ipAddress] = DateTime.UtcNow;
            }

            // Contar tentativas por User-Agent
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                _userAgentAttempts.AddOrUpdate(userAgent, 1, (key, value) => value + 1);
                _lastSeen[userAgent] = DateTime.UtcNow;
            }

            // Contar tentativas por domínio de email
            if (!string.IsNullOrWhiteSpace(email))
            {
                var domain = email.Split('@').LastOrDefault()?.ToLower();
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    _emailDomainAttempts.AddOrUpdate(domain, 1, (key, value) => value + 1);
                    _lastSeen[domain] = DateTime.UtcNow;
                }
            }

            // Analisar nomes suspeitos
            if (!string.IsNullOrWhiteSpace(name) && IsSuspiciousName(name))
            {
                _suspiciousNameAttempts.AddOrUpdate(name, 1, (key, value) => value + 1);
                _lastSeen[name] = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar interação {InteractionId}", interaction.Id);
        }
    }

    protected virtual (string? email, string? name) ExtractDataFromInteraction(TInteraction interaction)
    {
        // Implementação padrão para JsonDocument
        if (interaction.Dados is JsonDocument dados)
        {
            var email = dados.RootElement.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var name = dados.RootElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            return (email, name);
        }
        
        return (null, null);
    }

    protected virtual bool IsSuspiciousName(string name)
    {
        // Detectar texto em cirílico (russo)
        if (name.Any(c => c >= 0x0400 && c <= 0x04FF))
            return true;

        // Padrões específicos dos bots russos
        var russianPatterns = new[]
        {
            "Поздравляем", "Поздравялем", "выбраны для участия",
            "Wilberries", "Wilberies", "бесплатные попытки"
        };

        return russianPatterns.Any(pattern => name.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual async Task AnalyzePatternsAndCreateRulesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var antiSpamService = GetAntiSpamService(scope.ServiceProvider);

        try
        {
            var rulesAdded = 0;

            // Analisar IPs suspeitos
            foreach (var (ip, count) in _ipAttempts.Where(kvp => kvp.Value >= _config.IpThreshold))
            {
                if (await ShouldAddRule(context, AntiSpamRuleTypes.IP, ip))
                {
                    await antiSpamService.AddRuleAsync(
                        AntiSpamRuleTypes.IP,
                        ip,
                        $"IP com {count} tentativas suspeitas detectadas automaticamente",
                        SeverityLevels.High,
                        false,
                        "background_service"
                    );
                    rulesAdded++;
                    _logger.LogWarning("🚫 IP suspeito adicionado automaticamente: {IP} ({Count} tentativas)", ip, count);
                }
            }

            // Analisar User-Agents suspeitos
            foreach (var (userAgent, count) in _userAgentAttempts.Where(kvp => kvp.Value >= _config.UserAgentThreshold))
            {
                if (await ShouldAddRule(context, AntiSpamRuleTypes.UserAgent, userAgent))
                {
                    await antiSpamService.AddRuleAsync(
                        AntiSpamRuleTypes.UserAgent,
                        userAgent,
                        $"User-Agent com {count} tentativas suspeitas detectadas automaticamente",
                        SeverityLevels.Medium,
                        false,
                        "background_service"
                    );
                    rulesAdded++;
                    _logger.LogWarning("🤖 User-Agent suspeito adicionado automaticamente: {UserAgent} ({Count} tentativas)", userAgent, count);
                }
            }

            // Analisar domínios suspeitos
            foreach (var (domain, count) in _emailDomainAttempts.Where(kvp => kvp.Value >= _config.EmailDomainThreshold))
            {
                var suspiciousTlds = new[] { ".ru", ".tk", ".ml", ".ga", ".cf" };
                if (suspiciousTlds.Any(tld => domain.EndsWith(tld)) && await ShouldAddRule(context, AntiSpamRuleTypes.Domain, domain))
                {
                    await antiSpamService.AddRuleAsync(
                        AntiSpamRuleTypes.Domain,
                        domain,
                        $"Domínio suspeito com {count} tentativas detectadas automaticamente",
                        SeverityLevels.High,
                        false,
                        "background_service"
                    );
                    rulesAdded++;
                    _logger.LogWarning("📧 Domínio suspeito adicionado automaticamente: {Domain} ({Count} tentativas)", domain, count);
                }
            }

            // Analisar nomes suspeitos
            foreach (var (name, count) in _suspiciousNameAttempts.Where(kvp => kvp.Value >= _config.NameThreshold))
            {
                if (await ShouldAddRule(context, AntiSpamRuleTypes.NamePattern, name))
                {
                    var severity = name.Any(c => c >= 0x0400 && c <= 0x04FF) ? SeverityLevels.Critical : SeverityLevels.High;
                    
                    await antiSpamService.AddRuleAsync(
                        AntiSpamRuleTypes.NamePattern,
                        Regex.Escape(name),
                        $"Nome suspeito detectado {count} vezes automaticamente",
                        severity,
                        true,
                        "background_service"
                    );
                    rulesAdded++;
                    _logger.LogWarning("👤 Nome suspeito adicionado automaticamente: {Name} ({Count} tentativas)", name, count);
                }
            }

            if (rulesAdded > 0)
            {
                _logger.LogInformation("✅ Background Service adicionou {Count} novas regras anti-spam automaticamente", rulesAdded);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao analisar padrões e criar regras");
        }
    }

    protected abstract Task<bool> ShouldAddRule(TContext context, string ruleType, string ruleValue);

    protected virtual void CleanupOldEntries()
    {
        var cutoff = DateTime.UtcNow.AddHours(-48);
        var keysToRemove = _lastSeen
            .Where(kvp => kvp.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _lastSeen.TryRemove(key, out _);
            _ipAttempts.TryRemove(key, out _);
            _userAgentAttempts.TryRemove(key, out _);
            _emailDomainAttempts.TryRemove(key, out _);
            _suspiciousNameAttempts.TryRemove(key, out _);
        }

        if (keysToRemove.Any())
        {
            _logger.LogDebug("🧹 Limpeza do cache: removidas {Count} entradas antigas", keysToRemove.Count);
        }
    }
}
