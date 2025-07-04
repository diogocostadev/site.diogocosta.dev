# 📥 Tracking de Downloads do PDF - Manual da Primeira Virada

Este guia mostra **todas as formas** de acompanhar quem está baixando seu PDF "Manual da Primeira Virada".

## 🎯 Onde Verificar os Downloads

### 1. **📊 Analytics Online (Recomendado)**

#### Google Analytics 4
- **URL**: [Google Analytics](https://analytics.google.com)
- **ID**: `G-0YZHXVSL7M`
- **Como ver**: Eventos > Todos os eventos > Procure por `file_download`
- **Dados disponíveis**: Data/hora, origem, dispositivo, localização

#### Matomo Analytics
- **URL**: `matomo.codigocentral.com.br`
- **Como ver**: Ações > Downloads > Manual da Primeira Virada
- **Dados disponíveis**: IP, referrer, data/hora, dispositivo

### 2. **📋 Logs da Aplicação (Detalhado)**

#### PowerShell (Windows)
```powershell
# Executar da pasta raiz do projeto
.\site.diogocosta.dev\ver-downloads-pdf.ps1
```

#### Bash (Linux/Mac)
```bash
# Executar da pasta raiz do projeto
./site.diogocosta.dev/ver-downloads-pdf.sh
```

#### Comando Manual (PowerShell)
```powershell
Select-String -Path "site.diogocosta.dev/logs/app-*.txt" -Pattern "DOWNLOAD PDF REALIZADO"
```

#### Comando Manual (Linux/Mac)
```bash
grep "DOWNLOAD PDF REALIZADO" site.diogocosta.dev/logs/app-*.txt
```

### 3. **🌐 Dashboard Web (Desenvolvimento)**

**URL**: `http://localhost:5000/desbloqueio/downloads-stats`

⚠️ **Apenas funciona em desenvolvimento** (localhost)

---

## 📊 Informações Capturadas

Para cada download, o sistema registra:

### 🔍 Dados Básicos
- ✅ **Data e Hora** exata do download
- ✅ **Endereço IP** do usuário
- ✅ **User Agent** (navegador/dispositivo)
- ✅ **Referrer** (página de origem)
- ✅ **Email** (quando disponível)

### 📈 Analytics Avançados
- ✅ **Localização geográfica** (via Google Analytics)
- ✅ **Tipo de dispositivo** (desktop/mobile/tablet)
- ✅ **Navegador e sistema operacional**
- ✅ **Origem do tráfego** (direto, referral, social)

---

## 🚀 Como Usar

### Verificação Rápida (Diária)
```powershell
# Windows
.\site.diogocosta.dev\ver-downloads-pdf.ps1
```

### Monitoramento em Tempo Real
```powershell
# Windows - Ver downloads conforme acontecem
Get-Content -Path "site.diogocosta.dev/logs/app-$(Get-Date -Format 'yyyyMMdd').txt" -Wait | Where-Object {$_ -match 'DOWNLOAD'}
```

```bash
# Linux/Mac - Ver downloads conforme acontecem
tail -f site.diogocosta.dev/logs/app-$(date +%Y%m%d).txt | grep 'DOWNLOAD'
```

### Relatório Semanal
```powershell
# Ver downloads dos últimos 7 dias
$data_limite = (Get-Date).AddDays(-7)
Get-ChildItem "site.diogocosta.dev/logs/app-*.txt" | Where-Object {$_.CreationTime -ge $data_limite} | ForEach-Object {
    $downloads = (Select-String -Path $_.FullName -Pattern "DOWNLOAD PDF REALIZADO").Count
    if ($downloads -gt 0) {
        Write-Host "$($_.Name): $downloads downloads"
    }
}
```

---

## 📋 Exemplos de Saída

### Log Detalhado
```
📥 DOWNLOAD PDF REALIZADO: {
  "IpAddress": "189.123.45.67",
  "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
  "Referer": "https://diogocosta.dev/desbloqueio/obrigado",
  "Email": "joao@email.com",
  "Timestamp": "2024-12-27T18:30:45.1234567Z",
  "FileName": "Manual_da_Primeira_Virada_Diogo_Costa.pdf"
}
```

### Resumo do Script
```
🔍 VERIFICANDO DOWNLOADS DO PDF - Manual da Primeira Virada
============================================================

📥 DOWNLOADS ENCONTRADOS:
------------------------

📁 Arquivo: app-20241227.txt
   Downloads: 3
   📅 2024-12-27 15:30:45
      🌐 IP: 189.123.45.67
      🔗 Origem: https://diogocosta.dev/desbloqueio/obrigado
      ────────────────────

📊 RESUMO:
Total de downloads encontrados: 3

📈 DOWNLOADS DOS ÚLTIMOS 7 DIAS:
--------------------------------
📅 2024-12-27: 3 downloads
```

---

## 🎯 Estratégias de Análise

### 1. **Conversão**
- Compare downloads com cadastros na newsletter
- Identifique horários de maior conversão
- Analise origem do tráfego mais efetiva

### 2. **Comportamento**
- Veja quantos baixam direto vs. por email
- Identifique padrões geográficos
- Monitore dispositivos mais usados

### 3. **Otimização**
- Teste diferentes textos na página de obrigado
- Otimize horários de envio de email
- Melhore páginas com baixa conversão

---

## 🔧 Automação

Para automatizar relatórios, você pode:

### Relatório Diário por Email
Configure um cron job (Linux) ou Task Scheduler (Windows) para executar:

```bash
# Linux/Mac - Enviar relatório diário
./site.diogocosta.dev/ver-downloads-pdf.sh > relatorio-downloads-$(date +%Y%m%d).txt
```

### Dashboard Personalizado
Considere criar um dashboard personalizado usando os dados dos logs para ter uma visão em tempo real.

---

## ❗ Importante

### Produção vs Desenvolvimento
- **Analytics** funcionam em produção e desenvolvimento
- **Dashboard web** (`/downloads-stats`) só funciona em desenvolvimento
- **Logs** funcionam em ambos os ambientes

### Privacidade
- IPs são registrados apenas para analytics internos
- Dados pessoais seguem LGPD
- Cookies são necessários para analytics completos

---

## 🆘 Troubleshooting

### Não vejo downloads nos logs
1. Verifique se o arquivo de log do dia existe
2. Confirme se o download foi feito hoje
3. Verifique se o sistema de logs está funcionando

### Analytics não mostram dados
1. Verifique se o código do Google Analytics está correto
2. Confirme se o usuário tem cookies habilitados
3. Pode levar até 24h para aparecer no Google Analytics

### Script não executa
1. **Windows**: Execute no PowerShell como Administrador
2. **Linux/Mac**: Verifique permissões com `ls -la`
3. Confirme se está na pasta raiz do projeto

---

✅ **Agora você tem controle total sobre quem baixa seu PDF!** 