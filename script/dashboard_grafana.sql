-- dashboard_grafana.sql
-- Exemplos de queries para análise no Grafana usando os dados do sistema
-- Base: schema leads_system
-- Compatível com PostgreSQL

-- Dashboard: Visão Geral de Leads
-- Painel: Total de Leads por Desafio
-- Visualização recomendada: Gráfico de barras horizontal (Bar Chart) ou Tabela
SELECT desafio_slug, COUNT(*) AS total_leads
FROM leads_system.leads
GROUP BY desafio_slug
ORDER BY total_leads DESC;

-- Dashboard: Leads ao Longo do Tempo
-- Painel: Leads Capturados por Dia
-- Visualização recomendada: Série temporal (Time Series/Line Chart)
SELECT DATE(created_at) AS dia, COUNT(*) AS total
FROM leads_system.leads
WHERE created_at >= (NOW() - INTERVAL '30 days')
GROUP BY dia
ORDER BY dia DESC;

-- Dashboard: Status dos Leads
-- Painel: Distribuição dos Leads por Status
-- Visualização recomendada: Gráfico de pizza (Pie Chart) ou barras
SELECT status, COUNT(*) AS total
FROM leads_system.leads
GROUP BY status;

-- Dashboard: Downloads de PDF --------------------------------------------------------OK
-- Painel: Downloads por Arquivo e Sucesso/Erro
-- Visualização recomendada: Gráfico de barras empilhadas (Stacked Bar Chart) ou Tabela
SELECT arquivo_nome, sucesso, COUNT(*) AS total
FROM leads_system.pdf_downloads
GROUP BY arquivo_nome, sucesso
ORDER BY arquivo_nome;

-- Dashboard: Downloads Analytics - RECOMENDADO PARA ACOMPANHAMENTO
-- Painel 1: Downloads ao Longo do Tempo (Time Series)
-- Visualização: Time Series com múltiplas linhas
SELECT 
    DATE_TRUNC('hour', created_at) AS time,
    COUNT(*) AS "Total Downloads",
    COUNT(CASE WHEN sucesso = true THEN 1 END) AS "Downloads Bem-sucedidos",
    COUNT(CASE WHEN sucesso = false THEN 1 END) AS "Downloads com Erro"
FROM leads_system.pdf_downloads
WHERE created_at >= NOW() - INTERVAL '7 days'
GROUP BY time
ORDER BY time;

-- Painel 2: KPI - Total de Downloads Hoje (Stat)
-- Visualização: Stat/Single Value
SELECT COUNT(*) AS downloads_hoje
FROM leads_system.pdf_downloads
WHERE DATE(created_at) = CURRENT_DATE;

-- Painel 3: KPI - Taxa de Sucesso (Gauge)
-- Visualização: Gauge (0-100%)
SELECT 
    ROUND(
        COUNT(CASE WHEN sucesso = true THEN 1 END)::numeric / 
        NULLIF(COUNT(*), 0) * 100, 
        1
    ) AS taxa_sucesso_pct
FROM leads_system.pdf_downloads
WHERE created_at >= CURRENT_DATE - INTERVAL '7 days';

-- Painel 4: Top Arquivos Mais Baixados (Bar Chart Horizontal)
-- Visualização: Bar Chart (Horizontal)
SELECT 
    arquivo_nome,
    COUNT(*) AS total_downloads,
    COUNT(CASE WHEN sucesso = true THEN 1 END) AS downloads_sucesso
FROM leads_system.pdf_downloads
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY arquivo_nome
ORDER BY total_downloads DESC
LIMIT 10;

-- Painel 5: Downloads por Hora do Dia (Heatmap)
-- Visualização: Bar Chart ou Heatmap
SELECT 
    EXTRACT(HOUR FROM created_at) AS hora,
    COUNT(*) AS total_downloads
FROM leads_system.pdf_downloads
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY hora
ORDER BY hora;

-- Painel 6: Comparação Período Atual vs Anterior (Stat com comparação)
-- Visualização: Stat com comparação percentual
WITH periodo_atual AS (
    SELECT COUNT(*) AS downloads_atual
    FROM leads_system.pdf_downloads
    WHERE created_at >= CURRENT_DATE - INTERVAL '7 days'
),
periodo_anterior AS (
    SELECT COUNT(*) AS downloads_anterior
    FROM leads_system.pdf_downloads
    WHERE created_at >= CURRENT_DATE - INTERVAL '14 days'
    AND created_at < CURRENT_DATE - INTERVAL '7 days'
)
SELECT 
    pa.downloads_atual,
    pan.downloads_anterior,
    CASE 
        WHEN pan.downloads_anterior > 0 THEN
            ROUND(((pa.downloads_atual - pan.downloads_anterior)::numeric / pan.downloads_anterior) * 100, 1)
        ELSE 0
    END AS variacao_pct
FROM periodo_atual pa, periodo_anterior pan;

-- Dashboard: Interações dos Leads
-- Painel: Tipos de Interações Realizadas
-- Visualização recomendada: Gráfico de barras ou pizza
SELECT tipo, COUNT(*) AS total
FROM leads_system.lead_interactions
GROUP BY tipo
ORDER BY total DESC;

-- Dashboard: Atividade de Email
-- Painel: Status dos Emails Enviados
-- Visualização recomendada: Gráfico de pizza ou barras
SELECT status, COUNT(*) AS total
FROM leads_system.email_logs
GROUP BY status;

-- Dashboard: Origem dos Leads
-- Painel: Top 10 UTM Source
-- Visualização recomendada: Gráfico de barras horizontal ou tabela
SELECT utm_source, COUNT(*) AS total
FROM leads_system.leads
WHERE utm_source IS NOT NULL AND utm_source <> ''
GROUP BY utm_source
ORDER BY total DESC
LIMIT 10;

-- Dashboard: Picos de Cadastro
-- Painel: Leads por Hora do Dia
-- Visualização recomendada: Gráfico de barras (Bar Chart) com eixo X de 0 a 23
SELECT EXTRACT(HOUR FROM created_at) AS hora, COUNT(*) AS total
FROM leads_system.leads
GROUP BY hora
ORDER BY hora;

-- Dashboard: Domínio de Email dos Leads
-- Painel: Distribuição por Domínio de Email
-- Visualização recomendada: Gráfico de barras horizontal ou tabela
SELECT SUBSTRING(email FROM POSITION('@' IN email) + 1) AS dominio, COUNT(*) AS total
FROM leads_system.leads
GROUP BY dominio
ORDER BY total DESC;

-- Dashboard: Funil de Conversão
-- Painel: Taxa de Conversão (Leads x Downloads)
-- Visualização recomendada: Indicador (Stat/Single Value) ou Gauge
SELECT COUNT(DISTINCT l.id) AS total_leads, COUNT(DISTINCT p.lead_id) AS leads_baixaram_pdf,
       ROUND(COUNT(DISTINCT p.lead_id)::numeric / NULLIF(COUNT(DISTINCT l.id),0), 2) AS taxa_conversao
FROM leads_system.leads l
LEFT JOIN leads_system.pdf_downloads p ON l.id = p.lead_id;

-- Dashboard: Funil Manual da Primeira Virada
-- Painel: Leads que se cadastraram para o Manual da Primeira Virada
-- Visualização recomendada: Indicador (Stat/Single Value)
SELECT COUNT(*) AS total_leads_manual
FROM leads_system.leads
WHERE desafio_slug = 'manual-primeira-virada';

-- Dashboard: Funil Manual da Primeira Virada
-- Painel: Taxa de Conversão (Cadastro → Download)
-- Visualização recomendada: Gauge ou Stat
SELECT 
    COUNT(DISTINCT l.id) AS total_cadastros,
    COUNT(DISTINCT p.id) AS total_downloads,
    ROUND(COUNT(DISTINCT p.id)::numeric / NULLIF(COUNT(DISTINCT l.id), 0) * 100, 2) AS taxa_conversao_pct
FROM leads_system.leads l
LEFT JOIN leads_system.pdf_downloads p ON l.email = p.email
WHERE l.desafio_slug = 'manual-primeira-virada';

-- Dashboard: Funil Manual da Primeira Virada
-- Painel: Cadastros vs Downloads por Dia
-- Visualização recomendada: Série temporal (Time Series) com duas linhas
SELECT 
    DATE(l.created_at) AS dia,
    COUNT(DISTINCT l.id) AS cadastros,
    COUNT(DISTINCT p.id) AS downloads
FROM leads_system.leads l
LEFT JOIN leads_system.pdf_downloads p ON l.email = p.email AND DATE(l.created_at) = DATE(p.created_at)
WHERE l.desafio_slug = 'manual-primeira-virada'
    AND l.created_at >= (NOW() - INTERVAL '30 days')
GROUP BY DATE(l.created_at)
ORDER BY dia DESC;

-- Dashboard: Análise de Desempenho por Desafio
-- Painel: Comparação entre Desafios e Manual
-- Visualização recomendada: Tabela ou Gráfico de barras
SELECT 
    desafio_slug,
    COUNT(*) AS total_leads,
    COUNT(CASE WHEN created_at >= (NOW() - INTERVAL '7 days') THEN 1 END) AS leads_ultima_semana,
    COUNT(CASE WHEN created_at >= (NOW() - INTERVAL '30 days') THEN 1 END) AS leads_ultimo_mes
FROM leads_system.leads
GROUP BY desafio_slug
ORDER BY total_leads DESC;

-- Ideias de dashboards para o Grafana:
-- 1. Visão geral de leads: total, por status, por desafio, por dia
-- 2. Funil de conversão: leads capturados -> leads que baixaram PDF -> leads que receberam email
-- 3. Atividade de email: entregas, aberturas, erros
-- 4. Origem dos leads: utm_source, domínio de email
-- 5. Picos de cadastro: leads por hora/dia
-- 6. Downloads de PDF: por arquivo, por sucesso/erro
-- 7. Interações: tipos mais comuns, evolução ao longo do tempo
-- 8. Análise de campanhas: utm_campaign, utm_medium
-- 9. Funil Manual da Primeira Virada: cadastros → downloads → conversão
-- 10. Comparação de desempenho entre diferentes desafios/produtos

-- QUERIES EXTRAS PARA ANÁLISE DETALHADA DE DOWNLOADS -----------------------------

-- Painel 7: Análise de Falhas de Download (Table)
-- Visualização: Table para debug
SELECT 
    arquivo_nome,
    erro_detalhes,
    COUNT(*) AS total_erros,
    DATE(created_at) AS data_erro
FROM leads_system.pdf_downloads
WHERE sucesso = false
    AND created_at >= NOW() - INTERVAL '7 days'
GROUP BY arquivo_nome, erro_detalhes, DATE(created_at)
ORDER BY total_erros DESC, data_erro DESC;

-- Painel 8: Velocidade de Download (Time to Download)
-- Visualização: Histogram ou Stat
SELECT 
    arquivo_nome,
    AVG(EXTRACT(EPOCH FROM (updated_at - created_at))) AS tempo_medio_segundos,
    COUNT(*) AS total_downloads
FROM leads_system.pdf_downloads
WHERE sucesso = true 
    AND updated_at IS NOT NULL
    AND created_at >= NOW() - INTERVAL '7 days'
GROUP BY arquivo_nome
ORDER BY tempo_medio_segundos DESC;

-- Painel 9: Downloads por Dispositivo/User Agent (Pie Chart)
-- Visualização: Pie Chart ou Bar Chart
SELECT 
    CASE 
        WHEN user_agent ILIKE '%mobile%' OR user_agent ILIKE '%android%' OR user_agent ILIKE '%iphone%' THEN 'Mobile'
        WHEN user_agent ILIKE '%tablet%' OR user_agent ILIKE '%ipad%' THEN 'Tablet'
        WHEN user_agent ILIKE '%bot%' OR user_agent ILIKE '%crawler%' THEN 'Bot'
        ELSE 'Desktop'
    END AS dispositivo,
    COUNT(*) AS total_downloads
FROM leads_system.pdf_downloads
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY dispositivo
ORDER BY total_downloads DESC;

-- Painel 10: Correlação Downloads x Cadastros (Time Series)
-- Visualização: Time Series com duas linhas
SELECT 
    DATE(d.created_at) AS dia,
    COUNT(d.id) AS downloads,
    COUNT(DISTINCT l.id) AS novos_cadastros
FROM leads_system.pdf_downloads d
LEFT JOIN leads_system.leads l ON DATE(l.created_at) = DATE(d.created_at)
WHERE d.created_at >= NOW() - INTERVAL '30 days'
GROUP BY DATE(d.created_at)
ORDER BY dia;

-- Painel 11: Top Downloads por Lead (Table)
-- Visualização: Table - Quem mais baixa?
SELECT 
    p.email,
    COUNT(p.id) AS total_downloads,
    COUNT(DISTINCT p.arquivo_nome) AS arquivos_diferentes,
    MAX(p.created_at) AS ultimo_download
FROM leads_system.pdf_downloads p
WHERE p.created_at >= NOW() - INTERVAL '30 days'
GROUP BY p.email
HAVING COUNT(p.id) > 1
ORDER BY total_downloads DESC
LIMIT 20;

-- Observação: Para uso no Grafana, configure o datasource como PostgreSQL e utilize as queries acima para criar painéis dinâmicos. 