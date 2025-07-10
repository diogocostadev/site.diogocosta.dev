@echo off
REM Script para executar o Bot Detector no Windows
REM Site: site.diogocosta.dev
REM Data: 2025-07-10

echo ===================================
echo    Bot Detector Anti-Spam
echo    site.diogocosta.dev
echo ===================================
echo.

REM Verificar se Python está instalado
python --version >nul 2>&1
if errorlevel 1 (
    echo ERRO: Python nao encontrado! Instale Python 3.8+ antes de continuar.
    pause
    exit /b 1
)

REM Verificar se requests está instalado
python -c "import requests" >nul 2>&1
if errorlevel 1 (
    echo Instalando dependencia requests...
    pip install requests
)

REM Configurar variáveis
set SCRIPT_DIR=%~dp0
set LOG_FILE=%SCRIPT_DIR%\..\logs\app-%date:~-4,4%%date:~-10,2%%date:~-7,2%.txt
set API_URL=https://localhost:5000/api/antispam
set CONFIG_FILE=%SCRIPT_DIR%\bot_detector_config.json

echo Configuracao:
echo   Script: %SCRIPT_DIR%\bot_detector.py
echo   Log: %LOG_FILE%
echo   API: %API_URL%
echo   Config: %CONFIG_FILE%
echo.

REM Perguntar modo de execução
echo Escolha o modo de execucao:
echo   1) Monitoramento em tempo real (recomendado)
echo   2) Analise do log atual (uma vez)
echo   3) Usar arquivo de configuracao
echo.
set /p choice="Digite sua opcao (1-3): "

if "%choice%"=="1" (
    echo.
    echo ===================================
    echo  Iniciando monitoramento em tempo real...
    echo  Pressione Ctrl+C para parar
    echo ===================================
    python "%SCRIPT_DIR%\bot_detector.py" --log-file "%LOG_FILE%" --api-url "%API_URL%" --follow
) else if "%choice%"=="2" (
    echo.
    echo ===================================
    echo  Analisando log atual...
    echo ===================================
    python "%SCRIPT_DIR%\bot_detector.py" --log-file "%LOG_FILE%" --api-url "%API_URL%"
) else if "%choice%"=="3" (
    if exist "%CONFIG_FILE%" (
        echo.
        echo ===================================
        echo  Usando arquivo de configuracao...
        echo ===================================
        python "%SCRIPT_DIR%\bot_detector.py" --config "%CONFIG_FILE%"
    ) else (
        echo ERRO: Arquivo de configuracao nao encontrado: %CONFIG_FILE%
        pause
        exit /b 1
    )
) else (
    echo Opcao invalida!
    pause
    exit /b 1
)

echo.
echo ===================================
echo  Bot Detector finalizado!
echo ===================================
pause
