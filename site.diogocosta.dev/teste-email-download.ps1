# =====================================================
# SCRIPT: teste-email-download.ps1
# DESCRIÇÃO: Testar se o email está sendo passado para o download
# VERSÃO: 1.0
# DATA: 2024-12-27
# =====================================================

Write-Host "🧪 TESTANDO PASSAGEM DE EMAIL NO DOWNLOAD" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# URLs de teste
$baseUrl = "http://localhost:5000"
$urls = @(
    "$baseUrl/desbloqueio/download-pdf",
    "$baseUrl/desbloqueio/download-pdf?email=teste@example.com",
    "$baseUrl/desbloqueio/download-pdf?email=usuario@teste.com&origem=teste_manual"
)

Write-Host ""
Write-Host "🔗 URLs que serão testadas:" -ForegroundColor Yellow
foreach ($url in $urls) {
    Write-Host "   $url" -ForegroundColor White
}

Write-Host ""
Write-Host "📝 Para testar manualmente:" -ForegroundColor Green
Write-Host "1. Execute a aplicação: dotnet run" -ForegroundColor White
Write-Host "2. Acesse uma das URLs acima no navegador" -ForegroundColor White
Write-Host "3. Verifique os logs ou banco de dados" -ForegroundColor White

Write-Host ""
Write-Host "🔍 Para ver estatísticas após o teste:" -ForegroundColor Green
Write-Host "   $baseUrl/desbloqueio/downloads-stats" -ForegroundColor White

Write-Host ""
Write-Host "📊 Para verificar no banco:" -ForegroundColor Green
Write-Host "   .\scripts\verificar-email-downloads.ps1" -ForegroundColor White

Write-Host ""
Write-Host "✅ Script de teste criado!" -ForegroundColor Green 