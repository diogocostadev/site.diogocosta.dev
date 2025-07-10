# Script PowerShell para Bot Detector Anti-Spam
# Site: site.diogocosta.dev
# Data: 2025-07-10

param(
    [string]$Mode = "interactive",
    [string]$LogFile = "",
    [string]$ApiUrl = "https://localhost:5000/api/antispam",
    [string]$ConfigFile = ""
)

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "    Bot Detector Anti-Spam" -ForegroundColor Yellow
Write-Host "    site.diogocosta.dev" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se Python está instalado
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✅ Python encontrado: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ ERRO: Python não encontrado! Instale Python 3.8+ antes de continuar." -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

# Verificar se requests está instalado
try {
    python -c "import requests" 2>$null
    Write-Host "✅ Dependência 'requests' encontrada" -ForegroundColor Green
} catch {
    Write-Host "📦 Instalando dependência 'requests'..." -ForegroundColor Yellow
    pip install requests
}

# Configurar caminhos
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DefaultLogFile = Join-Path $ScriptDir "..\logs\app-$(Get-Date -Format 'yyyyMMdd').txt"
$DefaultConfigFile = Join-Path $ScriptDir "bot_detector_config.json"

if ($LogFile -eq "") { $LogFile = $DefaultLogFile }
if ($ConfigFile -eq "") { $ConfigFile = $DefaultConfigFile }

Write-Host "Configuração:" -ForegroundColor Cyan
Write-Host "  Script: $ScriptDir\bot_detector.py" -ForegroundColor Gray
Write-Host "  Log: $LogFile" -ForegroundColor Gray
Write-Host "  API: $ApiUrl" -ForegroundColor Gray
Write-Host "  Config: $ConfigFile" -ForegroundColor Gray
Write-Host ""

if ($Mode -eq "interactive") {
    Write-Host "Escolha o modo de execução:" -ForegroundColor Cyan
    Write-Host "  1) Monitoramento em tempo real (recomendado)" -ForegroundColor White
    Write-Host "  2) Análise do log atual (uma vez)" -ForegroundColor White
    Write-Host "  3) Usar arquivo de configuração" -ForegroundColor White
    Write-Host ""
    
    $choice = Read-Host "Digite sua opção (1-3)"
    
    switch ($choice) {
        "1" {
            Write-Host ""
            Write-Host "====================================" -ForegroundColor Green
            Write-Host " Iniciando monitoramento em tempo real..." -ForegroundColor Yellow
            Write-Host " Pressione Ctrl+C para parar" -ForegroundColor Yellow
            Write-Host "====================================" -ForegroundColor Green
            
            python "$ScriptDir\bot_detector.py" --log-file "$LogFile" --api-url "$ApiUrl" --follow
        }
        "2" {
            Write-Host ""
            Write-Host "====================================" -ForegroundColor Green
            Write-Host " Analisando log atual..." -ForegroundColor Yellow
            Write-Host "====================================" -ForegroundColor Green
            
            python "$ScriptDir\bot_detector.py" --log-file "$LogFile" --api-url "$ApiUrl"
        }
        "3" {
            if (Test-Path $ConfigFile) {
                Write-Host ""
                Write-Host "====================================" -ForegroundColor Green
                Write-Host " Usando arquivo de configuração..." -ForegroundColor Yellow
                Write-Host "====================================" -ForegroundColor Green
                
                python "$ScriptDir\bot_detector.py" --config "$ConfigFile"
            } else {
                Write-Host "❌ ERRO: Arquivo de configuração não encontrado: $ConfigFile" -ForegroundColor Red
                Read-Host "Pressione Enter para sair"
                exit 1
            }
        }
        default {
            Write-Host "❌ Opção inválida!" -ForegroundColor Red
            Read-Host "Pressione Enter para sair"
            exit 1
        }
    }
} else {
    # Modo não-interativo
    switch ($Mode) {
        "monitor" {
            python "$ScriptDir\bot_detector.py" --log-file "$LogFile" --api-url "$ApiUrl" --follow
        }
        "analyze" {
            python "$ScriptDir\bot_detector.py" --log-file "$LogFile" --api-url "$ApiUrl"
        }
        "config" {
            if (Test-Path $ConfigFile) {
                python "$ScriptDir\bot_detector.py" --config "$ConfigFile"
            } else {
                Write-Host "❌ ERRO: Arquivo de configuração não encontrado: $ConfigFile" -ForegroundColor Red
                exit 1
            }
        }
        default {
            Write-Host "❌ Modo inválido: $Mode" -ForegroundColor Red
            Write-Host "Modos válidos: interactive, monitor, analyze, config" -ForegroundColor Yellow
            exit 1
        }
    }
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host " Bot Detector finalizado!" -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Green

if ($Mode -eq "interactive") {
    Read-Host "Pressione Enter para sair"
}
