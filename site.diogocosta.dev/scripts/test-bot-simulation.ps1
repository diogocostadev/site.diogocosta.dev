# Script de Simulacao de Bots para testar BackgroundService
$baseUrl = "http://localhost:5000"

Write-Host "=== Iniciando simulacao de bots ===" -ForegroundColor Yellow

# Teste 1: Bot com mesmo IP (12 tentativas - threshold: 10)
Write-Host "Teste 1: Bot com mesmo IP (12 tentativas)" -ForegroundColor Cyan
for ($i = 1; $i -le 12; $i++) {
    $formData = "Email=bot$i%40tempmail.ru&Nome=Bot+Test+$i&Whatsapp=%2B5511999900$i&Website=&Phone=&EmailConfirm="
    
    try {
        $headers = @{
            'Content-Type' = 'application/x-www-form-urlencoded'
            'User-Agent' = 'BotTester/1.0'
            'X-Forwarded-For' = '192.168.100.50'
        }
        
        Invoke-WebRequest -Uri "$baseUrl/Desbloqueio/Cadastrar" -Method POST -Body $formData -Headers $headers -ErrorAction SilentlyContinue | Out-Null
        Write-Host "  -> Tentativa $i enviada (IP: 192.168.100.50)" -ForegroundColor Green
    }
    catch {
        Write-Host "  -> Tentativa $i erro/bloqueada" -ForegroundColor Red
    }
    Start-Sleep -Milliseconds 300
}

Write-Host ""

# Teste 2: Bot com mesmo User-Agent (16 tentativas - threshold: 15)
Write-Host "Teste 2: Bot com mesmo User-Agent (16 tentativas)" -ForegroundColor Cyan
for ($i = 1; $i -le 16; $i++) {
    $formData = "Email=test$i%40gmail.com&Nome=User+$i&Whatsapp=%2B5511888800$i&Website=&Phone=&EmailConfirm="
    
    try {
        $headers = @{
            'Content-Type' = 'application/x-www-form-urlencoded'
            'User-Agent' = 'SuperBot/2.0 (Automated Testing Tool)'
            'X-Forwarded-For' = "192.168.100.$($i + 60)"
        }
        
        Invoke-WebRequest -Uri "$baseUrl/Desbloqueio/Cadastrar" -Method POST -Body $formData -Headers $headers -ErrorAction SilentlyContinue | Out-Null
        Write-Host "  -> Tentativa $i enviada (User-Agent: SuperBot/2.0)" -ForegroundColor Green
    }
    catch {
        Write-Host "  -> Tentativa $i erro/bloqueada" -ForegroundColor Red
    }
    Start-Sleep -Milliseconds 200
}

Write-Host ""

# Teste 3: Bot com dominio suspeito (6 tentativas - threshold: 5)
Write-Host "Teste 3: Bot com dominio suspeito (6 tentativas)" -ForegroundColor Cyan
$domains = @("tempmail.tk", "fakeemail.ml", "spammail.ga", "botmail.cf")

for ($i = 1; $i -le 6; $i++) {
    $domain = $domains[($i - 1) % $domains.Length]
    $formData = "Email=user$i%40$domain&Nome=Normal+User+$i&Whatsapp=%2B5511777700$i&Website=&Phone=&EmailConfirm="
    
    try {
        $headers = @{
            'Content-Type' = 'application/x-www-form-urlencoded'
            'User-Agent' = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) $i"
            'X-Forwarded-For' = "192.168.101.$($i + 10)"
        }
        
        Invoke-WebRequest -Uri "$baseUrl/Desbloqueio/Cadastrar" -Method POST -Body $formData -Headers $headers -ErrorAction SilentlyContinue | Out-Null
        Write-Host "  -> Tentativa $i enviada (Domain: $domain)" -ForegroundColor Green
    }
    catch {
        Write-Host "  -> Tentativa $i erro/bloqueada" -ForegroundColor Red
    }
    Start-Sleep -Milliseconds 400
}

Write-Host ""
Write-Host "=== Simulacao concluida! ===" -ForegroundColor Green
Write-Host "Aguarde ate 5 minutos para o BackgroundService detectar automaticamente..." -ForegroundColor Yellow
Write-Host "Verifique os logs da aplicacao para ver as deteccoes!" -ForegroundColor Cyan
