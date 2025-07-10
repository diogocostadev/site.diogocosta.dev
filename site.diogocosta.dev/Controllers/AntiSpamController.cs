using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos;

namespace site.diogocosta.dev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AntiSpamController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAntiSpamService _antiSpamService;
        private readonly ILogger<AntiSpamController> _logger;

        public AntiSpamController(
            ApplicationDbContext context, 
            IAntiSpamService antiSpamService,
            ILogger<AntiSpamController> logger)
        {
            _context = context;
            _antiSpamService = antiSpamService;
            _logger = logger;
        }

        /// <summary>
        /// Lista todas as regras anti-spam
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AntiSpamRuleResponse>>> GetRules(
            [FromQuery] string? ruleType = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? severity = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.AntiSpamRules.AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(ruleType))
                query = query.Where(r => r.RuleType == ruleType);

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(severity))
                query = query.Where(r => r.Severity == severity);

            // Paginação
            var totalCount = await query.CountAsync();
            var rules = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AntiSpamRuleResponse
                {
                    Id = r.Id,
                    RuleType = r.RuleType,
                    RuleValue = r.RuleValue,
                    Description = r.Description,
                    Severity = r.Severity,
                    IsActive = r.IsActive,
                    IsRegex = r.IsRegex,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    CreatedBy = r.CreatedBy,
                    DetectionCount = r.DetectionCount,
                    LastDetection = r.LastDetection
                })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(rules);
        }

        /// <summary>
        /// Obtém uma regra específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AntiSpamRuleResponse>> GetRule(int id)
        {
            var rule = await _context.AntiSpamRules.FindAsync(id);
            
            if (rule == null)
                return NotFound();

            var response = new AntiSpamRuleResponse
            {
                Id = rule.Id,
                RuleType = rule.RuleType,
                RuleValue = rule.RuleValue,
                Description = rule.Description,
                Severity = rule.Severity,
                IsActive = rule.IsActive,
                IsRegex = rule.IsRegex,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
                CreatedBy = rule.CreatedBy,
                DetectionCount = rule.DetectionCount,
                LastDetection = rule.LastDetection
            };

            return Ok(response);
        }

        /// <summary>
        /// Cria uma nova regra anti-spam
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AntiSpamRuleResponse>> CreateRule([FromBody] CreateAntiSpamRuleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar tipo de regra
            var validTypes = new[] { "ip", "domain", "email_pattern", "name_pattern", "user_agent" };
            if (!validTypes.Contains(request.RuleType))
                return BadRequest($"Tipo de regra inválido. Tipos válidos: {string.Join(", ", validTypes)}");

            // Validar severidade
            var validSeverities = new[] { "low", "medium", "high", "critical" };
            if (!validSeverities.Contains(request.Severity))
                return BadRequest($"Severidade inválida. Severidades válidas: {string.Join(", ", validSeverities)}");

            // Verificar se a regra já existe
            var existingRule = await _context.AntiSpamRules
                .Where(r => r.RuleType == request.RuleType && r.RuleValue == request.RuleValue)
                .FirstOrDefaultAsync();

            if (existingRule != null)
                return Conflict("Regra já existe para este tipo e valor");

            var newRule = new AntiSpamRule
            {
                RuleType = request.RuleType,
                RuleValue = request.RuleValue,
                Description = request.Description,
                Severity = request.Severity,
                IsActive = request.IsActive,
                IsRegex = request.IsRegex,
                CreatedBy = request.CreatedBy ?? "api"
            };

            _context.AntiSpamRules.Add(newRule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nova regra anti-spam criada via API: {RuleType} - {RuleValue} - {Description}", 
                newRule.RuleType, newRule.RuleValue, newRule.Description);

            var response = new AntiSpamRuleResponse
            {
                Id = newRule.Id,
                RuleType = newRule.RuleType,
                RuleValue = newRule.RuleValue,
                Description = newRule.Description,
                Severity = newRule.Severity,
                IsActive = newRule.IsActive,
                IsRegex = newRule.IsRegex,
                CreatedAt = newRule.CreatedAt,
                UpdatedAt = newRule.UpdatedAt,
                CreatedBy = newRule.CreatedBy,
                DetectionCount = newRule.DetectionCount,
                LastDetection = newRule.LastDetection
            };

            return CreatedAtAction(nameof(GetRule), new { id = newRule.Id }, response);
        }

        /// <summary>
        /// Atualiza uma regra anti-spam existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<AntiSpamRuleResponse>> UpdateRule(int id, [FromBody] UpdateAntiSpamRuleRequest request)
        {
            var rule = await _context.AntiSpamRules.FindAsync(id);
            
            if (rule == null)
                return NotFound();

            // Atualizar apenas campos fornecidos
            if (!string.IsNullOrWhiteSpace(request.RuleValue))
                rule.RuleValue = request.RuleValue;

            if (!string.IsNullOrWhiteSpace(request.Description))
                rule.Description = request.Description;

            if (!string.IsNullOrWhiteSpace(request.Severity))
            {
                var validSeverities = new[] { "low", "medium", "high", "critical" };
                if (!validSeverities.Contains(request.Severity))
                    return BadRequest($"Severidade inválida. Severidades válidas: {string.Join(", ", validSeverities)}");
                
                rule.Severity = request.Severity;
            }

            if (request.IsActive.HasValue)
                rule.IsActive = request.IsActive.Value;

            if (request.IsRegex.HasValue)
                rule.IsRegex = request.IsRegex.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Regra anti-spam atualizada via API: {Id} - {RuleType} - {RuleValue}", 
                rule.Id, rule.RuleType, rule.RuleValue);

            var response = new AntiSpamRuleResponse
            {
                Id = rule.Id,
                RuleType = rule.RuleType,
                RuleValue = rule.RuleValue,
                Description = rule.Description,
                Severity = rule.Severity,
                IsActive = rule.IsActive,
                IsRegex = rule.IsRegex,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
                CreatedBy = rule.CreatedBy,
                DetectionCount = rule.DetectionCount,
                LastDetection = rule.LastDetection
            };

            return Ok(response);
        }

        /// <summary>
        /// Ativa/Desativa uma regra
        /// </summary>
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult> ToggleRule(int id)
        {
            var rule = await _context.AntiSpamRules.FindAsync(id);
            
            if (rule == null)
                return NotFound();

            rule.IsActive = !rule.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Regra anti-spam {Action} via API: {Id} - {RuleType} - {RuleValue}", 
                rule.IsActive ? "ativada" : "desativada", rule.Id, rule.RuleType, rule.RuleValue);

            return Ok(new { id = rule.Id, isActive = rule.IsActive });
        }

        /// <summary>
        /// Remove uma regra anti-spam
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRule(int id)
        {
            var rule = await _context.AntiSpamRules.FindAsync(id);
            
            if (rule == null)
                return NotFound();

            _context.AntiSpamRules.Remove(rule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Regra anti-spam removida via API: {Id} - {RuleType} - {RuleValue}", 
                rule.Id, rule.RuleType, rule.RuleValue);

            return NoContent();
        }

        /// <summary>
        /// Endpoint para robô detectar e adicionar automaticamente novos bots
        /// </summary>
        [HttpPost("detect-and-add")]
        public async Task<ActionResult> DetectAndAddBot([FromBody] BotDetectionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rulesAdded = new List<string>();

            // Adicionar IP se fornecido
            if (!string.IsNullOrWhiteSpace(request.IpAddress))
            {
                await _antiSpamService.AddRuleAsync(
                    AntiSpamRuleTypes.IP, 
                    request.IpAddress, 
                    $"Bot detectado automaticamente - {request.Description}", 
                    request.Severity ?? "medium",
                    false,
                    "bot_detector"
                );
                rulesAdded.Add($"IP: {request.IpAddress}");
            }

            // Adicionar User-Agent se fornecido
            if (!string.IsNullOrWhiteSpace(request.UserAgent))
            {
                await _antiSpamService.AddRuleAsync(
                    AntiSpamRuleTypes.UserAgent, 
                    request.UserAgent, 
                    $"User-Agent bot detectado automaticamente - {request.Description}", 
                    request.Severity ?? "medium",
                    false,
                    "bot_detector"
                );
                rulesAdded.Add($"User-Agent: {request.UserAgent}");
            }

            // Adicionar domínio de email se fornecido
            if (!string.IsNullOrWhiteSpace(request.EmailDomain))
            {
                await _antiSpamService.AddRuleAsync(
                    AntiSpamRuleTypes.Domain, 
                    request.EmailDomain, 
                    $"Domínio spam detectado automaticamente - {request.Description}", 
                    request.Severity ?? "medium",
                    false,
                    "bot_detector"
                );
                rulesAdded.Add($"Domain: {request.EmailDomain}");
            }

            // Adicionar padrão de nome se fornecido
            if (!string.IsNullOrWhiteSpace(request.NamePattern))
            {
                await _antiSpamService.AddRuleAsync(
                    AntiSpamRuleTypes.NamePattern, 
                    request.NamePattern, 
                    $"Padrão de nome spam detectado automaticamente - {request.Description}", 
                    request.Severity ?? "medium",
                    request.IsRegex,
                    "bot_detector"
                );
                rulesAdded.Add($"Name Pattern: {request.NamePattern}");
            }

            _logger.LogInformation("Regras anti-spam adicionadas automaticamente: {Rules} - Descrição: {Description}", 
                string.Join(", ", rulesAdded), request.Description);

            return Ok(new { 
                message = "Regras adicionadas com sucesso", 
                rulesAdded = rulesAdded,
                totalRules = rulesAdded.Count
            });
        }

        /// <summary>
        /// Estatísticas das regras anti-spam
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var stats = await _context.AntiSpamRules
                .GroupBy(r => r.RuleType)
                .Select(g => new { 
                    RuleType = g.Key, 
                    Total = g.Count(),
                    Active = g.Count(r => r.IsActive),
                    TotalDetections = g.Sum(r => r.DetectionCount)
                })
                .ToListAsync();

            var totalRules = await _context.AntiSpamRules.CountAsync();
            var activeRules = await _context.AntiSpamRules.CountAsync(r => r.IsActive);
            var totalDetections = await _context.AntiSpamRules.SumAsync(r => r.DetectionCount);
            var lastDetection = await _context.AntiSpamRules
                .Where(r => r.LastDetection != null)
                .MaxAsync(r => r.LastDetection);

            return Ok(new {
                totalRules,
                activeRules,
                totalDetections,
                lastDetection,
                rulesByType = stats
            });
        }
    }

    // Request para detecção automática de bots
    public class BotDetectionRequest
    {
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? EmailDomain { get; set; }
        public string? NamePattern { get; set; }
        public bool IsRegex { get; set; } = false;
        public string? Severity { get; set; } = "medium";
        public string Description { get; set; } = "Bot detectado automaticamente";
    }
}
