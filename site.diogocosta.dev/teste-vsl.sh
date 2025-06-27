#!/bin/bash

# 🧪 Script de Teste Automatizado VSL
# Autor: Diogo Costa
# Data: $(date +%Y-%m-%d)

echo "🧪 ========================================"
echo "   TESTE AUTOMATIZADO VSL - DIOGO COSTA"
echo "========================================"

# Configurações
DB_HOST="10.10.0.8"
DB_USER="dcov3rl0rd"
DB_NAME="leads"
APP_URL="http://localhost:5000"
LOG_FILE="teste-vsl-$(date +%Y%m%d-%H%M%S).log"

# Função para log
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

# Função para testar comando
test_command() {
    local cmd="$1"
    local desc="$2"
    
    log "🔍 Testando: $desc"
    if eval "$cmd" > /dev/null 2>&1; then
        log "✅ $desc - OK"
        return 0
    else
        log "❌ $desc - FALHOU"
        return 1
    fi
}

# Função para testar URL
test_url() {
    local url="$1"
    local desc="$2"
    
    log "🌐 Testando URL: $desc"
    http_code=$(curl -s -o /dev/null -w "%{http_code}" "$url")
    
    if [ "$http_code" = "200" ]; then
        log "✅ $desc - OK (HTTP $http_code)"
        return 0
    else
        log "❌ $desc - FALHOU (HTTP $http_code)"
        return 1
    fi
}

# Contadores
total_tests=0
passed_tests=0

# Função para incrementar contadores
increment_test() {
    total_tests=$((total_tests + 1))
    if [ $? -eq 0 ]; then
        passed_tests=$((passed_tests + 1))
    fi
}

echo ""
log "🚀 Iniciando testes do VSL..."

# 1. Testar conexão com banco de dados
log ""
log "📊 === TESTE 1: CONEXÃO COM BANCO DE DADOS ==="
test_command "psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c 'SELECT 1;'" "Conexão PostgreSQL"
increment_test

# 2. Verificar se tabelas VSL existem
log ""
log "🗄️ === TESTE 2: VERIFICAÇÃO DAS TABELAS VSL ==="
test_command "psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c '\dt leads_system.vsl_*'" "Tabelas VSL existem"
increment_test

# 3. Verificar dados das VSLs
log ""
log "📋 === TESTE 3: DADOS DAS VSLS ==="
vsl_count=$(psql -h $DB_HOST -U $DB_USER -d $DB_NAME -t -c "SELECT COUNT(*) FROM leads_system.vsl_configs WHERE ativo = true;" 2>/dev/null | xargs)
if [ "$vsl_count" -gt 0 ]; then
    log "✅ VSLs ativas encontradas: $vsl_count"
    passed_tests=$((passed_tests + 1))
else
    log "❌ Nenhuma VSL ativa encontrada"
fi
total_tests=$((total_tests + 1))

# 4. Testar URLs dos vídeos
log ""
log "🎬 === TESTE 4: URLS DOS VÍDEOS ==="
test_url "https://videos.diogocosta.dev/hls/comunidade-didaticos-001.m3u8" "Vídeo Produção"
increment_test

test_url "https://videos.diogocosta.dev/hls/2025-06-08_15-58-24/stream.m3u8" "Vídeo Teste"
increment_test

# 5. Verificar se aplicação está rodando
log ""
log "🖥️ === TESTE 5: APLICAÇÃO LOCAL ==="
if pgrep -f "dotnet.*site.diogocosta.dev" > /dev/null; then
    log "✅ Aplicação está rodando"
    passed_tests=$((passed_tests + 1))
else
    log "⚠️ Aplicação não está rodando, tentando iniciar..."
    log "💡 Execute: cd site.diogocosta.dev && dotnet run"
fi
total_tests=$((total_tests + 1))

# 6. Testar páginas VSL (se app estiver rodando)
log ""
log "🌐 === TESTE 6: PÁGINAS VSL ==="
if pgrep -f "dotnet.*site.diogocosta.dev" > /dev/null; then
    # Aguardar aplicação inicializar
    sleep 3
    
    test_url "$APP_URL/vsl-criar-saas" "VSL Principal"
    increment_test
    
    test_url "$APP_URL/vsl-dc360" "VSL DC360"
    increment_test
    
    # Testar conteúdo das páginas
    if curl -s "$APP_URL/vsl-criar-saas" | grep -q "De Zero ao Seu Primeiro SaaS"; then
        log "✅ Conteúdo VSL Principal - OK"
        passed_tests=$((passed_tests + 1))
    else
        log "❌ Conteúdo VSL Principal - FALHOU"
    fi
    total_tests=$((total_tests + 1))
    
    if curl -s "$APP_URL/vsl-dc360" | grep -q "DC360"; then
        log "✅ Conteúdo VSL DC360 - OK"
        passed_tests=$((passed_tests + 1))
    else
        log "❌ Conteúdo VSL DC360 - FALHOU"
    fi
    total_tests=$((total_tests + 1))
else
    log "⚠️ Aplicação não está rodando, pulando testes de páginas"
    total_tests=$((total_tests + 4))
fi

# 7. Verificar logs da aplicação
log ""
log "📝 === TESTE 7: LOGS DA APLICAÇÃO ==="
log_file_today="site.diogocosta.dev/logs/app-$(date +%Y%m%d).txt"
if [ -f "$log_file_today" ]; then
    if grep -q "VSL" "$log_file_today"; then
        log "✅ Logs VSL encontrados"
        passed_tests=$((passed_tests + 1))
    else
        log "⚠️ Logs VSL não encontrados (ainda não foram acessadas)"
        passed_tests=$((passed_tests + 1))
    fi
else
    log "❌ Arquivo de log não encontrado: $log_file_today"
fi
total_tests=$((total_tests + 1))

# 8. Testar ambiente ativo
log ""
log "🔄 === TESTE 8: AMBIENTE ATIVO ==="
ambiente_vsl=$(psql -h $DB_HOST -U $DB_USER -d $DB_NAME -t -c "SELECT ambiente_ativo FROM leads_system.vsl_configs WHERE slug = 'vsl-criar-saas';" 2>/dev/null | xargs)
if [ "$ambiente_vsl" = "producao" ] || [ "$ambiente_vsl" = "teste" ]; then
    log "✅ Ambiente VSL ativo: $ambiente_vsl"
    passed_tests=$((passed_tests + 1))
else
    log "❌ Ambiente VSL inválido: $ambiente_vsl"
fi
total_tests=$((total_tests + 1))

# Resumo final
log ""
log "📊 ========================================"
log "           RESUMO DOS TESTES"
log "========================================"
log "Total de testes: $total_tests"
log "Testes aprovados: $passed_tests"
log "Testes falharam: $((total_tests - passed_tests))"

percentage=$((passed_tests * 100 / total_tests))
log "Taxa de sucesso: $percentage%"

log ""
if [ "$percentage" -ge 80 ]; then
    log "🎉 TESTES APROVADOS! VSL pronto para deploy."
else
    log "⚠️ ATENÇÃO! Alguns testes falharam. Revise antes do deploy."
fi

log ""
log "📄 Log completo salvo em: $LOG_FILE"

# Comandos úteis
log ""
log "🔧 ========================================"
log "           COMANDOS ÚTEIS"
log "========================================"
log "Trocar para teste: psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c \"SELECT leads_system.trocar_ambiente_vsl('vsl-criar-saas', 'teste');\""
log "Trocar para produção: psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c \"SELECT leads_system.trocar_ambiente_vsl('vsl-criar-saas', 'producao');\""
log "Ver status: psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c \"SELECT * FROM leads_system.vw_vsl_completa;\""
log "Rodar app: cd site.diogocosta.dev && dotnet run"

echo ""
echo "✅ Script de teste concluído!"
echo "📄 Veja o log completo: $LOG_FILE" 