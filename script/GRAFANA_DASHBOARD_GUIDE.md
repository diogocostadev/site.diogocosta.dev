# 📊 Guia Prático: Dashboard de Downloads no Grafana

## 🎯 Objetivo
Criar um dashboard completo para monitorar downloads de PDF em tempo real.

## 📋 Pré-requisitos
- ✅ Grafana já configurado (https://grafana.diogocosta.dev)
- ✅ Data Source PostgreSQL configurado para banco `leads`
- ✅ Tabela `leads_system.pdf_downloads` com dados

## 🚀 Passo a Passo

### 1. Criar Novo Dashboard
1. Acesse https://grafana.diogocosta.dev
2. Clique em **"+"** no menu lateral → **"Dashboard"**
3. Clique em **"Add visualization"**
4. Selecione o data source **PostgreSQL (leads)**

### 2. Painel 1: KPI - Downloads Hoje
**Tipo:** Stat
**Query:**
```sql
SELECT COUNT(*) AS downloads_hoje
FROM leads_system.pdf_downloads
WHERE DATE(created_at) = CURRENT_DATE;
```
**Configurações:**
- Title: "Downloads Hoje"
- Display name: "Downloads"
- Color scheme: Green

### 3. Painel 2: Taxa de Sucesso
**Tipo:** Gauge
**Query:**
```sql
SELECT 
    ROUND(
        COUNT(CASE WHEN sucesso = true THEN 1 END)::numeric / 
        NULLIF(COUNT(*), 0) * 100, 
        1
    ) AS taxa_sucesso_pct
FROM leads_system.pdf_downloads
WHERE created_at >= CURRENT_DATE - INTERVAL '7 days';
```
**Configurações:**
- Title: "Taxa de Sucesso (7 dias)"
- Unit: "Percent (0-100)"
- Thresholds: Red (0-80), Yellow (80-95), Green (95-100)

### 4. Painel 3: Downloads ao Longo do Tempo
**Tipo:** Time series
**Query:**
```sql
SELECT 
    $__timeGroup(created_at, '1h') AS time,
    COUNT(*) AS "Total Downloads",
    COUNT(CASE WHEN sucesso = true THEN 1 END) AS "Sucessos",
    COUNT(CASE WHEN sucesso = false THEN 1 END) AS "Erros"
FROM leads_system.pdf_downloads
WHERE $__timeFilter(created_at)
GROUP BY time
ORDER BY time;
```
**Configurações:**
- Title: "Downloads ao Longo do Tempo"
- Legend: Bottom
- Time range: Last 24 hours

### 5. Painel 4: Top Arquivos
**Tipo:** Bar chart (Horizontal)
**Query:**
```sql
SELECT 
    arquivo_nome,
    COUNT(*) AS total_downloads
FROM leads_system.pdf_downloads
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY arquivo_nome
ORDER BY total_downloads DESC
LIMIT 10;
```
**Configurações:**
- Title: "Top 10 Arquivos (30 dias)"
- Orientation: Horizontal

### 6. Painel 5: Downloads por Hora
**Tipo:** Bar chart
**Query:**
```sql
SELECT 
    EXTRACT(HOUR FROM created_at) AS hora,
    COUNT(*) AS total_downloads
FROM leads_system.pdf_downloads
WHERE created_at >= NOW() - INTERVAL '7 days'
GROUP BY hora
ORDER BY hora;
```
**Configurações:**
- Title: "Downloads por Hora do Dia"
- X-axis: 0-23

## 🎨 Layout Sugerido

```
┌─────────────┬─────────────┬─────────────────────────────────┐
│ Downloads   │ Taxa de     │                                 │
│ Hoje        │ Sucesso     │         (Espaço livre)          │
│ (Stat)      │ (Gauge)     │                                 │
└─────────────┴─────────────┴─────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                Downloads ao Longo do Tempo                  │
│                    (Time Series)                           │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────────┬──────────────────────────────────┐
│    Top 10 Arquivos      │     Downloads por Hora          │
│   (Bar Chart H)         │      (Bar Chart)                │
└──────────────────────────┴──────────────────────────────────┘
```

## ⚡ Configurações Avançadas

### Variables (Opcional)
Crie uma variável `arquivo` para filtrar por arquivo específico:
```sql
SELECT DISTINCT arquivo_nome 
FROM leads_system.pdf_downloads 
ORDER BY arquivo_nome;
```

### Alertas
Configure alerta quando taxa de sucesso < 95%:
```sql
SELECT 
    ROUND(COUNT(CASE WHEN sucesso = true THEN 1 END)::numeric / COUNT(*) * 100, 1) AS taxa
FROM leads_system.pdf_downloads
WHERE created_at >= NOW() - INTERVAL '1 hour';
```

### Auto Refresh
- Configure para 5 minutos
- Time range padrão: Last 24 hours

## 🎯 Resultado Final
Você terá um dashboard completo que mostra:
- ✅ KPIs principais em tempo real
- ✅ Tendências ao longo do tempo
- ✅ Análise por arquivo
- ✅ Padrões de uso por hora
- ✅ Taxa de sucesso/erro

## 💡 Dicas
1. **Salve o dashboard** com nome "Downloads Analytics"
2. **Organize na pasta "Sites"** junto com os outros
3. **Configure permissões** se necessário
4. **Teste as queries** antes de criar os painéis
