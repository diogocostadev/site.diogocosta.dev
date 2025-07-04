# =====================================================
# SCRIPT: verificar-email-downloads.ps1
# DESCRIÇÃO: Verificar se emails estão sendo gravados nos downloads
# VERSÃO: 1.0
# DATA: 2024-12-27
# =====================================================

Write-Host "🔍 VERIFICANDO SE EMAILS ESTÃO SENDO GRAVADOS NOS DOWNLOADS" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# Consulta SQL para verificar downloads com e sem email
$query = @"
SET search_path TO leads_system;

-- Estatísticas de emails nos downloads
SELECT 
    'Downloads com email' as tipo,
    COUNT(*) as quantidade,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM pdf_downloads), 2) as percentual
FROM pdf_downloads 
WHERE email IS NOT NULL AND email != ''

UNION ALL

SELECT 
    'Downloads sem email' as tipo,
    COUNT(*) as quantidade,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM pdf_downloads), 2) as percentual
FROM pdf_downloads 
WHERE email IS NULL OR email = ''

UNION ALL

SELECT 
    'Total de downloads' as tipo,
    COUNT(*) as quantidade,
    100.00 as percentual
FROM pdf_downloads;
"@

Write-Host "📊 CONSULTA PARA EXECUTAR NO BANCO:" -ForegroundColor Yellow
Write-Host $query -ForegroundColor Green

Write-Host ""
Write-Host "📋 CONSULTA DETALHADA - ÚLTIMOS 10 DOWNLOADS:" -ForegroundColor Yellow

$queryDetalhes = @"
SET search_path TO leads_system;

SELECT 
    created_at,
    CASE 
        WHEN email IS NOT NULL AND email != '' THEN email 
        ELSE '❌ SEM EMAIL' 
    END as email_status,
    COALESCE(origem, 'não_informado') as origem,
    dispositivo,
    navegador,
    ip_address
FROM pdf_downloads 
ORDER BY created_at DESC 
LIMIT 10;
"@

Write-Host $queryDetalhes -ForegroundColor Green

Write-Host ""
Write-Host "🎯 COMO TESTAR O SISTEMA:" -ForegroundColor Magenta
Write-Host "=========================" -ForegroundColor Magenta
Write-Host "1. Acesse: http://localhost:5000/desbloqueio" -ForegroundColor White
Write-Host "2. Cadastre um email de teste" -ForegroundColor White
Write-Host "3. Na página de obrigado, clique em 'Download Direto'" -ForegroundColor White
Write-Host "4. Execute esta consulta novamente para verificar" -ForegroundColor White

Write-Host ""
Write-Host "📧 TESTE VIA EMAIL:" -ForegroundColor Magenta
Write-Host "==================" -ForegroundColor Magenta
Write-Host "1. Verifique se recebeu o email com o link" -ForegroundColor White
Write-Host "2. O link agora deve incluir ?email=seu@email.com" -ForegroundColor White
Write-Host "3. Clique no link do email para testar" -ForegroundColor White

Write-Host ""
Write-Host "🚀 MELHORIAS IMPLEMENTADAS:" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green
Write-Host "✅ Email incluído na URL do email enviado" -ForegroundColor DarkGreen
Write-Host "✅ Email passado via sessão para página de obrigado" -ForegroundColor DarkGreen
Write-Host "✅ Recuperação automática de email por IP (últimas 24h)" -ForegroundColor DarkGreen
Write-Host "✅ Melhor rastreamento de origem do download" -ForegroundColor DarkGreen
Write-Host "✅ Logs mais detalhados para debug" -ForegroundColor DarkGreen 