-- Script para criar tabela de interessados nas lives do YouTube e Twitch
-- Arquivo: 005_create_interessados_lives_table.sql
-- Data: 2024-12-29
-- Descrição: Sistema para capturar e gerenciar interessados nas lives

-- Verificar se a tabela já existe
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'leads_system' 
        AND table_name = 'interessados_lives'
    ) THEN
        -- Criar a tabela de interessados nas lives
        CREATE TABLE leads_system.interessados_lives (
            id SERIAL PRIMARY KEY,
            nome VARCHAR(200) NOT NULL,
            email VARCHAR(320) NULL,
            whatsapp VARCHAR(20) NULL,
            codigo_pais VARCHAR(5) NOT NULL DEFAULT '+55',
            data_cadastro TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
            data_descadastro TIMESTAMP WITH TIME ZONE NULL,
            ativo BOOLEAN NOT NULL DEFAULT TRUE,
            ip_address VARCHAR(45) NULL,
            user_agent VARCHAR(500) NULL,
            origem VARCHAR(50) NULL,
            boas_vindas_email_enviado BOOLEAN NOT NULL DEFAULT FALSE,
            boas_vindas_whatsapp_enviado BOOLEAN NOT NULL DEFAULT FALSE,
            data_boas_vindas_email TIMESTAMP WITH TIME ZONE NULL,
            data_boas_vindas_whatsapp TIMESTAMP WITH TIME ZONE NULL,
            
            -- Constraints
            CONSTRAINT ck_interessados_lives_tem_contato CHECK (
                email IS NOT NULL OR whatsapp IS NOT NULL
            ),
            CONSTRAINT ck_interessados_lives_email_format CHECK (
                email IS NULL OR email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'
            ),
            CONSTRAINT ck_interessados_lives_whatsapp_format CHECK (
                whatsapp IS NULL OR whatsapp ~ '^[0-9]{8,15}$'
            ),
            CONSTRAINT ck_interessados_lives_codigo_pais_format CHECK (
                codigo_pais ~ '^\+[0-9]{1,4}$'
            ),
            CONSTRAINT ck_interessados_lives_origem_valida CHECK (
                origem IS NULL OR origem IN ('youtube', 'twitch', 'site', 'externo')
            )
        );

        -- Criar índices para performance
        CREATE INDEX idx_interessados_lives_email ON leads_system.interessados_lives (email) 
        WHERE email IS NOT NULL;
        
        CREATE INDEX idx_interessados_lives_whatsapp ON leads_system.interessados_lives (whatsapp) 
        WHERE whatsapp IS NOT NULL;
        
        CREATE INDEX idx_interessados_lives_ativo ON leads_system.interessados_lives (ativo);
        
        CREATE INDEX idx_interessados_lives_data_cadastro ON leads_system.interessados_lives (data_cadastro);
        
        CREATE INDEX idx_interessados_lives_origem ON leads_system.interessados_lives (origem) 
        WHERE origem IS NOT NULL;
        
        CREATE INDEX idx_interessados_lives_boas_vindas ON leads_system.interessados_lives (boas_vindas_email_enviado, boas_vindas_whatsapp_enviado);

        -- Índice único para evitar duplicatas de email ativo
        CREATE UNIQUE INDEX idx_interessados_lives_email_ativo_unique 
        ON leads_system.interessados_lives (email) 
        WHERE ativo = TRUE AND email IS NOT NULL;

        -- Índice único para evitar duplicatas de whatsapp ativo
        CREATE UNIQUE INDEX idx_interessados_lives_whatsapp_ativo_unique 
        ON leads_system.interessados_lives (codigo_pais, whatsapp) 
        WHERE ativo = TRUE AND whatsapp IS NOT NULL;

        -- Comentários na tabela
        COMMENT ON TABLE leads_system.interessados_lives IS 'Tabela para armazenar interessados nas lives do YouTube e Twitch';
        COMMENT ON COLUMN leads_system.interessados_lives.id IS 'ID único do interessado';
        COMMENT ON COLUMN leads_system.interessados_lives.nome IS 'Nome completo do interessado';
        COMMENT ON COLUMN leads_system.interessados_lives.email IS 'Email do interessado (opcional)';
        COMMENT ON COLUMN leads_system.interessados_lives.whatsapp IS 'Número do WhatsApp sem código do país (opcional)';
        COMMENT ON COLUMN leads_system.interessados_lives.codigo_pais IS 'Código do país para WhatsApp (ex: +55)';
        COMMENT ON COLUMN leads_system.interessados_lives.data_cadastro IS 'Data e hora do cadastro';
        COMMENT ON COLUMN leads_system.interessados_lives.data_descadastro IS 'Data e hora do descadastro (se aplicável)';
        COMMENT ON COLUMN leads_system.interessados_lives.ativo IS 'Se o interessado está ativo na lista';
        COMMENT ON COLUMN leads_system.interessados_lives.ip_address IS 'IP do usuário no momento do cadastro';
        COMMENT ON COLUMN leads_system.interessados_lives.user_agent IS 'User Agent do navegador';
        COMMENT ON COLUMN leads_system.interessados_lives.origem IS 'Origem do cadastro (youtube, twitch, site, externo)';
        COMMENT ON COLUMN leads_system.interessados_lives.boas_vindas_email_enviado IS 'Se o email de boas-vindas foi enviado';
        COMMENT ON COLUMN leads_system.interessados_lives.boas_vindas_whatsapp_enviado IS 'Se o WhatsApp de boas-vindas foi enviado';
        COMMENT ON COLUMN leads_system.interessados_lives.data_boas_vindas_email IS 'Data do envio do email de boas-vindas';
        COMMENT ON COLUMN leads_system.interessados_lives.data_boas_vindas_whatsapp IS 'Data do envio do WhatsApp de boas-vindas';

        RAISE NOTICE 'Tabela interessados_lives criada com sucesso!';
    ELSE
        RAISE NOTICE 'Tabela interessados_lives já existe!';
    END IF;
END
$$;

-- Criar função para obter estatísticas dos interessados
CREATE OR REPLACE FUNCTION leads_system.get_interessados_lives_stats()
RETURNS TABLE (
    total_interessados INTEGER,
    total_ativos INTEGER,
    total_desativados INTEGER,
    com_email INTEGER,
    com_whatsapp INTEGER,
    com_ambos INTEGER,
    cadastros_hoje INTEGER,
    cadastros_ultima_semana INTEGER,
    cadastros_ultimo_mes INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::INTEGER as total_interessados,
        COUNT(*) FILTER (WHERE ativo = TRUE)::INTEGER as total_ativos,
        COUNT(*) FILTER (WHERE ativo = FALSE)::INTEGER as total_desativados,
        COUNT(*) FILTER (WHERE ativo = TRUE AND email IS NOT NULL)::INTEGER as com_email,
        COUNT(*) FILTER (WHERE ativo = TRUE AND whatsapp IS NOT NULL)::INTEGER as com_whatsapp,
        COUNT(*) FILTER (WHERE ativo = TRUE AND email IS NOT NULL AND whatsapp IS NOT NULL)::INTEGER as com_ambos,
        COUNT(*) FILTER (WHERE data_cadastro::DATE = CURRENT_DATE)::INTEGER as cadastros_hoje,
        COUNT(*) FILTER (WHERE data_cadastro >= CURRENT_DATE - INTERVAL '7 days')::INTEGER as cadastros_ultima_semana,
        COUNT(*) FILTER (WHERE data_cadastro >= CURRENT_DATE - INTERVAL '30 days')::INTEGER as cadastros_ultimo_mes
    FROM leads_system.interessados_lives;
END;
$$ LANGUAGE plpgsql;

-- Comentário na função
COMMENT ON FUNCTION leads_system.get_interessados_lives_stats() IS 'Função para obter estatísticas dos interessados nas lives';

-- Exemplo de uso da função
-- SELECT * FROM leads_system.get_interessados_lives_stats();

-- Consultas úteis para administração:

-- 1. Listar todos os interessados ativos
/*
SELECT 
    id,
    nome,
    email,
    CASE 
        WHEN whatsapp IS NOT NULL THEN codigo_pais || ' ' || whatsapp
        ELSE NULL 
    END as whatsapp_formatado,
    origem,
    data_cadastro,
    boas_vindas_email_enviado,
    boas_vindas_whatsapp_enviado
FROM leads_system.interessados_lives 
WHERE ativo = TRUE 
ORDER BY data_cadastro DESC;
*/

-- 2. Interessados por origem
/*
SELECT 
    origem,
    COUNT(*) as quantidade,
    COUNT(*) FILTER (WHERE email IS NOT NULL) as com_email,
    COUNT(*) FILTER (WHERE whatsapp IS NOT NULL) as com_whatsapp
FROM leads_system.interessados_lives 
WHERE ativo = TRUE 
GROUP BY origem 
ORDER BY quantidade DESC;
*/

-- 3. Cadastros por dia (últimos 30 dias)
/*
SELECT 
    data_cadastro::DATE as data,
    COUNT(*) as cadastros,
    COUNT(*) FILTER (WHERE email IS NOT NULL) as com_email,
    COUNT(*) FILTER (WHERE whatsapp IS NOT NULL) as com_whatsapp
FROM leads_system.interessados_lives 
WHERE data_cadastro >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY data_cadastro::DATE 
ORDER BY data DESC;
*/

-- 4. Interessados sem boas-vindas enviadas
/*
SELECT 
    id,
    nome,
    email,
    whatsapp,
    data_cadastro,
    boas_vindas_email_enviado,
    boas_vindas_whatsapp_enviado
FROM leads_system.interessados_lives 
WHERE ativo = TRUE 
AND (
    (email IS NOT NULL AND boas_vindas_email_enviado = FALSE) OR
    (whatsapp IS NOT NULL AND boas_vindas_whatsapp_enviado = FALSE)
)
ORDER BY data_cadastro DESC;
*/

-- 5. Relatório de descadastros
/*
SELECT 
    DATE_TRUNC('day', data_descadastro) as dia,
    COUNT(*) as descadastros
FROM leads_system.interessados_lives 
WHERE data_descadastro IS NOT NULL 
AND data_descadastro >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY DATE_TRUNC('day', data_descadastro)
ORDER BY dia DESC;
*/

RAISE NOTICE 'Script executado com sucesso!';
RAISE NOTICE 'Tabela: leads_system.interessados_lives';
RAISE NOTICE 'Função: leads_system.get_interessados_lives_stats()';
RAISE NOTICE 'Para obter estatísticas, execute: SELECT * FROM leads_system.get_interessados_lives_stats();'; 