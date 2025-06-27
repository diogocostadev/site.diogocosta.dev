# 🧪 Guia Completo para Testar VSL Localmente

## 📋 Checklist de Testes Antes do Deploy

### 1. 🔍 Verificar Conexão com o Banco de Dados

```bash
# Testar conexão com PostgreSQL
psql -h 10.10.0.8 -U dcov3rl0rd -d leads -c "SELECT current_database();"
```

Se a conexão funcionar, verás:
```
current_database 
------------------
 leads
(1 row)
```

### 2. 🎬 Verificar se as Tabelas VSL Existem

```sql
-- Conectar ao banco
psql -h 10.10.0.8 -U dcov3rl0rd -d leads

-- Verificar se as tabelas existem
\dt leads_system.vsl_*

-- Deve mostrar:
-- leads_system | vsl_configs | table | dcov3rl0rd
-- leads_system | vsl_videos  | table | dcov3rl0rd
```

### 3. 📊 Consultar Status Atual das VSLs

```sql
-- Ver todas as VSLs configuradas
SELECT 
    slug,
    nome,
    ambiente_ativo,
    video_url_ativo,
    preco_promocional,
    ativo
FROM leads_system.vw_vsl_completa
ORDER BY slug;
```

### 4. 🎯 Testar URLs dos Vídeos

```bash
# Testar se os vídeos estão acessíveis
curl -I "https://videos.diogocosta.dev/hls/comunidade-didaticos-001.m3u8"
curl -I "https://videos.diogocosta.dev/hls/2025-06-08_15-58-24/stream.m3u8"

# Resposta esperada: HTTP 200 OK
```

### 5. 🚀 Executar a Aplicação Localmente

```bash
cd site.diogocosta.dev
dotnet run

# A aplicação deve iniciar em:
# http://localhost:5000
```

### 6. 🧪 Testar as Páginas VSL

#### VSL Principal (Desafio SaaS)
```bash
# Abrir no navegador ou testar com curl
curl -s http://localhost:5000/vsl-criar-saas | grep -i "video"
```

#### VSL DC360
```bash
# Abrir no navegador ou testar com curl  
curl -s http://localhost:5000/vsl-dc360 | grep -i "video"
```

### 7. 📝 Verificar Logs da Aplicação

```bash
# Verificar logs em tempo real
tail -f site.diogocosta.dev/logs/app-$(date +%Y%m%d).txt

# Procurar por mensagens sobre VSL:
# ✅ VSL carregada do banco: vsl-criar-saas - Ambiente: producao
# ⚠️ VSL não encontrada no banco de dados. Usando dados padrão.
```

## 🔄 Como Trocar Entre Ambiente de Teste e Produção

### Usar Vídeo de Teste
```sql
-- Trocar VSL principal para teste
SELECT leads_system.trocar_ambiente_vsl('vsl-criar-saas', 'teste');

-- Trocar VSL DC360 para teste
SELECT leads_system.trocar_ambiente_vsl('vsl-dc360', 'teste');
```

### Voltar para Produção
```sql
-- Trocar VSL principal para produção
SELECT leads_system.trocar_ambiente_vsl('vsl-criar-saas', 'producao');

-- Trocar VSL DC360 para produção
SELECT leads_system.trocar_ambiente_vsl('vsl-dc360', 'producao');
```

### Verificar Mudança
```sql
-- Confirmar ambiente ativo
SELECT slug, ambiente_ativo, video_url_ativo 
FROM leads_system.vw_vsl_completa 
WHERE slug IN ('vsl-criar-saas', 'vsl-dc360');
```

## 🎮 Testes Manuais no Navegador

### 1. Testar VSL Principal
1. Ir para: `http://localhost:5000/vsl-criar-saas`
2. Verificar se o vídeo carrega
3. Verificar se os preços estão corretos
4. Testar se o botão CTA funciona
5. Verificar responsividade (mobile/desktop)

### 2. Testar VSL DC360
1. Ir para: `http://localhost:5000/vsl-dc360`
2. Verificar se o vídeo carrega
3. Verificar se os preços estão corretos
4. Testar se o botão CTA funciona
5. Verificar responsividade (mobile/desktop)

### 3. Verificar Console do Navegador
- Abrir DevTools (F12)
- Verificar se não há erros JavaScript
- Verificar se os vídeos HLS carregam corretamente
- Verificar se o tracking está funcionando

## 🔧 Problemas Comuns e Soluções

### ❌ VSL não carrega vídeo
**Possíveis causas:**
1. Banco de dados não acessível
2. Tabelas VSL não existem
3. URLs de vídeo incorretas
4. Problemas de rede

**Solução:**
```bash
# Verificar conexão DB
dotnet ef database update

# Executar script de criação das tabelas
psql -h 10.10.0.8 -U dcov3rl0rd -d leads -f scripts/002_create_vsl_videos_table.sql
```

### ❌ Vídeo não reproduz
**Possíveis causas:**
1. URL do vídeo inacessível
2. Formato HLS não suportado
3. CORS bloqueando o vídeo

**Solução:**
```bash
# Testar URL do vídeo
curl -I "https://videos.diogocosta.dev/hls/comunidade-didaticos-001.m3u8"

# Se não funcionar, usar vídeo de fallback no código
```

### ❌ Preços errados na VSL
**Possível causa:** Dados no banco desatualizados

**Solução:**
```sql
-- Atualizar preços da VSL
UPDATE leads_system.vsl_configs 
SET preco_original = 997.00, preco_promocional = 197.00
WHERE slug = 'vsl-criar-saas';

-- Confirmar alteração
SELECT slug, preco_original, preco_promocional 
FROM leads_system.vsl_configs 
WHERE slug = 'vsl-criar-saas';
```

## 📊 Comandos de Monitoramento

### Ver Estatísticas dos Vídeos
```sql
-- Listar todos os vídeos e status
SELECT 
    slug,
    nome,
    ambiente,
    ativo,
    created_at
FROM leads_system.vsl_videos
ORDER BY created_at DESC;
```

### Ver Configurações das VSLs
```sql
-- Listar todas as VSLs
SELECT 
    slug,
    nome,
    titulo,
    ambiente_ativo,
    ativo
FROM leads_system.vsl_configs
ORDER BY created_at DESC;
```

### Verificar Logs de Mudanças
```sql
-- Ver últimas atualizações
SELECT 
    slug,
    nome,
    ambiente_ativo,
    updated_at
FROM leads_system.vsl_configs
ORDER BY updated_at DESC;
```

## 🚀 Pré-Deploy Final

### Checklist Obrigatório:
- [ ] ✅ Banco de dados acessível
- [ ] ✅ Tabelas VSL existem e populadas
- [ ] ✅ URLs de vídeo funcionam
- [ ] ✅ VSLs carregam localmente
- [ ] ✅ Vídeos reproduzem no navegador
- [ ] ✅ Preços estão corretos
- [ ] ✅ CTAs redirecionam corretamente
- [ ] ✅ Logs não mostram erros
- [ ] ✅ Responsividade funciona
- [ ] ✅ Ambiente de produção ativo

### Comando Final de Verificação:
```bash
# Testar tudo de uma vez
echo "=== TESTE COMPLETO VSL ===" && \
curl -s http://localhost:5000/vsl-criar-saas | grep -q "De Zero ao Seu Primeiro SaaS" && echo "✅ VSL Principal OK" || echo "❌ VSL Principal FALHOU" && \
curl -s http://localhost:5000/vsl-dc360 | grep -q "DC360" && echo "✅ VSL DC360 OK" || echo "❌ VSL DC360 FALHOU" && \
echo "=== TESTE CONCLUÍDO ==="
```

## 📱 Teste Mobile

### Simular no Navegador:
1. Abrir DevTools (F12)
2. Ativar modo mobile (Ctrl+Shift+M)  
3. Testar em diferentes tamanhos:
   - iPhone SE (375x667)
   - iPhone 12 Pro (390x844)
   - Samsung Galaxy S20 (360x800)

### Verificar:
- [ ] Vídeo redimensiona corretamente
- [ ] Botões são clicáveis
- [ ] Texto legível
- [ ] Scroll funciona
- [ ] Loading rápido

---

**💡 Dica:** Sempre teste em ambiente de teste antes de colocar em produção. Use `SELECT trocar_ambiente_vsl('vsl-criar-saas', 'teste')` para alternar entre vídeos.

**🔄 Lembre-se:** Após confirmar que tudo funciona, volte para produção com `SELECT trocar_ambiente_vsl('vsl-criar-saas', 'producao')`. 