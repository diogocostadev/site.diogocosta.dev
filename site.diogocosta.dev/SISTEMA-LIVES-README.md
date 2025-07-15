# Sistema de Interessados nas Lives - YouTube e Twitch

## 📋 Visão Geral

Sistema completo para capturar e gerenciar interessados nas lives do YouTube e Twitch, permitindo que os usuários se cadastrem para receber notificações via email e/ou WhatsApp.

## ✨ Funcionalidades Implementadas

### 1. Cadastro de Interessados
- **Rota**: `/lives` (GET/POST)
- **Campos obrigatórios**: Nome
- **Campos opcionais**: Email, WhatsApp
- **Validação**: Pelo menos um contato (email ou WhatsApp) deve ser informado
- **Recursos**:
  - Validação de formato de email
  - Validação de WhatsApp (apenas números, 8-15 dígitos)
  - Seleção de código de país (Brasil +55 por padrão)
  - Detecção automática de origem (YouTube, Twitch, site)
  - Captura de IP e User-Agent

### 2. Sistema de Boas-vindas
- **Email**: Envio automático de email de boas-vindas em HTML
- **WhatsApp**: Envio automático de mensagem de boas-vindas
- **Controle**: Rastreamento de envio para evitar duplicatas

### 3. Descadastramento
- **Rota**: `/lives/sair` (GET/POST)
- **Funcionalidade**: Remoção por email com motivo opcional
- **Segurança**: Soft delete (marca como inativo)

### 4. Páginas de Confirmação
- **Sucesso**: `/lives/sucesso` - Confirmação de cadastro
- **Descadastro**: `/lives/descadastro-sucesso` - Confirmação de remoção

### 5. APIs Administrativas
- **Estatísticas**: `/api/lives/stats` - Dados estatísticos completos
- **Lista Ativos**: `/api/lives/lista-ativos` - Interessados ativos

## 🗄️ Estrutura do Banco de Dados

### Tabela: `leads_system.interessados_lives`

```sql
CREATE TABLE leads_system.interessados_lives (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(200) NOT NULL,
    email VARCHAR(320) NULL,
    whatsapp VARCHAR(20) NULL,
    codigo_pais VARCHAR(5) NOT NULL DEFAULT '+55',
    data_cadastro TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    data_descadastro TIMESTAMP WITH TIME ZONE NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    ip_address VARCHAR(45) NULL,
    user_agent VARCHAR(500) NULL,
    origem VARCHAR(50) NULL,
    boas_vindas_email_enviado BOOLEAN NOT NULL DEFAULT FALSE,
    boas_vindas_whatsapp_enviado BOOLEAN NOT NULL DEFAULT FALSE,
    data_boas_vindas_email TIMESTAMP WITH TIME ZONE NULL,
    data_boas_vindas_whatsapp TIMESTAMP WITH TIME ZONE NULL
);
```

### Função de Estatísticas

```sql
SELECT * FROM leads_system.get_interessados_lives_stats();
```

## 🔧 Configuração

### 1. Appsettings.json

```json
{
  "WhatsAppSettings": {
    "ApiUrl": "https://zap.didaticos.com/message/sendText/Código Central - Atendimentos",
    "ApiKey": "429683C4C977415CAAFCCE10F7D57E11"
  }
}
```

### 2. Execução do Script SQL

```bash
# Execute o script no banco de dados
psql -h host -U user -d database -f scripts/005_create_interessados_lives_table.sql
```

## 📁 Arquivos Criados/Modificados

### Models
- `Models/InteressadoLiveModel.cs` - Modelo principal e DTOs
- `Models/WhatsAppSettings.cs` - Configurações do WhatsApp

### Services
- `Servicos/WhatsAppService.cs` - Serviço de envio de WhatsApp
- `Servicos/InteressadoLiveService.cs` - Serviço de gerenciamento de interessados

### Controllers
- `Controllers/LivesController.cs` - Controller principal

### Views
- `Views/Lives/Index.cshtml` - Página de cadastro
- `Views/Lives/Sucesso.cshtml` - Página de sucesso
- `Views/Lives/Sair.cshtml` - Página de descadastramento
- `Views/Lives/DescadastroSucesso.cshtml` - Confirmação de descadastro

### Database
- `scripts/005_create_interessados_lives_table.sql` - Script de criação da tabela

### Configuration
- `Program.cs` - Registro dos serviços
- `Data/ApplicationDbContext.cs` - Adição do DbSet
- `appsettings.json` - Configurações do WhatsApp

## 🚀 Como Usar

### 1. Acessar a Página de Cadastro
```
https://diogocosta.dev/lives
```

### 2. Cadastrar Interessado
- Preencher nome (obrigatório)
- Preencher email e/ou WhatsApp
- Selecionar código do país se necessário
- Submeter formulário

### 3. Receber Boas-vindas
- Email e/ou WhatsApp enviados automaticamente
- Confirmação na página de sucesso

### 4. Descadastrar (se necessário)
```
https://diogocosta.dev/lives/sair
```

## 📊 Consultas Úteis

### Listar Interessados Ativos
```sql
SELECT 
    nome,
    email,
    CASE 
        WHEN whatsapp IS NOT NULL THEN codigo_pais || ' ' || whatsapp
        ELSE NULL 
    END as whatsapp_formatado,
    origem,
    data_cadastro
FROM leads_system.interessados_lives 
WHERE ativo = TRUE 
ORDER BY data_cadastro DESC;
```

### Estatísticas por Origem
```sql
SELECT 
    origem,
    COUNT(*) as quantidade
FROM leads_system.interessados_lives 
WHERE ativo = TRUE 
GROUP BY origem;
```

### Interessados sem Boas-vindas
```sql
SELECT * FROM leads_system.interessados_lives 
WHERE ativo = TRUE 
AND (
    (email IS NOT NULL AND boas_vindas_email_enviado = FALSE) OR
    (whatsapp IS NOT NULL AND boas_vindas_whatsapp_enviado = FALSE)
);
```

## 🔒 Segurança e Validações

### Validações Implementadas
1. **Email**: Formato válido usando regex
2. **WhatsApp**: Apenas números, 8-15 dígitos
3. **Contato**: Pelo menos um (email ou WhatsApp) obrigatório
4. **Duplicatas**: Índices únicos para evitar cadastros duplicados
5. **Soft Delete**: Desativação em vez de exclusão

### Constraints do Banco
- Email format validation
- WhatsApp format validation
- País code validation
- Contact requirement (email OR whatsapp)

## 📱 Mensagens de Boas-vindas

### Email
- HTML responsivo
- Links para YouTube e Twitch
- Informações sobre o que esperar
- Link de descadastro

### WhatsApp
- Mensagem em texto com emojis
- Informações sobre canais
- Instruções para descadastro

## 🛠️ Manutenção

### Monitoramento
- Logs detalhados de todas as operações
- Rastreamento de envios de boas-vindas
- Estatísticas em tempo real

### Backup
- Dados preservados mesmo após descadastro
- Histórico completo de interações

## 🔄 Próximos Passos

1. **Notificações de Lives**: Sistema para avisar sobre lives ao vivo
2. **Dashboard Admin**: Interface web para gerenciar interessados
3. **Relatórios**: Dashboards com métricas detalhadas
4. **Integração**: Conectar com sistemas de streaming

## 📞 Suporte

Para dúvidas ou problemas:
- **Email**: noreply@diogocosta.dev
- **WhatsApp**: Via sistema implementado
- **Logs**: Verificar logs do aplicativo para debugging

---

**Desenvolvido por**: Diogo Costa - MVP Microsoft  
**Data**: 29/12/2024  
**Versão**: 1.0.0 