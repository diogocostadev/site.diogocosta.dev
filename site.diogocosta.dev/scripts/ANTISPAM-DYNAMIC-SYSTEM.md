# Sistema Dinâmico de Anti-Spam
*Data: 10/07/2025*

## 📋 Visão Geral

Este sistema permite adicionar e gerenciar regras anti-spam em tempo real, sem necessidade de deploy. Ideal para bloquear novos bots e padrões de spam rapidamente.

## 🗃️ Banco de Dados

### Tabela: `antispam_rules`

Execute o script: `scripts/003_create_antispam_rules_table.sql`

**Campos principais:**
- `rule_type`: Tipo da regra (`ip`, `domain`, `email_pattern`, `name_pattern`, `user_agent`)
- `rule_value`: Valor da regra (IP, domínio, padrão, etc)
- `description`: Descrição da regra
- `severity`: Severidade (`low`, `medium`, `high`, `critical`)
- `is_active`: Se a regra está ativa
- `is_regex`: Se o valor é uma regex
- `detection_count`: Quantas vezes foi detectada
- `last_detection`: Última detecção

## 🔌 API Endpoints

### Base URL: `/api/antispam`

#### 1. Listar Regras
```http
GET /api/antispam
```

**Parâmetros:**
- `ruleType`: Filtrar por tipo
- `isActive`: Filtrar por status ativo
- `severity`: Filtrar por severidade
- `page`: Página (padrão: 1)
- `pageSize`: Itens por página (padrão: 50)

**Exemplo:**
```http
GET /api/antispam?ruleType=ip&isActive=true&page=1&pageSize=20
```

#### 2. Criar Nova Regra
```http
POST /api/antispam
```

**Body:**
```json
{
  "ruleType": "ip",
  "ruleValue": "192.168.1.100",
  "description": "Bot russo detectado",
  "severity": "high",
  "isActive": true,
  "isRegex": false,
  "createdBy": "admin"
}
```

#### 3. Atualizar Regra
```http
PUT /api/antispam/{id}
```

**Body:**
```json
{
  "description": "Nova descrição",
  "severity": "critical",
  "isActive": false
}
```

#### 4. Ativar/Desativar Regra
```http
PATCH /api/antispam/{id}/toggle
```

#### 5. Remover Regra
```http
DELETE /api/antispam/{id}
```

#### 6. **🤖 Endpoint para Bot Detector Automático**
```http
POST /api/antispam/detect-and-add
```

**Body:**
```json
{
  "ipAddress": "45.141.215.111",
  "userAgent": "Mozilla/5.0 (Bot)",
  "emailDomain": "tempmail.ru",
  "namePattern": "Поздравляем",
  "isRegex": false,
  "severity": "critical",
  "description": "Bot russo detectado automaticamente pelo sistema"
}
```

#### 7. Estatísticas
```http
GET /api/antispam/stats
```

## 🎯 Tipos de Regras

### 1. **IP** (`ip`)
Bloqueia endereços IP específicos:
```json
{
  "ruleType": "ip",
  "ruleValue": "45.141.215.111",
  "description": "Bot russo",
  "severity": "high"
}
```

### 2. **Domínio** (`domain`)
Bloqueia domínios de email:
```json
{
  "ruleType": "domain",
  "ruleValue": "tempmail.com",
  "description": "Email temporário",
  "severity": "medium"
}
```

### 3. **Padrão de Email** (`email_pattern`)
Bloqueia padrões em emails:
```json
{
  "ruleType": "email_pattern",
  "ruleValue": "no-reply@",
  "description": "Padrão bot",
  "severity": "medium"
}
```

### 4. **Padrão de Nome** (`name_pattern`)
Bloqueia padrões em nomes (suporte a regex):
```json
{
  "ruleType": "name_pattern",
  "ruleValue": "^test$",
  "description": "Nome teste",
  "severity": "medium",
  "isRegex": true
}
```

### 5. **User-Agent** (`user_agent`)
Bloqueia User-Agents específicos:
```json
{
  "ruleType": "user_agent",
  "ruleValue": "Mozilla/5.0 (Bot)",
  "description": "Bot user-agent",
  "severity": "high"
}
```

## 🤖 Sistema de Detecção Automática

### Como Funciona:
1. **Detecção**: Um script/robô monitora tentativas de spam
2. **Análise**: Identifica padrões suspeitos (IP, User-Agent, email, nome)
3. **Adição Automática**: Chama a API para adicionar novas regras
4. **Bloqueio Imediato**: Regras entram em vigor instantaneamente

### Exemplo de Script Python para Robô Detector:
```python
import requests
import json

def add_bot_rule(ip, user_agent, description):
    url = "https://seu-site.com/api/antispam/detect-and-add"
    
    data = {
        "ipAddress": ip,
        "userAgent": user_agent,
        "severity": "critical",
        "description": f"Bot detectado automaticamente - {description}"
    }
    
    response = requests.post(url, json=data)
    if response.status_code == 200:
        print(f"✅ Regra adicionada: {ip}")
    else:
        print(f"❌ Erro: {response.text}")

# Exemplo de uso
add_bot_rule("45.141.215.111", "Mozilla/5.0 (Bot)", "Spam russo")
```

## 📊 Monitoramento

### Logs Disponíveis:
- ✅ Regras criadas/atualizadas/removidas
- 🎯 Detecções em tempo real
- 📈 Contadores de detecção por regra
- ⏰ Última detecção por regra

### Exemplo de Log:
```
[2025-07-10 19:30:15] INFO: Nova regra anti-spam criada via API: ip - 45.141.215.111 - Bot russo detectado
[2025-07-10 19:30:25] WARN: IP blacklistado detectado no banco: 45.141.215.111 - Regra: Bot russo detectado
[2025-07-10 19:30:25] INFO: Contador de detecção incrementado para regra ID: 123
```

## 🚀 Fluxo de Trabalho Recomendado

### Para Administradores:
1. **Monitorar** logs de spam
2. **Identificar** novos padrões
3. **Adicionar** regras via API
4. **Verificar** efetividade através de estatísticas

### Para Sistema Automático:
1. **Detector** identifica comportamento suspeito
2. **API** adiciona regra automaticamente
3. **Sistema** bloqueia futuras tentativas
4. **Admin** revisa e ajusta se necessário

## 🔧 Exemplos Práticos

### Bloquear novo IP bot:
```bash
curl -X POST "https://seu-site.com/api/antispam" \
  -H "Content-Type: application/json" \
  -d '{
    "ruleType": "ip",
    "ruleValue": "123.456.789.0",
    "description": "Novo bot detectado hoje",
    "severity": "high",
    "createdBy": "admin"
  }'
```

### Listar regras críticas:
```bash
curl "https://seu-site.com/api/antispam?severity=critical&isActive=true"
```

### Desativar temporariamente uma regra:
```bash
curl -X PATCH "https://seu-site.com/api/antispam/123/toggle"
```

## 💡 Dicas de Uso

1. **Severidade Critical**: Use para ameaças imediatas
2. **Regex Patterns**: Útil para nomes com caracteres especiais
3. **Detecção Automática**: Configure scripts para alimentar o sistema
4. **Monitoramento**: Acompanhe as estatísticas regularmente
5. **Backup**: Exporte regras importantes antes de mudanças

## 🔒 Segurança

- ✅ API protegida por rate limiting existente
- ✅ Validação de tipos e severidades
- ✅ Logs detalhados para auditoria
- ✅ Detecção não afeta performance (cache local + BD)
- ✅ Regras aplicadas instantaneamente

---

**Resultado:** Sistema completamente dinâmico onde novos bots são bloqueados automaticamente sem deploy! 🎯
