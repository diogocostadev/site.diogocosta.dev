# 🔥 LANDING PAGES DOS DESAFIOS - GUERRA TOTAL

## 💣 O que foi implementado:

Landing pages brutalistas para conversão de desafios de SaaS. Sem header, footer, menu - só conversão pura.

### ✅ URLs Disponíveis:

- **Desafio Financeiro**: `https://diogocosta.dev/desafio-financeiro`
- **Desafio Leads**: `https://diogocosta.dev/desafio-leads` 
- **Desafio Vendas**: `https://diogocosta.dev/desafio-vendas`

### ✅ Páginas de Obrigado:

- `https://diogocosta.dev/obrigado-desafio-financeiro`
- `https://diogocosta.dev/obrigado-desafio-leads`
- `https://diogocosta.dev/obrigado-desafio-vendas`

## 🚀 Características:

### Layout Brutalista:
- ✔️ Fundo preto
- ✔️ Texto branco/laranja (#ff6b35)
- ✔️ Typography agressiva
- ✔️ Sem distrações
- ✔️ Mobile-first

### Fluxo de Conversão Híbrido:
1. **Botão Direto de Compra** → R$ 197 (direciona para Stripe)
2. **Formulário de Lead** → Captura Nome + Email

### Copy de Guerra:
- ✔️ Headlines massivas
- ✔️ Bullets de funcionalidades
- ✔️ Valor fixo R$ 197
- ✔️ CTA agressivo
- ✔️ Social proof implícito

## ⚙️ Configuração Necessária:

### 1. Checkout URLs (Stripe):
Editar no `DesafiosController.cs`:

```csharp
CheckoutUrl = "https://buy.stripe.com/seu-link-real-aqui"
```

### 2. Email de Notificação:
Alterar o email no controller:

```csharp
await _emailService.EnviarEmailAsync("SEU-EMAIL@domain.com", subject, body);
```

### 3. Tracking/Analytics:
Adicionar scripts de tracking nas páginas:
- Google Analytics
- Facebook Pixel
- Conversions tracking

## 📊 Estrutura Técnica:

### Controller: `DesafiosController.cs`
- Gerencia as 3 landing pages
- Sistema de captura de leads
- Integração com newsletter
- Notificação por email

### Views:
- `Views/Desafios/Index.cshtml` → Landing principal
- `Views/Desafios/Obrigado.cshtml` → Página de obrigado

### Models:
- `DesafioModel.cs` → Dados do desafio
- `DesafioLeadModel.cs` → Captura de leads

## 🔥 Próximos Passos:

1. **Configurar Stripe** → Criar produtos e checkout URLs
2. **Adicionar Tracking** → GA4, Facebook Pixel, etc.
3. **Testar Conversões** → A/B testing de headlines
4. **Email Marketing** → Sequência para leads capturados
5. **Métricas** → Configurar dashboards de conversão

## 💥 Comando para Rodar:

```bash
dotnet run
```

As páginas estarão disponíveis em:
- http://localhost:5000/desafio-financeiro
- http://localhost:5000/desafio-leads  
- http://localhost:5000/desafio-vendas

---

**🏆 RESULTADO: Landing pages de guerra pronta para conversão real.** 