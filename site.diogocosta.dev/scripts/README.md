# 🗄️ Scripts de Banco de Dados

Sistema de gestão de leads e email para os desafios SaaS.

## 📋 Ordem de Execução

Execute os scripts na ordem numerada:

### 001 - Criação Inicial
```bash
# Conectar ao PostgreSQL e executar
psql -h 10.10.0.8 -U dcov3rl0rd -d leads -f 001_create_schema_and_tables.sql
```

## 🏗️ Estrutura Criada

### Schema: `leads_system`

### Tabelas Principais:
- **`lead_sources`** - Fontes dos leads (desafio-vendas, desafio-financeiro, etc)
- **`leads`** - Dados dos leads capturados
- **`lead_interactions`** - Histórico de interações
- **`email_templates`** - Templates de email para automação
- **`email_campaigns`** - Campanhas de email
- **`email_logs`** - Log de emails enviados

### Views Úteis:
- **`vw_leads_stats`** - Estatísticas por desafio
- **`vw_lead_email_history`** - Histórico de emails por lead

## 🚀 Queries Úteis

### Ver leads por desafio
```sql
SELECT * FROM leads_system.vw_leads_stats;
```

### Leads de hoje
```sql
SELECT nome, email, desafio_slug, created_at 
FROM leads_system.leads 
WHERE created_at >= CURRENT_DATE
ORDER BY created_at DESC;
```

### Histórico de emails de um lead
```sql
SELECT * FROM leads_system.vw_lead_email_history 
WHERE email = 'email@exemplo.com';
```

### Leads sem email de boas-vindas
```sql
SELECT l.* 
FROM leads_system.leads l
LEFT JOIN leads_system.email_logs el ON l.id = el.lead_id
WHERE el.id IS NULL;
```

## 🔧 Manutenção

### Backup
```bash
pg_dump -h 10.10.0.8 -U dcov3rl0rd -n leads_system leads > backup_leads_$(date +%Y%m%d).sql
```

### Verificar integridade
```sql
-- Verificar leads órfãos
SELECT COUNT(*) FROM leads_system.leads WHERE source_id IS NULL;

-- Verificar emails não enviados
SELECT COUNT(*) FROM leads_system.email_logs WHERE status = 'falhou';
```

## 📊 Relatórios

### Performance por desafio
```sql
SELECT 
    desafio_slug,
    total_leads,
    leads_hoje,
    leads_ultima_semana,
    ROUND(leads_ultima_semana::numeric / NULLIF(total_leads, 0) * 100, 2) as taxa_crescimento_semanal
FROM leads_system.vw_leads_stats
ORDER BY total_leads DESC;
```

### Taxa de abertura de emails
```sql
SELECT 
    et.nome as template,
    COUNT(el.id) as emails_enviados,
    COUNT(el.aberto_em) as emails_abertos,
    ROUND(COUNT(el.aberto_em)::numeric / COUNT(el.id) * 100, 2) as taxa_abertura
FROM leads_system.email_logs el
JOIN leads_system.email_templates et ON el.template_id = et.id
GROUP BY et.nome
ORDER BY taxa_abertura DESC;
``` 