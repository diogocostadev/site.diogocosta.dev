# =====================================================
# SCRIPT: ver-downloads-completo.ps1
# DESCRIÇÃO: Ver downloads com localização (cidade/país)
# VERSÃO: 1.0
# DATA: 2024-12-27
# =====================================================

Write-Host "🌍 DOWNLOADS COM LOCALIZAÇÃO GEOGRÁFICA" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Consulta SQL para ver downloads com localização
$query = @"
SET search_path TO leads_system;

-- Últimos 10 downloads com localização
SELECT 
    id,
    arquivo_nome,
    COALESCE(email, 'anônimo') as email,
    ip_address,
    COALESCE(cidade, 'N/A') as cidade,
    COALESCE(pais, 'N/A') as pais,
    origem,
    dispositivo,
    navegador,
    created_at
FROM pdf_downloads 
ORDER BY created_at DESC 
LIMIT 10;
"@

Write-Host "📊 Executando consulta no PostgreSQL..." -ForegroundColor Yellow

try {
    # Tentar obter string de conexão do appsettings.json
    $appsettingsPath = "appsettings.json"
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        $connectionString = $appsettings.ConnectionStrings."conexao-site"
        
        if ($connectionString) {
            Write-Host "✅ String de conexão encontrada" -ForegroundColor Green
            
            # Salvar query em arquivo temporário
            $queryFile = "temp_query.sql"
            $query | Out-File -FilePath $queryFile -Encoding UTF8
            
            Write-Host "🔄 Executando consulta..." -ForegroundColor Blue
            
            # Executar via psql se disponível
            if (Get-Command psql -ErrorAction SilentlyContinue) {
                psql $connectionString -f $queryFile
            } else {
                Write-Host "⚠️ psql não encontrado. Instale PostgreSQL CLI ou use outro cliente" -ForegroundColor Yellow
                Write-Host "📄 Query salva em: $queryFile" -ForegroundColor Yellow
                Write-Host ""
                Write-Host "📋 Query para executar manualmente:" -ForegroundColor Cyan
                Write-Host $query -ForegroundColor White
            }
            
            # Limpar arquivo temporário
            if (Test-Path $queryFile) {
                Remove-Item $queryFile
            }
        } else {
            Write-Host "❌ String de conexão não encontrada" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Arquivo appsettings.json não encontrado" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🌐 Alternativa: Ver via endpoint da aplicação" -ForegroundColor Green
Write-Host "   http://localhost:5000/desbloqueio/downloads-stats" -ForegroundColor White

Write-Host ""
Write-Host "📱 O que verificar:" -ForegroundColor Yellow
Write-Host "   ✅ Campo 'cidade' preenchido" -ForegroundColor White
Write-Host "   ✅ Campo 'pais' preenchido" -ForegroundColor White
Write-Host "   ✅ Campo 'email' preenchido" -ForegroundColor White
Write-Host "   ✅ IP não local (não 127.0.0.1)" -ForegroundColor White 