# 🚀 Setup Completo - Sistema de Leads e Email

Sistema completo de captura de leads, gestão por email e monitoramento integrado com PostgreSQL e Seq.

## 📋 Pré-requisitos

### 1. PostgreSQL
- Servidor: `10.10.0.8`
- Usuário: `dcov3rl0rd`
- Database: `leads`
- Port: `5432` (padrão)

### 2. Seq (Logging)
- URL: `https://seq.diogocosta.dev`
- API Key: `OyTLRUtv96SYrvz3pyiQ`

### 3. Email SMTP
- Servidor: `mail.didaticos.com`
- Port: `587`
- Email: `noreply@diogocosta.dev`

## 🗄️ Setup do Banco de Dados

### Passo 1: Testar Conexão
```bash
cd site.diogocosta.dev/scripts

# Testar conexão básica
psql -h 10.10.0.8 -U dcov3rl0rd -d leads -f 000_test_connection.sql
```

### Passo 2: Criar Schema e Tabelas
```bash
# Executar script principal
psql -h 10.10.0.8 -U dcov3rl0rd -d leads -f 001_create_schema_and_tables.sql
```

### Passo 3: Verificar Criação
```sql
-- Conectar ao banco e verificar
psql -h 10.10.0.8 -U dcov3rl0rd -d leads

-- Verificar tabelas criadas
\dt leads_system.*

-- Verificar dados iniciais
SELECT * FROM leads_system.lead_sources;
SELECT * FROM leads_system.email_templates;

-- Verificar views
SELECT * FROM leads_system.vw_leads_stats;
```

## 🔧 Setup da Aplicação

### Passo 1: Instalar Dependências
```bash
cd site.diogocosta.dev
dotnet restore
```

### Passo 2: Verificar Configurações
Confirmar que `appsettings.json` e `appsettings.Development.json` estão com:
- ✅ ConnectionStrings
- ✅ Configuração Seq
- ✅ Configuração Email

### Passo 3: Executar Aplicação
```bash
# Modo desenvolvimento
dotnet run --launch-profile http

# A aplicação estará em http://localhost:5000
```

## 🧪 Testes

### 1. Teste de Conexão com Banco
```bash
# Via aplicação
curl http://localhost:5000/api/leads/health
```

Resposta esperada:
```json
{
  "status": "healthy",
  "service": "leads",
  "timestamp": "2024-12-20T...",
  "version": "1.0.0"
}
```

### 2. Teste de Captura de Lead
Acesse uma das landing pages:
- http://localhost:5000/desafio-vendas
- http://localhost:5000/desafio-financeiro
- http://localhost:5000/desafio-leads

Preencha o formulário e submeta.

### 3. Verificar Lead no Banco
```sql
-- Conectar ao banco
psql -h 10.10.0.8 -U dcov3rl0rd -d leads

-- Ver leads recentes
SELECT nome, email, desafio_slug, created_at 
FROM leads_system.leads 
ORDER BY created_at DESC 
LIMIT 5;

-- Ver interações
SELECT l.nome, l.email, i.tipo, i.descricao, i.created_at
FROM leads_system.leads l
JOIN leads_system.lead_interactions i ON l.id = i.lead_id
ORDER BY i.created_at DESC
LIMIT 10;

-- Ver emails enviados
SELECT l.nome, l.email, el.assunto, el.status, el.enviado_em
FROM leads_system.leads l
JOIN leads_system.email_logs el ON l.id = el.lead_id
ORDER BY el.enviado_em DESC
LIMIT 10;
```

### 4. Estatísticas (Dev Only)
```bash
curl http://localhost:5000/leads/stats
```

## 📧 Fluxo de Email

### Automático (Após Captura)
1. Lead se cadastra no formulário
2. Sistema salva no banco PostgreSQL
3. Email de boas-vindas é enviado automaticamente
4. Email de notificação interna é enviado para `diogo@diogocosta.dev`
5. Interações são logadas no banco

### Templates Disponíveis
- **boas-vindas-desafio**: Email automático para novos leads
- Variáveis: `{{nome}}`, `{{desafio}}`, `{{email}}`

## 📊 Monitoramento

### Logs com Seq
- URL: https://seq.diogocosta.dev
- Buscar por: `Application:"Site.DiogoCosta.Dev"`
- Eventos importantes:
  - `💰 Lead capturado`
  - `✅ Email enviado`
  - `❌ Erro ao enviar email`

### Logs Locais
```bash
# Ver logs da aplicação
tail -f site.diogocosta.dev/logs/app-*.txt

# Filtrar apenas leads
grep "Lead" site.diogocosta.dev/logs/app-*.txt
```

### Queries Úteis para Monitoramento

#### Leads de hoje
```sql
SELECT desafio_slug, COUNT(*) as total
FROM leads_system.leads 
WHERE created_at >= CURRENT_DATE
GROUP BY desafio_slug
ORDER BY total DESC;
```

#### Taxa de conversão de email
```sql
SELECT 
    COUNT(DISTINCT l.id) as leads_total,
    COUNT(DISTINCT el.lead_id) as emails_enviados,
    ROUND(COUNT(DISTINCT el.lead_id)::numeric / COUNT(DISTINCT l.id) * 100, 2) as taxa_email
FROM leads_system.leads l
LEFT JOIN leads_system.email_logs el ON l.id = el.lead_id
WHERE l.created_at >= CURRENT_DATE - INTERVAL '7 days';
```

#### Leads com problemas de email
```sql
SELECT l.nome, l.email, l.desafio_slug, l.created_at
FROM leads_system.leads l
LEFT JOIN leads_system.email_logs el ON l.id = el.lead_id 
    AND el.template_id = (SELECT id FROM leads_system.email_templates WHERE slug = 'boas-vindas-desafio')
WHERE el.id IS NULL 
AND l.created_at > NOW() - INTERVAL '1 day'
ORDER BY l.created_at DESC;
```

## 🔧 Manutenção

### Backup Automático
```bash
# Script de backup diário
pg_dump -h 10.10.0.8 -U dcov3rl0rd -n leads_system leads > backup_leads_$(date +%Y%m%d).sql

# Compactar backup
gzip backup_leads_$(date +%Y%m%d).sql
```

### Limpeza de Logs Antigos
```bash
# Remover logs mais antigos que 30 dias
find logs/ -name "app-*.txt" -mtime +30 -delete
```

### Reenvio de Emails (Dev)
```bash
# Reenviar emails para leads sem boas-vindas
curl -X POST http://localhost:5000/api/leads/resend-welcome-emails
```

## 🚨 Troubleshooting

### Erro de Conexão com Banco
1. Verificar se PostgreSQL está rodando: `pg_isready -h 10.10.0.8`
2. Testar conexão manual: `psql -h 10.10.0.8 -U dcov3rl0rd -d leads`
3. Verificar firewall/rede

### Emails Não Enviados
1. Verificar configurações SMTP no `appsettings.json`
2. Testar conexão SMTP: `telnet mail.didaticos.com 587`
3. Verificar logs no Seq ou arquivos locais
4. Verificar credenciais de email

### Logs Não Aparecem no Seq
1. Verificar URL e API Key do Seq
2. Testar acesso: `curl https://seq.diogocosta.dev/api`
3. Verificar se aplicação consegue conectar na rede

## 📈 Próximos Passos

- [ ] Implementar dashboard administrativo com autenticação
- [ ] Adicionar mais templates de email (follow-up, abandoned cart, etc)
- [ ] Implementar sistema de tags automáticas
- [ ] Integrar com ferramentas de analytics
- [ ] Adicionar webhooks para integrações externas
- [ ] Implementar A/B testing para landing pages

---

## 🎯 Resultado Final

Sistema completo funcionando com:
- ✅ 3 Landing pages brutalistas ativas
- ✅ Captura de leads com dados UTM
- ✅ Banco PostgreSQL com schema completo
- ✅ Emails automáticos de boas-vindas
- ✅ Notificações internas por email
- ✅ Logs estruturados com Seq
- ✅ Monitoramento e estatísticas
- ✅ Sistema de backup e manutenção

**O sistema está pronto para receber tráfego e converter leads! 🔥** 