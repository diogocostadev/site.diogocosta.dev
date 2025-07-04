-- =====================================================
-- SCRIPT: 004_create_pdf_downloads_table.sql
-- DESCRIÇÃO: Criar tabela para tracking de downloads de PDF
-- VERSÃO: 1.0
-- DATA: 2024-12-27
-- =====================================================

-- Usar o schema leads_system
SET search_path TO leads_system;

-- TABELA: pdf_downloads
-- Armazena informações detalhadas sobre downloads de PDFs
CREATE TABLE IF NOT EXISTS pdf_downloads (
    id SERIAL PRIMARY KEY,
    arquivo_nome VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    ip_address VARCHAR(45), -- Suporte para IPv6
    user_agent VARCHAR(500),
    referer VARCHAR(500),
    origem VARCHAR(100), -- Ex: download_direto_obrigado, email_link, etc.
    pais VARCHAR(2), -- Código ISO do país
    cidade VARCHAR(100),
    dispositivo VARCHAR(50), -- mobile, desktop, tablet
    navegador VARCHAR(50),
    sistema_operacional VARCHAR(50),
    sucesso BOOLEAN DEFAULT TRUE,
    tamanho_arquivo BIGINT,
    tempo_download_ms INTEGER,
    dados_extra JSONB, -- Dados adicionais em formato JSON
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    lead_id INTEGER,
    
    -- Relacionamento com leads (opcional)
    CONSTRAINT fk_pdf_downloads_lead_id 
        FOREIGN KEY (lead_id) 
        REFERENCES leads(id) 
        ON DELETE SET NULL
);

-- ÍNDICES para otimizar consultas
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_arquivo_nome ON pdf_downloads(arquivo_nome);
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_email ON pdf_downloads(email);
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_created_at ON pdf_downloads(created_at);
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_lead_id ON pdf_downloads(lead_id);
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_ip_address ON pdf_downloads(ip_address);
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_origem ON pdf_downloads(origem);
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_sucesso ON pdf_downloads(sucesso);

-- Índice composto para consultas de relatórios
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_arquivo_data ON pdf_downloads(arquivo_nome, created_at);
CREATE INDEX IF NOT EXISTS idx_pdf_downloads_origem_data ON pdf_downloads(origem, created_at);

-- COMENTÁRIOS nas colunas
COMMENT ON TABLE pdf_downloads IS 'Registros de downloads de arquivos PDF';
COMMENT ON COLUMN pdf_downloads.arquivo_nome IS 'Nome do arquivo PDF baixado';
COMMENT ON COLUMN pdf_downloads.email IS 'Email do usuário que fez download (quando disponível)';
COMMENT ON COLUMN pdf_downloads.ip_address IS 'Endereço IP do usuário';
COMMENT ON COLUMN pdf_downloads.user_agent IS 'User Agent do navegador';
COMMENT ON COLUMN pdf_downloads.referer IS 'URL de origem do download';
COMMENT ON COLUMN pdf_downloads.origem IS 'Contexto do download (ex: download_direto_obrigado)';
COMMENT ON COLUMN pdf_downloads.pais IS 'Código ISO do país (2 letras)';
COMMENT ON COLUMN pdf_downloads.cidade IS 'Cidade do usuário';
COMMENT ON COLUMN pdf_downloads.dispositivo IS 'Tipo de dispositivo (mobile/desktop/tablet)';
COMMENT ON COLUMN pdf_downloads.navegador IS 'Navegador utilizado';
COMMENT ON COLUMN pdf_downloads.sistema_operacional IS 'Sistema operacional';
COMMENT ON COLUMN pdf_downloads.sucesso IS 'Se o download foi concluído com sucesso';
COMMENT ON COLUMN pdf_downloads.tamanho_arquivo IS 'Tamanho do arquivo em bytes';
COMMENT ON COLUMN pdf_downloads.tempo_download_ms IS 'Tempo de download em milissegundos';
COMMENT ON COLUMN pdf_downloads.dados_extra IS 'Dados adicionais em formato JSON';
COMMENT ON COLUMN pdf_downloads.lead_id IS 'ID do lead associado (opcional)';

-- =====================================================
-- INSERIR DADOS DE EXEMPLO (OPCIONAL - PARA TESTES)
-- =====================================================

-- Exemplo de dados para desenvolvimento/teste
-- DESCOMENTE as linhas abaixo se quiser dados de exemplo

/*
INSERT INTO pdf_downloads (
    arquivo_nome, 
    email, 
    ip_address, 
    origem, 
    dispositivo, 
    navegador,
    sucesso,
    dados_extra
) VALUES 
(
    'Manual_da_Primeira_Virada_Diogo_Costa.pdf',
    'teste@example.com',
    '127.0.0.1',
    'download_direto_obrigado',
    'desktop',
    'Chrome',
    true,
    '{"teste": true, "ambiente": "desenvolvimento"}'
),
(
    'Manual_da_Primeira_Virada_Diogo_Costa.pdf',
    'usuario@email.com',
    '192.168.1.100',
    'email_link',
    'mobile',
    'Safari',
    true,
    '{"fonte": "email", "campanha": "desbloqueio_mental"}'
);
*/

-- =====================================================
-- VIEWS PARA RELATÓRIOS
-- =====================================================

-- View para estatísticas básicas
CREATE OR REPLACE VIEW vw_pdf_downloads_stats AS
SELECT 
    COUNT(*) as total_downloads,
    COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE) as downloads_hoje,
    COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE - INTERVAL '7 days') as downloads_ultima_semana,
    COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE - INTERVAL '30 days') as downloads_ultimo_mes,
    COUNT(DISTINCT email) FILTER (WHERE email IS NOT NULL) as usuarios_unicos,
    COUNT(DISTINCT ip_address) as ips_unicos
FROM pdf_downloads;

-- View para downloads por dia (últimos 30 dias)
CREATE OR REPLACE VIEW vw_pdf_downloads_por_dia AS
SELECT 
    DATE(created_at) as data,
    COUNT(*) as quantidade,
    COUNT(DISTINCT email) FILTER (WHERE email IS NOT NULL) as usuarios_unicos,
    COUNT(DISTINCT ip_address) as ips_unicos
FROM pdf_downloads 
WHERE created_at >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY DATE(created_at)
ORDER BY data DESC;

-- View para downloads por origem
CREATE OR REPLACE VIEW vw_pdf_downloads_por_origem AS
SELECT 
    COALESCE(origem, 'nao_informado') as origem,
    COUNT(*) as quantidade,
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (), 2) as percentual
FROM pdf_downloads
GROUP BY origem
ORDER BY quantidade DESC;

-- View para análise de dispositivos
CREATE OR REPLACE VIEW vw_pdf_downloads_por_dispositivo AS
SELECT 
    COALESCE(dispositivo, 'nao_informado') as dispositivo,
    COUNT(*) as quantidade,
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (), 2) as percentual
FROM pdf_downloads
GROUP BY dispositivo
ORDER BY quantidade DESC;

-- =====================================================
-- FUNÇÕES AUXILIARES
-- =====================================================

-- Função para obter estatísticas de um período específico
CREATE OR REPLACE FUNCTION get_pdf_download_stats(
    data_inicio DATE DEFAULT CURRENT_DATE - INTERVAL '30 days',
    data_fim DATE DEFAULT CURRENT_DATE + INTERVAL '1 day'
) 
RETURNS JSON AS $$
DECLARE
    resultado JSON;
BEGIN
    SELECT json_build_object(
        'periodo', json_build_object(
            'inicio', data_inicio,
            'fim', data_fim
        ),
        'total_downloads', COUNT(*),
        'downloads_sucesso', COUNT(*) FILTER (WHERE sucesso = true),
        'downloads_falha', COUNT(*) FILTER (WHERE sucesso = false),
        'usuarios_unicos', COUNT(DISTINCT email) FILTER (WHERE email IS NOT NULL),
        'ips_unicos', COUNT(DISTINCT ip_address),
        'arquivo_mais_baixado', (
            SELECT arquivo_nome 
            FROM pdf_downloads 
            WHERE created_at BETWEEN data_inicio AND data_fim
            GROUP BY arquivo_nome 
            ORDER BY COUNT(*) DESC 
            LIMIT 1
        ),
        'origem_mais_comum', (
            SELECT origem 
            FROM pdf_downloads 
            WHERE created_at BETWEEN data_inicio AND data_fim 
            AND origem IS NOT NULL
            GROUP BY origem 
            ORDER BY COUNT(*) DESC 
            LIMIT 1
        ),
        'dispositivo_mais_comum', (
            SELECT dispositivo 
            FROM pdf_downloads 
            WHERE created_at BETWEEN data_inicio AND data_fim 
            AND dispositivo IS NOT NULL
            GROUP BY dispositivo 
            ORDER BY COUNT(*) DESC 
            LIMIT 1
        )
    ) INTO resultado
    FROM pdf_downloads
    WHERE created_at BETWEEN data_inicio AND data_fim;
    
    RETURN resultado;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- SCRIPT EXECUTADO COM SUCESSO
-- =====================================================

SELECT 'Tabela pdf_downloads criada com sucesso!' as mensagem; 