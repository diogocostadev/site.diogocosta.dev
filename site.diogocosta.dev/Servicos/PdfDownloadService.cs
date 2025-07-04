using Microsoft.EntityFrameworkCore;
using site.diogocosta.dev.Data;
using site.diogocosta.dev.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace site.diogocosta.dev.Servicos
{
    public interface IPdfDownloadService
    {
        Task<PdfDownloadModel> RegistrarDownloadAsync(string arquivoNome, HttpContext httpContext, string? origem = null, string? email = null);
        Task<PdfDownloadStats> ObterEstatisticasAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
        Task<List<PdfDownloadModel>> ObterDownloadsRecentesAsync(int quantidade = 50);
        Task<List<PdfDownloadModel>> BuscarDownloadsPorEmailAsync(string email);
        Task<List<PdfDownloadModel>> BuscarDownloadsPorArquivoAsync(string arquivoNome);
    }

    public class PdfDownloadService : IPdfDownloadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PdfDownloadService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _environment;

        public PdfDownloadService(
            ApplicationDbContext context, 
            ILogger<PdfDownloadService> logger,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _environment = environment;
        }

        public async Task<PdfDownloadModel> RegistrarDownloadAsync(string arquivoNome, HttpContext httpContext, string? origem = null, string? email = null)
        {
            try
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                var referer = httpContext.Request.Headers["Referer"].ToString();
                var ipAddress = GetClientIpAddress(httpContext);

                // Tentar descobrir email automaticamente se não fornecido
                if (string.IsNullOrEmpty(email))
                {
                    email = await TentarRecuperarEmailAsync(ipAddress, referer);
                    if (!string.IsNullOrEmpty(email))
                    {
                        _logger.LogInformation("📧 Email recuperado automaticamente para download: {Email} (IP: {IP})", email, ipAddress);
                    }
                }

                // Parse básico do User Agent
                var userAgentInfo = ParseUserAgent(userAgent);

                // Obter localização do IP
                var localizacao = await ObterLocalizacaoIpAsync(ipAddress);

                // Tentar encontrar o lead associado pelo email
                int? leadId = null;
                if (!string.IsNullOrEmpty(email))
                {
                    var lead = await _context.Leads
                        .Where(l => l.Email == email.ToLower())
                        .OrderByDescending(l => l.CreatedAt)
                        .FirstOrDefaultAsync();
                    
                    leadId = lead?.Id;
                }

                var download = new PdfDownloadModel
                {
                    ArquivoNome = arquivoNome,
                    Email = email?.ToLower(),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Referer = referer,
                    Origem = origem,
                    Pais = localizacao?.Pais,
                    Cidade = localizacao?.Cidade,
                    Dispositivo = userAgentInfo.Dispositivo,
                    Navegador = userAgentInfo.Navegador,
                    SistemaOperacional = userAgentInfo.SistemaOperacional,
                    LeadId = leadId,
                    Sucesso = true,
                    DadosExtra = JsonSerializer.Serialize(new
                    {
                        ua_full = userAgent,
                        request_method = httpContext.Request.Method,
                        request_scheme = httpContext.Request.Scheme,
                        request_host = httpContext.Request.Host.Value,
                        is_mobile = userAgentInfo.Dispositivo == "mobile",
                        parsed_browser = userAgentInfo.Navegador,
                        parsed_os = userAgentInfo.SistemaOperacional,
                        email_recuperado_automaticamente = string.IsNullOrEmpty(httpContext.Request.Query["email"]) && !string.IsNullOrEmpty(email),
                        localizacao_api = localizacao != null ? new { 
                            pais = localizacao.Pais, 
                            cidade = localizacao.Cidade,
                            ip_consultado = ipAddress
                        } : null
                    })
                };

                _context.PdfDownloads.Add(download);
                await _context.SaveChangesAsync();

                _logger.LogInformation("📥 Download PDF registrado: {ArquivoNome} por {Email} ({IP}) de {Cidade}, {Pais} - ID: {Id}", 
                    arquivoNome, email ?? "anônimo", ipAddress, localizacao?.Cidade ?? "N/A", localizacao?.Pais ?? "N/A", download.Id);

                return download;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar download no banco: {ArquivoNome}", arquivoNome);
                
                // Em caso de erro, criar um registro básico
                var downloadBasico = new PdfDownloadModel
                {
                    ArquivoNome = arquivoNome,
                    Email = email?.ToLower(),
                    IpAddress = GetClientIpAddress(httpContext),
                    Origem = origem,
                    Sucesso = false,
                    DadosExtra = JsonSerializer.Serialize(new { erro = ex.Message })
                };

                try
                {
                    _context.PdfDownloads.Add(downloadBasico);
                    await _context.SaveChangesAsync();
                    return downloadBasico;
                }
                catch
                {
                    // Se ainda assim falhar, retornar objeto com ID 0
                    return downloadBasico;
                }
            }
        }

        /// <summary>
        /// Obtém localização (país e cidade) baseada no IP usando a API do usuario
        /// </summary>
        private async Task<LocalizacaoInfo?> ObterLocalizacaoIpAsync(string ipAddress)
        {
            try
            {
                // Não tentar localizar IPs locais/privados
                if (string.IsNullOrEmpty(ipAddress) || 
                    ipAddress == "unknown" || 
                    ipAddress.StartsWith("127.") ||
                    ipAddress.StartsWith("192.168.") ||
                    ipAddress.StartsWith("10.") ||
                    ipAddress.StartsWith("172."))
                {
                    _logger.LogInformation("🌍 IP local/privado detectado, pulando localização: {IP}", ipAddress);
                    return null;
                }

                // Determinar URL base da API baseada no ambiente
                var baseUrl = _environment.IsDevelopment() 
                    ? "https://dev-localiza-ip.diogocosta.dev"
                    : "https://localiza-ip.diogocosta.dev";

                _logger.LogInformation("🌍 Consultando localização para IP {IP} via {BaseUrl}", ipAddress, baseUrl);

                // Criar cliente HTTP e fazer requisições paralelas
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "site.diogocosta.dev");
                
                var paisTask = httpClient.GetStringAsync($"{baseUrl}/GetIp/GetCountry?ip={ipAddress}");
                var cidadeTask = httpClient.GetStringAsync($"{baseUrl}/GetIp/GetCity?ip={ipAddress}");

                // Aguardar ambas com timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var pais = await paisTask.WaitAsync(cts.Token);
                var cidade = await cidadeTask.WaitAsync(cts.Token);

                var localizacao = new LocalizacaoInfo
                {
                    Pais = !string.IsNullOrWhiteSpace(pais) ? pais.Trim() : null,
                    Cidade = !string.IsNullOrWhiteSpace(cidade) ? cidade.Trim() : null
                };

                _logger.LogInformation("🌍 Localização obtida para {IP}: {Cidade}, {Pais}", 
                    ipAddress, localizacao.Cidade ?? "N/A", localizacao.Pais ?? "N/A");

                return localizacao;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⏰ Timeout ao consultar localização para IP {IP}", ipAddress);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "🌍 Erro ao obter localização para IP {IP}", ipAddress);
                return null;
            }
        }

        /// <summary>
        /// Tenta recuperar o email de um usuário baseado no IP e referer
        /// </summary>
        private async Task<string?> TentarRecuperarEmailAsync(string ipAddress, string referer)
        {
            try
            {
                // Tentar por leads recentes do mesmo IP (últimas 24 horas)
                var lead = await _context.Leads
                    .Where(l => l.IpAddress == ipAddress)
                    .Where(l => l.CreatedAt >= DateTime.UtcNow.AddHours(-24))
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lead != null)
                {
                    _logger.LogInformation("📧 Email recuperado automaticamente do lead recente: {Email} para IP {IP}", lead.Email, ipAddress);
                    return lead.Email;
                }

                // Se não encontrar pelo IP, tentar pelo referer (se vier da página de obrigado)
                if (!string.IsNullOrEmpty(referer) && referer.Contains("obrigado"))
                {
                    var leadRecente = await _context.Leads
                        .Where(l => l.CreatedAt >= DateTime.UtcNow.AddHours(-2))
                        .OrderByDescending(l => l.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (leadRecente != null)
                    {
                        _logger.LogInformation("📧 Email recuperado do lead mais recente (últimas 2h): {Email}", leadRecente.Email);
                        return leadRecente.Email;
                    }
                }

                _logger.LogInformation("🔍 Não foi possível recuperar email automaticamente para IP {IP}", ipAddress);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao tentar recuperar email automaticamente para IP {IP}", ipAddress);
                return null;
            }
        }

        public async Task<PdfDownloadStats> ObterEstatisticasAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            try
            {
                dataInicio ??= DateTime.UtcNow.AddDays(-30);
                dataFim ??= DateTime.UtcNow.AddDays(1);

                var query = _context.PdfDownloads.Where(d => d.CreatedAt >= dataInicio && d.CreatedAt <= dataFim);

                var stats = new PdfDownloadStats
                {
                    TotalDownloads = await query.CountAsync(),
                    DownloadsHoje = await query.Where(d => d.CreatedAt >= DateTime.UtcNow.Date).CountAsync(),
                    DownloadsUltimaSemana = await query.Where(d => d.CreatedAt >= DateTime.UtcNow.AddDays(-7)).CountAsync(),
                    DownloadsUltimoMes = await query.Where(d => d.CreatedAt >= DateTime.UtcNow.AddDays(-30)).CountAsync()
                };

                // Arquivo mais baixado
                stats.ArquivoMaisBaixado = await query
                    .GroupBy(d => d.ArquivoNome)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();

                // Origem mais comum
                stats.OrigemMaisComum = await query
                    .Where(d => d.Origem != null)
                    .GroupBy(d => d.Origem)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();

                // Dispositivo mais comum
                stats.DispositivoMaisComum = await query
                    .Where(d => d.Dispositivo != null)
                    .GroupBy(d => d.Dispositivo)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();

                // Downloads por dia
                stats.DownloadsPorDia = await query
                    .GroupBy(d => d.CreatedAt.Date)
                    .Select(g => new DownloadPorDia
                    {
                        Data = g.Key,
                        Quantidade = g.Count()
                    })
                    .OrderBy(d => d.Data)
                    .ToListAsync();

                // Downloads por origem
                stats.DownloadsPorOrigem = await query
                    .Where(d => d.Origem != null)
                    .GroupBy(d => d.Origem)
                    .Select(g => new DownloadPorOrigem
                    {
                        Origem = g.Key!,
                        Quantidade = g.Count()
                    })
                    .OrderByDescending(d => d.Quantidade)
                    .ToListAsync();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de downloads");
                return new PdfDownloadStats();
            }
        }

        public async Task<List<PdfDownloadModel>> ObterDownloadsRecentesAsync(int quantidade = 50)
        {
            try
            {
                return await _context.PdfDownloads
                    .Include(d => d.Lead)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(quantidade)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter downloads recentes");
                return new List<PdfDownloadModel>();
            }
        }

        public async Task<List<PdfDownloadModel>> BuscarDownloadsPorEmailAsync(string email)
        {
            try
            {
                return await _context.PdfDownloads
                    .Where(d => d.Email == email.ToLower())
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar downloads por email: {Email}", email);
                return new List<PdfDownloadModel>();
            }
        }

        public async Task<List<PdfDownloadModel>> BuscarDownloadsPorArquivoAsync(string arquivoNome)
        {
            try
            {
                return await _context.PdfDownloads
                    .Include(d => d.Lead)
                    .Where(d => d.ArquivoNome == arquivoNome)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar downloads por arquivo: {ArquivoNome}", arquivoNome);
                return new List<PdfDownloadModel>();
            }
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            
            // Se há múltiplos IPs (proxy chain), pegar o primeiro
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(','))
                ipAddress = ipAddress.Split(',')[0].Trim();
            
            return ipAddress ?? "unknown";
        }

        private static UserAgentInfo ParseUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return new UserAgentInfo
                {
                    Dispositivo = "unknown",
                    Navegador = "unknown",
                    SistemaOperacional = "unknown"
                };
            }

            userAgent = userAgent.ToLower();

            // Detectar dispositivo
            string dispositivo = "desktop";
            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
                dispositivo = "mobile";
            else if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                dispositivo = "tablet";

            // Detectar navegador
            string navegador = "unknown";
            if (userAgent.Contains("chrome") && !userAgent.Contains("edg"))
                navegador = "Chrome";
            else if (userAgent.Contains("firefox"))
                navegador = "Firefox";
            else if (userAgent.Contains("safari") && !userAgent.Contains("chrome"))
                navegador = "Safari";
            else if (userAgent.Contains("edg"))
                navegador = "Edge";
            else if (userAgent.Contains("opera"))
                navegador = "Opera";

            // Detectar sistema operacional
            string sistemaOperacional = "unknown";
            if (userAgent.Contains("windows"))
                sistemaOperacional = "Windows";
            else if (userAgent.Contains("mac"))
                sistemaOperacional = "macOS";
            else if (userAgent.Contains("linux"))
                sistemaOperacional = "Linux";
            else if (userAgent.Contains("android"))
                sistemaOperacional = "Android";
            else if (userAgent.Contains("ios") || userAgent.Contains("iphone") || userAgent.Contains("ipad"))
                sistemaOperacional = "iOS";

            return new UserAgentInfo
            {
                Dispositivo = dispositivo,
                Navegador = navegador,
                SistemaOperacional = sistemaOperacional
            };
        }
    }

    // Classe auxiliar para informações do User Agent
    public class UserAgentInfo
    {
        public string Dispositivo { get; set; } = string.Empty;
        public string Navegador { get; set; } = string.Empty;
        public string SistemaOperacional { get; set; } = string.Empty;
    }

    // Classe auxiliar para informações de localização
    public class LocalizacaoInfo
    {
        public string? Pais { get; set; }
        public string? Cidade { get; set; }
    }
} 