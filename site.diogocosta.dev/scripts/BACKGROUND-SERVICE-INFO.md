# 🤖 Bot Detector Background Service
*Data: 10/07/2025*

## 🎯 **Visão Geral**

Substituímos o script Python por um **BackgroundService** nativo do .NET que roda dentro da própria aplicação. Muito mais elegante e integrado!

## ✅ **Vantagens do BackgroundService**

### 🔧 **Integração Nativa**
- ✅ Roda dentro da aplicação ASP.NET Core
- ✅ Acesso direto ao banco de dados via Entity Framework
- ✅ Usa o sistema de injeção de dependência
- ✅ Logs integrados com Serilog

### ⚡ **Performance**
- ✅ Sem overhead de processo externo
- ✅ Cache em memória para contadores
- ✅ Operações assíncronas eficientes
- ✅ Limpeza automática de cache

### 🔄 **Manutenção**
- ✅ Deploy único (sem scripts separados)
- ✅ Configuração via appsettings.json
- ✅ Controle via logs da aplicação
- ✅ Para/inicia com a aplicação

## 📊 **Como Funciona**

### 1. **Coleta de Dados** (a cada 5 min)
```csharp
// Analisa interações recentes dos últimos 30 minutos
var recentInteractions = await context.LeadInteractions
    .Where(i => i.CreatedAt >= recentCutoff)
    .ToListAsync();
```

### 2. **Análise em Tempo Real**
- 🔍 **IPs**: Conta tentativas por endereço
- 🤖 **User-Agents**: Detecta padrões repetitivos
- 📧 **Domínios**: Analisa domains suspeitos (.ru, .tk, etc)
- 👤 **Nomes**: Detecta caracteres cirílicos e padrões russos

### 3. **Cache Inteligente**
```csharp
private readonly ConcurrentDictionary<string, int> _ipAttempts = new();
private readonly ConcurrentDictionary<string, int> _userAgentAttempts = new();
private readonly ConcurrentDictionary<string, DateTime> _lastSeen = new();
```

### 4. **Criação Automática de Regras**
- 🚫 **IP Threshold**: 10+ tentativas → Adiciona regra IP
- 🤖 **User-Agent Threshold**: 15+ tentativas → Adiciona regra User-Agent
- 📧 **Domain Threshold**: 5+ tentativas → Adiciona regra Domain
- 👤 **Name Threshold**: 3+ tentativas → Adiciona regra Name Pattern

## ⚙️ **Configurações**

### `appsettings.json`
```json
{
  "BotDetector": {
    "IpThreshold": 10,
    "UserAgentThreshold": 15,
    "EmailDomainThreshold": 5,
    "NameThreshold": 3,
    "AnalysisIntervalMinutes": 5,
    "CacheCleanupHours": 24
  }
}
```

### **Parâmetros Configuráveis:**
- **IpThreshold**: Quantas tentativas de um IP para criar regra
- **UserAgentThreshold**: Quantas tentativas de um User-Agent
- **EmailDomainThreshold**: Quantas tentativas de um domínio
- **NameThreshold**: Quantas tentativas de um nome suspeito
- **AnalysisIntervalMinutes**: Intervalo entre análises (padrão: 5 min)
- **CacheCleanupHours**: Limpeza do cache (padrão: 24h)

## 📋 **Logs Detalhados**

### **Startup**
```
[INFO] 🤖 Bot Detector Background Service iniciado!
[INFO] 📊 Configurações: IP Threshold=10, UserAgent Threshold=15, Análise a cada 00:05:00
```

### **Detecções**
```
[WARN] 🚫 IP suspeito adicionado automaticamente: 45.141.215.111 (12 tentativas)
[WARN] 🤖 User-Agent suspeito adicionado automaticamente: Mozilla/5.0... (18 tentativas)
[WARN] 📧 Domínio suspeito adicionado automaticamente: tempmail.ru (7 tentativas)
[WARN] 👤 Nome suspeito adicionado automaticamente: Поздравляем (4 tentativas)
```

### **Resumos**
```
[INFO] ✅ Background Service adicionou 3 novas regras anti-spam automaticamente
[DEBUG] 🔍 Analisadas 45 interações recentes
[DEBUG] 🧹 Limpeza do cache: removidas 12 entradas antigas
```

## 🎯 **Tipos de Detecção**

### 1. **IPs Suspeitos**
- Contagem de tentativas por IP
- Threshold configurável (padrão: 10)
- Severidade: **HIGH**

### 2. **User-Agents Repetitivos**
- Detecta bots com UA idênticos
- Threshold configurável (padrão: 15)
- Severidade: **MEDIUM**

### 3. **Domínios Suspeitos**
- Foca em TLDs suspeitos (.ru, .tk, .ml, .ga, .cf)
- Threshold configurável (padrão: 5)
- Severidade: **HIGH**

### 4. **Nomes Suspeitos**
- **Caracteres cirílicos** → Severidade **CRITICAL**
- **Padrões russos** (Поздравляем, Wilberries) → Severidade **CRITICAL**
- Threshold configurável (padrão: 3)

## 🔄 **Ciclo de Vida**

### **Startup**
1. Carrega configurações do appsettings.json
2. Inicializa caches em memória
3. Inicia timer de análise

### **Execução** (a cada 5 min)
1. **Coleta**: Busca interações dos últimos 30 min
2. **Análise**: Processa dados e atualiza contadores
3. **Detecção**: Identifica padrões que excedem thresholds
4. **Ação**: Cria regras automaticamente via AntiSpamService
5. **Log**: Registra todas as ações

### **Limpeza** (a cada 24h)
1. Remove entradas antigas do cache
2. Libera memória
3. Mantém performance

### **Shutdown**
1. Para timer
2. Processa dados pendentes
3. Log de encerramento

## 🚀 **Vantagens Práticas**

### **Para Desenvolvedores:**
- ✅ Código C# nativo (sem Python)
- ✅ Debug integrado no Visual Studio
- ✅ Testes unitários possíveis
- ✅ Sem dependências externas

### **Para Operações:**
- ✅ Deploy único
- ✅ Logs centralizados
- ✅ Configuração via JSON
- ✅ Monitoramento via Seq

### **Para Performance:**
- ✅ Baixo consumo de memória
- ✅ Cache inteligente
- ✅ Operações assíncronas
- ✅ Limpeza automática

## 📈 **Exemplo de Funcionamento**

### **Cenário Real:**
```
15:30 - Bot russo faz 3 tentativas com IP 45.141.215.111
15:32 - Bot russo faz mais 4 tentativas (total: 7)
15:35 - Bot russo faz mais 5 tentativas (total: 12)
15:35 - 🎯 THRESHOLD ATINGIDO! Criando regra automática...
15:35 - ✅ Regra criada: IP 45.141.215.111 bloqueado
15:37 - Bot russo tenta novamente → BLOQUEADO pela nova regra!
```

### **Log Correspondente:**
```
[15:35:12] WARN: 🚫 IP suspeito adicionado automaticamente: 45.141.215.111 (12 tentativas)
[15:35:12] INFO: ✅ Background Service adicionou 1 novas regras anti-spam automaticamente
[15:37:23] WARN: IP blacklistado detectado no banco: 45.141.215.111 - Regra: IP com 12 tentativas suspeitas detectadas automaticamente pelo Background Service
```

## 🎉 **Resultado Final**

**Sistema 100% nativo .NET que:**
- 🤖 Detecta bots automaticamente
- 🚫 Cria regras em tempo real
- 📊 Monitora via logs integrados
- ⚙️ Configura via appsettings.json
- 🔄 Funciona 24/7 sem intervenção

**Muito mais elegante que o script Python!** 🎯
