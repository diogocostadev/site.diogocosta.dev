using Microsoft.AspNetCore.Mvc;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;
using site.diogocosta.dev.Servicos;
using site.diogocosta.dev.Contratos.Entrada;
using System.Net;

namespace site.diogocosta.dev.Controllers
{
    public class DesafiosController : Controller
    {
        private readonly INewsletterService _newsletterService;
        private readonly IEmailService _emailService;
        private readonly ILeadService _leadService;
        private readonly ILogger<DesafiosController> _logger;

        public DesafiosController(
            INewsletterService newsletterService, 
            IEmailService emailService,
            ILeadService leadService,
            ILogger<DesafiosController> logger)
        {
            _newsletterService = newsletterService;
            _emailService = emailService;
            _leadService = leadService;
            _logger = logger;
        }

        private List<DesafioModel> ObterDesafios()
        {
            return new List<DesafioModel>
            {
                new DesafioModel
                {
                    Nome = "Desafio Financeiro",
                    Slug = "desafio-financeiro",
                    Titulo = "DESAFIO FINANCEIRO — Seu SaaS no ar. Controlando. Faturando. Em 7 dias.",
                    Subtitulo = "Isso não é um curso. Isso é uma fábrica de SaaS.",
                    Produto = "SaaS de Controle Financeiro",
                    Descricao = "Sistema completo de controle financeiro pessoal e empresarial",
                    Funcionalidades = new List<string>
                    {
                        "API REST completa rodando",
                        "Banco de dados conectado e funcionando",
                        "Sistema de checkout integrado (Stripe)",
                        "CRUD completo de receitas e despesas",
                        "Deploy em VPS real com SSL",
                        "DNS configurado e backup automático",
                        "Dashboard responsivo e landing page",
                        "Relatórios financeiros e gráficos"
                    },
                    Aprendizados = new List<string>
                    {
                        "Arquitetura de SaaS escalável",
                        "Integração com gateway de pagamento",
                        "Deploy profissional e DevOps",
                        "Banco de dados e otimização",
                        "Interface moderna e responsiva",
                        "Segurança e autenticação",
                        "Monitoramento e logs"
                    },
                    CheckoutUrl = "https://buy.stripe.com/seu-link-aqui",
                    Ativo = true
                },
                new DesafioModel
                {
                    Nome = "Desafio Leads",
                    Slug = "desafio-leads",
                    Titulo = "DESAFIO LEADS — Seu SaaS no ar. Capturando. Convertendo. Em 7 dias.",
                    Subtitulo = "Isso não é um curso. Isso é uma fábrica de SaaS.",
                    Produto = "SaaS de Captura de Leads",
                    Descricao = "Sistema completo de captura, nutrição e conversão de leads",
                    Funcionalidades = new List<string>
                    {
                        "API REST completa rodando",
                        "Banco de dados conectado e funcionando",
                        "Sistema de checkout integrado (Stripe)",
                        "CRUD completo de leads e campanhas",
                        "Deploy em VPS real com SSL",
                        "DNS configurado e backup automático",
                        "Dashboard responsivo e landing page",
                        "Automação de email marketing"
                    },
                    Aprendizados = new List<string>
                    {
                        "Arquitetura de SaaS escalável",
                        "Integração com APIs de email",
                        "Deploy profissional e DevOps",
                        "Automação de marketing",
                        "Interface moderna e responsiva",
                        "Análise de conversão",
                        "Webhooks e integrações"
                    },
                    CheckoutUrl = "https://buy.stripe.com/seu-link-aqui",
                    Ativo = true
                },
                new DesafioModel
                {
                    Nome = "Desafio Vendas",
                    Slug = "desafio-vendas",
                    Titulo = "DESAFIO VENDAS — Seu SaaS no ar. Funcionando. Faturando. Em 7 dias.",
                    Subtitulo = "Isso não é um curso. Isso é uma fábrica de SaaS.",
                    Produto = "SaaS de Gestão de Vendas",
                    Descricao = "Sistema completo de CRM e gestão de vendas",
                    Funcionalidades = new List<string>
                    {
                        "API REST completa rodando",
                        "Banco de dados conectado e funcionando",
                        "Sistema de checkout integrado (Stripe)",
                        "CRM completo com pipeline de vendas",
                        "Deploy em VPS real com SSL",
                        "DNS configurado e backup automático",
                        "Dashboard responsivo e landing page",
                        "Relatórios de vendas e métricas"
                    },
                    Aprendizados = new List<string>
                    {
                        "Arquitetura de CRM escalável",
                        "Pipeline de vendas automatizado",
                        "Deploy profissional e DevOps",
                        "Métricas e analytics",
                        "Interface moderna e responsiva",
                        "Integração com ferramentas de vendas",
                        "Relatórios avançados"
                    },
                    CheckoutUrl = "https://buy.stripe.com/seu-link-aqui",
                    Ativo = true
                }
            };
        }

        public IActionResult Index(string slug)
        {
            var desafio = ObterDesafios().FirstOrDefault(d => d.Slug == slug && d.Ativo);
            
            if (desafio == null)
            {
                return NotFound();
            }

            return View(desafio);
        }

        [HttpPost]
        public async Task<IActionResult> CapturarLead(DesafioLeadModel model)
        {
            if (!ModelState.IsValid)
            {
                var desafio = ObterDesafios().FirstOrDefault(d => d.Slug == model.DesafioSlug && d.Ativo);
                if (desafio == null) return NotFound();
                
                ViewData["ErrorMessage"] = "Por favor, corrija os erros abaixo.";
                return View("Index", desafio);
            }

            try
            {
                // Capturar dados do usuário
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // Capturar UTM parameters
                var utmSource = Request.Query["utm_source"];
                var utmMedium = Request.Query["utm_medium"];
                var utmCampaign = Request.Query["utm_campaign"];
                var utmContent = Request.Query["utm_content"];
                var utmTerm = Request.Query["utm_term"];

                // Criar request para o sistema de leads
                var leadRequest = new CriarLeadRequest
                {
                    Nome = model.Nome,
                    Email = model.Email,
                    DesafioSlug = model.DesafioSlug,
                    UtmSource = utmSource,
                    UtmMedium = utmMedium,
                    UtmCampaign = utmCampaign,
                    UtmContent = utmContent,
                    UtmTerm = utmTerm
                };

                // Criar lead no banco de dados
                var lead = await _leadService.CriarLeadAsync(leadRequest, ipAddress, userAgent);

                if (lead != null)
                {
                    _logger.LogInformation("💰 Lead capturado com sucesso: {Email} para {Desafio} (ID: {LeadId})", 
                        model.Email, model.DesafioSlug, lead.Id);

                    // Também registrar no newsletter existente (integração dupla)
                    try
                    {
                        var usuario = new UsuarioNewsletter 
                        { 
                            Nome = model.Nome,
                            Email = model.Email 
                        };
                        
                        await _newsletterService.CadastrarUsuarioAsync(usuario);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao registrar no newsletter, mas lead foi capturado: {Email}", model.Email);
                    }

                    // Enviar email de notificação interna
                    _ = Task.Run(async () => await EnviarNotificacaoInternaAsync(lead));

                    return RedirectToAction("Obrigado", new { slug = model.DesafioSlug });
                }
                else
                {
                    _logger.LogError("Falha ao criar lead: {Email} para {Desafio}", model.Email, model.DesafioSlug);
                    throw new Exception("Falha ao processar lead");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao capturar lead: {Email} para {Desafio}", model.Email, model.DesafioSlug);
                
                var desafio = ObterDesafios().FirstOrDefault(d => d.Slug == model.DesafioSlug && d.Ativo);
                if (desafio == null) return NotFound();
                
                ViewData["ErrorMessage"] = "Erro ao processar sua solicitação. Tente novamente em alguns instantes.";
                return View("Index", desafio);
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                // Verificar headers de proxy
                var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',')[0].Trim();
                }

                var realIp = Request.Headers["X-Real-IP"].ToString();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // Fallback para RemoteIpAddress
                return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private async Task EnviarNotificacaoInternaAsync(LeadModel lead)
        {
            try
            {
                var nomeDesafio = GetNomeDesafio(lead.DesafioSlug);
                var subject = $"🔥 Novo Lead: {nomeDesafio}";
                
                var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px;'>
                    <h2 style='color: #ff6b35;'>🔥 Novo Lead Capturado!</h2>
                    
                    <div style='background: #f5f5f5; padding: 20px; border-left: 4px solid #ff6b35;'>
                        <p><strong>Nome:</strong> {lead.Nome}</p>
                        <p><strong>Email:</strong> {lead.Email}</p>
                        <p><strong>Desafio:</strong> {nomeDesafio}</p>
                        <p><strong>Data:</strong> {lead.CreatedAt:dd/MM/yyyy HH:mm}</p>
                        <p><strong>IP:</strong> {lead.IpAddress}</p>
                        
                        {(!string.IsNullOrEmpty(lead.UtmSource) ? $"<p><strong>UTM Source:</strong> {lead.UtmSource}</p>" : "")}
                        {(!string.IsNullOrEmpty(lead.UtmMedium) ? $"<p><strong>UTM Medium:</strong> {lead.UtmMedium}</p>" : "")}
                        {(!string.IsNullOrEmpty(lead.UtmCampaign) ? $"<p><strong>UTM Campaign:</strong> {lead.UtmCampaign}</p>" : "")}
                    </div>

                    <p style='margin-top: 20px;'>
                        <a href='mailto:{lead.Email}' style='background: #ff6b35; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                            Responder Lead
                        </a>
                    </p>
                </div>";

                await _emailService.EnviarEmailAsync("diogo@diogocosta.dev", subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação interna para lead {LeadId}", lead.Id);
            }
        }

        private static string GetNomeDesafio(string slug)
        {
            return slug switch
            {
                "desafio-vendas" => "Desafio SaaS Vendas",
                "desafio-financeiro" => "Desafio SaaS Financeiro",
                "desafio-leads" => "Desafio SaaS Leads",
                _ => slug.Replace("-", " ").ToTitleCase()
            };
        }

        public IActionResult Obrigado(string slug)
        {
            var desafio = ObterDesafios().FirstOrDefault(d => d.Slug == slug && d.Ativo);
            
            if (desafio == null)
            {
                return NotFound();
            }

            return View(desafio);
        }

        [HttpGet("/vsl-criar-saas")]
        public IActionResult VSL()
        {
            var model = new DesafioModel
            {
                Nome = "Método VSL SaaS",
                Slug = "vsl-criar-saas",
                Titulo = "De Zero ao Seu Primeiro SaaS Lucrativo em 7 Dias",
                Subtitulo = "O método completo para transformar sua ideia em um SaaS lucrativo, mesmo se você nunca programou antes",
                Produto = "Curso Completo Criar SaaS",
                Descricao = "Sistema passo-a-passo para criar, lançar e monetizar seu primeiro SaaS",
                EhVSL = true,
                VideoUrl = "https://comunidade.didaticos.com/videos-moodle/comunidade-didaticos-001.m3u8",
                PrecoOriginal = 997,
                PrecoPromocional = 197,
                ValidadePromocao = DateTime.Now.AddDays(3), // 3 dias de oferta
                Funcionalidades = new List<string>
                {
                    "7 módulos completos do zero ao SaaS no ar",
                    "Código fonte completo e comentado",
                    "Deploy em VPS real com SSL e domínio",
                    "Integração com Stripe para pagamentos",
                    "Dashboard administrativo completo",
                    "Sistema de autenticação e segurança",
                    "Backup automático e monitoramento",
                    "Landing page e checkout otimizados",
                    "Suporte direto no grupo privado",
                    "Atualizações gratuitas por 1 ano"
                },
                Aprendizados = new List<string>
                {
                    "Arquitetura de SaaS escalável",
                    "ASP.NET Core + PostgreSQL",
                    "Integração com gateway de pagamento",
                    "Deploy profissional e DevOps",
                    "Design system e UX/UI",
                    "SEO e marketing digital",
                    "Métricas e analytics",
                    "Estratégias de monetização"
                },
                CheckoutUrl = "https://buy.stripe.com/vsl-curso-completo",
                Ativo = true
            };

            return View(model);
        }

        [HttpGet("/obrigado-vsl")]
        public IActionResult ObrigadoVSL()
        {
            var model = new DesafioModel
            {
                Nome = "Método VSL SaaS",
                Slug = "vsl-criar-saas",
                Titulo = "Bem-vindo ao Método SaaS!",
                Subtitulo = "Você está a 7 dias do seu primeiro SaaS lucrativo",
                EhVSL = true
            };

            return View("Obrigado", model);
        }
    }
} 