# 🎬 Sistema de Vídeos VSL - Guia Completo

## 📋 Visão Geral

Este sistema permite gerenciar os vídeos das VSLs (Video Sales Letter) através do banco de dados, ao invés de ter URLs hardcoded no código. Agora você pode facilmente trocar entre vídeos de teste e produção, além de gerenciar múltiplas VSLs.

## 🗄️ Estrutura do Banco de Dados

### Tabela: `vsl_videos`
Armazena os vídeos disponíveis para uso nas VSLs.

```sql
-- Colunas principais:
- id: Identificador único
- slug: Identificador amigável (ex: 'comunidade-didaticos-001')
- nome: Nome descritivo do vídeo
- video_url: URL completa do stream HLS
- ambiente: 'teste', 'homologacao', 'producao'
- ativo: true/false
```

### Tabela: `vsl_configs` 
Configurações das VSLs e associação com vídeos.

```sql
-- Colunas principais:
- slug: Identificador da VSL (ex: 'vsl-criar-saas')
- titulo, subtitulo, descricao: Conteúdo da VSL
- video_id: ID do vídeo de produção
- video_id_teste: ID do vídeo de teste
- ambiente_ativo: 'teste' ou 'producao'
- preco_original, preco_promocional: Preços
```

## 🚀 Como Usar

### 1. Executar o Script SQL
```bash
# Execute o script para criar as tabelas e dados iniciais
psql -h HOST -U USER -d DATABASE -f scripts/002_create_vsl_videos_table.sql
```

### 2. Trocar Ambiente (Teste ↔ Produção)

```sql
-- Usar vídeo de TESTE na VSL principal
SELECT trocar_ambiente_vsl('vsl-criar-saas', 'teste');

-- Voltar para PRODUÇÃO
SELECT trocar_ambiente_vsl('vsl-criar-saas', 'producao');

-- Trocar ambiente da VSL DC360
SELECT trocar_ambiente_vsl('vsl-dc360', 'teste');
```

### 3. Consultar Status Atual

```sql
-- Ver todas as VSLs e seus vídeos ativos
SELECT 
    slug,
    nome,
    ambiente_ativo,
    video_url_ativo,
    preco_promocional
FROM vw_vsl_completa
ORDER BY slug;
```

### 4. Adicionar Novo Vídeo

```sql
-- Inserir novo vídeo de teste
INSERT INTO vsl_videos (slug, nome, descricao, video_url, ambiente) 
VALUES (
    'novo-teste-2025',
    'Novo Vídeo Teste 2025',
    'Descrição do novo vídeo',
    'https://videos.diogocosta.dev/hls/novo-path/stream.m3u8',
    'teste'
);

-- Associar à VSL
UPDATE vsl_configs 
SET video_id_teste = (SELECT id FROM vsl_videos WHERE slug = 'novo-teste-2025')
WHERE slug = 'vsl-criar-saas';
```

## 🎯 URLs das VSLs

- **VSL Principal**: `/vsl-criar-saas`
  - Configuração: `vsl-criar-saas` na tabela `vsl_configs`
  
- **VSL DC360**: `/vsl-dc360`
  - Configuração: `vsl-dc360` na tabela `vsl_configs`

## 🔄 Fluxo Atual

1. **Request para VSL** → Controller busca configuração no banco
2. **Ambiente Ativo** → Sistema escolhe vídeo de teste ou produção automaticamente
3. **Fallback** → Se não encontrar no banco, usa dados padrão hardcoded
4. **Logs** → Sistema registra qual vídeo foi carregado

## 📊 Vídeos Configurados

### Produção
- **Slug**: `comunidade-didaticos-001`
- **URL**: `https://videos.diogocosta.dev/hls/comunidade-didaticos-001.m3u8`
- **Status**: Ativo para produção

### Teste  
- **Slug**: `teste-2025-06-08`
- **URL**: `https://videos.diogocosta.dev/hls/2025-06-08_15-58-24/stream.m3u8`
- **Status**: Ativo para testes

## 🛠️ Comandos Úteis

### Listar Todos os Vídeos
```sql
SELECT slug, nome, video_url, ambiente, ativo 
FROM vsl_videos 
ORDER BY ambiente, created_at DESC;
```

### Ver Configuração Completa de uma VSL
```sql
SELECT * FROM vw_vsl_completa WHERE slug = 'vsl-criar-saas';
```

### Desativar um Vídeo
```sql
UPDATE vsl_videos SET ativo = false WHERE slug = 'video-antigo';
```

### Atualizar URL de um Vídeo
```sql
UPDATE vsl_videos 
SET video_url = 'https://nova-url.com/stream.m3u8'
WHERE slug = 'comunidade-didaticos-001';
```

## 🔧 Solução de Problemas

### VSL Não Carrega Vídeo
1. Verificar se a VSL existe na tabela: `SELECT * FROM vsl_configs WHERE slug = 'vsl-criar-saas';`
2. Verificar se os vídeos estão ativos: `SELECT * FROM vsl_videos WHERE ativo = true;`
3. Verificar logs da aplicação para erros de conexão

### Vídeo Não Reproduz
1. Verificar se a URL está acessível no navegador
2. Verificar se o arquivo `stream.m3u8` existe no servidor
3. Verificar logs do servidor de vídeos

### Mudança de Ambiente Não Funciona
1. Verificar se a função existe: `SELECT trocar_ambiente_vsl('teste', 'teste');`
2. Verificar permissões do usuário da aplicação
3. Verificar se os IDs dos vídeos estão corretos na tabela `vsl_configs`

## 📱 Benefícios do Sistema

✅ **Flexibilidade**: Troca fácil entre teste e produção  
✅ **Centralizado**: Todas as configurações no banco de dados  
✅ **Versionamento**: Histórico de mudanças com timestamps  
✅ **Fallback**: Sistema não quebra se banco estiver indisponível  
✅ **Logs**: Rastreamento completo de qual vídeo foi carregado  
✅ **Escalabilidade**: Fácil adicionar novas VSLs e vídeos  

## 🔄 Próximos Passos

1. Executar o script SQL no ambiente de produção
2. Testar as VSLs para garantir funcionamento
3. Configurar monitoramento dos vídeos
4. Documentar processo para a equipe

---

**Desenvolvido por**: Diogo Costa - MVP Microsoft  
**Data**: Janeiro 2025  
**Versão**: 1.0.0 