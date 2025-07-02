# =====================================================
# SCRIPT: executar-script-pdf-downloads.ps1
# DESCRIÇÃO: Executar script SQL para criar tabela de downloads de PDF
# VERSÃO: 1.0
# DATA: 2024-12-27
# USAGE: .\executar-script-pdf-downloads.ps1
# =====================================================

param(
    [string]$ConnectionString = "",
    [switch]$TestMode = $false
)

Write-Host "🚀 Executando script de criação da tabela pdf_downloads..." -ForegroundColor Green

# Configurações padrão
$scriptPath = Join-Path $PSScriptRoot "004_create_pdf_downloads_table.sql"

# Se a string de conexão não foi fornecida, tentar obter do appsettings
if ([string]::IsNullOrEmpty($ConnectionString)) {
    $appsettingsPath = Join-Path $PSScriptRoot ".." "appsettings.json"
    
    if (Test-Path $appsettingsPath) {
        Write-Host "📁 Lendo configuração do appsettings.json..." -ForegroundColor Yellow
        
        try {
            $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
            $ConnectionString = $appsettings.ConnectionStrings."conexao-site"
            
            if ([string]::IsNullOrEmpty($ConnectionString)) {
                Write-Host "❌ String de conexão não encontrada no appsettings.json" -ForegroundColor Red
                exit 1
            }
            
            Write-Host "✅ String de conexão obtida do appsettings.json" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Erro ao ler appsettings.json: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
    }
    else {
        Write-Host "❌ Arquivo appsettings.json não encontrado e string de conexão não fornecida" -ForegroundColor Red
        Write-Host "   Use: .\executar-script-pdf-downloads.ps1 -ConnectionString 'sua_string_aqui'" -ForegroundColor Yellow
        exit 1
    }
}

# Verificar se o arquivo SQL existe
if (-not (Test-Path $scriptPath)) {
    Write-Host "❌ Script SQL não encontrado: $scriptPath" -ForegroundColor Red
    exit 1
}

# Verificar se o psql está disponível
try {
    $psqlVersion = & psql --version 2>$null
    Write-Host "✅ PostgreSQL Client encontrado: $psqlVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ psql não encontrado. Instale PostgreSQL Client ou adicione ao PATH" -ForegroundColor Red
    Write-Host "   Download: https://www.postgresql.org/download/" -ForegroundColor Yellow
    exit 1
}

# Modo de teste - apenas verificar sem executar
if ($TestMode) {
    Write-Host "🧪 MODO TESTE - Verificando script..." -ForegroundColor Cyan
    Write-Host "📄 Script: $scriptPath" -ForegroundColor Gray
    Write-Host "🔗 Connection String: $($ConnectionString -replace 'Password=[^;]*', 'Password=***')" -ForegroundColor Gray
    Write-Host "✅ Verificação completa. Script não executado (modo teste)." -ForegroundColor Green
    exit 0
}

# Confirmar execução
Write-Host ""
Write-Host "📋 INFORMAÇÕES DA EXECUÇÃO:" -ForegroundColor Cyan
Write-Host "   Script SQL: $scriptPath" -ForegroundColor Gray
Write-Host "   Conexão: $($ConnectionString -replace 'Password=[^;]*', 'Password=***')" -ForegroundColor Gray
Write-Host ""

$confirmation = Read-Host "Deseja executar o script? (s/N)"
if ($confirmation -ne 's' -and $confirmation -ne 'S' -and $confirmation -ne 'sim') {
    Write-Host "❌ Execução cancelada pelo usuário." -ForegroundColor Yellow
    exit 0
}

# Executar o script
Write-Host ""
Write-Host "🔄 Executando script SQL..." -ForegroundColor Yellow

try {
    # Executar o script SQL usando psql
    $env:PGPASSWORD = ""  # Limpar variável de ambiente
    
    # Extrair partes da connection string para psql
    if ($ConnectionString -match "Host=([^;]+).*Database=([^;]+).*Username=([^;]+).*Password=([^;]+)") {
        $hostName = $matches[1]
        $database = $matches[2]
        $username = $matches[3]
        $password = $matches[4]
        
        # Definir senha como variável de ambiente
        $env:PGPASSWORD = $password
        
        # Executar psql
        $result = & psql -h $hostName -d $database -U $username -f $scriptPath 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Script executado com sucesso!" -ForegroundColor Green
            Write-Host ""
            Write-Host "📊 RESULTADO:" -ForegroundColor Cyan
            $result | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
            Write-Host ""
            Write-Host "🎉 Tabela pdf_downloads criada/atualizada com sucesso!" -ForegroundColor Green
            Write-Host "🔍 Você pode verificar a tabela usando:" -ForegroundColor Yellow
            Write-Host "   SELECT * FROM leads_system.pdf_downloads LIMIT 5;" -ForegroundColor Gray
        }
        else {
            Write-Host "❌ Erro na execução do script:" -ForegroundColor Red
            $result | ForEach-Object { Write-Host "   $_" -ForegroundColor Red }
            exit 1
        }
    }
    else {
        Write-Host "❌ Formato da string de conexão inválido" -ForegroundColor Red
        Write-Host "   Formato esperado: Host=...;Database=...;Username=...;Password=..." -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "❌ Erro durante a execução: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    # Limpar variável de ambiente da senha
    $env:PGPASSWORD = ""
}

Write-Host ""
Write-Host "🎯 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "   1. Execute a aplicação para testar" -ForegroundColor Gray
Write-Host "   2. Faça um download de PDF para gerar dados" -ForegroundColor Gray
Write-Host "   3. Acesse /desbloqueio/downloads-stats para ver estatísticas" -ForegroundColor Gray
Write-Host "   4. Use os scripts de consulta para análises" -ForegroundColor Gray
Write-Host ""
Write-Host "✨ Script concluído com sucesso!" -ForegroundColor Green 