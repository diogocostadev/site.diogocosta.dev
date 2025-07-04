Write-Host "=== DOWNLOADS DO PDF - Manual da Primeira Virada ===" -ForegroundColor Cyan

$logsPath = "site.diogocosta.dev/logs"
if (!(Test-Path $logsPath)) {
    Write-Host "Pasta de logs nao encontrada em: $logsPath" -ForegroundColor Red
    exit 1
}

$totalDownloads = 0
$logFiles = Get-ChildItem "$logsPath/app-*.txt" -ErrorAction SilentlyContinue

Write-Host "Procurando downloads nos logs..." -ForegroundColor Green

foreach ($logFile in $logFiles) {
    $downloads = Select-String -Path $logFile.FullName -Pattern "DOWNLOAD PDF REALIZADO" -ErrorAction SilentlyContinue
    
    if ($downloads.Count -gt 0) {
        Write-Host ""
        Write-Host "Arquivo: $($logFile.Name)" -ForegroundColor Yellow
        Write-Host "Downloads encontrados: $($downloads.Count)" -ForegroundColor Yellow
        
        foreach ($download in $downloads) {
            $linha = $download.Line
            $dataHora = ($linha -split ' ')[0..1] -join ' '
            Write-Host "  Data/Hora: $dataHora" -ForegroundColor White
            
            if ($linha -match '"IpAddress":"([^"]*)"') {
                Write-Host "  IP: $($matches[1])" -ForegroundColor Gray
            }
            
            if ($linha -match '"Referer":"([^"]*)"') {
                $referer = $matches[1]
                if ($referer -ne "") {
                    Write-Host "  Origem: $referer" -ForegroundColor Gray
                }
            }
            Write-Host "  ---" -ForegroundColor DarkGray
        }
        
        $totalDownloads += $downloads.Count
    }
}

Write-Host ""
Write-Host "RESUMO:" -ForegroundColor Magenta
Write-Host "Total de downloads: $totalDownloads" -ForegroundColor Magenta

Write-Host ""
Write-Host "Para ver mais detalhes:" -ForegroundColor Green
Write-Host "Select-String -Path 'site.diogocosta.dev/logs/app-*.txt' -Pattern 'DOWNLOAD PDF REALIZADO'" -ForegroundColor White 