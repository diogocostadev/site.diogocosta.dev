-- ========================================
-- SCRIPT 001 - CREATE SCHEMA AND TABLES
-- Versão: 1.0.0
-- Data: 2024-12-20
-- Descrição: Criação do schema e tabelas para sistema de leads dos desafios
-- ========================================

-- Criar schema se não existir
CREATE SCHEMA IF NOT EXISTS leads_system;

-- Usar o schema
SET search_path TO leads_system;

-- ========================================
-- TABELA: lead_sources
-- Descrição: Fontes de onde vêm os leads
-- ========================================
CREATE TABLE IF NOT EXISTS lead_sources (
    id SERIAL PRIMARY KEY,
    slug VARCHAR(100) NOT NULL UNIQUE,
    nome VARCHAR(200) NOT NULL,
    descricao TEXT,
    ativo BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ========================================
-- TABELA: leads
-- Descrição: Informações dos leads capturados
-- ========================================
CREATE TABLE IF NOT EXISTS leads (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(200) NOT NULL,
    email VARCHAR(320) NOT NULL,
    source_id INTEGER REFERENCES lead_sources(id),
    desafio_slug VARCHAR(100) NOT NULL,
    ip_address INET,
    user_agent TEXT,
    utm_source VARCHAR(100),
    utm_medium VARCHAR(100),
    utm_campaign VARCHAR(100),
    utm_content VARCHAR(100),
    utm_term VARCHAR(100),
    status VARCHAR(50) DEFAULT 'novo',
    tags TEXT[], -- Array de tags
    notas TEXT,
    opt_in BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(email, desafio_slug) -- Evita duplicatas por email/desafio
);

-- ========================================
-- TABELA: lead_interactions
-- Descrição: Histórico de interações com leads
-- ========================================
CREATE TABLE IF NOT EXISTS lead_interactions (
    id SERIAL PRIMARY KEY,
    lead_id INTEGER REFERENCES leads(id) ON DELETE CASCADE,
    tipo VARCHAR(100) NOT NULL, -- 'email_sent', 'email_opened', 'email_clicked', 'form_submitted', etc
    descricao TEXT,
    dados JSONB, -- Dados flexíveis da interação
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ========================================
-- TABELA: email_templates
-- Descrição: Templates de email para automação
-- ========================================
CREATE TABLE IF NOT EXISTS email_templates (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(200) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    assunto VARCHAR(500) NOT NULL,
    corpo_html TEXT NOT NULL,
    corpo_texto TEXT,
    variaveis TEXT[], -- Array de variáveis disponíveis
    ativo BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ========================================
-- TABELA: email_campaigns
-- Descrição: Campanhas de email enviadas
-- ========================================
CREATE TABLE IF NOT EXISTS email_campaigns (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(200) NOT NULL,
    template_id INTEGER REFERENCES email_templates(id),
    lead_filter JSONB, -- Filtros para selecionar leads
    agendado_para TIMESTAMP,
    enviado_em TIMESTAMP,
    status VARCHAR(50) DEFAULT 'rascunho', -- 'rascunho', 'agendado', 'enviando', 'enviado', 'cancelado'
    total_leads INTEGER DEFAULT 0,
    emails_enviados INTEGER DEFAULT 0,
    emails_abertos INTEGER DEFAULT 0,
    emails_clicados INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ========================================
-- TABELA: email_logs
-- Descrição: Log de todos os emails enviados
-- ========================================
CREATE TABLE IF NOT EXISTS email_logs (
    id SERIAL PRIMARY KEY,
    lead_id INTEGER REFERENCES leads(id),
    campaign_id INTEGER REFERENCES email_campaigns(id),
    template_id INTEGER REFERENCES email_templates(id),
    email_to VARCHAR(320) NOT NULL,
    assunto VARCHAR(500) NOT NULL,
    status VARCHAR(50) NOT NULL, -- 'enviado', 'falhou', 'bounce', 'spam'
    erro_msg TEXT,
    enviado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    aberto_em TIMESTAMP,
    clicado_em TIMESTAMP,
    bounce_em TIMESTAMP,
    spam_em TIMESTAMP
);

-- ========================================
-- ÍNDICES PARA PERFORMANCE
-- ========================================

-- Índices na tabela leads
CREATE INDEX IF NOT EXISTS idx_leads_email ON leads(email);
CREATE INDEX IF NOT EXISTS idx_leads_desafio_slug ON leads(desafio_slug);
CREATE INDEX IF NOT EXISTS idx_leads_status ON leads(status);
CREATE INDEX IF NOT EXISTS idx_leads_created_at ON leads(created_at);
CREATE INDEX IF NOT EXISTS idx_leads_opt_in ON leads(opt_in);

-- Índices na tabela lead_interactions
CREATE INDEX IF NOT EXISTS idx_interactions_lead_id ON lead_interactions(lead_id);
CREATE INDEX IF NOT EXISTS idx_interactions_tipo ON lead_interactions(tipo);
CREATE INDEX IF NOT EXISTS idx_interactions_created_at ON lead_interactions(created_at);

-- Índices na tabela email_logs
CREATE INDEX IF NOT EXISTS idx_email_logs_lead_id ON email_logs(lead_id);
CREATE INDEX IF NOT EXISTS idx_email_logs_email_to ON email_logs(email_to);
CREATE INDEX IF NOT EXISTS idx_email_logs_status ON email_logs(status);
CREATE INDEX IF NOT EXISTS idx_email_logs_enviado_em ON email_logs(enviado_em);

-- ========================================
-- FUNÇÕES E TRIGGERS
-- ========================================

-- Função para atualizar updated_at automaticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Triggers para updated_at
CREATE TRIGGER update_lead_sources_updated_at BEFORE UPDATE ON lead_sources 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_leads_updated_at BEFORE UPDATE ON leads 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_email_templates_updated_at BEFORE UPDATE ON email_templates 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_email_campaigns_updated_at BEFORE UPDATE ON email_campaigns 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ========================================
-- DADOS INICIAIS
-- ========================================

-- Inserir fontes de leads dos desafios
INSERT INTO lead_sources (slug, nome, descricao) VALUES 
    ('desafio-vendas', 'Desafio SaaS Vendas', 'Landing page do desafio de vendas'),
    ('desafio-financeiro', 'Desafio SaaS Financeiro', 'Landing page do desafio financeiro'),
    ('desafio-leads', 'Desafio SaaS Leads', 'Landing page do desafio de leads'),
    ('site-geral', 'Site Principal', 'Outras páginas do site')
ON CONFLICT (slug) DO NOTHING;

-- Template de boas-vindas
INSERT INTO email_templates (nome, slug, assunto, corpo_html, corpo_texto, variaveis) VALUES 
    (
        'Boas Vindas Desafio',
        'boas-vindas-desafio',
        '🔥 {{nome}}, seu lugar no {{desafio}} está garantido!',
        '
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <style>
                body { font-family: Arial, sans-serif; background: #000; color: #fff; margin: 0; padding: 20px; }
                .container { max-width: 600px; margin: 0 auto; background: #1a1a1a; padding: 30px; border: 2px solid #ff6b35; }
                .header { text-align: center; color: #ff6b35; font-size: 24px; font-weight: bold; margin-bottom: 20px; }
                .content { line-height: 1.6; font-size: 16px; }
                .highlight { color: #ff6b35; font-weight: bold; }
                .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #333; text-align: center; color: #888; }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="header">🔥 {{nome}}, você está dentro!</div>
                <div class="content">
                    <p>Seu lugar no <span class="highlight">{{desafio}}</span> está garantido!</p>
                    
                    <p><strong>Próximos passos:</strong></p>
                    <ul>
                        <li>Aguarde o início do desafio</li>
                        <li>Prepare seu ambiente de desenvolvimento</li>
                        <li>Tenha uma VPS e domínio prontos (te ajudo com isso)</li>
                    </ul>
                    
                    <p>Enquanto isso, <span class="highlight">alguma dúvida específica?</span></p>
                    <p>Responda este email que eu te ajudo.</p>
                    
                    <p>Aqui é produto real. Aqui é guerra. 🔥</p>
                </div>
                <div class="footer">
                    <p><strong>Diogo Costa</strong><br>Fundador DC360</p>
                </div>
            </div>
        </body>
        </html>',
        '{{nome}}, você está dentro!

Seu lugar no {{desafio}} está garantido!

Próximos passos:
- Aguarde o início do desafio
- Prepare seu ambiente de desenvolvimento  
- Tenha uma VPS e domínio prontos (te ajudo com isso)

Enquanto isso, alguma dúvida específica?
Responda este email que eu te ajudo.

Aqui é produto real. Aqui é guerra.

Diogo Costa
Fundador DC360',
        ARRAY['nome', 'desafio', 'email']
    )
ON CONFLICT (slug) DO NOTHING;

-- ========================================
-- VIEWS ÚTEIS
-- ========================================

-- View com estatísticas de leads por desafio
CREATE OR REPLACE VIEW vw_leads_stats AS
SELECT 
    l.desafio_slug,
    ls.nome as fonte_nome,
    COUNT(*) as total_leads,
    COUNT(*) FILTER (WHERE l.opt_in = true) as opt_in_leads,
    COUNT(*) FILTER (WHERE l.status = 'novo') as leads_novos,
    COUNT(*) FILTER (WHERE l.status = 'qualificado') as leads_qualificados,
    COUNT(*) FILTER (WHERE l.created_at >= CURRENT_DATE) as leads_hoje,
    COUNT(*) FILTER (WHERE l.created_at >= CURRENT_DATE - INTERVAL '7 days') as leads_ultima_semana
FROM leads l
LEFT JOIN lead_sources ls ON l.source_id = ls.id
GROUP BY l.desafio_slug, ls.nome;

-- View com histórico de emails por lead
CREATE OR REPLACE VIEW vw_lead_email_history AS
SELECT 
    l.id as lead_id,
    l.nome,
    l.email,
    l.desafio_slug,
    el.assunto,
    el.status as email_status,
    el.enviado_em,
    el.aberto_em,
    el.clicado_em,
    et.nome as template_nome
FROM leads l
LEFT JOIN email_logs el ON l.id = el.lead_id
LEFT JOIN email_templates et ON el.template_id = et.id
ORDER BY l.id, el.enviado_em DESC;

-- ========================================
-- PERMISSÕES
-- ========================================

-- Garantir que o usuário da aplicação tenha acesso
-- (Ajustar conforme necessário para o usuário da aplicação)
GRANT USAGE ON SCHEMA leads_system TO dcov3rl0rd;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA leads_system TO dcov3rl0rd;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA leads_system TO dcov3rl0rd;

-- ========================================
-- SCRIPT CONCLUÍDO
-- ========================================
-- Para executar: psql -h 10.10.0.8 -U dcov3rl0rd -d leads -f 001_create_schema_and_tables.sql 