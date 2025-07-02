# =====================================================
# SCRIPT: consultar-downloads-pdf.ps1
# DESCRIÇÃO: Consultar downloads de PDF do banco PostgreSQL
# VERSÃO: 1.0
# DATA: 2024-12-27
# =====================================================

param(
    [string]$Tipo = "stats",
    [string]$Email = "",
    [int]$Limite = 20
)

Write-Host "📊 Consultando downloads de PDF..." -ForegroundColor Green

# Definir consultas SQL
switch ($Tipo.ToLower()) {
    "stats" {
        Write-Host "📈 Estatísticas gerais de downloads" -ForegroundColor Yellow
        $query = @"
        SET search_path TO leads_system;
        SELECT 
            COUNT(*) as total_downloads,
            COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE) as downloads_hoje,
            COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE - INTERVAL '7 days') as downloads_semana,
            COUNT(DISTINCT email) FILTER (WHERE email IS NOT NULL) as usuarios_unicos,
            COUNT(DISTINCT ip_address) as ips_unicos
        FROM pdf_downloads;
"@
    }
    
    "recentes" {
        Write-Host "⏰ Últimos $Limite downloads" -ForegroundColor Yellow
        $query = @"
        SET search_path TO leads_system;
        SELECT 
            created_at,
            COALESCE(email, 'anônimo') as email,
            dispositivo,
            navegador,
            ip_address,
            origem
        FROM pdf_downloads 
        ORDER BY created_at DESC 
        LIMIT $Limite;
"@
    }
    
    "por-email" {
        if ([string]::IsNullOrEmpty($Email)) {
            Write-Host "❌ Email obrigatório para este tipo" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "📧 Downloads do email: $Email" -ForegroundColor Yellow
        $query = @"
        SET search_path TO leads_system;
        SELECT 
            created_at,
            arquivo_nome,
            dispositivo,
            navegador,
            ip_address,
            origem,
            sucesso
        FROM pdf_downloads 
        WHERE email = '$Email'
        ORDER BY created_at DESC;
"@
    }
    
    default {
        Write-Host "📚 Tipos disponíveis: stats, recentes, por-email" -ForegroundColor Cyan
        exit 0
    }
}

Write-Host "Consulta SQL preparada. Execute manualmente no banco:" -ForegroundColor Gray
Write-Host $query -ForegroundColor DarkGray 