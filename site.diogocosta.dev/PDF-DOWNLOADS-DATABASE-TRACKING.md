# 📊 Sistema de Tracking de Downloads de PDF - Banco de Dados

## 🎯 Visão Geral

Este sistema registra **todos os downloads de PDF** diretamente no banco de dados PostgreSQL, oferecendo análises muito mais detalhadas e confiáveis que os logs em arquivo.

## 🚀 Benefícios do Sistema no Banco

✅ **Dados estruturados** - Consultas SQL poderosas  
✅ **Performance** - Índices otimizados para relatórios  
✅ **Relacionamentos** - Conecta com tabela de leads  
✅ **Persistência** - Dados não se perdem  
✅ **Escalabilidade** - Funciona bem em Docker/produção  
✅ **Analytics avançados** - Views e funções SQL  

## 📋 Estrutura da Tabela `pdf_downloads`

```sql
CREATE TABLE leads_system.pdf_downloads (
    id SERIAL PRIMARY KEY,
    arquivo_nome VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    referer VARCHAR(500),
    origem VARCHAR(100),
    pais VARCHAR(2),
    cidade VARCHAR(100),
    dispositivo VARCHAR(50),
    navegador VARCHAR(50),
    sistema_operacional VARCHAR(50),
    sucesso BOOLEAN DEFAULT TRUE,
    tamanho_arquivo BIGINT,
    tempo_download_ms INTEGER,
    dados_extra JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    lead_id INTEGER -- FK para tabela leads
);
```

## 🛠️ Como Configurar

### 1. Executar Script de Criação da Tabela

```powershell
# No diretório site.diogocosta.dev/scripts
.\executar-script-pdf-downloads.ps1
```

### 2. Verificar Configuração

O sistema já está integrado ao código. O serviço `PdfDownloadService` registra automaticamente cada download.

### 3. Testar o Sistema

1. Execute a aplicação
2. Faça um download do PDF
3. Acesse `/desbloqueio/downloads-stats` (desenvolvimento)

## 📊 Como Ver as Estatísticas

### 1. Dashboard Web (Desenvolvimento)

```
http://localhost:5000/desbloqueio/downloads-stats
```

**Dados mostrados:**
- Total de downloads
- Downloads hoje/semana/mês
- Arquivo mais baixado
- Origem mais comum
- Dispositivo mais usado
- Downloads recentes
- Estatísticas por dia/origem

### 2. Scripts PowerShell

```powershell
# Estatísticas gerais
.\consultar-downloads-pdf.ps1 -Tipo stats

# Downloads recentes
.\consultar-downloads-pdf.ps1 -Tipo recentes -Limite 50

# Downloads de um email específico
.\consultar-downloads-pdf.ps1 -Tipo por-email -Email "usuario@email.com"
```

### 3. Consultas SQL Diretas

```sql
-- Estatísticas básicas
SELECT * FROM leads_system.vw_pdf_downloads_stats;

-- Downloads por dia
SELECT * FROM leads_system.vw_pdf_downloads_por_dia;

-- Downloads por origem
SELECT * FROM leads_system.vw_pdf_downloads_por_origem;

-- Downloads por dispositivo
SELECT * FROM leads_system.vw_pdf_downloads_por_dispositivo;

-- Downloads recentes com detalhes
SELECT 
    created_at,
    email,
    dispositivo,
    navegador,
    sistema_operacional,
    origem,
    ip_address
FROM leads_system.pdf_downloads 
ORDER BY created_at DESC 
LIMIT 20;
```

## 🔍 Consultas Úteis

### Downloads por Email
```sql
SELECT 
    created_at,
    arquivo_nome,
    dispositivo,
    navegador,
    origem
FROM leads_system.pdf_downloads 
WHERE email = 'usuario@exemplo.com'
ORDER BY created_at DESC;
```

### Relatório Mensal
```sql
SELECT 
    DATE(created_at) as data,
    COUNT(*) as downloads,
    COUNT(DISTINCT email) FILTER (WHERE email IS NOT NULL) as usuarios_unicos
FROM leads_system.pdf_downloads 
WHERE created_at >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY DATE(created_at)
ORDER BY data DESC;
```

### Top IPs
```sql
SELECT 
    ip_address,
    COUNT(*) as downloads,
    COUNT(DISTINCT email) FILTER (WHERE email IS NOT NULL) as emails_diferentes
FROM leads_system.pdf_downloads 
GROUP BY ip_address 
ORDER BY downloads DESC 
LIMIT 10;
```

### Análise de Dispositivos
```sql
SELECT 
    dispositivo,
    navegador,
    COUNT(*) as quantidade,
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (), 2) as percentual
FROM leads_system.pdf_downloads 
GROUP BY dispositivo, navegador 
ORDER BY quantidade DESC;
```

## 🎯 Funcionalidades Avançadas

### 1. Views Pré-criadas

- `vw_pdf_downloads_stats` - Estatísticas gerais
- `vw_pdf_downloads_por_dia` - Downloads por dia
- `vw_pdf_downloads_por_origem` - Downloads por origem
- `vw_pdf_downloads_por_dispositivo` - Downloads por dispositivo

### 2. Função SQL para Relatórios

```sql
-- Estatísticas de um período específico
SELECT get_pdf_download_stats('2024-12-01', '2024-12-31');
```

### 3. Dados Coletados

**Dados básicos:**
- Nome do arquivo
- Email (quando disponível)
- IP do usuário
- User Agent completo
- Referer (página de origem)
- Data/hora exata

**Dados analisados:**
- Tipo de dispositivo (mobile/desktop/tablet)
- Navegador usado
- Sistema operacional
- Origem do download

**Relacionamentos:**
- Link com tabela de leads (quando email conhecido)
- Dados extras em JSON para futuras expansões

## 🔧 Troubleshooting

### Erro ao executar script SQL
```bash
# Verificar conexão
psql -h SEU_HOST -d SUA_DATABASE -U SEU_USER -c "SELECT 1;"

# Verificar se tabela foi criada
psql -h SEU_HOST -d SUA_DATABASE -U SEU_USER -c "SELECT COUNT(*) FROM leads_system.pdf_downloads;"
```

### Dashboard não mostra dados
1. Verifique se está em ambiente de desenvolvimento
2. Faça um download para gerar dados
3. Verifique logs da aplicação

### Performance lenta
```sql
-- Verificar índices
SELECT indexname, indexdef 
FROM pg_indexes 
WHERE tablename = 'pdf_downloads';

-- Estatísticas da tabela
SELECT COUNT(*), MIN(created_at), MAX(created_at) 
FROM leads_system.pdf_downloads;
```

## 🎉 Próximos Passos

1. **Execute o script de criação da tabela**
2. **Teste fazendo downloads de PDF**
3. **Explore o dashboard em desenvolvimento**
4. **Configure relatórios automáticos se necessário**
5. **Use as consultas SQL para análises personalizadas**

## 📈 Vantagens vs Sistema Anterior

| Recurso | Logs em Arquivo | Banco de Dados |
|---------|----------------|----------------|
| **Estruturação** | ❌ Texto não estruturado | ✅ Dados relacionais |
| **Consultas** | ❌ Grep limitado | ✅ SQL poderoso |
| **Performance** | ❌ Lento em grandes volumes | ✅ Índices otimizados |
| **Relacionamentos** | ❌ Não suporta | ✅ FK com leads |
| **Persistência** | ❌ Pode ser perdido | ✅ Backup automático |
| **Analytics** | ❌ Muito limitado | ✅ Views, funções, relatórios |
| **Escalabilidade** | ❌ Problemas em produção | ✅ Funciona perfeitamente |

Agora você tem um sistema completo e profissional de tracking de downloads! 🚀 