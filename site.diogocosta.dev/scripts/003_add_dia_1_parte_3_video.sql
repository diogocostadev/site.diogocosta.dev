-- ========================================
-- SCRIPT 003 - ADD DIA 1 PARTE 3 VIDEO
-- Versão: 1.0.0
-- Data: 2025-06-27
-- Descrição: Adicionar novo vídeo "dia-1-parte-3" e configurar como teste para vsl-criar-saas
-- ========================================

-- Usar o schema leads_system
SET search_path TO leads_system;

-- ========================================
-- INSERIR NOVO VÍDEO
-- ========================================
INSERT INTO vsl_videos (
    slug, 
    nome, 
    descricao, 
    video_url, 
    formato, 
    ambiente,
    ativo,
    created_at,
    updated_at
) VALUES (
    'dia-1-parte-3',
    'Vídeo Dia 1 Parte 3 - Teste VSL',
    'Vídeo de teste para VSL - Dia 1 Parte 3 do conteúdo',
    'https://videos.diogocosta.dev/hls/dia-1-parte-3/stream.m3u8',
    'hls',
    'teste',
    TRUE,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
)
ON CONFLICT (slug) DO UPDATE SET
    nome = EXCLUDED.nome,
    descricao = EXCLUDED.descricao,
    video_url = EXCLUDED.video_url,
    formato = EXCLUDED.formato,
    ambiente = EXCLUDED.ambiente,
    ativo = EXCLUDED.ativo,
    updated_at = CURRENT_TIMESTAMP;

-- ========================================
-- ATUALIZAR VSL PARA USAR O NOVO VÍDEO DE TESTE
-- ========================================

-- Primeiro, substituir o vídeo de teste atual (ID 2) pelo novo vídeo
UPDATE vsl_configs 
SET 
    video_id_teste = (SELECT id FROM vsl_videos WHERE slug = 'dia-1-parte-3'),
    ambiente_ativo = 'teste', -- Ativar modo teste para ver o novo vídeo imediatamente
    updated_at = CURRENT_TIMESTAMP
WHERE slug = 'vsl-criar-saas';

-- ========================================
-- VERIFICAR OS RESULTADOS
-- ========================================

-- Mostrar o vídeo recém-adicionado
SELECT 
    'NOVO VÍDEO ADICIONADO:' as status,
    id,
    slug,
    nome,
    video_url,
    ambiente,
    ativo
FROM vsl_videos 
WHERE slug = 'dia-1-parte-3';

-- Mostrar a configuração da VSL atualizada
SELECT 
    'VSL ATUALIZADA:' as status,
    vc.slug as vsl_slug,
    vc.nome as vsl_nome,
    vc.ambiente_ativo,
    -- Vídeo de produção
    vv_prod.slug as video_producao_slug,
    vv_prod.video_url as video_producao_url,
    -- Vídeo de teste
    vv_teste.slug as video_teste_slug,
    vv_teste.video_url as video_teste_url
FROM vsl_configs vc
LEFT JOIN vsl_videos vv_prod ON vc.video_id = vv_prod.id
LEFT JOIN vsl_videos vv_teste ON vc.video_id_teste = vv_teste.id
WHERE vc.slug = 'vsl-criar-saas';

-- ========================================
-- FUNÇÃO PARA ALTERNAR ENTRE TESTE E PRODUÇÃO
-- ========================================

-- Verificar o status atual após as alterações
SELECT 
    '🎬 STATUS APÓS ATUALIZAÇÃO:' as status,
    vc.slug as vsl_slug,
    vc.ambiente_ativo,
    CASE 
        WHEN vc.ambiente_ativo = 'teste' THEN '🔴 MODO TESTE ATIVO'
        ELSE '🟢 MODO PRODUÇÃO ATIVO'
    END as modo_atual,
    CASE 
        WHEN vc.ambiente_ativo = 'teste' THEN vv_teste.video_url
        ELSE vv_prod.video_url
    END as video_ativo_url,
    CASE 
        WHEN vc.ambiente_ativo = 'teste' THEN vv_teste.slug
        ELSE vv_prod.slug
    END as video_ativo_slug
FROM vsl_configs vc
LEFT JOIN vsl_videos vv_prod ON vc.video_id = vv_prod.id
LEFT JOIN vsl_videos vv_teste ON vc.video_id_teste = vv_teste.id
WHERE vc.slug = 'vsl-criar-saas';

-- ========================================
-- COMANDOS ÚTEIS PARA GERENCIAMENTO
-- ========================================

-- ⚠️ IMPORTANTE: APÓS EXECUTAR ESTE SCRIPT, A VSL ESTARÁ EM MODO TESTE
-- Isso significa que ao acessar http://localhost:5000/vsl-criar-saas você verá o novo vídeo!

-- Para voltar para produção depois dos testes:
-- SELECT trocar_ambiente_vsl('vsl-criar-saas', 'producao');

-- Para verificar todos os vídeos disponíveis:
-- SELECT id, slug, nome, video_url, ambiente, ativo FROM vsl_videos ORDER BY created_at DESC;

-- Para verificar todas as VSLs e seus vídeos:
-- SELECT * FROM vw_vsl_completa ORDER BY created_at DESC;

-- Para alternar rapidamente entre os ambientes:
-- SELECT trocar_ambiente_vsl('vsl-criar-saas', 'teste');    -- Ver o novo vídeo
-- SELECT trocar_ambiente_vsl('vsl-criar-saas', 'producao'); -- Voltar para o vídeo original

-- ========================================
-- Para executar: psql -h 10.10.0.8 -U dcov3rl0rd -d leads -f 003_add_dia_1_parte_3_video.sql
-- ========================================
