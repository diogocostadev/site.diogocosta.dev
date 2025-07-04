# Script PowerShell para verificar downloads do PDF
Write-Host "🔍 VERIFICANDO DOWNLOADS DO PDF - Manual da Primeira Virada" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# Verificar se existem logs
if (!(Test-Path "logs")) {
    Write-Host "❌ Pasta de logs não encontrada" -ForegroundColor Red
    exit 1
}

# Contador de downloads
$totalDownloads = 0

Write-Host ""
Write-Host "📥 DOWNLOADS ENCONTRADOS:" -ForegroundColor Green
Write-Host "------------------------" -ForegroundColor Green

# Procurar por downloads em todos os arquivos de log
$logFiles = Get-ChildItem "logs/app-*.txt" -ErrorAction SilentlyContinue

foreach ($logFile in $logFiles) {
    $downloadsNoArquivo = (Select-String -Path $logFile.FullName -Pattern "DOWNLOAD PDF REALIZADO" -ErrorAction SilentlyContinue).Count
    
    if ($downloadsNoArquivo -gt 0) {
        Write-Host ""
        Write-Host "📁 Arquivo: $($logFile.Name)" -ForegroundColor Yellow
        Write-Host "   Downloads: $downloadsNoArquivo" -ForegroundColor Yellow
        
        # Mostrar detalhes dos downloads
        $linhas = Select-String -Path $logFile.FullName -Pattern "DOWNLOAD PDF REALIZADO" -ErrorAction SilentlyContinue
        
        foreach ($linha in $linhas) {
            $dataHora = ($linha.Line -split ' ')[0..1] -join ' '
            Write-Host "   📅 $dataHora" -ForegroundColor White
            
            # Extrair IP se possível
            if ($linha.Line -match '"IpAddress":"([^"]*)"') {
                $ip = $matches[1]
                Write-Host "      🌐 IP: $ip" -ForegroundColor Gray
            }
            
            # Extrair Referer se possível
            if ($linha.Line -match '"Referer":"([^"]*)"') {
                $referer = $matches[1]
                if ($referer -ne "") {
                    Write-Host "      🔗 Origem: $referer" -ForegroundColor Gray
                }
            }
            
            Write-Host "      ────────────────────" -ForegroundColor DarkGray
        }
        
        $totalDownloads += $downloadsNoArquivo
    }
}

Write-Host ""
Write-Host "📊 RESUMO:" -ForegroundColor Magenta
Write-Host "Total de downloads encontrados: $totalDownloads" -ForegroundColor Magenta

# Mostrar downloads dos últimos 7 dias
Write-Host ""
Write-Host "📈 DOWNLOADS DOS ÚLTIMOS 7 DIAS:" -ForegroundColor Blue
Write-Host "--------------------------------" -ForegroundColor Blue

$dataLimite = (Get-Date).AddDays(-7)

foreach ($logFile in $logFiles) {
    if ($logFile.Name -match 'app-(\d{8})\.txt') {
        $dataArquivo = $matches[1]
        $ano = $dataArquivo.Substring(0, 4)
        $mes = $dataArquivo.Substring(4, 2)
        $dia = $dataArquivo.Substring(6, 2)
        
        try {
            $dataFormatada = Get-Date "$ano-$mes-$dia"
            
            if ($dataFormatada -ge $dataLimite) {
                $downloadsRecentes = (Select-String -Path $logFile.FullName -Pattern "DOWNLOAD PDF REALIZADO" -ErrorAction SilentlyContinue).Count
                if ($downloadsRecentes -gt 0) {
                    Write-Host "📅 $($dataFormatada.ToString('yyyy-MM-dd')): $downloadsRecentes downloads" -ForegroundColor Cyan
                }
            }
        } catch {
            # Ignorar erros de data
        }
    }
}

Write-Host ""
Write-Host "💡 DICAS:" -ForegroundColor Green
Write-Host "- Para ver detalhes completos: Select-String -Path 'logs/app-*.txt' -Pattern 'DOWNLOAD PDF REALIZADO'" -ForegroundColor White
Write-Host "- Para monitorar em tempo real: Get-Content -Path 'logs/app-`$(Get-Date -Format 'yyyyMMdd').txt' -Wait | Where-Object {`$_ -match 'DOWNLOAD'}" -ForegroundColor White
Write-Host "- Para ver via web (desenvolvimento): http://localhost:5000/desbloqueio/downloads-stats" -ForegroundColor White

Write-Host ""
Write-Host "✅ Análise concluída!" -ForegroundColor Green 