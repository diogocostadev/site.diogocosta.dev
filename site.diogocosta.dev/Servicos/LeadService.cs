using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;
using System.Text.Json;
using System.Net;

namespace site.diogocosta.dev.Servicos
{
    public interface ILeadService
    {
        Task<LeadModel?> CriarLeadAsync(CriarLeadRequest request, string? ipAddress = null, string? userAgent = null);
        Task<LeadModel?> BuscarLeadAsync(string email, string desafioSlug);
        Task<List<LeadStats>> ObterEstatisticasAsync();
        Task<bool> RegistrarInteracaoAsync(int leadId, string tipo, string descricao, object? dados = null);
        Task<List<LeadModel>> BuscarLeadsSemEmailBoasVindasAsync();
        Task<bool> MarcarEmailEnviadoAsync(int leadId, string tipoEmail);
    }

    public class LeadService : ILeadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeadService> _logger;
        private readonly IEmailService _emailService;

        public LeadService(
            ApplicationDbContext context, 
            ILogger<LeadService> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<LeadModel?> CriarLeadAsync(CriarLeadRequest request, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                // Verificar se já existe um lead com esse email para esse desafio
                var leadExistente = await BuscarLeadAsync(request.Email, request.DesafioSlug);
                if (leadExistente != null)
                {
                    _logger.LogInformation("Lead já existe: {Email} para {Desafio}", request.Email, request.DesafioSlug);
                    return leadExistente;
                }

                // Buscar a fonte do lead
                var fonte = await _context.LeadSources
                    .FirstOrDefaultAsync(ls => ls.Slug == request.DesafioSlug);

                if (fonte == null)
                {
                    // Criar fonte se não existir
                    fonte = new LeadSourceModel
                    {
                        Slug = request.DesafioSlug,
                        Nome = $"Desafio {request.DesafioSlug.Replace("-", " ").ToTitleCase()}",
                        Descricao = $"Landing page do {request.DesafioSlug}",
                        Ativo = true
                    };
                    _context.LeadSources.Add(fonte);
                    await _context.SaveChangesAsync();
                }

                // Criar o novo lead
                var novoLead = new LeadModel
                {
                    Nome = request.Nome,
                    Email = request.Email.ToLower(),
                    DesafioSlug = request.DesafioSlug,
                    SourceId = fonte.Id,
                    IpAddress = string.IsNullOrEmpty(ipAddress) ? null : IPAddress.Parse(ipAddress),
                    UserAgent = userAgent,
                    UtmSource = request.UtmSource,
                    UtmMedium = request.UtmMedium,
                    UtmCampaign = request.UtmCampaign,
                    UtmContent = request.UtmContent,
                    UtmTerm = request.UtmTerm,
                    Status = "novo",
                    OptIn = true
                };

                _context.Leads.Add(novoLead);
                await _context.SaveChangesAsync();

                // Registrar interação de cadastro
                await RegistrarInteracaoAsync(novoLead.Id, "form_submitted", "Lead se cadastrou no desafio", new
                {
                    desafio = request.DesafioSlug,
                    utm_source = request.UtmSource,
                    utm_medium = request.UtmMedium,
                    utm_campaign = request.UtmCampaign
                });

                _logger.LogInformation("Novo lead criado: {Email} para {Desafio} (ID: {LeadId})", 
                    request.Email, request.DesafioSlug, novoLead.Id);

                // Enviar email de boas-vindas em background
                _ = Task.Run(async () => await EnviarEmailBoasVindasAsync(novoLead));

                return novoLead;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar lead: {Email} para {Desafio}", request.Email, request.DesafioSlug);
                return null;
            }
        }

        public async Task<LeadModel?> BuscarLeadAsync(string email, string desafioSlug)
        {
            return await _context.Leads
                .Include(l => l.LeadSource)
                .FirstOrDefaultAsync(l => l.Email == email.ToLower() && l.DesafioSlug == desafioSlug);
        }

        public async Task<List<LeadStats>> ObterEstatisticasAsync()
        {
            var query = @"
                SELECT 
                    l.desafio_slug as DesafioSlug,
                    COALESCE(ls.nome, l.desafio_slug) as FonteNome,
                    COUNT(*) as TotalLeads,
                    COUNT(*) FILTER (WHERE l.opt_in = true) as OptInLeads,
                    COUNT(*) FILTER (WHERE l.status = 'novo') as LeadsNovos,
                    COUNT(*) FILTER (WHERE l.status = 'qualificado') as LeadsQualificados,
                    COUNT(*) FILTER (WHERE l.created_at >= CURRENT_DATE) as LeadsHoje,
                    COUNT(*) FILTER (WHERE l.created_at >= CURRENT_DATE - INTERVAL '7 days') as LeadsUltimaSemana
                FROM leads_system.leads l
                LEFT JOIN leads_system.lead_sources ls ON l.source_id = ls.id
                GROUP BY l.desafio_slug, ls.nome
                ORDER BY COUNT(*) DESC";

            var stats = await _context.Database
                .SqlQueryRaw<LeadStats>(query)
                .ToListAsync();

            return stats;
        }

        public async Task<bool> RegistrarInteracaoAsync(int leadId, string tipo, string descricao, object? dados = null)
        {
            try
            {
                var interacao = new LeadInteractionModel
                {
                    LeadId = leadId,
                    Tipo = tipo,
                    Descricao = descricao,
                    Dados = dados != null ? JsonDocument.Parse(JsonSerializer.Serialize(dados)) : null
                };

                _context.LeadInteractions.Add(interacao);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar interação para lead {LeadId}: {Tipo}", leadId, tipo);
                return false;
            }
        }

        public async Task<List<LeadModel>> BuscarLeadsSemEmailBoasVindasAsync()
        {
            var query = @"
                SELECT l.* 
                FROM leads_system.leads l
                LEFT JOIN leads_system.email_logs el ON l.id = el.lead_id 
                    AND el.template_id = (SELECT id FROM leads_system.email_templates WHERE slug = 'boas-vindas-desafio')
                WHERE el.id IS NULL 
                AND l.opt_in = true
                AND l.created_at > NOW() - INTERVAL '7 days'
                ORDER BY l.created_at DESC";

            return await _context.Leads
                .FromSqlRaw(query)
                .ToListAsync();
        }

        public async Task<bool> MarcarEmailEnviadoAsync(int leadId, string tipoEmail)
        {
            try
            {
                await RegistrarInteracaoAsync(leadId, "email_sent", $"Email enviado: {tipoEmail}", new
                {
                    tipo_email = tipoEmail,
                    enviado_em = DateTime.UtcNow
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar email enviado para lead {LeadId}: {TipoEmail}", leadId, tipoEmail);
                return false;
            }
        }

        private async Task EnviarEmailBoasVindasAsync(LeadModel lead)
        {
            try
            {
                // Buscar template de boas-vindas
                var template = await _context.EmailTemplates
                    .FirstOrDefaultAsync(t => t.Slug == "boas-vindas-desafio" && t.Ativo);

                if (template == null)
                {
                    _logger.LogWarning("Template de boas-vindas não encontrado");
                    return;
                }

                // Substituir variáveis no template
                var assunto = template.Assunto
                    .Replace("{{nome}}", lead.Nome)
                    .Replace("{{desafio}}", GetNomeDesafio(lead.DesafioSlug));

                var corpoHtml = template.CorpoHtml
                    .Replace("{{nome}}", lead.Nome)
                    .Replace("{{desafio}}", GetNomeDesafio(lead.DesafioSlug))
                    .Replace("{{email}}", lead.Email);

                var corpoTexto = template.CorpoTexto?
                    .Replace("{{nome}}", lead.Nome)
                    .Replace("{{desafio}}", GetNomeDesafio(lead.DesafioSlug))
                    .Replace("{{email}}", lead.Email);

                // Enviar email
                var emailEnviado = await _emailService.EnviarEmailAsync(
                    lead.Email, 
                    assunto, 
                    corpoHtml, 
                    corpoTexto);

                // Registrar log do email
                var emailLog = new EmailLogModel
                {
                    LeadId = lead.Id,
                    TemplateId = template.Id,
                    EmailTo = lead.Email,
                    Assunto = assunto,
                    Status = emailEnviado ? "enviado" : "falhou",
                    ErroMsg = emailEnviado ? null : "Falha no envio"
                };

                _context.EmailLogs.Add(emailLog);
                await _context.SaveChangesAsync();

                if (emailEnviado)
                {
                    await MarcarEmailEnviadoAsync(lead.Id, "boas-vindas");
                    _logger.LogInformation("Email de boas-vindas enviado para {Email}", lead.Email);
                }
                else
                {
                    _logger.LogError("Falha ao enviar email de boas-vindas para {Email}", lead.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de boas-vindas para lead {LeadId}", lead.Id);
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
    }

    // Extension method para Title Case
    public static class StringExtensions
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var words = input.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i][1..].ToLower();
                }
            }
            return string.Join(" ", words);
        }
    }
} 