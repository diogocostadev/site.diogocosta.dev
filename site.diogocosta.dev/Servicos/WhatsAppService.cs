using Microsoft.Extensions.Options;
using site.diogocosta.dev.Models;
using System.Text;
using System.Text.Json;

namespace site.diogocosta.dev.Servicos
{
    public interface IWhatsAppService
    {
        Task<bool> EnviarMensagemAsync(string numero, string mensagem);
        Task<bool> EnviarBoasVindasAsync(string numero, string nome);
        Task<bool> EnviarNotificacaoLiveAsync(string numero, string nome, string tituloLive, string dataLive, string linkLive);
        Task<bool> TestarEnvioAsync();
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(
            HttpClient httpClient,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            
            // Configurar timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> EnviarMensagemAsync(string numero, string mensagem)
        {
            try
            {
                // Limpar o número (remover espaços e caracteres especiais)
                var numeroLimpo = LimparNumero(numero);
                
                if (string.IsNullOrWhiteSpace(numeroLimpo))
                {
                    _logger.LogError("Número de WhatsApp inválido: {Numero}", numero);
                    return false;
                }

                // URL exata conforme exemplo que funcionou
                var url = "https://zap.didaticos.com/message/sendText/Código Central - Atendimentos";

                // Preparar dados exatamente como no exemplo que funcionou
                var dados = new
                {
                    number = numeroLimpo,
                    text = mensagem
                };

                var json = JsonSerializer.Serialize(dados);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Headers exatamente como no exemplo
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);

                // Enviar requisição
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("✅ WhatsApp enviado com sucesso para {Numero}. Response: {Response}", 
                        FormatarNumeroParaLog(numeroLimpo), responseContent);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Erro ao enviar WhatsApp para {Numero}: {StatusCode} - {Error}", 
                        FormatarNumeroParaLog(numeroLimpo), response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "❌ Erro HTTP ao enviar WhatsApp para {Numero}", FormatarNumeroParaLog(numero));
                return false;
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "❌ Timeout ao enviar WhatsApp para {Numero}", FormatarNumeroParaLog(numero));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro geral ao enviar WhatsApp para {Numero}", FormatarNumeroParaLog(numero));
                return false;
            }
        }

        public async Task<bool> EnviarBoasVindasAsync(string numero, string nome)
        {
            var nomeUsuario = !string.IsNullOrWhiteSpace(nome) ? nome : "Olá";
            
            var mensagem = $@"🎉 *Bem-vindo(a) às nossas Lives, {nomeUsuario}!*

Você foi cadastrado(a) com sucesso para receber notificações das nossas lives no YouTube e Twitch!

📺 *O que você vai receber:*
• Notificações de lives ao vivo
• Lembretes antes das transmissões
• Conteúdo exclusivo sobre programação
• Dicas de tecnologia e carreira

🚀 *Nossos canais:*
• YouTube: youtube.com/@diogocostadev
• Twitch: twitch.tv/diogocostadev

📱 *Próximas lives:*
Fique ligado(a) que você será avisado(a) sempre que formos ao ar!

---
*Diogo Costa - MVP Microsoft*
_Para parar de receber essas mensagens, acesse: diogocosta.dev/lives/sair_";

            return await EnviarMensagemAsync(numero, mensagem);
        }

        public async Task<bool> EnviarNotificacaoLiveAsync(string numero, string nome, string tituloLive, string dataLive, string linkLive)
        {
            var nomeUsuario = !string.IsNullOrWhiteSpace(nome) ? nome : "Olá";
            
            var mensagem = $@"🔴 *LIVE AO VIVO AGORA!*

Olá {nomeUsuario}! 

📺 *{tituloLive}*

⏰ *Data/Hora:* {dataLive}

🎯 *Acesse agora:*
{linkLive}

💡 Não perca essa oportunidade de aprender e interagir ao vivo!

---
*Diogo Costa - MVP Microsoft*
_Para parar de receber: diogocosta.dev/lives/sair_";

            return await EnviarMensagemAsync(numero, mensagem);
        }

        public async Task<bool> TestarEnvioAsync()
        {
            var numeroTeste = "5513997635900"; // Seu número para teste
            var mensagemTeste = $@"🔧 *TESTE DE SISTEMA - LIVES*

Olá Diogo! 

Este é um teste do sistema de notificações de lives.

📅 *Data/Hora do teste:* {DateTime.Now:dd/MM/yyyy HH:mm:ss}

✅ Se você recebeu esta mensagem, o sistema está funcionando corretamente!

---
*Sistema de Lives - Diogo Costa*
_Este é apenas um teste do sistema_";

            _logger.LogInformation("🧪 Iniciando teste de WhatsApp para o número: {Numero}", FormatarNumeroParaLog(numeroTeste));
            var resultado = await EnviarMensagemAsync(numeroTeste, mensagemTeste);
            
            if (resultado)
            {
                _logger.LogInformation("🎉 Teste de WhatsApp realizado com sucesso!");
            }
            else
            {
                _logger.LogError("❌ Falha no teste de WhatsApp");
            }
            
            return resultado;
        }

        private string LimparNumero(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return string.Empty;

            // Remove todos os caracteres não numéricos
            var numeroLimpo = System.Text.RegularExpressions.Regex.Replace(numero, @"[^\d]", "");

            // Se não começar com código do país, adicionar +55 (Brasil)
            if (!numeroLimpo.StartsWith("55") && numeroLimpo.Length >= 8)
            {
                numeroLimpo = "55" + numeroLimpo;
            }

            return numeroLimpo;
        }

        private string FormatarNumeroParaLog(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero) || numero.Length < 4)
                return "***";

            // Mascarar o número para logs (mostrar apenas os primeiros 4 e últimos 4 dígitos)
            var inicio = numero.Substring(0, 4);
            var fim = numero.Length > 8 ? numero.Substring(numero.Length - 4) : "****";
            var meio = new string('*', Math.Max(0, numero.Length - 8));

            return $"{inicio}{meio}{fim}";
        }
    }
}