# AntiSpam.Core

[![NuGet](https://img.shields.io/nuget/v/AntiSpam.Core.svg)](https://www.nuget.org/packages/AntiSpam.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Biblioteca reutilizável para detecção e prevenção de spam/bots em aplicações ASP.NET Core com Entity Framework.

## 🎯 Funcionalidades

- ✅ **Detecção automática de bots** via Background Service
- ✅ **Rate limiting** configurável por IP
- ✅ **Sistema de regras dinâmicas** (IP, User-Agent, domínios, padrões de nomes)
- ✅ **Middleware integrado** para ASP.NET Core
- ✅ **Cache em memória** para performance
- ✅ **Configuração via appsettings.json**
- ✅ **Logs detalhados** com Serilog/ILogger
- ✅ **Suporte a múltiplos contextos** Entity Framework

## 🚀 Instalação

```bash
dotnet add package AntiSpam.Core
```

## ⚙️ Configuração Básica

### 1. **Implemente as interfaces no seu projeto:**

```csharp
// Sua regra anti-spam
public class AntiSpamRule : IAntiSpamRule
{
    public int Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string RuleValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsRegex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public int DetectionCount { get; set; }
    public DateTime? LastDetection { get; set; }
}

// Sua interação de lead/usuário
public class LeadInteraction : ILeadInteraction
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public object? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public object? Dados { get; set; }
    // seus campos específicos...
}
```

### 2. **Implemente o serviço anti-spam:**

```csharp
public class AntiSpamService : AntiSpamServiceBase<AntiSpamRule, ApplicationDbContext>
{
    protected override DbSet<AntiSpamRule> Rules => _context.AntiSpamRules;

    public AntiSpamService(ApplicationDbContext context, ILogger<AntiSpamService> logger)
        : base(context, logger) { }
}
```

### 3. **Implemente o background service:**

```csharp
public class BotDetectorBackgroundService : BotDetectorBackgroundServiceBase<AntiSpamRule, LeadInteraction, ApplicationDbContext>
{
    protected override DbSet<LeadInteraction> GetInteractions(ApplicationDbContext context) 
        => context.LeadInteractions;

    protected override IAntiSpamService<AntiSpamRule> GetAntiSpamService(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IAntiSpamService<AntiSpamRule>>();

    protected override async Task<bool> ShouldAddRule(ApplicationDbContext context, string ruleType, string ruleValue)
    {
        return !await context.AntiSpamRules
            .AnyAsync(r => r.RuleType == ruleType && r.RuleValue == ruleValue);
    }

    public BotDetectorBackgroundService(
        ILogger<BotDetectorBackgroundService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
        : base(logger, serviceProvider, configuration) { }
}
```

### 4. **Configure no Program.cs:**

```csharp
using AntiSpam.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registrar serviços AntiSpam.Core
builder.Services.AddAntiSpamCore(builder.Configuration);
builder.Services.AddAntiSpamService<AntiSpamService, AntiSpamRule, ApplicationDbContext>();
builder.Services.AddBotDetectorBackgroundService<BotDetectorBackgroundService>();

var app = builder.Build();

// Adicionar middleware
app.UseRateLimiting();

app.Run();
```

### 5. **Configure no appsettings.json:**

```json
{
  "BotDetector": {
    "IpThreshold": 10,
    "UserAgentThreshold": 15,
    "EmailDomainThreshold": 5,
    "NameThreshold": 3,
    "AnalysisIntervalMinutes": 5,
    "CacheCleanupHours": 24
  },
  "RateLimit": {
    "RequestsPerMinute": 5,
    "BurstSize": 10,
    "WindowSizeMinutes": 1
  }
}
```

## 📊 Uso no Controller

```csharp
[ApiController]
public class LeadController : ControllerBase
{
    private readonly IAntiSpamService<AntiSpamRule> _antiSpamService;

    public LeadController(IAntiSpamService<AntiSpamRule> antiSpamService)
    {
        _antiSpamService = antiSpamService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLead(LeadModel model)
    {
        var isSpam = await _antiSpamService.CheckAsync(
            model.Email, 
            model.Nome, 
            Request.Headers["User-Agent"], 
            HttpContext.Connection.RemoteIpAddress
        );

        if (isSpam)
        {
            return BadRequest("Spam detectado");
        }

        // Processar lead...
        return Ok();
    }
}
```

## 🎯 Funcionalidades Avançadas

### **Tipos de Regras Suportadas:**

- `AntiSpamRuleTypes.IP` - Bloqueio por endereço IP
- `AntiSpamRuleTypes.UserAgent` - Bloqueio por User-Agent
- `AntiSpamRuleTypes.Domain` - Bloqueio por domínio de email
- `AntiSpamRuleTypes.EmailPattern` - Padrões de email
- `AntiSpamRuleTypes.NamePattern` - Padrões de nomes

### **Severidades:**

- `SeverityLevels.Low` - Baixa
- `SeverityLevels.Medium` - Média  
- `SeverityLevels.High` - Alta
- `SeverityLevels.Critical` - Crítica

### **Detecções Automáticas:**

- 🇷🇺 **Texto cirílico** (caracteres russos)
- 🤖 **Padrões de bots conhecidos** (Поздравляем, Wilberries, etc.)
- 🚫 **IPs com muitas tentativas**
- 🌐 **Domínios suspeitos** (.ru, .tk, .ml, etc.)
- 🔄 **User-Agents repetitivos**

## 🔧 Personalização

A biblioteca é altamente extensível. Você pode:

- Sobrescrever métodos de detecção
- Adicionar novos tipos de regras
- Personalizar extração de dados
- Implementar suas próprias heurísticas

## 📝 Licença

MIT License - veja [LICENSE](LICENSE) para detalhes.

## 🤝 Contribuição

Contribuições são bem-vindas! Abra uma issue ou pull request.

---

**Desenvolvido por [Diogo Costa](https://github.com/diogocostadev)**
