using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using site.diogocosta.dev.Servicos.Interfaces;
using System.Net;

namespace site.diogocosta.dev.Servicos
{
    public interface IInteressadoLiveService
    {
        Task<InteressadoLiveModel?> CadastrarInteressadoAsync(CadastroInteressadoLiveRequest request, string? ipAddress = null, string? userAgent = null);
        Task<bool> DescadastrarInteressadoAsync(string email);
        Task<InteressadoLiveModel?> BuscarInteressadoAsync(string email);
        Task<EstatisticasInteressadosLives> ObterEstatisticasAsync();
        Task<List<InteressadoLiveModel>> ListarInteressadosAtivosAsync();
        Task<List<InteressadoLiveModel>> ListarInteressadosComEmailAsync();
        Task<List<InteressadoLiveModel>> ListarInteressadosComWhatsAppAsync();
        Task<bool> MarcarBoasVindasEnviadoAsync(int interessadoId, string tipoContato);
        Task<bool> ExisteInteressadoAsync(string email);
        Task<bool> ExisteInteressadoWhatsAppAsync(string whatsapp);
    }

    public class InteressadoLiveService : IInteressadoLiveService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InteressadoLiveService> _logger;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;

        public InteressadoLiveService(
            ApplicationDbContext context,
            ILogger<InteressadoLiveService> logger,
            IEmailService emailService,
            IWhatsAppService whatsAppService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
        }

        public async Task<InteressadoLiveModel?> CadastrarInteressadoAsync(CadastroInteressadoLiveRequest request, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                // Validar request
                if (!request.IsValid(out string errorMessage))
                {
                    _logger.LogWarning("Dados inválidos para cadastro de interessado: {Error}", errorMessage);
                    return null;
                }

                // Verificar se já existe um interessado com o email (se fornecido)
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var existeEmail = await ExisteInteressadoAsync(request.Email);
                    if (existeEmail)
                    {
                        _logger.LogInformation("Interessado já cadastrado com email: {Email}", request.Email);
                        return await BuscarInteressadoAsync(request.Email);
                    }
                }

                // Verificar se já existe um interessado com o WhatsApp (se fornecido)
                if (!string.IsNullOrWhiteSpace(request.WhatsApp))
                {
                    var whatsAppCompleto = $"{request.CodigoPais}{request.WhatsApp}";
                    var existeWhatsApp = await ExisteInteressadoWhatsAppAsync(whatsAppCompleto);
                    if (existeWhatsApp)
                    {
                        _logger.LogInformation("Interessado já cadastrado com WhatsApp: {WhatsApp}", whatsAppCompleto);
                        return await _context.InteressadosLives
                            .FirstOrDefaultAsync(i => i.CodigoPais == request.CodigoPais && i.WhatsApp == request.WhatsApp && i.Ativo);
                    }
                }

                // Criar novo interessado
                var interessado = new InteressadoLiveModel
                {
                    Nome = request.Nome.Trim(),
                    Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower(),
                    WhatsApp = string.IsNullOrWhiteSpace(request.WhatsApp) ? null : request.WhatsApp.Trim(),
                    CodigoPais = request.CodigoPais,
                    Origem = request.Origem,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DataCadastro = DateTime.UtcNow,
                    Ativo = true
                };

                _context.InteressadosLives.Add(interessado);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Novo interessado cadastrado: {Nome} - Email: {Email} - WhatsApp: {WhatsApp} (ID: {Id})",
                    interessado.Nome, interessado.Email ?? "N/A", interessado.WhatsAppFormatado ?? "N/A", interessado.Id);

                // Enviar boas-vindas de forma assíncrona mas aguardando a conclusão
                await EnviarBoasVindasAsync(interessado);

                return interessado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar interessado: {Nome} - {Email}", request.Nome, request.Email);
                return null;
            }
        }

        public async Task<bool> DescadastrarInteressadoAsync(string email)
        {
            try
            {
                var interessado = await _context.InteressadosLives
                    .FirstOrDefaultAsync(i => i.Email == email.ToLower() && i.Ativo);

                if (interessado == null)
                {
                    _logger.LogWarning("Tentativa de descadastro de email não encontrado: {Email}", email);
                    return false;
                }

                interessado.Ativo = false;
                interessado.DataDescadastro = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Interessado descadastrado: {Email}", email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descadastrar interessado: {Email}", email);
                return false;
            }
        }

        public async Task<InteressadoLiveModel?> BuscarInteressadoAsync(string email)
        {
            return await _context.InteressadosLives
                .FirstOrDefaultAsync(i => i.Email == email.ToLower() && i.Ativo);
        }

        public async Task<EstatisticasInteressadosLives> ObterEstatisticasAsync()
        {
            var hoje = DateTime.UtcNow.Date;
            var ultimaSemana = hoje.AddDays(-7);
            var ultimoMes = hoje.AddMonths(-1);

            var stats = new EstatisticasInteressadosLives
            {
                TotalInteressados = await _context.InteressadosLives.CountAsync(),
                TotalAtivos = await _context.InteressadosLives.CountAsync(i => i.Ativo),
                TotalDesativados = await _context.InteressadosLives.CountAsync(i => !i.Ativo),
                ComEmail = await _context.InteressadosLives.CountAsync(i => i.Ativo && !string.IsNullOrWhiteSpace(i.Email)),
                ComWhatsApp = await _context.InteressadosLives.CountAsync(i => i.Ativo && !string.IsNullOrWhiteSpace(i.WhatsApp)),
                ComAmbos = await _context.InteressadosLives.CountAsync(i => i.Ativo && !string.IsNullOrWhiteSpace(i.Email) && !string.IsNullOrWhiteSpace(i.WhatsApp)),
                CadastrosHoje = await _context.InteressadosLives.CountAsync(i => i.DataCadastro.Date == hoje),
                CadastrosUltimaSemana = await _context.InteressadosLives.CountAsync(i => i.DataCadastro >= ultimaSemana),
                CadastrosUltimoMes = await _context.InteressadosLives.CountAsync(i => i.DataCadastro >= ultimoMes)
            };

            // Cadastros por dia (últimos 30 dias)
            stats.CadastrosPorDia = await _context.InteressadosLives
                .Where(i => i.DataCadastro >= ultimoMes)
                .GroupBy(i => i.DataCadastro.Date)
                .Select(g => new CadastrosPorDia { Data = g.Key, Quantidade = g.Count() })
                .OrderBy(c => c.Data)
                .ToListAsync();

            // Cadastros por origem
            stats.CadastrosPorOrigem = await _context.InteressadosLives
                .Where(i => i.Ativo && !string.IsNullOrWhiteSpace(i.Origem))
                .GroupBy(i => i.Origem)
                .Select(g => new CadastrosPorOrigem { Origem = g.Key!, Quantidade = g.Count() })
                .OrderByDescending(c => c.Quantidade)
                .ToListAsync();

            return stats;
        }

        public async Task<List<InteressadoLiveModel>> ListarInteressadosAtivosAsync()
        {
            return await _context.InteressadosLives
                .Where(i => i.Ativo)
                .OrderByDescending(i => i.DataCadastro)
                .ToListAsync();
        }

        public async Task<List<InteressadoLiveModel>> ListarInteressadosComEmailAsync()
        {
            return await _context.InteressadosLives
                .Where(i => i.Ativo && !string.IsNullOrWhiteSpace(i.Email))
                .OrderByDescending(i => i.DataCadastro)
                .ToListAsync();
        }

        public async Task<List<InteressadoLiveModel>> ListarInteressadosComWhatsAppAsync()
        {
            return await _context.InteressadosLives
                .Where(i => i.Ativo && !string.IsNullOrWhiteSpace(i.WhatsApp))
                .OrderByDescending(i => i.DataCadastro)
                .ToListAsync();
        }

        public async Task<bool> MarcarBoasVindasEnviadoAsync(int interessadoId, string tipoContato)
        {
            try
            {
                var interessado = await _context.InteressadosLives.FindAsync(interessadoId);
                if (interessado == null) return false;

                if (tipoContato.ToLower() == "email")
                {
                    interessado.BoasVindasEmailEnviado = true;
                    interessado.DataBoasVindasEmail = DateTime.UtcNow;
                }
                else if (tipoContato.ToLower() == "whatsapp")
                {
                    interessado.BoasVindasWhatsAppEnviado = true;
                    interessado.DataBoasVindasWhatsApp = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar boas-vindas enviado para interessado {Id}: {Tipo}", interessadoId, tipoContato);
                return false;
            }
        }

        public async Task<bool> ExisteInteressadoAsync(string email)
        {
            return await _context.InteressadosLives
                .AnyAsync(i => i.Email == email.ToLower() && i.Ativo);
        }

        public async Task<bool> ExisteInteressadoWhatsAppAsync(string whatsapp)
        {
            // Precisa separar o código do país do número para consultar as colunas mapeadas
            // O whatsapp vem no formato completo (ex: +5511999999999)
            
            // Usar os códigos de país definidos no modelo PaisesWhatsApp
            var codigosPais = PaisesWhatsApp.CodigosPaises.Keys;
            
            foreach (var codigo in codigosPais)
            {
                if (whatsapp.StartsWith(codigo))
                {
                    var numeroLimpo = whatsapp.Substring(codigo.Length);
                    var existe = await _context.InteressadosLives
                        .AnyAsync(i => i.CodigoPais == codigo && i.WhatsApp == numeroLimpo && i.Ativo);
                    
                    if (existe) return true;
                }
            }
            
            return false;
        }

        private async Task EnviarBoasVindasAsync(InteressadoLiveModel interessado)
        {
            try
            {
                // Enviar email se tiver email
                if (!string.IsNullOrWhiteSpace(interessado.Email))
                {
                    var emailEnviado = await EnviarEmailBoasVindasAsync(interessado);
                    if (emailEnviado)
                    {
                        await MarcarBoasVindasEnviadoAsync(interessado.Id, "email");
                    }
                }

                // Enviar WhatsApp se tiver WhatsApp
                if (!string.IsNullOrWhiteSpace(interessado.WhatsApp))
                {
                    var whatsAppEnviado = await _whatsAppService.EnviarBoasVindasAsync(interessado.WhatsAppCompleto, interessado.Nome);
                    if (whatsAppEnviado)
                    {
                        await MarcarBoasVindasEnviadoAsync(interessado.Id, "whatsapp");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar boas-vindas para interessado {Id}", interessado.Id);
            }
        }

        private async Task<bool> EnviarEmailBoasVindasAsync(InteressadoLiveModel interessado)
        {
            try
            {
                var assunto = "🎉 Bem-vindo(a) às nossas Lives!";
                
                var corpoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #1f2937; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .live-info {{ background: #e5e7eb; padding: 15px; margin: 20px 0; border-radius: 8px; }}
        .channels {{ background: #dbeafe; padding: 15px; margin: 20px 0; border-radius: 8px; }}
        .footer {{ background: #e5e7eb; padding: 15px; text-align: center; font-size: 12px; }}
        .btn {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Bem-vindo(a) às nossas Lives!</h1>
        </div>
        <div class='content'>
            <h2>Olá, {interessado.Nome}!</h2>
            
            <p>Você foi cadastrado(a) com sucesso para receber notificações das nossas lives no YouTube e Twitch!</p>
            
            <div class='live-info'>
                <h3>📺 O que você vai receber:</h3>
                <ul>
                    <li>Notificações de lives ao vivo</li>
                    <li>Lembretes antes das transmissões</li>
                    <li>Conteúdo exclusivo sobre programação</li>
                    <li>Dicas de tecnologia e carreira</li>
                </ul>
            </div>
            
            <div class='channels'>
                <h3>🚀 Nossos canais:</h3>
                <p>
                    <strong>YouTube:</strong> <a href='https://youtube.com/@diogocostadev'>youtube.com/@diogocostadev</a><br>
                    <strong>Twitch:</strong> <a href='https://twitch.tv/diogocostadev'>twitch.tv/diogocostadev</a>
                </p>
            </div>
            
            <p>📱 <strong>Próximas lives:</strong><br>
            Fique ligado(a) que você será avisado(a) sempre que formos ao ar!</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='https://youtube.com/@diogocostadev' class='btn'>Acessar YouTube</a>
                <a href='https://twitch.tv/diogocostadev' class='btn'>Acessar Twitch</a>
            </div>
        </div>
        <div class='footer'>
            <p><strong>Diogo Costa</strong> - MVP Microsoft</p>
            <p>Para parar de receber essas notificações, <a href='https://diogocosta.dev/lives/sair'>clique aqui</a></p>
        </div>
    </div>
</body>
</html>";

                return await _emailService.EnviarEmailAsync(interessado.Email!, assunto, corpoHtml, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de boas-vindas para {Email}", interessado.Email);
                return false;
            }
        }
    }
} 