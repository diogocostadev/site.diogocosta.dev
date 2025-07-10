using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using System.Text.RegularExpressions;

namespace site.diogocosta.dev.Servicos
{
    public interface IAntiSpamService
    {
        bool IsBlacklistedIp(string ipAddress);
        bool IsDisposableEmail(string email);
        bool IsSuspiciousEmail(string email);
        bool IsSuspiciousUserAgent(string userAgent);
        bool IsSuspiciousName(string name);
        void AddToBlacklist(string ipAddress, string reason);
        Task<bool> IsBlacklistedIpAsync(string ipAddress);
        Task<bool> IsDisposableEmailAsync(string email);
        Task<bool> IsSuspiciousEmailAsync(string email);
        Task<bool> IsSuspiciousUserAgentAsync(string userAgent);
        Task<bool> IsSuspiciousNameAsync(string name);
        Task AddRuleAsync(string ruleType, string ruleValue, string description, string severity = "medium", bool isRegex = false, string createdBy = "system");
        Task IncrementDetectionCountAsync(int ruleId);
    }

    public class AntiSpamService : IAntiSpamService
    {
        private readonly ILogger<AntiSpamService> _logger;
        private readonly ApplicationDbContext _context;
        
        // IPs conhecidos de bots/spam
        private static readonly HashSet<string> BlacklistedIps = new()
        {
            "185.220.101.0/24", // Tor exit nodes conhecidos
            "192.168.1.1", // Exemplo - adicionar IPs reais conforme necessário
            // IPs dos bots russos detectados
            "45.141.215.111",
            "103.251.167.20", 
            "192.42.116.217",
            "154.41.95.2",
            "185.246.188.74",
            "45.90.185.110",
            "192.42.116.198",
            "185.40.4.150",
        };

        // Domínios de email temporário/descartável
        private static readonly HashSet<string> DisposableEmailDomains = new()
        {
            "10minutemail.com",
            "mailinator.com",
            "guerrillamail.com",
            "temp-mail.org",
            "yopmail.com",
            "tempmail.email",
            "dispostable.com",
            "throwaway.email",
            "maildrop.cc",
            "tempinbox.com"
        };

        // Padrões suspeitos de User-Agent
        private static readonly HashSet<string> SuspiciousUserAgents = new()
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36",
        };

        public AntiSpamService(ILogger<AntiSpamService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public bool IsBlacklistedIp(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            // Verificar IPs específicos
            if (BlacklistedIps.Contains(ipAddress))
            {
                _logger.LogWarning("IP blacklistado detectado: {IP}", ipAddress);
                return true;
            }

            // Verificar ranges de rede conhecidos
            // Implementar lógica de CIDR se necessário

            return false;
        }

        public bool IsDisposableEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var domain = email.Split('@').LastOrDefault()?.ToLower();
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            var isDisposable = DisposableEmailDomains.Contains(domain);
            
            if (isDisposable)
            {
                _logger.LogWarning("Email descartável detectado: {Email}", email);
            }

            return isDisposable;
        }

        public void AddToBlacklist(string ipAddress, string reason)
        {
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                BlacklistedIps.Add(ipAddress);
                _logger.LogInformation("IP adicionado à blacklist: {IP} - Razão: {Reason}", ipAddress, reason);
            }
        }

        public bool IsSuspiciousUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return false;

            var isSuspicious = SuspiciousUserAgents.Contains(userAgent);
            
            if (isSuspicious)
            {
                _logger.LogWarning("User-Agent suspeito detectado: {UserAgent}", userAgent);
            }

            return isSuspicious;
        }

        public bool IsSuspiciousEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            // Padrões de emails suspeitos dos bots russos
            var russianSpamPatterns = new[]
            {
                "no-reply@",
                "noreply@",
                "test@",
                "admin@",
                "bot@",
                "spam@",
                "fake@",
                "temp@",
                "disposable@",
                "@tempmail",
                "@guerrillamail",
                "@10minutemail",
                "@mailinator",
                "@yopmail",
                "@throwaway",
                ".ru",
                ".tk",
                ".ml"
            };

            foreach (var pattern in russianSpamPatterns)
            {
                if (email.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Padrão de spam russo detectado no email: {Email} - Padrão: {Pattern}", email, pattern);
                    return true;
                }
            }

            // Verificar se é email descartável (reutiliza a lógica existente)
            if (IsDisposableEmail(email))
                return true;

            return false;
        }

        public bool IsSuspiciousName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            // Detectar texto em cirílico (russo)
            var hasCyrillic = name.Any(c => c >= 0x0400 && c <= 0x04FF);
            if (hasCyrillic)
            {
                _logger.LogWarning("Nome com caracteres cirílicos detectado: {Name}", name);
                return true;
            }

            // Detectar padrões específicos dos bots russos
            var russianSpamPatterns = new[]
            {
                "Поздравляем",
                "Поздравялем", 
                "выбраны для участия",
                "Wilberries",
                "Wilberies",
                "бесплатные попытки",
                "🏆",
                "* * *"
            };

            foreach (var pattern in russianSpamPatterns)
            {
                if (name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Padrão de spam russo detectado no nome: {Name} - Padrão: {Pattern}", name, pattern);
                    return true;
                }
            }

            // Padrões gerais suspeitos
            var suspiciousPatterns = new[]
            {
                @"^test$",
                @"^Test$", 
                @"^testing$",
                @"^bot$",
                @"^spam$",
                @"^admin$",
                @"^user$",
                @"^[a-zA-Z]{1,2}$", // Nomes muito curtos
                @"^\d+$", // Apenas números
                @"^[^a-zA-ZÀ-ÿ\s]+$", // Sem letras normais
                @"(.)\1{4,}", // Caracteres repetidos 5+ vezes
                @"^(qwerty|asdf|zxcv|1234|abcd)",
            };

            foreach (var pattern in suspiciousPatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(name.ToLower(), pattern))
                {
                    _logger.LogWarning("Padrão suspeito detectado no nome: {Name} - Padrão: {Pattern}", name, pattern);
                    return true;
                }
            }

            // Verificar se tem pelo menos 2 caracteres alfabéticos
            var alphabeticCount = name.Count(c => char.IsLetter(c));
            if (alphabeticCount < 2)
            {
                _logger.LogWarning("Nome com poucos caracteres alfabéticos: {Name}", name);
                return true;
            }

            return false;
        }

        // Métodos Async que utilizam o banco de dados
        public async Task<bool> IsBlacklistedIpAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            // Primeiro verifica cache local (métodos síncronos existentes)
            if (IsBlacklistedIp(ipAddress))
                return true;

            // Depois verifica no banco de dados
            var rule = await _context.AntiSpamRules
                .Where(r => r.RuleType == AntiSpamRuleTypes.IP && 
                           r.RuleValue == ipAddress && 
                           r.IsActive)
                .FirstOrDefaultAsync();

            if (rule != null)
            {
                await IncrementDetectionCountAsync(rule.Id);
                _logger.LogWarning("IP blacklistado detectado no banco: {IP} - Regra: {Description}", ipAddress, rule.Description);
                return true;
            }

            return false;
        }

        public async Task<bool> IsDisposableEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Primeiro verifica cache local
            if (IsDisposableEmail(email))
                return true;

            var domain = email.Split('@').LastOrDefault()?.ToLower();
            if (string.IsNullOrWhiteSpace(domain))
                return false;

            // Verifica domínios no banco
            var rule = await _context.AntiSpamRules
                .Where(r => r.RuleType == AntiSpamRuleTypes.Domain && 
                           r.RuleValue.ToLower() == domain && 
                           r.IsActive)
                .FirstOrDefaultAsync();

            if (rule != null)
            {
                await IncrementDetectionCountAsync(rule.Id);
                _logger.LogWarning("Domínio descartável detectado no banco: {Email} - Regra: {Description}", email, rule.Description);
                return true;
            }

            return false;
        }

        public async Task<bool> IsSuspiciousEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            // Primeiro verifica métodos síncronos
            if (IsSuspiciousEmail(email))
                return true;

            // Verifica padrões de email no banco
            var emailPatterns = await _context.AntiSpamRules
                .Where(r => r.RuleType == AntiSpamRuleTypes.EmailPattern && r.IsActive)
                .ToListAsync();

            foreach (var pattern in emailPatterns)
            {
                bool matches = pattern.IsRegex 
                    ? Regex.IsMatch(email, pattern.RuleValue, RegexOptions.IgnoreCase)
                    : email.Contains(pattern.RuleValue, StringComparison.OrdinalIgnoreCase);

                if (matches)
                {
                    await IncrementDetectionCountAsync(pattern.Id);
                    _logger.LogWarning("Padrão de email suspeito detectado no banco: {Email} - Padrão: {Pattern}", email, pattern.RuleValue);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> IsSuspiciousUserAgentAsync(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return false;

            // Primeiro verifica cache local
            if (IsSuspiciousUserAgent(userAgent))
                return true;

            // Verifica user-agents no banco
            var rule = await _context.AntiSpamRules
                .Where(r => r.RuleType == AntiSpamRuleTypes.UserAgent && 
                           r.RuleValue == userAgent && 
                           r.IsActive)
                .FirstOrDefaultAsync();

            if (rule != null)
            {
                await IncrementDetectionCountAsync(rule.Id);
                _logger.LogWarning("User-Agent suspeito detectado no banco: {UserAgent} - Regra: {Description}", userAgent, rule.Description);
                return true;
            }

            return false;
        }

        public async Task<bool> IsSuspiciousNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            // Primeiro verifica métodos síncronos
            if (IsSuspiciousName(name))
                return true;

            // Verifica padrões de nome no banco
            var namePatterns = await _context.AntiSpamRules
                .Where(r => r.RuleType == AntiSpamRuleTypes.NamePattern && r.IsActive)
                .ToListAsync();

            foreach (var pattern in namePatterns)
            {
                bool matches = pattern.IsRegex 
                    ? Regex.IsMatch(name, pattern.RuleValue, RegexOptions.IgnoreCase)
                    : name.Contains(pattern.RuleValue, StringComparison.OrdinalIgnoreCase);

                if (matches)
                {
                    await IncrementDetectionCountAsync(pattern.Id);
                    _logger.LogWarning("Padrão de nome suspeito detectado no banco: {Name} - Padrão: {Pattern}", name, pattern.RuleValue);
                    return true;
                }
            }

            return false;
        }

        public async Task AddRuleAsync(string ruleType, string ruleValue, string description, string severity = "medium", bool isRegex = false, string createdBy = "system")
        {
            // Verificar se a regra já existe
            var existingRule = await _context.AntiSpamRules
                .Where(r => r.RuleType == ruleType && r.RuleValue == ruleValue)
                .FirstOrDefaultAsync();

            if (existingRule != null)
            {
                _logger.LogInformation("Regra anti-spam já existe: {RuleType} - {RuleValue}", ruleType, ruleValue);
                return;
            }

            var newRule = new AntiSpamRule
            {
                RuleType = ruleType,
                RuleValue = ruleValue,
                Description = description,
                Severity = severity,
                IsRegex = isRegex,
                CreatedBy = createdBy,
                IsActive = true
            };

            _context.AntiSpamRules.Add(newRule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nova regra anti-spam adicionada: {RuleType} - {RuleValue} - {Description}", ruleType, ruleValue, description);
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
    }
}
