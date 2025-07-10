using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace site.diogocosta.dev.Servicos
{
    public class BotDetectorBackgroundService : BackgroundService
    {
        private readonly ILogger<BotDetectorBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        
        // Cache em memória para contagem de tentativas
        private readonly ConcurrentDictionary<string, int> _ipAttempts = new();
        private readonly ConcurrentDictionary<string, int> _userAgentAttempts = new();
        private readonly ConcurrentDictionary<string, int> _emailDomainAttempts = new();
        private readonly ConcurrentDictionary<string, int> _suspiciousNameAttempts = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastSeen = new();
        
        // Configurações (com valores padrão)
        private readonly int _ipThreshold;
        private readonly int _userAgentThreshold;
        private readonly int _emailDomainThreshold;
        private readonly int _nameThreshold;
        private readonly TimeSpan _analysisInterval;
        private readonly TimeSpan _cacheCleanupInterval;

        public BotDetectorBackgroundService(
            ILogger<BotDetectorBackgroundService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            // Carregar configurações (com fallback para valores padrão)
            _ipThreshold = _configuration.GetValue("BotDetector:IpThreshold", 10);
            _userAgentThreshold = _configuration.GetValue("BotDetector:UserAgentThreshold", 15);
            _emailDomainThreshold = _configuration.GetValue("BotDetector:EmailDomainThreshold", 5);
            _nameThreshold = _configuration.GetValue("BotDetector:NameThreshold", 3);
            _analysisInterval = TimeSpan.FromMinutes(_configuration.GetValue("BotDetector:AnalysisIntervalMinutes", 5));
            _cacheCleanupInterval = TimeSpan.FromHours(_configuration.GetValue("BotDetector:CacheCleanupHours", 24));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🤖 Bot Detector Background Service iniciado!");
            _logger.LogInformation("📊 Configurações: IP Threshold={IpThreshold}, UserAgent Threshold={UserAgentThreshold}, Análise a cada {AnalysisInterval}", 
                _ipThreshold, _userAgentThreshold, _analysisInterval);

            var lastCleanup = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Analisar logs e detectar padrões
                    await AnalyzeRecentLogsAsync();

                    // Analisar padrões acumulados e criar regras
                    await AnalyzePatternsAndCreateRulesAsync();

                    // Limpeza do cache (a cada 24 horas por padrão)
                    if (DateTime.UtcNow - lastCleanup > _cacheCleanupInterval)
                    {
                        CleanupOldEntries();
                        lastCleanup = DateTime.UtcNow;
                    }

                    // Aguardar próxima análise
                    await Task.Delay(_analysisInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Serviço sendo parado - normal
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro no Bot Detector Background Service");
                    
                    // Aguardar um pouco antes de tentar novamente em caso de erro
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("🛑 Bot Detector Background Service parado.");
        }

        private async Task AnalyzeRecentLogsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Analisar interações recentes (últimos 30 minutos)
                var recentCutoff = DateTime.UtcNow.AddMinutes(-30);
                
                var recentInteractions = await context.LeadInteractions
                    .Where(i => i.CreatedAt >= recentCutoff)
                    .ToListAsync();

                foreach (var interaction in recentInteractions)
                {
                    await ProcessInteractionAsync(interaction);
                }

                _logger.LogDebug("🔍 Analisadas {Count} interações recentes", recentInteractions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao analisar logs recentes");
            }
        }

        private Task ProcessInteractionAsync(LeadInteractionModel interaction)
        {
            try
            {
                var ipAddress = interaction.IpAddress?.ToString();
                var dados = interaction.Dados?.RootElement;

                if (dados.HasValue)
                {
                    // Extrair dados do JSON
                    var userAgent = dados.Value.TryGetProperty("userAgent", out var uaProp) ? uaProp.GetString() : null;
                    var email = dados.Value.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                    var name = dados.Value.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

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
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (IsSuspiciousName(name))
                        {
                            _suspiciousNameAttempts.AddOrUpdate(name, 1, (key, value) => value + 1);
                            _lastSeen[name] = DateTime.UtcNow;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro ao processar interação {InteractionId}", interaction.Id);
            }

            return Task.CompletedTask;
        }

        private bool IsSuspiciousName(string name)
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

        private async Task AnalyzePatternsAndCreateRulesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var antiSpamService = scope.ServiceProvider.GetRequiredService<IAntiSpamService>();

            try
            {
                var rulesAdded = 0;

                // Analisar IPs suspeitos
                foreach (var (ip, count) in _ipAttempts.Where(kvp => kvp.Value >= _ipThreshold))
                {
                    if (await ShouldAddRule(context, AntiSpamRuleTypes.IP, ip))
                    {
                        await antiSpamService.AddRuleAsync(
                            AntiSpamRuleTypes.IP,
                            ip,
                            $"IP com {count} tentativas suspeitas detectadas automaticamente pelo Background Service",
                            "high",
                            false,
                            "background_service"
                        );
                        rulesAdded++;
                        _logger.LogWarning("🚫 IP suspeito adicionado automaticamente: {IP} ({Count} tentativas)", ip, count);
                    }
                }

                // Analisar User-Agents suspeitos
                foreach (var (userAgent, count) in _userAgentAttempts.Where(kvp => kvp.Value >= _userAgentThreshold))
                {
                    if (await ShouldAddRule(context, AntiSpamRuleTypes.UserAgent, userAgent))
                    {
                        await antiSpamService.AddRuleAsync(
                            AntiSpamRuleTypes.UserAgent,
                            userAgent,
                            $"User-Agent com {count} tentativas suspeitas detectadas automaticamente pelo Background Service",
                            "medium",
                            false,
                            "background_service"
                        );
                        rulesAdded++;
                        _logger.LogWarning("🤖 User-Agent suspeito adicionado automaticamente: {UserAgent} ({Count} tentativas)", userAgent, count);
                    }
                }

                // Analisar domínios de email suspeitos
                foreach (var (domain, count) in _emailDomainAttempts.Where(kvp => kvp.Value >= _emailDomainThreshold))
                {
                    // Verificar se é domínio suspeito (.ru, .tk, etc)
                    var suspiciousTlds = new[] { ".ru", ".tk", ".ml", ".ga", ".cf" };
                    if (suspiciousTlds.Any(tld => domain.EndsWith(tld)) && await ShouldAddRule(context, AntiSpamRuleTypes.Domain, domain))
                    {
                        await antiSpamService.AddRuleAsync(
                            AntiSpamRuleTypes.Domain,
                            domain,
                            $"Domínio suspeito com {count} tentativas detectadas automaticamente pelo Background Service",
                            "high",
                            false,
                            "background_service"
                        );
                        rulesAdded++;
                        _logger.LogWarning("📧 Domínio suspeito adicionado automaticamente: {Domain} ({Count} tentativas)", domain, count);
                    }
                }

                // Analisar nomes suspeitos
                foreach (var (name, count) in _suspiciousNameAttempts.Where(kvp => kvp.Value >= _nameThreshold))
                {
                    if (await ShouldAddRule(context, AntiSpamRuleTypes.NamePattern, name))
                    {
                        var severity = name.Any(c => c >= 0x0400 && c <= 0x04FF) ? "critical" : "high"; // Cirílico = crítico
                        
                        await antiSpamService.AddRuleAsync(
                            AntiSpamRuleTypes.NamePattern,
                            Regex.Escape(name), // Escape para usar como regex literal
                            $"Nome suspeito detectado {count} vezes automaticamente pelo Background Service",
                            severity,
                            true, // É regex (escaped)
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

        private async Task<bool> ShouldAddRule(ApplicationDbContext context, string ruleType, string ruleValue)
        {
            // Verificar se a regra já existe
            return !await context.AntiSpamRules
                .AnyAsync(r => r.RuleType == ruleType && r.RuleValue == ruleValue);
        }

        private void CleanupOldEntries()
        {
            var cutoff = DateTime.UtcNow.AddHours(-48); // Remover entradas mais antigas que 48 horas
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 Parando Bot Detector Background Service...");
            await base.StopAsync(cancellationToken);
        }
    }
}
