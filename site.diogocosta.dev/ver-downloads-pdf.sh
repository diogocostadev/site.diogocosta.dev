#!/bin/bash

echo "🔍 VERIFICANDO DOWNLOADS DO PDF - Manual da Primeira Virada"
echo "============================================================"

# Verificar se existem logs
if [ ! -d "logs" ]; then
    echo "❌ Pasta de logs não encontrada"
    exit 1
fi

# Contador de downloads
total_downloads=0

echo ""
echo "📥 DOWNLOADS ENCONTRADOS:"
echo "------------------------"

# Procurar por downloads em todos os arquivos de log
for log_file in logs/app-*.txt; do
    if [ -f "$log_file" ]; then
        downloads_no_arquivo=$(grep -c "DOWNLOAD PDF REALIZADO" "$log_file" 2>/dev/null || echo "0")
        
        if [ "$downloads_no_arquivo" -gt 0 ]; then
            echo ""
            echo "📁 Arquivo: $(basename "$log_file")"
            echo "   Downloads: $downloads_no_arquivo"
            
            # Mostrar detalhes dos downloads
            grep "DOWNLOAD PDF REALIZADO" "$log_file" | while read -r linha; do
                data_hora=$(echo "$linha" | cut -d' ' -f1,2)
                echo "   📅 $data_hora"
                
                # Extrair IP se possível
                if [[ $linha == *"IpAddress"* ]]; then
                    ip=$(echo "$linha" | grep -o '"IpAddress":"[^"]*"' | cut -d'"' -f4)
                    echo "      🌐 IP: $ip"
                fi
                
                # Extrair Referer se possível
                if [[ $linha == *"Referer"* ]]; then
                    referer=$(echo "$linha" | grep -o '"Referer":"[^"]*"' | cut -d'"' -f4)
                    if [ "$referer" != "" ]; then
                        echo "      🔗 Origem: $referer"
                    fi
                fi
                
                echo "      ────────────────────"
            done
            
            total_downloads=$((total_downloads + downloads_no_arquivo))
        fi
    fi
done

echo ""
echo "📊 RESUMO:"
echo "Total de downloads encontrados: $total_downloads"

# Mostrar downloads dos últimos 7 dias
echo ""
echo "📈 DOWNLOADS DOS ÚLTIMOS 7 DIAS:"
echo "--------------------------------"

data_limite=$(date -d '7 days ago' '+%Y-%m-%d')

for log_file in logs/app-*.txt; do
    if [ -f "$log_file" ]; then
        nome_arquivo=$(basename "$log_file")
        data_arquivo=$(echo "$nome_arquivo" | grep -o '[0-9]\{8\}')
        
        if [ ${#data_arquivo} -eq 8 ]; then
            data_formatada="${data_arquivo:0:4}-${data_arquivo:4:2}-${data_arquivo:6:2}"
            
            if [[ "$data_formatada" > "$data_limite" ]] || [[ "$data_formatada" == "$data_limite" ]]; then
                downloads_recentes=$(grep -c "DOWNLOAD PDF REALIZADO" "$log_file" 2>/dev/null || echo "0")
                if [ "$downloads_recentes" -gt 0 ]; then
                    echo "📅 $data_formatada: $downloads_recentes downloads"
                fi
            fi
        fi
    fi
done

echo ""
echo "💡 DICAS:"
echo "- Para ver detalhes completos: grep 'DOWNLOAD PDF REALIZADO' logs/app-*.txt"
echo "- Para monitorar em tempo real: tail -f logs/app-$(date +%Y%m%d).txt | grep 'DOWNLOAD'"
echo "- Para ver via web (desenvolvimento): http://localhost:5000/desbloqueio/downloads-stats"

echo ""
echo "✅ Análise concluída!" 