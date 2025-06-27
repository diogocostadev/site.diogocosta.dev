-- ========================================
-- SCRIPT 002 - CREATE VSL VIDEOS TABLE
-- Versão: 1.0.0
-- Data: 2025-01-27
-- Descrição: Criação da tabela para gerenciar vídeos das VSLs
-- ========================================

-- Usar o schema leads_system
SET search_path TO leads_system;

-- ========================================
-- TABELA: vsl_videos
-- Descrição: Configuração dos vídeos para as VSLs
-- ========================================
CREATE TABLE IF NOT EXISTS vsl_videos (
    id SERIAL PRIMARY KEY,
    slug VARCHAR(100) NOT NULL UNIQUE,
    nome VARCHAR(200) NOT NULL,
    descricao TEXT,
    video_url TEXT NOT NULL,
    thumbnail_url TEXT,
    duracao_segundos INTEGER,
    formato VARCHAR(50) DEFAULT 'hls', -- 'hls', 'mp4', 'youtube', etc
    qualidade VARCHAR(50) DEFAULT '1080p',
    tamanho_bytes BIGINT,
    ativo BOOLEAN DEFAULT TRUE,
    ambiente VARCHAR(20) DEFAULT 'producao', -- 'teste', 'homologacao', 'producao'
    observacoes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ========================================
-- TABELA: vsl_configs
-- Descrição: Configurações das VSLs e associação com vídeos
-- ========================================
CREATE TABLE IF NOT EXISTS vsl_configs (
    id SERIAL PRIMARY KEY,
    slug VARCHAR(100) NOT NULL UNIQUE,
    nome VARCHAR(200) NOT NULL,
    titulo VARCHAR(500) NOT NULL,
    subtitulo TEXT,
    descricao TEXT,
    video_id INTEGER REFERENCES vsl_videos(id),
    video_id_teste INTEGER REFERENCES vsl_videos(id), -- Vídeo para testes
    preco_original DECIMAL(10,2),
    preco_promocional DECIMAL(10,2),
    checkout_url TEXT,
    ativo BOOLEAN DEFAULT TRUE,
    ambiente_ativo VARCHAR(20) DEFAULT 'producao', -- 'teste', 'producao'
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ========================================
-- ÍNDICES PARA PERFORMANCE
-- ========================================
CREATE INDEX IF NOT EXISTS idx_vsl_videos_slug ON vsl_videos(slug);
CREATE INDEX IF NOT EXISTS idx_vsl_videos_ativo ON vsl_videos(ativo);
CREATE INDEX IF NOT EXISTS idx_vsl_videos_ambiente ON vsl_videos(ambiente);

CREATE INDEX IF NOT EXISTS idx_vsl_configs_slug ON vsl_configs(slug);
CREATE INDEX IF NOT EXISTS idx_vsl_configs_ativo ON vsl_configs(ativo);

-- ========================================
-- TRIGGERS PARA UPDATED_AT
-- ========================================
CREATE TRIGGER update_vsl_videos_updated_at BEFORE UPDATE ON vsl_videos 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_vsl_configs_updated_at BEFORE UPDATE ON vsl_configs 
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ========================================
-- DADOS INICIAIS
-- ========================================

-- Inserir vídeos iniciais
INSERT INTO vsl_videos (slug, nome, descricao, video_url, formato, ambiente) VALUES 
    (
        'comunidade-didaticos-001',
        'Vídeo VSL Principal - Produção',
        'Vídeo principal das VSLs em produção',
        'https://videos.diogocosta.dev/hls/comunidade-didaticos-001.m3u8',
        'hls',
        'producao'
    ),
    (
        'teste-2025-06-08',
        'Vídeo VSL Teste - 08/06/2025',
        'Vídeo de teste para validação da estrutura HLS',
        'https://videos.diogocosta.dev/hls/2025-06-08_15-58-24/stream.m3u8',
        'hls',
        'teste'
    )
ON CONFLICT (slug) DO UPDATE SET
    video_url = EXCLUDED.video_url,
    updated_at = CURRENT_TIMESTAMP;

-- Inserir configurações das VSLs
INSERT INTO vsl_configs (slug, nome, titulo, subtitulo, descricao, video_id, video_id_teste, preco_original, preco_promocional, checkout_url) VALUES 
    (
        'vsl-criar-saas',
        'VSL Criar SaaS',
        'De Zero ao Seu Primeiro SaaS Lucrativo em 7 Dias',
        'O método completo para transformar sua ideia em um SaaS lucrativo, mesmo se você nunca programou antes',
        'Sistema passo-a-passo para criar, lançar e monetizar seu primeiro SaaS',
        (SELECT id FROM vsl_videos WHERE slug = 'comunidade-didaticos-001'),
        (SELECT id FROM vsl_videos WHERE slug = 'teste-2025-06-08'),
        997.00,
        197.00,
        'https://pay.kiwify.com.br/1ToZyFr'
    ),
    (
        'vsl-dc360',
        'VSL DC360',
        'DC360 — Transforme Seu Código em Império Digital',
        'A única formação que ensina programadores a construir, operar e escalar SaaS de forma perpétua',
        'Pare de vender hora. Pare de depender de cliente. Pare de ser apenas dev. Torne-se dono de império digital.',
        (SELECT id FROM vsl_videos WHERE slug = 'comunidade-didaticos-001'),
        (SELECT id FROM vsl_videos WHERE slug = 'teste-2025-06-08'),
        2997.00,
        1497.00,
        'https://pay.kiwify.com.br/BclEImU'
    )
ON CONFLICT (slug) DO UPDATE SET
    titulo = EXCLUDED.titulo,
    subtitulo = EXCLUDED.subtitulo,
    descricao = EXCLUDED.descricao,
    preco_original = EXCLUDED.preco_original,
    preco_promocional = EXCLUDED.preco_promocional,
    checkout_url = EXCLUDED.checkout_url,
    updated_at = CURRENT_TIMESTAMP;

-- ========================================
-- VIEWS ÚTEIS
-- ========================================

-- View para facilitar consultas das VSLs com vídeos
CREATE OR REPLACE VIEW vw_vsl_completa AS
SELECT 
    vc.id,
    vc.slug,
    vc.nome,
    vc.titulo,
    vc.subtitulo,
    vc.descricao,
    vc.preco_original,
    vc.preco_promocional,
    vc.checkout_url,
    vc.ativo,
    vc.ambiente_ativo,
    -- Vídeo de produção
    vv_prod.video_url as video_url_producao,
    vv_prod.thumbnail_url as thumbnail_url_producao,
    vv_prod.duracao_segundos as duracao_producao,
    -- Vídeo de teste
    vv_teste.video_url as video_url_teste,
    vv_teste.thumbnail_url as thumbnail_url_teste,
    vv_teste.duracao_segundos as duracao_teste,
    -- URL do vídeo ativo baseado no ambiente
    CASE 
        WHEN vc.ambiente_ativo = 'teste' THEN vv_teste.video_url
        ELSE vv_prod.video_url
    END as video_url_ativo,
    vc.created_at,
    vc.updated_at
FROM vsl_configs vc
LEFT JOIN vsl_videos vv_prod ON vc.video_id = vv_prod.id
LEFT JOIN vsl_videos vv_teste ON vc.video_id_teste = vv_teste.id
WHERE vc.ativo = TRUE;

-- Comentários para documentação
COMMENT ON TABLE vsl_videos IS 'Tabela para armazenar os vídeos das VSLs com metadados';
COMMENT ON TABLE vsl_configs IS 'Tabela para configurar as VSLs e associar com vídeos';
COMMENT ON VIEW vw_vsl_completa IS 'View que junta VSL configs com vídeos para facilitar consultas';

COMMENT ON COLUMN vsl_configs.ambiente_ativo IS 'Define se usa vídeo de teste ou produção: teste, producao';
COMMENT ON COLUMN vsl_videos.ambiente IS 'Ambiente do vídeo: teste, homologacao, producao';

-- ========================================
-- PROCEDURE PARA TROCAR AMBIENTE
-- ========================================

-- Procedure para facilitar troca entre teste/produção
CREATE OR REPLACE FUNCTION trocar_ambiente_vsl(
    p_vsl_slug VARCHAR(100),
    p_novo_ambiente VARCHAR(20)
)
RETURNS TEXT AS $$
DECLARE
    resultado TEXT;
BEGIN
    -- Validar ambiente
    IF p_novo_ambiente NOT IN ('teste', 'producao') THEN
        RAISE EXCEPTION 'Ambiente deve ser: teste ou producao';
    END IF;
    
    -- Atualizar ambiente
    UPDATE vsl_configs 
    SET ambiente_ativo = p_novo_ambiente,
        updated_at = CURRENT_TIMESTAMP
    WHERE slug = p_vsl_slug AND ativo = TRUE;
    
    -- Verificar se foi atualizado
    IF FOUND THEN
        resultado := 'VSL ' || p_vsl_slug || ' alterada para ambiente: ' || p_novo_ambiente;
    ELSE
        resultado := 'VSL não encontrada: ' || p_vsl_slug;
    END IF;
    
    RETURN resultado;
END;
$$ LANGUAGE plpgsql;

-- Exemplo de uso da procedure:
-- SELECT trocar_ambiente_vsl('vsl-criar-saas', 'teste');
-- SELECT trocar_ambiente_vsl('vsl-criar-saas', 'producao');

-- ========================================
-- GRANTS (se necessário)
-- ========================================

-- Conceder permissões se houver usuário específico da aplicação
-- GRANT SELECT, INSERT, UPDATE ON vsl_videos TO app_user;
-- GRANT SELECT, INSERT, UPDATE ON vsl_configs TO app_user;
-- GRANT SELECT ON vw_vsl_completa TO app_user; 