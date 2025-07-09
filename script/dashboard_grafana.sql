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

-- Observação: Para uso no Grafana, configure o datasource como PostgreSQL e utilize as queries acima para criar painéis dinâmicos. 