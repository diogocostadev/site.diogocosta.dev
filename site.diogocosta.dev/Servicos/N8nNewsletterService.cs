using site.diogocosta.dev.Contratos.Entrada;
using site.diogocosta.dev.Servicos.Interfaces;

namespace site.diogocosta.dev.Servicos;

public class N8nNewsletterService : INewsletterService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<N8nNewsletterService> _logger;
    private readonly string _webhookUrl;

    public N8nNewsletterService(HttpClient httpClient, ILogger<N8nNewsletterService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _webhookUrl = configuration["NewsletterSettings:N8nWebhookUrl"] ?? throw new ArgumentNullException(nameof(configuration), "A URL do webhook do n8n não foi configurada corretamente.");
    }

    public async Task<bool> CadastrarUsuarioAsync(UsuarioNewsletter usuario)
    {
        try
        {
            _logger.LogInformation("Iniciando cadastro de usuário na newsletter: {Email}", usuario.Email);
            
            var response = await _httpClient.PostAsJsonAsync(_webhookUrl, usuario);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Usuário cadastrado com sucesso na newsletter: {Email}", usuario.Email);
                return true;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Falha ao cadastrar usuário na newsletter. Status: {StatusCode}, Detalhes: {Details}", response.StatusCode, content);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar usuário na newsletter: {Email}", usuario.Email);
            return false;
        }
    }
}